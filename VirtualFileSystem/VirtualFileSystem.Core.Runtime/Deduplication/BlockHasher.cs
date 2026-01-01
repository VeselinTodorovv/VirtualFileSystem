namespace VirtualFileSystem.Core.Deduplication;

public static class BlockHasher
{
    private const ulong FnvOffsetBasis = 0xcbf29ce484222325;
    private const ulong FnvPrime = 0x100000001b3;
    
    public static long Fnv1A(byte[] data, int length)
    {
        unchecked
        {
            ulong hash = FnvOffsetBasis;
            for (int i = 0; i < length; i++)
            {
                hash ^= data[i];
                hash *= FnvPrime;
            }
            
            return (long)hash;
        }
    }
}