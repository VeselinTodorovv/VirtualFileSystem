using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Services;

public class DirectoryOperations(MetadataTable metadata, PathResolver path)
{
    public void MakeDirectory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        
        if (name.Length > FileMetadataLayout.NameMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name));
        }
        
        if (metadata.FindByName(name, path.CurrentDir) != -1)
        {
            throw new InvalidOperationException($"A directory or file with the name '{name}' already exists.");
        }
        
        int index = metadata.FindFreeIndex();
        if (index == -1)
        {
            throw new InvalidOperationException("No space left for new metadata entries.");
        }
        
        var dirMeta = new DirectoryMetadata
        {
            IsUsed = true,
            Name = name,
            Size = 0,
            BlockCount = 0,
            ParentIndex = path.CurrentDir
        };
        
        metadata.SaveEntry(index, dirMeta);
    }
    
    public void ChangeDirectory(string name)
    {
        var index = metadata.FindByName(name, path.CurrentDir);
        if (index == -1)
        {
            throw new InvalidOperationException($"No directory named '{name}' found.");
        }
        
        var entry = metadata.LoadEntry(index);
        if (entry is not DirectoryMetadata)
        {
            throw new  InvalidOperationException($"'{name}' is not a directory.");
        }
        
        path.ChangeDirectory(index);
    }

    public void RemoveDirectory(string name)
    {
        // Note: Recursive
        throw new NotImplementedException();
    }
}