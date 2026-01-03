namespace VirtualFileSystem.Core.Util.DataStructures;

public partial class LinkedList<TKey, TValue>
{
    private sealed class Node(TKey key, TValue value, Node? node)
    {
        public readonly TKey Key = key;
        public readonly TValue Value = value;
        public Node? Next = node;
    }
}