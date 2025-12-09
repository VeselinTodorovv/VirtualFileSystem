using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core.Services;

public class FileValidator(MetadataTable metadata)
{
    internal int FindFile(string internalName, int parentIndex)
    {
        int index = metadata.FindByName(internalName, parentIndex);
        
        return index == -1
            ? throw new FileNotFoundException(internalName)
            : index;
    }
    
    internal void EnsureExternalFileExists(string externalPath)
    {
        if (!File.Exists(externalPath))
        {
            throw new FileNotFoundException(externalPath);
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
    
    // TODO: Implement a check for ensuring enough space available.
}