using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Mounting;

namespace VirtualFileSystem.Core;

public class FileSystem(string containerPath, int blockSize)
{
    private readonly FileSystemContext _context = FileSystemBootstrapper.OpenOrCreate(containerPath, blockSize);
    
    public void Cpin(string externalPath, string internalName) => _context.FileOps.Import(externalPath, internalName, _context.Path.CurrentDir);
    
    public void Cpout(string internalName, string destinationPath) => _context.FileOps.Export(internalName, destinationPath, _context.Path.CurrentDir);
    
    public FileMetadata[] Ls() => _context.FileOps.List(_context.Path.CurrentDir);
    
    public void Rm(string internalName) => _context.FileOps.Remove(internalName, _context.Path.CurrentDir);
    
    public void Md(string name) => _context.DirOps.MakeDirectory(name);
    
    public void Cd(string name) => _context.DirOps.ChangeDirectory(name);
    
    public void Rd(string name) => _context.DirOps.RemoveDirectory(name);
}