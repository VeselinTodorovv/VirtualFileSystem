using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core.Services;

public class DirectoryOperations(MetadataTable metadata, PathResolver path)
{
    private readonly MetadataTable _metadata = metadata;
    private readonly PathResolver _path = path;

    public void MakeDirectory(string name)
    {
        throw new NotImplementedException();
    }
    
    public void ChangeDirectory(string name)
    {
        throw new NotImplementedException();
    }

    public void RemoveDirectory(string name)
    {
        // Note: Recursive
        throw new NotImplementedException();
    }
}