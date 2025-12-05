using VirtualFileSystem.API.Contracts;

namespace VirtualFileSystem.API;

public interface IFileSystem :
    IImporter,
    IExporter,
    IDeleter,
    IListing,
    IFileSystemDebug;