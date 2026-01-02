using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Deduplication;

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
            if (reusedBlock != null)
            {
                blocks[blockCount] = reusedBlock.Value;
                blockCount++;
                
                continue;
            }
            
            int newBlock = AllocateAndWriteBlock(buffer);
            
            blocks[blockCount] = newBlock;
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
        int block = freeBlocks.Allocate();
        
        blockIO.WriteWithChecksum(block, buffer);
        deduplicationTable.AddNewBlock(block, buffer, blockSize);
        
        return block;
    }
}