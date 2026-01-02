using VirtualFileSystem.Core.Util.DataStructures;
using VirtualFileSystem.Core.Util.Diagnostics;

namespace VirtualFileSystem.Core.Deduplication;

public partial class DeduplicationTable
{
    // lookup by Hash (for collisions)
    private readonly ChainedHashMap<long, List<DedupEntry>> _hashLookup = new();
    
    // lookup by BlockIndex (for release/rebuild)
    private readonly ChainedHashMap<int, DedupEntry> _blockIndexMap = new();
    
    public int? FindDuplicateAndAddRef(byte[] data, int length)
    {
        long hash = BlockHasher.Fnv1A(data, length);
        
        if (!_hashLookup.TryGetValue(hash, out var bucket) || bucket == null)
        {
            return null;
        }
        
        foreach (var entry in bucket)
        {
            if (!entry.Matches(data, length))
            {
                continue;
            }
            
            entry.RefCount++;
            FsLogger.Log($"[DEDUP] Found existing block {entry.BlockIndex} for hash 0x{hash:X16}");
            
            return entry.BlockIndex;
        }
        
        return null;
    }
    
    public void AddNewBlock(int blockIndex, byte[] data, int length)
    {
        long hash = BlockHasher.Fnv1A(data, length);
        var entry = new DedupEntry(blockIndex, hash, data, length, 1);
        
        if (!_hashLookup.TryGetValue(hash, out var bucket) || bucket == null)
        {
            bucket = [];
            _hashLookup.Add(hash, bucket);
        }
        else
        {
            FsLogger.Log($"[DEDUP] Info: Hash collision detected for 0x{hash:X16}. Appending to existing bucket list.");
        }
        
        bucket.Add(entry);
        _blockIndexMap.Add(blockIndex, entry);
        
        FsLogger.Log($"[DEDUP] Registered new unique block: Index {blockIndex}, Hash 0x{hash:X16}");
    }
    
    public bool Release(int blockIndex)
    {
        if (!_blockIndexMap.TryGetValue(blockIndex, out var entry) || entry == null)
        {
            return false;
        }
        
        entry.RefCount--;
        if (entry.RefCount > 0)
        {
            return false;
        }
        
        _blockIndexMap.Remove(blockIndex);
        if (!_hashLookup.TryGetValue(entry.Hash, out var bucket) || bucket == null)
        {
            return true;
        }
        
        bucket.Remove(entry);
        if (bucket.Count == 0)
        {
            _hashLookup.Remove(entry.Hash);
        }
        
        return true;
    }
    
    public void RegisterExistingReference(int blockIndex, byte[] data, int length)
    {
        if (_blockIndexMap.TryGetValue(blockIndex, out var existingEntry) && existingEntry != null)
        {
            existingEntry.RefCount++;
            return;
        }
        
        AddNewBlock(blockIndex, data, length);
    }
}