using VirtualFileSystem.Core;
using VirtualFileSystem.Core.Diagnostics;

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
            result[i] = new FileEntryInfo(metas[i].Name, metas[i].Size);
        }
        
        return result;
    }
    
    public void Mkdir(string name)
    {
        _fs.Mkdir(name);
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