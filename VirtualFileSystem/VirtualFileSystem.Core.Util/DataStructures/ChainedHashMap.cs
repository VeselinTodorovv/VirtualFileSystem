namespace VirtualFileSystem.Core.Util.DataStructures;

public partial class ChainedHashMap<TKey, TValue>(int capacity = 16)
{
    private Node?[] _buckets = new Node[capacity];
    private int _count;
    private const float LoadFactor = 0.75f;
    
    public void Add(TKey key, TValue value)
    {
        if (_count >= _buckets.Length * LoadFactor)
        {
            Resize();
        }
        
        int index = GetBucketIndex(key);
        var current = _buckets[index];
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key)) 
            {
                current.Value = value;
                return;
            }
            
            current = current.Next;
        }
        
        _buckets[index] = new Node(key, value, _buckets[index]);
        _count++;
    }
    
    public bool TryGetValue(TKey key, out TValue? value)
    {
        int index = GetBucketIndex(key);
        var current = _buckets[index];
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key))
            {
                value = current.Value;
                return true;
            }
            
            current = current.Next;
        }
        
        value = default;
        return false;
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
        _buckets = new Node[oldBuckets.Length * 2];
        _count = 0;
        
        foreach (var oldBucket in oldBuckets)
        {
            var current = oldBucket;
            while (current != null)
            {
                Add(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
    
    public void Remove(TKey key)
    {
        int index = GetBucketIndex(key);
        Node? current = _buckets[index];
        Node? previous = null;
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key))
            {
                if (previous == null)
                {
                    _buckets[index] = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }
                
                _count--;
                return;
            }
            
            previous = current;
            current = current.Next;
        }
    }
}