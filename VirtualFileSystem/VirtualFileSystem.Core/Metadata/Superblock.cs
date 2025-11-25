using VirtualFileSystem.Core.Container;

namespace VirtualFileSystem.Core.Metadata;

public class Superblock
{
    private const string Signature = "VFS1"; // 4 bytes

    public int BlockSize;
    public int BlockCount;
    public int MetadataStartBlock;

    public Superblock() { }

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

    public static Superblock? LoadFrom(ContainerStream stream)
    {
        byte[] buf = new byte[stream.BlockSize];
        stream.ReadBlock(0, buf);

        string sig = System.Text.Encoding.ASCII.GetString(buf, 0, 4);
        if (sig != Signature)
        {
            return null;
        }
        
        return new Superblock
        {
            BlockSize = BitConverter.ToInt32(buf, 4),
            BlockCount = BitConverter.ToInt32(buf, 8),
            MetadataStartBlock = BitConverter.ToInt32(buf, 12)
        };
    }
}