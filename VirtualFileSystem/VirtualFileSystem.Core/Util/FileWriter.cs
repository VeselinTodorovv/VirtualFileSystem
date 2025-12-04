using VirtualFileSystem.Core.Container;

namespace VirtualFileSystem.Core.Util;

public class FileWriter(ContainerStream container, FreeBlockMap freeBlocks, int b)
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
            
            int block = freeBlocks.Allocate();
            container.WriteBlock(block, buffer);
            
            blocks[blockCount++] = block;
        }
        
        return blockCount;
    }
}