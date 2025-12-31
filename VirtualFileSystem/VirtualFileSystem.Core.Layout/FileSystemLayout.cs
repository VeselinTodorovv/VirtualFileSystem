using VirtualFileSystem.Core.Layout.Constants;
using VirtualFileSystem.Core.Layout.Models;

namespace VirtualFileSystem.Core.Layout;

public class FileSystemLayout(int blockSize, int totalBlocks = 4096, int metadataEntryCount = 256)
{
    public int BlockSize { get; } = blockSize;
    public int TotalBlocks { get; } = totalBlocks;
    
    public int MetadataStartBlock => 1;
    public int MetadataBlocks => (MetadataSizeBytes + BlockSize - 1) / BlockSize;
    private int MetadataSizeBytes => FileMetadataLayout.EntrySize * metadataEntryCount;
    
    public int FreeBlockMapStartBlock => MetadataStartBlock + MetadataBlocks;
    public int FreeBlockMapBlocks => (TotalBlocks + BlockSize - 1) / BlockSize;
    
    public int IntegrityTableStartBlock => FreeBlockMapStartBlock + FreeBlockMapBlocks;
    public int IntegrityTableBlocks => (IntegrityTableBytes + BlockSize - 1) / BlockSize;
    private int IntegrityTableBytes => TotalBlocks * 4;
    
    public int DataStartBlock => IntegrityTableStartBlock + IntegrityTableBlocks;
    public long ContainerSizeBytes => (long)BlockSize * TotalBlocks;
    
    public MetadataLayout MetadataLayout => new(MetadataStartBlock, FileMetadataLayout.EntrySize, metadataEntryCount);
    public FreeBlockMapLayout FreeBlockMapLayout => new(FreeBlockMapStartBlock, TotalBlocks);
    public IntegrityTableLayout IntegrityTableLayout => new(IntegrityTableStartBlock, IntegrityTableBlocks);
}