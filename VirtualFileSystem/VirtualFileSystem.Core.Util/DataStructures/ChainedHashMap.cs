namespace VirtualFileSystem.Core.Util.DataStructures;

public class ChainedHashMap<TKey, TValue>(int capacity = 16)
{
    private LinkedList<TKey, TValue>?[] _buckets = new LinkedList<TKey, TValue>?[capacity];
    private int _count;
    private const float LoadFactor = 0.75f;
    
    public void Add(TKey key, TValue value)
    {
        if (_count >= _buckets.Length * LoadFactor)
        {
            Resize();
        }
        
        int index = GetBucketIndex(key);
        _buckets[index] ??= new LinkedList<TKey, TValue>();
        
        _buckets[index]!.AddOrUpdate(key, value, out var isNew);
        
        if (isNew)
        {
            _count++;
        }
    }
    
    public bool TryGetValue(TKey key, out TValue? value)
    {
        int index = GetBucketIndex(key);
        var list = _buckets[index];
        
        if (list != null)
        {
            return list.TryFind(key, out value);
        }
        
        value = default;
        return false;
    }
    
    public void Remove(TKey key)
    {
        int index = GetBucketIndex(key);
        var list = _buckets[index];
        
        if (list != null && list.Remove(key))
        {
            _count--;
        }
    }
    
    private int GetBucketIndex(TKey key)
    {
        var hash = key switch
        {
            int i => (uint)i,
            long l => (ulong)l,
            _ => (ulong)(key?.GetHashCode() ?? 0)
        };
        
        return (int)(hash % (ulong)_buckets.Length);
    }
    
    private void Resize()
    {
        var oldBuckets = _buckets;
        _buckets = new LinkedList<TKey, TValue>[oldBuckets.Length * 2];
        _count = 0;
        
        foreach (var list in oldBuckets)
        {
            if (list == null)
            {
                continue;
            }
            
            foreach (var entry in list.GetAllEntries())
            {
                InsertInternal(entry.Key, entry.Value);
            }
        }
    }
    
    private void InsertInternal(TKey key, TValue value)
    {
        int index = GetBucketIndex(key);
        _buckets[index] ??= new LinkedList<TKey, TValue>();
        
        _buckets[index]?.Push(key, value);
        _count++;
    }
}