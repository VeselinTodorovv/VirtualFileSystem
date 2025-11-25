using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Metadata;

public class FileMetadata
{
    public bool IsUsed;
    public string Name = "";
    public int Size;
    public int BlockCount;
    public int[] Blocks;

    public FileMetadata()
    {
        Blocks = new int[128];
    }
    
    public byte[] ToBytes()
    {
        byte[] buffer = new byte[256];
        
        buffer[0] = (byte)(IsUsed ? 1 : 0);
        
        ByteUtil.WriteFixedString(Name ?? "", buffer, 1, 32);
        
        BitConverter.GetBytes(Size).CopyTo(buffer, 33);
        BitConverter.GetBytes(BlockCount).CopyTo(buffer, 37);
        
        const int pos = 41;
        for (int i = 0; i < Blocks.Length; i++)
        {
            BitConverter.GetBytes(Blocks[i]).CopyTo(buffer, pos + i * 4);
        }
        
        return buffer;
    }
    
    public static FileMetadata FromBytes(byte[] buf)
    {
        var m = new FileMetadata
        {
            IsUsed = buf[0] == 1,
            Name = ByteUtil.ReadFixedString(buf, 1, 32),
            Size = BitConverter.ToInt32(buf, 33),
            BlockCount = BitConverter.ToInt32(buf, 37)
        };
        
        const int pos = 41;
        for (int i = 0; i < m.Blocks.Length; i++)
        {
            m.Blocks[i] = BitConverter.ToInt32(buf, pos + i * 4);
        }
        
        return m;
    }
}