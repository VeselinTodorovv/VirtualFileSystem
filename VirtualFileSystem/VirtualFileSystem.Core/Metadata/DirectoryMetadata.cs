namespace VirtualFileSystem.Core.Metadata;

public class DirectoryMetadata : FileMetadata
{
    public override MetadataEntryType EntryType => MetadataEntryType.Directory;
}