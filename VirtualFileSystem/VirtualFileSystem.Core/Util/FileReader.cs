using VirtualFileSystem.Core.Container;

namespace VirtualFileSystem.Core.Util;

public class FileReader(ContainerStream container, int blockSize)
{
    public void ReadFile(int[] blocks, int blockCount, int fileSize, Stream output)
    {
        byte[] buffer = new byte[blockSize];
        int remaining = fileSize;
        
        for (int i = 0; i < blockCount; i++)
        {
            container.ReadBlock(blocks[i], buffer);
            
            int toWrite = Math.Min(remaining, blockSize);
            output.Write(buffer, 0, toWrite);
            remaining -= toWrite;
            
            if (remaining <= 0)
            {
                break;
            }
        }
    }
}
