using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;

namespace VirtualFileSystem.Core.IO;

public class FileWriter(ContainerStream container, FreeBlockMap freeBlocks, DeduplicationTable deduplicationTable, int b)
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

            int? existingBlock = deduplicationTable.FindDuplicate(buffer, b);
            if (existingBlock != null)
            {
                blockCount += existingBlock.Value;
                continue;
            }
            
            int block = freeBlocks.Allocate();
            container.WriteBlock(block, buffer);
            
            deduplicationTable.Add(block, buffer, b);
            blocks[blockCount++] = block;
        }
        
        return blockCount;
    }
}