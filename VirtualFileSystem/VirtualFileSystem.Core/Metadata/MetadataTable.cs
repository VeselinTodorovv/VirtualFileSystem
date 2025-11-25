using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Metadata;

public class MetadataTable(ContainerStream stream, int startBlock)
{

    private const int EntrySize = 256;
    private const int EntryCount = 256;

    private long OffsetOf(int index)
    {
        return stream.OffsetOfBlock(startBlock) + index * EntrySize;
    }

    public FileMetadata LoadEntry(int index)
    {
        byte[] buffer = new byte[EntrySize];
        stream.ReadBytes(OffsetOf(index), buffer, EntrySize);
        
        return FileMetadata.FromBytes(buffer);
    }
    
    public void SaveEntry(int index, FileMetadata m)
    {
        byte[] buf = m.ToBytes();
        stream.WriteBytes(OffsetOf(index), buf, EntrySize);
    }
    
    public int FindFreeIndex()
    {
        for (int i = 0; i < EntryCount; i++)
        {
            var e = LoadEntry(i);
            if (!e.IsUsed)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public int FindByName(string name)
    {
        for (int i = 0; i < EntryCount; i++)
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
        var arr = new FileMetadata[EntryCount];
        int count = 0;
        
        for (int i = 0; i < EntryCount; i++)
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