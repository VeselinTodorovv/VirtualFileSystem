using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Layout;

namespace VirtualFileSystem.Core.Resiliency;

public class IntegrityTable(ContainerStream stream, IntegrityTableLayout layout)
{
    private long OffsetEntry(int blockIndex)
    {
        return stream.OffsetOfBlock(layout.StartBlock) + (long)blockIndex * 4;
    }
    
    public int ReadChecksum(int blockIndex)
    {
        byte[] buffer = new byte[4];
        var offset = OffsetEntry(blockIndex);
        
        stream.ReadBytes(offset, buffer, 4);
        
        return BitConverter.ToInt32(buffer, 0);
    }
    
    public void WriteChecksum(int blockIndex, int checksum)
    {
        byte[] buffer = BitConverter.GetBytes(checksum);
        var offset = OffsetEntry(blockIndex);
        
        stream.WriteBytes(offset, buffer, 4);
    }
    
    public void ClearChecksum(int blockIndex) => WriteChecksum(blockIndex, 0);
    
    public void InitializeEmpty()
    {
        byte[] zero = new byte[stream.BlockSize];
        for (int i = 0; i < layout.BlockCount; i++)
        {
            stream.WriteBlock(layout.StartBlock + i, zero);
        }
    }
    
    // TODO: Change implementation to CRC32
    public static int Checksum(byte[] data, int length) => BlockHasher.Hash(data, length);
    
}