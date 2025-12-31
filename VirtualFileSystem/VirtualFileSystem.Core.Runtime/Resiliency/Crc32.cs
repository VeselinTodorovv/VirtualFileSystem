namespace VirtualFileSystem.Core.Resiliency;

public static class Crc32
{
    // Standard CRC-32 (IEEE 802.3) polynomial reversed
    private const uint Poly = 0xEDB88320u;
    
    public static int Compute(byte[] buffer, int length)
    {
        uint crc = 0xFFFFFFFFu;
        
        for (int i = 0; i < length; i++)
        {
            crc ^= buffer[i];
            
            for (int bit = 0; bit < 8; bit++)
            {
                uint mask = (uint)-(int)(crc & 1u);
                crc = (crc >> 1) ^ (Poly & mask);
            }
        }
        
        crc ^= 0xFFFFFFFFu;
        
        return (int)(crc & 0x7FFFFFFFu);
    }
}