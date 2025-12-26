using VirtualFileSystem.Core.Container;
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
            throw new InvalidDataException($"Block {blockIndex} is corrupted (checksum mismatch).");
        }
    }
    
    public void WriteWithChecksum(int blockIndex, byte[] buffer)
    {
        container.WriteBlock(blockIndex, buffer);
        
        // TODO: Use CRC32
        int checksum = IntegrityTable.Checksum(buffer, blockSize);
        integrity.WriteChecksum(blockIndex, checksum);
    }
}