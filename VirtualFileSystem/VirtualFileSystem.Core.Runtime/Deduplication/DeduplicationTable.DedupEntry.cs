namespace VirtualFileSystem.Core.Deduplication;

public partial class DeduplicationTable
{
    private class DedupEntry(int index, long hash, byte[] data, int length, int refCount)
    {
        public int BlockIndex { get; } = index;
        public long Hash { get; } = hash;
        public int RefCount { get; set; } = refCount;
        
        private readonly byte[] _preview = CopyData(data, length);
        
        private static byte[] CopyData(byte[] data, int length)
        {
            var copy = new byte[length];
            Array.Copy(data, copy, length);
            
            return copy;
        }
        
        public bool Matches(byte[] data, int length)
        {
            if (_preview.Length != length)
            {
                return false;
            }
            
            for (int i = 0; i < length; i++)
            {
                if (_preview[i] != data[i])
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}