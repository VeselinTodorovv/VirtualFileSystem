using VirtualFileSystem.Core.Container.Storage;

namespace VirtualFileSystem.Core.Metadata.Boot;

public class Superblock
{
    private const string Signature = "VFS1"; // 4 bytes
    
    public int BlockSize;
    public int BlockCount;
    public int MetadataStartBlock;
    
    private Superblock() { }
    
    public Superblock(int blockSize, int blockCount, int metadataStartBlock)
    {
        BlockSize = blockSize;
        BlockCount = blockCount;
        MetadataStartBlock = metadataStartBlock;
    }
    
    public void WriteTo(ContainerStream stream)
    {
        byte[] buf = new byte[stream.BlockSize];
        
        // Signature (4 bytes)
        Array.Copy(System.Text.Encoding.ASCII.GetBytes(Signature), 0, buf, 0, 4);
        
        BitConverter.GetBytes(BlockSize).CopyTo(buf, 4);
        BitConverter.GetBytes(BlockCount).CopyTo(buf, 8);
        BitConverter.GetBytes(MetadataStartBlock).CopyTo(buf, 12);
        
        stream.WriteBlock(0, buf);
    }
    
    public static Superblock? LoadFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }
        
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
        
        // signature (4 bytes) + 3 ints (BlockSize, BlockCount, MetadataStartBlock) = 16 bytes
        if (fs.Length < 16)
        {
            return null;
        }
        
        byte[] header = new byte[16];
        fs.ReadExactly(header, 0, 16);
        
        string sig = System.Text.Encoding.ASCII.GetString(header, 0, 4);
        if (sig != Signature)
        {
            return null;
        }
        
        return new Superblock
        {
            BlockSize = BitConverter.ToInt32(header, 4),
            BlockCount = BitConverter.ToInt32(header, 8),
            MetadataStartBlock = BitConverter.ToInt32(header, 12)
        };
    }
}