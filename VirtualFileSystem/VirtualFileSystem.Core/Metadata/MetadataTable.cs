using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Metadata;

public class MetadataTable(ContainerStream stream, MetadataLayout layout)
{
    private long OffsetOf(int index) => stream.OffsetOfBlock(layout.StartBlock) + index * layout.EntrySize;
    
    public FileMetadata LoadEntry(int index)
    {
        byte[] buffer = new byte[layout.EntrySize];
        stream.ReadBytes(OffsetOf(index), buffer, buffer.Length);
        
        return FileMetadata.FromBytes(buffer);
    }
    
    public void SaveEntry(int index, FileMetadata m)
    {
        stream.WriteBytes(OffsetOf(index), m.ToBytes(), layout.EntrySize);
    }
    
    public int FindFreeIndex()
    {
        for (int i = 0; i < layout.EntryCount; i++)
        {
            if (!LoadEntry(i).IsUsed)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public int FindByName(string name)
    {
        for (int i = 0; i < layout.EntryCount; i++)
        {
            var e = LoadEntry(i);
            if (e.IsUsed && StringUtil.Equals(e.Name, name))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public FileMetadata[] ListFiles()
    {
        var arr = new FileMetadata[layout.EntryCount];
        int count = 0;
        
        for (int i = 0; i < layout.EntryCount; i++)
        {
            var e = LoadEntry(i);
            if (e.IsUsed)
            {
                arr[count++] = e;
            }
        }
        
        // Trim
        Array.Resize(ref arr, count);
        
        return arr;
    }
}