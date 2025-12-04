namespace VirtualFileSystem.Core.Layout;

public class FileSystemLayout(int blockSize, int totalBlocks = 4096, int metadataEntryCount = 256)
{
    public int BlockSize { get; } = blockSize;
    
    public int TotalBlocks { get; } = totalBlocks;
    
    public int MetadataEntryCount { get; } = metadataEntryCount;
    
    public int MetadataStartBlock => 1;
    
    private int MetadataSizeBytes => FileMetadataLayout.EntrySize * MetadataEntryCount;
    
    public int MetadataBlocks => (MetadataSizeBytes + BlockSize - 1) / BlockSize;
    
    public int FreeBlockMapStartBlock => MetadataStartBlock + MetadataBlocks;
    
    private int BitmapBytes => TotalBlocks;
    
    public int BitmapBlocks => (BitmapBytes + BlockSize - 1) / BlockSize;
    
    public long ContainerSizeBytes => (long)BlockSize * TotalBlocks;
    
    public MetadataLayout MetadataLayout => new(
        MetadataStartBlock,
        FileMetadataLayout.EntrySize,
        MetadataEntryCount
    );
    
    public FreeBlockMapLayout FreeBlockMapLayout => new(
        FreeBlockMapStartBlock,
        TotalBlocks,
        BlockSize
    );
}