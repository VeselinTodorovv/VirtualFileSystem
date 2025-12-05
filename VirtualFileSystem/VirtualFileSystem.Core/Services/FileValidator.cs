using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core.Services;

public class FileValidator(MetadataTable metadata)
{
    internal int FindFile(string internalName)
    {
        int index = metadata.FindByName(internalName);
        
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
    
    internal void EnsureNameAvailable(string internalName)
    {
        if (metadata.FindByName(internalName) != -1)
        {
            throw new InvalidOperationException($"A file with the name {internalName} already exists.");
        }
    }
}