using VirtualFileSystem.Core;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Util;
using VirtualFileSystem.Core.Util.Diagnostics;

namespace VirtualFileSystem.API;

public class FileSystemService(string containerPath, int blockSize) : IFileSystem
{
    private readonly FileSystem _fs = new(containerPath, blockSize);
    
    public void Cpin(string externalPath, string internalName)
    {
        _fs.Cpin(externalPath, internalName);
    }
    
    public void Cpout(string internalName, string destinationPath)
    {
        _fs.Cpout(internalName, destinationPath);
    }
    
    public void Rm(string internalName)
    {
        _fs.Rm(internalName);
    }
    
    public FileEntryInfo[] Ls()
    {
        var metas = _fs.Ls();
        
        var result = new FileEntryInfo[metas.Length];
        for (int i = 0; i < metas.Length; i++)
        {
            var fileMetadata = metas[i];
            bool isDir = fileMetadata is DirectoryMetadata;
            
            result[i] = new FileEntryInfo(fileMetadata.Name, fileMetadata.Size, isDir);
        }
        
        return result;
    }
    
    public void Md(string name)
    {
        _fs.Md(name);
    }
    
    public void Cd(string name)
    {
        _fs.Cd(name);
    }
    
    public void Rd(string name)
    {
        _fs.Rd(name);
    }
    
    public void SetDebug(bool enabled)
    {
        if (enabled)
        {
            FsLogger.Enable();
        }
        else
        {
            FsLogger.Disable();
        }
    }
}