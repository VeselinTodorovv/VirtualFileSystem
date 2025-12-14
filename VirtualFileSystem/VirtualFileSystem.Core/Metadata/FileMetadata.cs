using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Metadata;

public class FileMetadata
{
    public bool IsUsed;
    public string Name = "";
    public int Size;
    public int BlockCount;
    public readonly int[] Blocks = new int[FileMetadataLayout.MaxBlocks];
    
    public int ParentIndex;
    protected virtual MetadataEntryType EntryType => MetadataEntryType.File;
    
    public byte[] ToBytes()
    {
        byte[] buf = new byte[FileMetadataLayout.EntrySize];
        
        buf[FileMetadataLayout.UsedOffset] = (byte)(IsUsed ? 1 : 0);
        buf[FileMetadataLayout.TypeOffset] = (byte)EntryType;
        BitConverter.GetBytes(ParentIndex).CopyTo(buf, FileMetadataLayout.ParentOffset);
        
        ByteUtil.WriteFixedString(Name, buf, FileMetadataLayout.NameOffset, FileMetadataLayout.NameMaxLength);
        
        BitConverter.GetBytes(Size).CopyTo(buf, FileMetadataLayout.SizeOffset);
        BitConverter.GetBytes(BlockCount).CopyTo(buf, FileMetadataLayout.BlockCountOffset);
        
        for (int i = 0; i < FileMetadataLayout.MaxBlocks; i++)
        {
            BitConverter.GetBytes(Blocks[i])
                .CopyTo(buf, FileMetadataLayout.BlockListOffset + i * sizeof(int));
        }
        
        return buf;
    }
    
    public static FileMetadata FromBytes(byte[] buf)
    {
        var type = (MetadataEntryType)buf[FileMetadataLayout.TypeOffset];
        
        var m = type switch
        {
            MetadataEntryType.Directory => new DirectoryMetadata(),
            _ => new FileMetadata()
        };
        
        m.IsUsed = buf[FileMetadataLayout.UsedOffset] == 1;
        m.ParentIndex = BitConverter.ToInt32(buf, FileMetadataLayout.ParentOffset);
        m.Name = ByteUtil.ReadFixedString(buf, FileMetadataLayout.NameOffset, FileMetadataLayout.NameMaxLength);
        m.Size = BitConverter.ToInt32(buf, FileMetadataLayout.SizeOffset);
        m.BlockCount = BitConverter.ToInt32(buf, FileMetadataLayout.BlockCountOffset);
        
        for (int i = 0; i < FileMetadataLayout.MaxBlocks; i++)
        {
            m.Blocks[i] = BitConverter.ToInt32(buf, FileMetadataLayout.BlockListOffset + i * sizeof(int));
        }
        
        return m;
    }
}