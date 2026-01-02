namespace VirtualFileSystem.Core.Util.DataStructures;

public partial class LinkedList<TKey, TValue>
{
    private Node? _head;
    
    public void AddOrUpdate(TKey key, TValue value, ref int count)
    {
        var current = _head;
        
        while (current != null)
        {
            if (current.Key != null && current.Key.Equals(key)) 
            {
                current.Value = value;
                return;
            }
            
            current = current.Next;
        }
        
        _head = new Node(key, value, _head);
        count++;
    }

    public bool TryFind(TKey key, out TValue? value)
    {
        var current = _head;
        
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

    public void Remove(TKey key, ref int count)
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
                
                count--;
                return;
            }
            
            previous = current;
            current = current.Next;
        }
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