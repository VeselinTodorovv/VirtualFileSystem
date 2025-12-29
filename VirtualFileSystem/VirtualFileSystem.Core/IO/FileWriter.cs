using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Resiliency;

namespace VirtualFileSystem.Core.IO;

public class FileWriter(BlockIO blockIO, FreeBlockMap freeBlocks, DeduplicationTable deduplicationTable, int b)
{
    public int WriteFile(Stream input, int[] blocks)
    {
        int blockCount = 0;
        byte[] buffer = new byte[b];
        
        int read;
        while ((read = input.Read(buffer, 0, b)) > 0)
        {
            if (read < b)
            {
                Array.Clear(buffer, read, b - read);
            }
            
            int hash = IntegrityTable.Checksum(buffer, b); // log only
            int? existingBlock = deduplicationTable.FindDuplicateAndAddRef(buffer, b);
            
            if (existingBlock != null)
            {
                FsLogger.Log($"WriteFile: REUSE block={existingBlock.Value}, hash=0x{hash:X8}");
                
                blocks[blockCount] = existingBlock.Value;
                blockCount++;
                
                continue;
            }
            
            int block = freeBlocks.Allocate();
            long blockOff = blockIO.OffsetOfBlock(block);
            
            FsLogger.Log($"WriteFile: NEW block={block} off=0x{blockOff:X} hash=0x{hash:X8}");
            
            blockIO.WriteWithChecksum(block, buffer);
            deduplicationTable.AddNewBlock(block, buffer, b);
            
            blocks[blockCount] = block;
            blockCount++;
        }
        
        return blockCount;
    }
}