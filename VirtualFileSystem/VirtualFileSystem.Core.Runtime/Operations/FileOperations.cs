using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Metadata.Tables;
using VirtualFileSystem.Core.Resiliency;
using VirtualFileSystem.Core.Util.Diagnostics;
using VirtualFileSystem.Core.Util.Layout;
using VirtualFileSystem.Core.Validation;

namespace VirtualFileSystem.Core.Operations;

public class FileOperations
    (MetadataTable metadata, FreeBlockMap freeBlocks, FileWriter fileWriter, FileReader fileReader, FileValidator fileValidator,
        DeduplicationTable deduplicationTable, IntegrityTable integrity, BlockIO blockIO, FileSystemLayout layout)
{
    public void Import(string externalPath, string internalName, int parentIndex)
    {
        fileValidator.EnsureNameAvailable(internalName, parentIndex);
        fileValidator.EnsureExternalFileExists(externalPath);
        fileValidator.ValidateNameLength(internalName);
        
        int index = metadata.FindFreeIndex();
        if (index == -1)
        {
            throw new InvalidOperationException("No free metadata entries");
        }
        
        using FileStream fileStream = new(externalPath, FileMode.Open, FileAccess.Read);
        
        var meta = new FileMetadata
        {
            IsUsed = false,
            Name = internalName,
            Size = (int)fileStream.Length,
            ParentIndex = parentIndex
        };
        
        // Placeholder
        metadata.SaveEntry(index, meta);
        
        meta.BlockCount = fileWriter.WriteFile(fileStream, meta.Blocks);
        meta.IsUsed = true;
        
        metadata.SaveEntry(index, meta);
        freeBlocks.Save();
        
        FsLogger.Log($"Importing '{internalName}' ({externalPath})");
    }
    
    public void Export(string internalName, string destinationPath, int parentIndex)
    {
        fileValidator.ValidateNameLength(internalName);
        
        var index = fileValidator.FindFile(internalName, parentIndex);
        var meta = metadata.LoadEntry(index);
        
        using FileStream output = new(destinationPath, FileMode.Create, FileAccess.Write);
        fileReader.ReadFile(meta.Blocks, meta.BlockCount, meta.Size, output);
    }
    
    public FileMetadata[] List(int parentIndex)
    {
        return metadata.ListFiles(parentIndex);
    }
    
    public void Remove(string internalName, int parentIndex)
    {
        fileValidator.ValidateNameLength(internalName);
        
        int index = fileValidator.FindFile(internalName, parentIndex);
        var meta = metadata.LoadEntry(index);
        
        byte[]? indirectCache = null;
        for (int logical = 0; logical < meta.BlockCount; logical++)
        {
            if (logical == BlockResolver.IndirectTriggerIndex)
            {
                int indexBlock = meta.Blocks[BlockResolver.IndirectTriggerIndex];
                indirectCache = new byte[layout.BlockSize];
                blockIO.ReadVerified(indexBlock, indirectCache);
            }
            
            int physical = BlockResolver.GetPhysicalIndex(logical, meta.Blocks, indirectCache);
            ReleaseBlock(physical);
        }
        
        if (meta.BlockCount > BlockResolver.IndirectTriggerIndex)
        {
            int indexBlock = meta.Blocks[BlockResolver.IndirectTriggerIndex];
            ReleaseBlock(indexBlock);
        }
        
        freeBlocks.Save();
        
        meta.IsUsed = false;
        metadata.SaveEntry(index, meta);
        
        FsLogger.Log($"Removing '{internalName}'");
    }
    
    private void ReleaseBlock(int blockIndex)
    {
        if (!deduplicationTable.Release(blockIndex))
        {
            return;
        }
        
        integrity.ClearChecksum(blockIndex);
        freeBlocks.MarkFree(blockIndex);
    }
}