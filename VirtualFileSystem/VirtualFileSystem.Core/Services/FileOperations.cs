using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core.Services;

public class FileOperations(MetadataTable metadata, FreeBlockMap freeBlocks, FileWriter fileWriter, FileReader fileReader, FileValidator fileValidator)
{
    public void Import(string externalPath, string internalName)
    {
        fileValidator.EnsureNameAvailable(internalName);
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
            Size = (int)fileStream.Length
        };
        
        meta.BlockCount = fileWriter.WriteFile(fileStream, meta.Blocks);
        metadata.SaveEntry(index, meta);
        
        FsLogger.Log($"Importing '{internalName}' ({externalPath})");
        FsLogger.Log($"Allocated blocks: {string.Join(", ", meta.Blocks.Take(meta.BlockCount))}");
    }
    
    public void Export(string internalName, string destinationPath)
    {
        fileValidator.ValidateNameLength(internalName);
        
        var index = fileValidator.FindFile(internalName);
        var meta = metadata.LoadEntry(index);
        
        using FileStream output = new(destinationPath, FileMode.Create, FileAccess.Write);
        
        fileReader.ReadFile(meta.Blocks, meta.BlockCount, meta.Size, output);
    }
    
    public FileMetadata[] List()
    {
        return metadata.ListFiles();
    }
    
    public void Remove(string internalName)
    {
        fileValidator.ValidateNameLength(internalName);
        
        int index = fileValidator.FindFile(internalName);
        var meta = metadata.LoadEntry(index);
        
        for (int i = 0; i < meta.BlockCount; i++)
        {
            freeBlocks.Free(meta.Blocks[i]);
        }
        
        meta.IsUsed = false;
        metadata.SaveEntry(index, meta);
    }
}