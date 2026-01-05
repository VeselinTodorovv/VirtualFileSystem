using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Layout.Constants;

namespace VirtualFileSystem.Core.IO;

public class FileWriter(BlockIO blockIO, FreeBlockMap freeBlocks, DeduplicationTable deduplicationTable, int blockSize)
{
    private const int IndirectTriggerIndex = FileMetadataLayout.MaxBlocks - 1;
    
    public int WriteFile(Stream input, int[] blocks)
    {
        int blockCount = 0;
        byte[] buffer = new byte[blockSize];
        
        while (TryReadBlock(input, buffer))
        {
            int? reusedBlock = deduplicationTable.FindDuplicateAndAddRef(buffer, buffer.Length);
            int targetBlock = reusedBlock ?? AllocateAndWriteBlock(buffer, blockCount, blocks);
            
            if (blockCount < FileMetadataLayout.MaxBlocks - 1)
            {
                blocks[blockCount] = targetBlock;
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
    
    private int AllocateAndWriteBlock(byte[] buffer, int currentBlockIndex, int[] metadataBlocks)
    {
        int newBlock = freeBlocks.Allocate();
        
        blockIO.WriteWithChecksum(newBlock, buffer);
        deduplicationTable.AddNewBlock(newBlock, buffer, blockSize);
        
        if (currentBlockIndex >= IndirectTriggerIndex)
        {
            UpdateIndirectIndex(currentBlockIndex, newBlock, metadataBlocks);
        }
        
        return newBlock;
    }
    
    private void UpdateIndirectIndex(int currentBlockIndex, int dataBlock, int[] metadataBlocks)
    {
        if (currentBlockIndex == IndirectTriggerIndex)
        {
            // create index block
            int indirectIndexBlock = freeBlocks.Allocate();
            metadataBlocks[IndirectTriggerIndex] = indirectIndexBlock;
            
            byte[] indexData = new byte[blockSize];
            BitConverter.GetBytes(dataBlock).CopyTo(indexData, 0);
            blockIO.WriteWithChecksum(indirectIndexBlock, indexData);
        }
        else
        {
            // add pointer to index block
            int indirectIndexBlock = metadataBlocks[IndirectTriggerIndex];
            int entryIndex = currentBlockIndex - IndirectTriggerIndex;
            int offset = entryIndex * 4;
            
            if (offset + 4 > blockSize)
            {
                throw new InvalidOperationException("File size exceeds maximum addressable blocks for this BlockSize.");
            }
            
            byte[] indexData = new byte[blockSize];
            
            blockIO.ReadVerified(indirectIndexBlock, indexData);
            BitConverter.GetBytes(dataBlock).CopyTo(indexData, offset);
            blockIO.WriteWithChecksum(indirectIndexBlock, indexData);
        }
    }
}