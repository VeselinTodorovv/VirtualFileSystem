using VirtualFileSystem.API.Contracts;

namespace VirtualFileSystem.API;

public interface IFileSystem : IFileManager, IDirectoryManager, IListing, IFileSystemDebug;