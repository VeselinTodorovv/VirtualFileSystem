namespace VirtualFileSystem.Core.Deduplication;

public class BlockHasher
{
    public static int Hash(byte[] data, int length)
    {
        unchecked
        {
            int hash = 5381;
            for (int i = 0; i < length; i++)
            {
                hash = ((hash << 5) + hash) ^ data[i];
            }
            
            return hash & 0x7FFFFFFF; // ensure positive
        }
    }
}