namespace VirtualFileSystem.Core.Layout;

public class FileSystemLayout(int blockSize, int totalBlocks = 4096, int metadataEntryCount = 256)
{
    public int BlockSize { get; } = blockSize;
    public int TotalBlocks { get; } = totalBlocks;
    
    public int MetadataStartBlock => 1;
    public int FreeBlockMapStartBlock => MetadataStartBlock + MetadataBlocks;
    public int IntegrityTableStartBlock => FreeBlockMapStartBlock + BitmapBlocks;
    public int DataStartBlock => IntegrityTableStartBlock + IntegrityTableBlocks;
    
    public int MetadataBlocks => (MetadataSizeBytes + BlockSize - 1) / BlockSize;
    public int BitmapBlocks => (TotalBlocks + BlockSize - 1) / BlockSize;
    
    private int IntegrityTableBytes => TotalBlocks * 4;
    public int IntegrityTableBlocks => (IntegrityTableBytes + BlockSize - 1) / BlockSize;
    
    public long ContainerSizeBytes => (long)BlockSize * TotalBlocks;
    private int MetadataSizeBytes => FileMetadataLayout.EntrySize * metadataEntryCount;
    
    public MetadataLayout MetadataLayout => new(MetadataStartBlock, FileMetadataLayout.EntrySize, metadataEntryCount);
    public FreeBlockMapLayout FreeBlockMapLayout => new(FreeBlockMapStartBlock, TotalBlocks);
    public IntegrityTableLayout IntegrityTableLayout => new(IntegrityTableStartBlock, IntegrityTableBlocks);
}