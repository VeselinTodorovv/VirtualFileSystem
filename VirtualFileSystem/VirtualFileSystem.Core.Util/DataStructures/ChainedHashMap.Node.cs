namespace VirtualFileSystem.Core.Util.DataStructures;

public partial class ChainedHashMap<TKey, TValue>
{
    private class Node(TKey key, TValue value, Node? node)
    {
        public TKey Key = key;
        public TValue Value = value;
        public Node? Next = node;
    }
}