namespace VirtualFileSystem.Core.Layout;

public class FileSystemLayout(int blockSize, int entrySize, int entryCount, int blockCount)
{
    public int BlockSize { get; } = blockSize;
    internal int EntrySize { get; } = entrySize;
    internal int EntryCount { get; } = entryCount;
    internal int BlockCount { get; } = blockCount;
    
    public int MetadataStartBlock => 1;
    
    private int MetadataSize => EntrySize * EntryCount;
    
    public int MetadataBlocks => (MetadataSize + BlockSize - 1) / BlockSize;
    
    public int FreeBlockMapStartBlock => MetadataStartBlock + MetadataBlocks;
    
    private int BitmapBytes => BlockCount;
    
    public int BitmapBlocks => (BitmapBytes + BlockSize - 1) / BlockSize;
    
    public long ContainerSizeBytes => (long)BlockCount * BlockSize;
}