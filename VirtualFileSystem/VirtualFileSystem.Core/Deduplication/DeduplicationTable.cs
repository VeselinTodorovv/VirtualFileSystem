using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Resiliency;

namespace VirtualFileSystem.Core.Deduplication;

public class DeduplicationTable
{
    private readonly List<DedupEntry> _entries = [];
    
    public int? FindDuplicateAndAddRef(byte[] data, int length)
    {
        int hash = IntegrityTable.Checksum(data, length);
        
        foreach (var entry in _entries)
        {
            if (entry.Hash != hash || !entry.Matches(data, length))
            {
                continue;
            }
            
            entry.RefCount++;
            FsLogger.Log("[FS:DEBUG] Duplicate hash found");
            
            return entry.BlockIndex;
        }
        
        return null;
    }
    
    public void AddNewBlock(int blockIndex, byte[] data, int length)
    {
        int hash = IntegrityTable.Checksum(data, length);
        var entry = new DedupEntry(blockIndex, hash, data, length, 1);
        
        _entries.Add(entry);
    }

    public bool Release(int blockIndex)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].BlockIndex != blockIndex)
            {
                continue;
            }
            
            _entries[i].RefCount--;
            if (_entries[i].RefCount > 0)
            {
                return false;
            }
            
            _entries.RemoveAt(i);
            return true;
        }
        
        return false;
    }
    
    public void RegisterExistingReference(int blockIndex, byte[] data, int length)
    {
        foreach (var entry in _entries)
        {
            if (entry.BlockIndex != blockIndex)
            {
                continue;
            }
            
            entry.RefCount++;
            return;
        }
        
        int hash = IntegrityTable.Checksum(data, length);
        var dedupEntry = new DedupEntry(blockIndex, hash, data, length, 1);
        
        _entries.Add(dedupEntry);
    }
    
    private class DedupEntry
    {
        public int BlockIndex { get; }
        public int Hash { get; }
        public int RefCount { get; set; }
        
        private readonly byte[] _preview;
        
        public DedupEntry(int index, int hash, byte[] data, int length, int refCount)
        {
            BlockIndex = index;
            Hash = hash;
            RefCount = refCount;
            
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