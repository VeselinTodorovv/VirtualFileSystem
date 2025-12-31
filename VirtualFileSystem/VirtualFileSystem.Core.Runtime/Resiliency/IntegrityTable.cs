using VirtualFileSystem.Core.Container.Storage;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Layout.Models;
using VirtualFileSystem.Core.Util.Diagnostics;

namespace VirtualFileSystem.Core.Resiliency;

public class IntegrityTable(ContainerStream stream, IntegrityTableLayout layout)
{
    private long OffsetEntry(int blockIndex) => stream.OffsetOfBlock(layout.StartBlock) + (long)blockIndex * 4;
    
    public int ReadChecksum(int blockIndex)
    {
        byte[] buffer = new byte[4];
        var offset = OffsetEntry(blockIndex);
        
        stream.ReadBytes(offset, buffer, 4);
        
        int value = BitConverter.ToInt32(buffer, 0);
        FsLogger.Log($"Integrity.ReadChecksum block={blockIndex} entryOff={offset} (0x{offset:X}) val={value} (0x{value:X8})");
        
        return value;
    }
    
    public void WriteChecksum(int blockIndex, int checksum)
    {
        byte[] buffer = BitConverter.GetBytes(checksum);
        var offset = OffsetEntry(blockIndex);
        
        FsLogger.Log($"Integrity.WriteChecksum block={blockIndex} entryOff={offset} (0x{offset:X}) val={checksum} (0x{checksum:X8})");
        
        stream.WriteBytes(offset, buffer, 4);
    }
    
    public void ClearChecksum(int blockIndex) => WriteChecksum(blockIndex, 0);
    
    // TODO: Change implementation to CRC32
    public static int Checksum(byte[] data, int length) => BlockHasher.Hash(data, length);
    
}