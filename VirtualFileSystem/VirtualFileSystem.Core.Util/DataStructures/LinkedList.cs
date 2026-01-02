namespace VirtualFileSystem.Core.Util.DataStructures;

public partial class LinkedList<TKey, TValue>
{
    private Node? _head;
    
    private Node? FindNode(TKey key)
    {
        var current = _head;
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key))
            {
                return current;
            }
            
            current = current.Next;
        }
        
        return null;
    }
    
    public void Push(TKey key, TValue value)
    {
        _head = new Node(key, value, _head);
    }
    
    public void AddOrUpdate(TKey key, TValue value, out bool isNew)
    {
        var existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            isNew = false;
            
            return;
        }
        
        Push(key, value);
        isNew = true;
    }
    
    public bool TryFind(TKey key, out TValue? value)
    {
        var node = FindNode(key);
        value = node != null ? node.Value : default;
        
        return node != null;
    }
    
    public bool Remove(TKey key)
    {
        Node? current = _head;
        Node? previous = null;
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key))
            {
                if (previous == null)
                {
                    _head = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }
                
                return true;
            }
            
            previous = current;
            current = current.Next;
        }
        
        return false;
    }
    
    public IEnumerable<(TKey Key, TValue Value)> GetAllEntries()
    {
        Node? current = _head;
        while (current != null)
        {
            yield return (current.Key, current.Value);
            current = current.Next;
        }
    }
}