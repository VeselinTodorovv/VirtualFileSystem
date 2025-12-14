namespace VirtualFileSystem.Core.Metadata;

public class DirectoryMetadata : FileMetadata
{
    protected override MetadataEntryType EntryType => MetadataEntryType.Directory;
}