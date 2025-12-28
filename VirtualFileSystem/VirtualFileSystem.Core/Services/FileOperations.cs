using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Deduplication;
using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Resiliency;

namespace VirtualFileSystem.Core.Services;

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
            IsUsed = true,
            Name = internalName,
            Size = (int)fileStream.Length,
            ParentIndex = parentIndex
        };
        
        meta.BlockCount = fileWriter.WriteFile(fileStream, meta.Blocks);
        metadata.SaveEntry(index, meta);
        
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
            freeBlocks.Free(blockIndex);
        }
        
        FsLogger.Log($"Removing '{internalName}'");
        
        meta.IsUsed = false;
        metadata.SaveEntry(index, meta);
    }
}