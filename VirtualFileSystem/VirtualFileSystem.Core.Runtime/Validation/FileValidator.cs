using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Layout.Constants;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Metadata.Tables;

namespace VirtualFileSystem.Core.Validation;

public class FileValidator(MetadataTable metadata)
{
    internal int FindFile(string internalName, int parentIndex)
    {
        int index = metadata.FindByName(internalName, parentIndex);
        if (index == -1)
        {
            throw new FileNotFoundException($"File '{internalName}' not found in the current directory.");
        }
        
        var entry = metadata.LoadEntry(index);
        
        return entry is DirectoryMetadata
            ? throw new InvalidOperationException($"'{internalName}' is a directory, not a file.")
            : index;
    }
    
    internal int FindDirectory(string internalName, int parentIndex)
    {
        int index = metadata.FindByName(internalName, parentIndex);
        if (index == -1)
        {
            throw new InvalidOperationException($"No directory named '{internalName}' found.");
        }
        
        var entry = metadata.LoadEntry(index);
        
        return entry is not DirectoryMetadata
            ? throw new FileNotFoundException($"Directory '{internalName}' not found in the current directory.")
            : index;
    }
    
    internal void EnsureExternalFileExists(string externalPath)
    {
        if (!File.Exists(externalPath))
        {
            throw new FileNotFoundException($"External file '{externalPath}' does not exist.");
        }
    }
    
    internal void EnsureNameAvailable(string internalName, int parentIndex)
    {
        var index = metadata.FindByName(internalName, parentIndex);
        if (index != -1)
        {
            throw new InvalidOperationException($"A file with the name {internalName} already exists.");
        }
    }
    
    internal void ValidateNameLength(string name)
    {
        if (name.Length > FileMetadataLayout.NameMaxLength)
        {
            throw new InvalidOperationException($"Filename too long (max {FileMetadataLayout.NameMaxLength}).");
        }
    }
    
    internal void ValidateDirName(string name)
    {
        if (name is "\\" or "..")
        {
            throw new InvalidOperationException("Invalid directory operation.");
        }
    }
    
    // TODO: Implement a check for ensuring enough space available.
}