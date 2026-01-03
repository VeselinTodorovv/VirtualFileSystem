using VirtualFileSystem.Core.Container.Storage;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata.Boot;
using VirtualFileSystem.Core.Metadata.Entries;

namespace VirtualFileSystem.Core.Mounting;

public abstract class FileSystemBootstrapper
{
    public static FileSystemContext OpenOrCreate(string containerPath, int requestedBlockSize)
    {
        var sb = Superblock.LoadFromPath(containerPath);
        
        if (sb == null)
        {
            var layout = new FileSystemLayout(requestedBlockSize);
            var container = new ContainerStream(containerPath, requestedBlockSize);
            
            var newSb = new Superblock(layout.BlockSize, layout.TotalBlocks, layout.MetadataStartBlock);
            newSb.WriteTo(container);
            
            var context = FileSystemContextFactory.CreateMounted(container, layout, newSb);
            
            FormatNew(context);
            return context;
        }
        
        if (sb.BlockSize != requestedBlockSize)
        {
            Console.WriteLine($"[WARN] Container uses block size {sb.BlockSize}, ignoring provided value {requestedBlockSize}.");
        }
        
        var existingLayout = new FileSystemLayout(sb.BlockSize, sb.BlockCount);
        var existingContainer = new ContainerStream(containerPath, sb.BlockSize);
        var mounted = FileSystemContextFactory.CreateMounted(existingContainer, existingLayout, sb);
        
        // repairs
        RebuildAllocationMapFromMetadata(mounted);
        RebuildDeduplicationFromMetadata(mounted);
        EnsureRootExists(mounted);
        
        return mounted;
    }
    
    private static void FormatNew(FileSystemContext context)
    {
        GrowContainerToFullSize(context);
        
        InitializeMetadataRegion(context);
        InitializeFreeBlockBitmap(context);
        InitializeIntegrityRegion(context);
        
        InitializeRootDirectory(context);
        
        context.FreeBlocks.Save();
    }
    
    private static void GrowContainerToFullSize(FileSystemContext context)
    {
        var offset = context.Layout.ContainerSizeBytes - 1;
        var buffer = new byte[1];
        
        context.Container.WriteBytes(offset, buffer, 1);
    }
    
    private static void InitializeMetadataRegion(FileSystemContext context)
    {
        context.FreeBlocks.MarkUsed(0); // superblock
        var buffer = new byte[context.Layout.BlockSize];
        
        for (int i = 0; i < context.Layout.MetadataBlocks; i++)
        {
            int block = context.Layout.MetadataStartBlock + i;
            
            context.Container.WriteBlock(block, buffer);
            context.FreeBlocks.MarkUsed(block);
        }
    }
    
    private static void InitializeFreeBlockBitmap(FileSystemContext context)
    {
        var buffer = new byte[context.Layout.TotalBlocks];
        
        for (int i = 0; i < context.Layout.FreeBlockMapBlocks; i++)
        {
            int block = context.Layout.FreeBlockMapStartBlock + i;
            
            context.Container.WriteBlock(block, buffer);
            context.FreeBlocks.MarkUsed(block);
        }
    }
    
    private static void InitializeIntegrityRegion(FileSystemContext context)
    {
        var buffer = new byte[context.Layout.BlockSize];
        
        for (int i = 0; i < context.Layout.IntegrityTableBlocks; i++)
        {
            int block = context.Layout.IntegrityTableStartBlock + i;
            
            context.Container.WriteBlock(block, buffer);
            context.FreeBlocks.MarkUsed(block);
        }
    }
    
    private static void InitializeRootDirectory(FileSystemContext context)
    {
        var root = new DirectoryMetadata
        {
            IsUsed = true,
            Name = "\\",
            ParentIndex = 0
        };
        
        context.Metadata.SaveEntry(0, root);
        context.Path.ChangeDirectory(0);
    }
    
    private static void RebuildDeduplicationFromMetadata(FileSystemContext context)
    {
        byte[] buffer = new byte[context.Layout.BlockSize];
        
        for (int i = 0; i < context.Layout.MetadataLayout.EntryCount; i++)
        {
            var entry = context.Metadata.LoadEntry(i);
            if (!entry.IsUsed || entry is DirectoryMetadata)
            {
                continue;
            }
            
            for (int j = 0; j < entry.BlockCount; j++)
            {
                int blockIndex = entry.Blocks[j];
                
                try
                {
                    context.BlockIO.ReadVerified(blockIndex, buffer);
                }
                catch (InvalidDataException)
                {
                    // Keep container usable.
                    continue;
                }
                
                context.Dedup.RegisterExistingReference(blockIndex, buffer, context.Layout.BlockSize);
            }
        }
    }
    
    private static void RebuildAllocationMapFromMetadata(FileSystemContext context)
    {
        context.FreeBlocks.ClearAll();
        
        context.FreeBlocks.MarkUsed(0);
        
        for (int i = 0; i < context.Layout.MetadataBlocks; i++)
        {
            context.FreeBlocks.MarkUsed(context.Layout.MetadataStartBlock + i);
        }
        
        for (int i = 0; i < context.Layout.FreeBlockMapBlocks; i++)
        {
            context.FreeBlocks.MarkUsed(context.Layout.FreeBlockMapStartBlock + i);
        }
        
        for (int i = 0; i < context.Layout.IntegrityTableBlocks; i++)
        {
            context.FreeBlocks.MarkUsed(context.Layout.IntegrityTableStartBlock + i);
        }
        
        for (int i = 0; i < context.Layout.MetadataLayout.EntryCount; i++)
        {
            var entry = context.Metadata.LoadEntry(i);
            if (!entry.IsUsed || entry is DirectoryMetadata)
            {
                continue;
            }
            
            for (int j = 0; j < entry.BlockCount; j++)
            {
                int b = entry.Blocks[j];
                if (b >= 0 && b < context.Layout.TotalBlocks)
                {
                    context.FreeBlocks.MarkUsed(b);
                }
            }
        }
        
        context.FreeBlocks.Save();
    }
    
    private static void EnsureRootExists(FileSystemContext context)
    {
        var entry0 = context.Metadata.LoadEntry(0);
        if (!entry0.IsUsed || entry0 is not DirectoryMetadata)
        {
            InitializeRootDirectory(context);
        }
    }
}