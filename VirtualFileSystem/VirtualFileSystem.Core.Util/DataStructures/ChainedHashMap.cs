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
        _buckets[index]?.AddOrUpdate(key, value, ref _count);
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
        _buckets[index]?.Remove(key, ref _count);
    }
    
    private int GetBucketIndex(TKey key)
    {
        long internalKey = key switch
        {
            int i => i,
            long l => l,
            _ => key?.GetHashCode() ?? 0
        };
        
        var ensurePositive = internalKey & 0x7FFFFFFF;
        return (int)(ensurePositive % _buckets.Length);
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
                Add(entry.Key, entry.Value);
            }
        }
    }
}