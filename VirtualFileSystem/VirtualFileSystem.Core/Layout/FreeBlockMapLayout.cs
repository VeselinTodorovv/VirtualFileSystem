namespace VirtualFileSystem.Core.Layout;

public class FreeBlockMapLayout(int startBlock, int blockCount)
{
    public int StartBlock { get; } = startBlock;
    public int BlockCount { get; } = blockCount;
}