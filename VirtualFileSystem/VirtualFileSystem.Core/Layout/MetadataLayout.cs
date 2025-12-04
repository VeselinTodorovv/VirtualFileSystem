namespace VirtualFileSystem.Core.Layout;

public class MetadataLayout(int startBlock, int entrySize, int entryCount)
{
    public int StartBlock { get; } = startBlock;
    public int EntrySize { get; } = entrySize;
    public int EntryCount { get; } = entryCount;
}