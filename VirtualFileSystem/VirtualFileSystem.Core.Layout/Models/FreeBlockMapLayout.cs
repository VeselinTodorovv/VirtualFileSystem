namespace VirtualFileSystem.Core.Layout.Models;

public class FreeBlockMapLayout(int startBlock, int blockCount)
{
    public int StartBlock { get; } = startBlock;
    public int BlockCount { get; } = blockCount;
}