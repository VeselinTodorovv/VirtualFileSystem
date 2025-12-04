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
    
    public byte[] ToBytes()
    {
        byte[] buf = new byte[FileMetadataLayout.EntrySize];
        
        buf[FileMetadataLayout.UsedOffset] = (byte)(IsUsed ? 1 : 0);
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
        var m = new FileMetadata
        {
            IsUsed = buf[FileMetadataLayout.UsedOffset] == 1,
            Name = ByteUtil.ReadFixedString(buf, FileMetadataLayout.NameOffset, FileMetadataLayout.NameMaxLength),
            Size = BitConverter.ToInt32(buf, FileMetadataLayout.SizeOffset),
            BlockCount = BitConverter.ToInt32(buf, FileMetadataLayout.BlockCountOffset)
        };
        
        for (int i = 0; i < FileMetadataLayout.MaxBlocks; i++)
        {
            m.Blocks[i] = BitConverter.ToInt32(buf, FileMetadataLayout.BlockListOffset + i * sizeof(int));
        }
        
        return m;
    }
}