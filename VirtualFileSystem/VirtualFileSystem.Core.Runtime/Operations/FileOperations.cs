using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Container.Allocation;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Metadata.Entries;
using VirtualFileSystem.Core.Metadata.Tables;
using VirtualFileSystem.Core.Resiliency;
using VirtualFileSystem.Core.Util;
using VirtualFileSystem.Core.Util.Diagnostics;
using VirtualFileSystem.Core.Validation;

namespace VirtualFileSystem.Core.Operations;

public class FileOperations
    (MetadataTable metadata, FreeBlockMap freeBlocks, FileWriter fileWriter, FileReader fileReader, FileValidator fileValidator,
        DeduplicationTable deduplicationTable, IntegrityTable integrity)
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
        
        for (int i = 0; i < meta.BlockCount; i++)
        {
            int blockIndex = meta.Blocks[i];
            
            bool shouldFree = deduplicationTable.Release(blockIndex);
            if (!shouldFree)
            {
                continue;
            }
            
            integrity.ClearChecksum(blockIndex);
            
            freeBlocks.MarkFree(blockIndex);
            freeBlocks.Save();
        }
        
        FsLogger.Log($"Removing '{internalName}'");
        
        meta.IsUsed = false;
        metadata.SaveEntry(index, meta);
    }
}