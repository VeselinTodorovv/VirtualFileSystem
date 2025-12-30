using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Resiliency;

namespace VirtualFileSystem.Core.IO;

public class FileWriter(BlockIO blockIO, FreeBlockMap freeBlocks, DeduplicationTable deduplicationTable, int blockSize)
{
    public int WriteFile(Stream input, int[] blocks)
    {
        int blockCount = 0;
        byte[] buffer = new byte[blockSize];
        
        while (TryReadBlock(input, buffer))
        {
            int hash = IntegrityTable.Checksum(buffer, buffer.Length);
            
            int? reusedBlock = TryReuseBlock(buffer, hash);
            if (reusedBlock != null)
            {
                blocks[blockCount] = reusedBlock.Value;
                blockCount++;
                
                continue;
            }
            
            int newBlock = AllocateAndWriteBlock(buffer, hash);
            
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
    
    private int? TryReuseBlock(byte[] buffer, int hash)
    {
        int? existing = deduplicationTable.FindDuplicateAndAddRef(buffer, buffer.Length);
        if (existing != null)
        {
            FsLogger.Log($"WriteFile: REUSE block={existing.Value}, hash=0x{hash:X8}");
        }
        
        return existing;
    }
    
    private int AllocateAndWriteBlock(byte[] buffer, int hash)
    {
        int block = freeBlocks.Allocate();
        long blockOff = blockIO.OffsetOfBlock(block);
        
        FsLogger.Log($"WriteFile: NEW block={block} off=0x{blockOff:X} hash=0x{hash:X8}");
        
        blockIO.WriteWithChecksum(block, buffer);
        deduplicationTable.AddNewBlock(block, buffer, blockSize);
        
        return block;
    }
}