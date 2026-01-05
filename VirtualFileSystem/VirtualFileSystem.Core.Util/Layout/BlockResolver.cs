using VirtualFileSystem.Core.Layout.Constants;

namespace VirtualFileSystem.Core.Util.Layout;

public static class BlockResolver
{
    public const int IndirectTriggerIndex = FileMetadataLayout.MaxBlocks - 1; // 47
    
    public static int GetPhysicalIndex(int logicalIndex, int[] metadataBlocks, byte[]? indirectCache)
    {
        if (logicalIndex < IndirectTriggerIndex)
        {
            return metadataBlocks[logicalIndex];
        }
        
        if (indirectCache == null)
        {
            throw new ArgumentNullException(nameof(indirectCache), $"Indirect cache is required for indices >= {FileMetadataLayout.MaxBlocks - 1}");
        }
        
        int entryInIndirect = (logicalIndex - IndirectTriggerIndex) * sizeof(int);
        return BitConverter.ToInt32(indirectCache, entryInIndirect);
    }
    
    public static int GetIndirectOffset(int logicalIndex)
    {
        return (logicalIndex - IndirectTriggerIndex) * sizeof(int);
    }
}