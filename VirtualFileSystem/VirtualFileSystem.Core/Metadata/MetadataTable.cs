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
        
        if (!IsEntryHeaderValid(buffer))
        {
            // treat as unused
            return new FileMetadata
            {
                IsUsed = false
            };
        }
        
        var entry = FileMetadata.FromBytes(buffer);
        
        if (entry.BlockCount is >= 0 and <= FileMetadataLayout.MaxBlocks)
        {
            return entry;
        }
        
        entry.IsUsed = false;
        entry.BlockCount = 0;
        
        return entry;
    }
    
    private static bool IsEntryHeaderValid(byte[] buffer)
    {
        byte used = buffer[FileMetadataLayout.UsedOffset];
        byte type = buffer[FileMetadataLayout.TypeOffset];
        
        bool usedOk = used is 0 or 1;
        bool typeOk = type is (byte)MetadataEntryType.File or (byte)MetadataEntryType.Directory;
        
        return usedOk && typeOk;
    }

    public void SaveEntry(int index, FileMetadata m)
    {
        var buffer = m.ToBytes();
        var offset = OffsetOf(index);
        
        stream.WriteBytes(offset, buffer, layout.EntrySize);
    }
    
    public int FindFreeIndex()
    {
        // skip 0 (root dir reserved)
        for (int i = 1; i < layout.EntryCount; i++)
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
        
        // Skip 0 (root dir)
        for (int i = 1; i < layout.EntryCount; i++)
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