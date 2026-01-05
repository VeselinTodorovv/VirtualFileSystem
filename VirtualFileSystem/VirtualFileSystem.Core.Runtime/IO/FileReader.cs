using VirtualFileSystem.Core.Layout.Constants;

namespace VirtualFileSystem.Core.IO;

public class FileReader(BlockIO blockIO, int blockSize)
{
    private const int IndirectTriggerIndex = FileMetadataLayout.MaxBlocks - 1;
    
    public void ReadFile(int[] blocks, int blockCount, int fileSize, Stream output)
    {
        byte[] buffer = new byte[blockSize];
        byte[] indirectCache = new byte[blockSize];
        int remaining = fileSize;
        
        for (int i = 0; i < blockCount; i++)
        {
            int physicalBlock = i < IndirectTriggerIndex 
                ? blocks[i] 
                : GetIndirectBlockIndex(i, blocks[IndirectTriggerIndex], indirectCache);
            
            blockIO.ReadVerified(physicalBlock, buffer);
            
            int toWrite = Math.Min(remaining, blockSize);
            output.Write(buffer, 0, toWrite);
            remaining -= toWrite;
            
            if (remaining <= 0)
            {
                break;
            }
        }
    }
    
    private int GetIndirectBlockIndex(int currentLogicalIndex, int indirectIndexBlock, byte[] cache)
    {
        if (currentLogicalIndex == IndirectTriggerIndex)
        {
            blockIO.ReadVerified(indirectIndexBlock, cache);
        }
        
        int entryIndex = currentLogicalIndex - IndirectTriggerIndex;
        int indirectBlockIndex = BitConverter.ToInt32(cache, entryIndex * 4);
        
        return indirectBlockIndex;
    }
}
