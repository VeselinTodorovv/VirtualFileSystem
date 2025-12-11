namespace VirtualFileSystem.CLI;

public class SimpleDictionary<TKey, TValue>(int capacity)
{
    private readonly TKey[] _keys = new TKey[capacity];
    private readonly TValue[] _values = new TValue[capacity];
    private int _count;
    
    public void Add(TKey key, TValue value)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_keys[i]!.Equals(key))
            {
                throw new InvalidOperationException($"The key {key} already exists in the dictionary.");
            }
        }
        
        if (_count >= _keys.Length)
        {
            throw new InvalidOperationException("Dictionary is full");
        }
        
        _keys[_count] = key;
        _values[_count] = value;
        _count++;
    }
    
    public bool TryGetValue(TKey key, out TValue? value)
    {
        for (int i = 0; i < _count; i++)
        {
            if (!_keys[i]!.Equals(key))
            {
                continue;
            }
            
            value = _values[i];
            return true;
        }
        
        value = default;
        return false;
    }
    
    public TValue[] Values()
    {
        var arr = new TValue[_count];
        for (int i = 0; i < _count; i++)
        {
            arr[i] = _values[i];
        }
        
        return arr;
    }
}