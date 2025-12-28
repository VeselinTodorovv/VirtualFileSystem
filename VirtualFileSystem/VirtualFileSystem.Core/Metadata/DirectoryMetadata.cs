namespace VirtualFileSystem.Core.Metadata;

public class DirectoryMetadata : FileMetadata
{
    protected override MetadataEntryType EntryType => MetadataEntryType.Directory;
    
    protected override void ReadTypeSpecific(byte[] buf)
    {
        // no specific logic
    }
    
    protected override void WriteTypeSpecific(byte[] buf)
    {
        // no specific logic
    }
}