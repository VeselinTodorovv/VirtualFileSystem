using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Resiliency;

namespace VirtualFileSystem.Core.IO;

public class BlockIO(ContainerStream container, IntegrityTable integrity, int blockSize)
{
    public void ReadVerified(int blockIndex, byte[] buffer)
    {
        container.ReadBlock(blockIndex, buffer);
        
        int expected = integrity.ReadChecksum(blockIndex);
        int actual = IntegrityTable.Checksum(buffer, blockSize);
        
        if (expected != 0 && expected != actual)
        {
            long blockOff = container.OffsetOfBlock(blockIndex);
            
            FsLogger.Log(
                $"CORRUPTION block={blockIndex} blockOff={blockOff} (0x{blockOff:X}) " +
                $"expected={expected} (0x{expected:X8}) actual={actual} (0x{actual:X8}) " +
                $"integrityEntryOff=? (see Integrity.ReadChecksum log above)"
            );
            
            throw new InvalidDataException($"Block {blockIndex} is corrupted (checksum mismatch).");
        }
        
        FsLogger.Log($"OK ReadVerified block={blockIndex} expected={expected} actual={actual}");
    }
    
    public void WriteWithChecksum(int blockIndex, byte[] buffer)
    {
        container.WriteBlock(blockIndex, buffer);
        
        // TODO: Use CRC32
        int checksum = IntegrityTable.Checksum(buffer, blockSize);
        integrity.WriteChecksum(blockIndex, checksum);
    }
    
    public long OffsetOfBlock(int blockIndex) => container.OffsetOfBlock(blockIndex);
}