using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Container.Storage;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Metadata.Boot;
using VirtualFileSystem.Core.Metadata.Tables;
using VirtualFileSystem.Core.Operations;
using VirtualFileSystem.Core.Resiliency;
using VirtualFileSystem.Core.Util;
using VirtualFileSystem.Core.Util.Path;
using VirtualFileSystem.Core.Validation;

namespace VirtualFileSystem.Core;

public abstract class FileSystemContextFactory
{
    public static FileSystemContext CreateMounted(ContainerStream container, FileSystemLayout layout, Superblock superblock)
    {
        var metadata = new MetadataTable(container, layout.MetadataLayout);
        var freeBlocks = new FreeBlockMap(container, layout.FreeBlockMapLayout, layout.DataStartBlock);
        
        var dedup = new DeduplicationTable();
        var integrity = new IntegrityTable(container, layout.IntegrityTableLayout);
        
        var blockIO = new BlockIO(container, integrity, layout.BlockSize);
        
        var writer = new FileWriter(blockIO, freeBlocks, dedup, layout.BlockSize);
        var reader = new FileReader(blockIO, layout.BlockSize);
        var validator = new FileValidator(metadata);
        
        var path = new PathResolver();
        
        var fileOps = new FileOperations(metadata, freeBlocks, writer, reader, validator, dedup, integrity);
        var dirOps = new DirectoryOperations(metadata, path, fileOps, validator);
        
        return new FileSystemContext
        {
            Container = container,
            Layout = layout,
            Superblock = superblock,
            Metadata = metadata,
            FreeBlocks = freeBlocks,
            Dedup = dedup,
            Integrity = integrity,
            BlockIO = blockIO,
            Writer = writer,
            Reader = reader,
            Validator = validator,
            FileOps = fileOps,
            DirOps = dirOps,
            Path = path
        };
    }
}