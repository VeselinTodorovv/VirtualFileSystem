using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Layout;

namespace VirtualFileSystem.Core.Metadata;

public class MetadataTable(ContainerStream stream, MetadataLayout layout)
{
    private long OffsetOf(int index) => stream.OffsetOfBlock(layout.StartBlock) + index * layout.EntrySize;
    
    public FileMetadata LoadEntry(int index)
    {
        byte[] buffer = new byte[layout.EntrySize];
        var offset = OffsetOf(index);
        
        stream.ReadBytes(offset, buffer, buffer.Length);
        
        return FileMetadata.FromBytes(buffer);
    }
    
    public void SaveEntry(int index, FileMetadata m)
    {
        var buffer = m.ToBytes();
        var offset = OffsetOf(index);
        
        stream.WriteBytes(offset, buffer, layout.EntrySize);
    }
    
    public int FindFreeIndex()
    {
        for (int i = 0; i < layout.EntryCount; i++)
        {
            var entry = LoadEntry(i);
            if (!entry.IsUsed)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public int FindByName(string name, int parentIndex)
    {
        for (int i = 0; i < layout.EntryCount; i++)
        {
            var entry = LoadEntry(i);
            if (entry.IsUsed && entry.Name == name && entry.ParentIndex == parentIndex)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public FileMetadata[] ListFiles(int parentIndex)
    {
        var arr = new FileMetadata[layout.EntryCount];
        int count = 0;
        
        for (int i = 0; i < layout.EntryCount; i++)
        {
            var entry = LoadEntry(i);
            if (!entry.IsUsed || entry.ParentIndex != parentIndex)
            {
                continue;
            }
            
            arr[count] = entry;
            count++;
        }
        
        // Trim
        Array.Resize(ref arr, count);
        
        return arr;
    }
}