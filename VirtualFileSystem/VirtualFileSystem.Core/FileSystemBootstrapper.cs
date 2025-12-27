using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core;

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
        
        return mounted;
    }
    
    private static void FormatNew(FileSystemContext context)
    {
        GrowContainerToFullSize(context);
        InitializeMetadataRegion(context);
        InitializeFreeBlockBitmap(context);
        
        context.Integrity.InitializeEmpty();
        
        ReserveSystemBlocks(context);
    }
    
    private static void GrowContainerToFullSize(FileSystemContext context)
    {
        var offset = context.Layout.ContainerSizeBytes - 1;
        var buffer = new byte[1];
        
        context.Container.WriteBytes(offset, buffer, 1);
    }
    
    private static void InitializeMetadataRegion(FileSystemContext context)
    {
        for (int i = 0; i < context.Layout.MetadataBlocks; i++)
        {
            var buffer = new byte[context.Layout.BlockSize];
            context.Container.WriteBlock(context.Layout.MetadataStartBlock + i, buffer);
        }
    }
    
    private static void InitializeFreeBlockBitmap(FileSystemContext context)
    {
        var offset = context.Container.OffsetOfBlock(context.Layout.FreeBlockMapStartBlock);
        var buffer = new byte[context.Layout.TotalBlocks];
        
        context.Container.WriteBytes(offset, buffer, context.Layout.TotalBlocks);
    }
    
    private static void ReserveSystemBlocks(FileSystemContext context)
    {
        // superblock
        context.FreeBlocks.Allocate();
        
        for (int i = 0; i < context.Layout.MetadataBlocks; i++)
        {
            context.FreeBlocks.Allocate();
        }
        
        for (int i = 0; i < context.Layout.BitmapBlocks; i++)
        {
            context.FreeBlocks.Allocate();
        }
        
        for (int i = 0; i < context.Layout.IntegrityTableBlocks; i++)
        {
            context.FreeBlocks.Allocate();
        }
        
        context.FreeBlocks.Save();
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
                    // Corruption will still be caught on operations like 'cpout'
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
        
        for (int i = 0; i < context.Layout.BitmapBlocks; i++)
        {
            context.FreeBlocks.MarkUsed(context.Layout.FreeBlockMapStartBlock + i);
        }
        
        for (int i = 0; i < context.Layout.IntegrityTableBlocks; i++)
        {
            context.FreeBlocks.MarkUsed(context.Layout.IntegrityTableLayout.StartBlock + i);
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
}