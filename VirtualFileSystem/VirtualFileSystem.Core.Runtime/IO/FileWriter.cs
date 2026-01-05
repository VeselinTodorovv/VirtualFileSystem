using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Util.Layout;

namespace VirtualFileSystem.Core.IO;

public class FileWriter(BlockIO blockIO, FreeBlockMap freeBlocks, DeduplicationTable deduplicationTable, int blockSize)
{
    public int WriteFile(Stream input, int[] blocks)
    {
        int blockCount = 0;
        byte[] buffer = new byte[blockSize];
        
        while (TryReadBlock(input, buffer))
        {
            int? reusedBlock = deduplicationTable.FindDuplicateAndAddRef(buffer, buffer.Length);
            int targetBlock = reusedBlock ?? AllocateAndWriteBlock(buffer);
            
            if (blockCount < BlockResolver.IndirectTriggerIndex)
            {
                blocks[blockCount] = targetBlock;
            }
            else
            {
                UpdateIndirectIndex(blockCount, targetBlock, blocks);
            }
            
            blockCount++;
        }
        
        return blockCount;
    }
    
    private static bool TryReadBlock(Stream input, byte[] buffer)
    {
        int bytesRead = input.Read(buffer, 0, buffer.Length);
        
        if (bytesRead <= 0)
        {
            return false;
        }
        if (bytesRead < buffer.Length)
        {
            Array.Clear(buffer, bytesRead, buffer.Length - bytesRead);
        }
        
        return true;
    }
    
    private int AllocateAndWriteBlock(byte[] buffer)
    {
        int newBlock = freeBlocks.Allocate();
        
        blockIO.WriteWithChecksum(newBlock, buffer);
        deduplicationTable.AddNewBlock(newBlock, buffer, blockSize);
        
        return newBlock;
    }
    
    private void UpdateIndirectIndex(int logicalIndex, int physicalDataBlock, int[] metadataBlocks)
    {
        int offset = BlockResolver.GetIndirectOffset(logicalIndex);
        
        if (offset + sizeof(int) > blockSize)
        {
            throw new InvalidOperationException("File exceeds indirect block capacity.");
        }
        
        byte[] indexData = new byte[blockSize];
        
        int indirectBlock;
        if (logicalIndex == BlockResolver.IndirectTriggerIndex)
        {
            indirectBlock = freeBlocks.Allocate();
            metadataBlocks[BlockResolver.IndirectTriggerIndex] = indirectBlock;
        }
        else
        {
            indirectBlock = metadataBlocks[BlockResolver.IndirectTriggerIndex];
            blockIO.ReadVerified(indirectBlock, indexData);
        }
        
        BitConverter.TryWriteBytes(indexData.AsSpan(offset), physicalDataBlock);
        
        blockIO.WriteWithChecksum(indirectBlock, indexData);
    }
}