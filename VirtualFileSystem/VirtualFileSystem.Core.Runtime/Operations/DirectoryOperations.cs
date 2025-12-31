using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Metadata.Tables;
using VirtualFileSystem.Core.Util;
using VirtualFileSystem.Core.Util.Diagnostics;
using VirtualFileSystem.Core.Util.Path;
using VirtualFileSystem.Core.Validation;

namespace VirtualFileSystem.Core.Operations;

public class DirectoryOperations(MetadataTable metadata, PathResolver path, FileOperations fileOperations, FileValidator fileValidator)
{
    public void MakeDirectory(string name)
    {
        fileValidator.ValidateDirName(name);
        fileValidator.ValidateNameLength(name);
        
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
                var currentEntry = metadata.LoadEntry(current);
                path.ChangeDirectory(currentEntry.ParentIndex);
                
                FsLogger.Log($"Moved up to parent (index {currentEntry.ParentIndex})");
                return;
        }
        
        int index = fileValidator.FindDirectory(name, path.CurrentDir);
        path.ChangeDirectory(index);
        
        FsLogger.Log($"Changed to '{name}' (index {index})");
    }
    
    public void RemoveDirectory(string name)
    {
        fileValidator.ValidateDirName(name);
        
        int index = fileValidator.FindDirectory(name, path.CurrentDir);
        RemoveDirectoryByIndex(index);
    }
    
    private void RemoveDirectoryByIndex(int index)
    {
        if (index == 0)
        {
            throw new InvalidOperationException("Cannot remove root directory.");
        }
        
        var entry = metadata.LoadEntry(index);
        if (entry is not DirectoryMetadata)
        {
            throw new InvalidOperationException($"Entry at index {index} is not a directory.");
        }
        
        var children = metadata.ListFiles(index);
        foreach (var child in children)
        {
            if (child is DirectoryMetadata)
            {
                int childIndex = metadata.FindByName(child.Name, index);
                
                FsLogger.Log($"[FS:DEBUG] Removing directory '{child.Name}'");
                RemoveDirectoryByIndex(childIndex);
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