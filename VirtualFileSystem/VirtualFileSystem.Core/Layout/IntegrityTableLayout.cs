namespace VirtualFileSystem.Core.Layout;

public class IntegrityTableLayout(int startBlock, int blockCount)
{
    public int StartBlock { get; } = startBlock;
    public int BlockCount { get; } = blockCount;
}