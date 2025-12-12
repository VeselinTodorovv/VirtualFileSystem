using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Services;

public class DirectoryOperations(MetadataTable metadata, PathResolver path, FileOperations fileOperations, FileValidator fileValidator)
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
        switch (name)
        {
            case "\\":
                path.ChangeDirectory(0);
                
                FsLogger.Log("Changed to root (index 0)");
                return;
            
            case "..":
                int current = path.CurrentDir;
                var currentMeta = metadata.LoadEntry(current);
                path.ChangeDirectory(currentMeta.ParentIndex);
                
                FsLogger.Log($"Moved up to parent (index {currentMeta.ParentIndex})");
                
                return;
        }
        
        int index = fileValidator.FindDirectory(name, path.CurrentDir);
        
        var entry = metadata.LoadEntry(index);
        if (entry is not DirectoryMetadata)
        {
            throw new InvalidOperationException($"'{name}' is not a directory.");
        }
        
        path.ChangeDirectory(index);
        
        FsLogger.Log($"Changed to '{name}' (index {index})");
    }
    
    public void RemoveDirectory(string name)
    {
        int index = metadata.FindByName(name, path.CurrentDir);
        if (index == -1)
        {
            throw new InvalidOperationException($"Directory '{name}' not found.");
        }
        
        var entry = metadata.LoadEntry(index);
        if (entry is not DirectoryMetadata)
        {
            throw new InvalidOperationException($"'{name}' is not a directory.");
        }
        
        var children = metadata.ListFiles(index);
        foreach (var child in children)
        {
            if (child is DirectoryMetadata)
            {
                RemoveDirectory(child.Name);
            }
            else
            {
                fileOperations.Remove(child.Name, index);
            }
        }
        
        entry.IsUsed = false;
        metadata.SaveEntry(index, entry);
    }
}