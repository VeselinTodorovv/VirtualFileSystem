using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Resiliency;
using VirtualFileSystem.Core.Services;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core;

public class FileSystemContext
{
    public required ContainerStream Container { get; init; }
    public required FileSystemLayout Layout { get; init; }
    public required Superblock Superblock { get; init; }
    
    public required MetadataTable Metadata { get; init; }
    public required FreeBlockMap FreeBlocks { get; init; }
    
    public required DeduplicationTable Dedup { get; init; }
    public required IntegrityTable Integrity { get; init; }
    public required BlockIO BlockIO { get; init; }
    
    public required FileWriter Writer { get; init; }
    public required FileReader Reader { get; init; }
    public required FileValidator Validator { get; init; }
    
    public required FileOperations FileOps { get; init; }
    public required DirectoryOperations DirOps { get; init; }
    
    public required PathResolver Path { get; init; }
}