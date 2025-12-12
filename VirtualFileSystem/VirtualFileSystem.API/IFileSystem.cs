using VirtualFileSystem.API.Contracts;

namespace VirtualFileSystem.API;

public interface IFileSystem : IImporter, IExporter, IDeleter, IListing, IFileSystemDebug
{
    void Md(string name);
    void Cd(string name);
    void Rd(string name);
}