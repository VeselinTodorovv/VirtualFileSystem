namespace VirtualFileSystem.Core.Deduplication;

public class DeduplicationTable
{
    private readonly List<DedupEntry> _entries = [];
    
    public int? FindDuplicate(byte[] data, int length)
    {
        int hash = BlockHasher.Hash(data, length);

        foreach (var entry in _entries)
        {
            if (entry.Hash == hash && entry.Matches(data, length))
            {
                return entry.BlockIndex;
            }
        }
        
        return null;
    }
    
    public void Add(int blockIndex, byte[] data, int length)
    {
        int hash = BlockHasher.Hash(data, length);
        var entry = new DedupEntry(blockIndex, hash, data, length);
        
        _entries.Add(entry);
    }
    
    private class DedupEntry
    {
        public int BlockIndex { get; }
        public int Hash { get; }
        private readonly byte[] _preview;
        
        public DedupEntry(int index, int hash, byte[] data, int length)
        {
            BlockIndex = index;
            Hash = hash;
            _preview = new byte[length];
            
            Array.Copy(data, _preview, length);
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