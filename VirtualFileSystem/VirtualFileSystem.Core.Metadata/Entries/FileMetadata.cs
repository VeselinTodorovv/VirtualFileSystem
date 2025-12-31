using VirtualFileSystem.Core.Layout.Constants;
using VirtualFileSystem.Core.Util.Text;

namespace VirtualFileSystem.Core.Metadata.Entries;

public class FileMetadata
{
    // Common
    public bool IsUsed;
    public string Name = "";
    public int ParentIndex;
    
    // File specific
    public int Size;
    public int BlockCount;
    public readonly int[] Blocks = new int[FileMetadataLayout.MaxBlocks];
    
    protected virtual MetadataEntryType EntryType => MetadataEntryType.File;
    
    public byte[] ToBytes()
    {
        byte[] buf = new byte[FileMetadataLayout.EntrySize];
        
        buf[FileMetadataLayout.UsedOffset] = (byte)(IsUsed ? 1 : 0);
        buf[FileMetadataLayout.TypeOffset] = (byte)EntryType;
        BitConverter.GetBytes(ParentIndex).CopyTo(buf, FileMetadataLayout.ParentOffset);
        ByteUtil.WriteFixedString(Name, buf, FileMetadataLayout.NameOffset, FileMetadataLayout.NameMaxLength);
        
        WriteTypeSpecific(buf);
        
        return buf;
    }
    
    protected virtual void WriteTypeSpecific(byte[] buf)
    {
        BitConverter.GetBytes(Size).CopyTo(buf, FileMetadataLayout.SizeOffset);
        BitConverter.GetBytes(BlockCount).CopyTo(buf, FileMetadataLayout.BlockCountOffset);
        
        for (int i = 0; i < FileMetadataLayout.MaxBlocks; i++)
        {
            BitConverter.GetBytes(Blocks[i])
                .CopyTo(buf, FileMetadataLayout.BlockListOffset + i * sizeof(int));
        }
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
        m.Name = ByteUtil.ReadFixedString(buf);
        
        m.ReadTypeSpecific(buf);
        
        return m;
    }
    
    protected virtual void ReadTypeSpecific(byte[] buf)
    {
        Size = BitConverter.ToInt32(buf, FileMetadataLayout.SizeOffset);
        BlockCount = BitConverter.ToInt32(buf, FileMetadataLayout.BlockCountOffset);
        
        for (int i = 0; i < FileMetadataLayout.MaxBlocks; i++)
        {
            Blocks[i] = BitConverter.ToInt32(buf, FileMetadataLayout.BlockListOffset + i * sizeof(int));
        }
    }
}