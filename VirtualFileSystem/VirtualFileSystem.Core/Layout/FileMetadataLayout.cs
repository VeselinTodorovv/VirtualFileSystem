namespace VirtualFileSystem.Core.Layout;

public static class FileMetadataLayout
{
    public const int NameMaxLength = 32;
    public const int MaxBlocks = 48;
    
    public const int UsedOffset = 0;
    public const int NameOffset = UsedOffset + 1;
    public const int SizeOffset = NameOffset + NameMaxLength;
    public const int BlockCountOffset = SizeOffset + 4;
    public const int BlockListOffset = BlockCountOffset + 4;
    
    public const int EntrySize = BlockListOffset + MaxBlocks * 4;
}