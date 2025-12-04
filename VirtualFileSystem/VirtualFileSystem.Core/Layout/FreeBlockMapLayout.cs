namespace VirtualFileSystem.Core.Layout;

public class FreeBlockMapLayout
{
    public int StartBlock { get; }
    public int BlockCount { get; }
    
    public int BitmapBytes => BlockCount;
    public int BitmapBlocks { get; }
    
    public FreeBlockMapLayout(int startBlock, int blockCount, int blockSize)
    {
        StartBlock = startBlock;
        BlockCount = blockCount;
        BitmapBlocks = (BitmapBytes + blockSize - 1) / blockSize;
    }
}