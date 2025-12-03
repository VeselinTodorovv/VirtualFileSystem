using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Metadata;

public class FileMetadata
{
    private const int NameMaxLength = 32;
    private const int MaxBlocks = 48;
    
    private const int UsedOffset = 0;
    private const int NameOffset = 1;
    private const int SizeOffset = NameOffset + NameMaxLength;
    private const int BlockCountOffset = SizeOffset + 4;
    private const int BlockListOffset = BlockCountOffset + 4;

    internal const int EntrySize = BlockListOffset + MaxBlocks * 4;
    
    public bool IsUsed;
    public string Name = "";
    public int Size;
    public int BlockCount;
    public readonly int[] Blocks = new int[MaxBlocks];
    
    public byte[] ToBytes()
    {
        byte[] buf = new byte[EntrySize];
        
        buf[UsedOffset] = (byte)(IsUsed ? 1 : 0);
        ByteUtil.WriteFixedString(Name, buf, NameOffset, NameMaxLength);
        
        BitConverter.GetBytes(Size).CopyTo(buf, SizeOffset);
        BitConverter.GetBytes(BlockCount).CopyTo(buf, BlockCountOffset);
        
        for (int i = 0; i < MaxBlocks; i++)
        {
            BitConverter.GetBytes(Blocks[i]).CopyTo(buf, BlockListOffset + i * 4);
        }
        
        return buf;
    }
    
    public static FileMetadata FromBytes(byte[] buf)
    {
        var m = new FileMetadata
        {
            IsUsed = buf[UsedOffset] == 1,
            Name = ByteUtil.ReadFixedString(buf, NameOffset, NameMaxLength),
            Size = BitConverter.ToInt32(buf, SizeOffset),
            BlockCount = BitConverter.ToInt32(buf, BlockCountOffset)
        };
        
        for (int i = 0; i < MaxBlocks; i++)
        {
            m.Blocks[i] = BitConverter.ToInt32(buf, BlockListOffset + i * 4);
        }
        
        return m;
    }
}