using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core;

public class FileSystem
{
    private readonly ContainerStream _container;
    private Superblock _superblock = null!;
    private MetadataTable _metadata = null!;
    private FreeBlockMap _freeBlocks = null!;
    
    public FileSystem(string containerPath, int blockSize)
    {
        _container = new ContainerStream(containerPath, blockSize);
        
        var loaded = Superblock.LoadFrom(_container);
        
        if (loaded == null)
        {
            FormatNewContainer(blockSize);
        }
        else
        {
            _superblock = loaded;
            _metadata = new MetadataTable(_container, _superblock.MetadataStartBlock);
            _freeBlocks = new FreeBlockMap(_container, _superblock.BlockCount, _superblock.MetadataStartBlock + 1);
            
            _freeBlocks.Load();
        }
    }
    
    private void FormatNewContainer(int blockSize)
    {
        const int containerBlocks = 10_000; // TODO: Adjusted or pass as param, try to find a cleaner variant
        
        // block 0 = superblock
        // block 1 = metadata entries
        // block 2 = free-block map
        const int metadataStartBlock = 1;
        
        _superblock = new Superblock(blockSize, containerBlocks, metadataStartBlock);
        _superblock.WriteTo(_container);
        
        _metadata = new MetadataTable(_container, _superblock.MetadataStartBlock);
        _freeBlocks = new FreeBlockMap(_container, containerBlocks, metadataStartBlock + 1);
        
        // Mark as used
        _freeBlocks.Load();
        _freeBlocks.Allocate(); // block 0
        _freeBlocks.Allocate(); // block 1
        _freeBlocks.Allocate(); // block 2
        _freeBlocks.Save();
    }
    
    public void Cpin(string externalPath, string internalName)
    {
        if (!File.Exists(externalPath))
        {
            throw new FileNotFoundException(externalPath);
        }
        
        using FileStream fileStream = new(externalPath, FileMode.Open, FileAccess.Read);
        
        int freeIndex = _metadata.FindFreeIndex();
        if (freeIndex == -1)
        {
            throw new ArgumentException("No free metadata entries");
        }
        
        var meta = new FileMetadata
        {
            IsUsed = true,
            Name = internalName,
            Size = (int)fileStream.Length
        };
        
        byte[] buffer = new byte[_superblock.BlockSize];
        
        int blocksWritten = 0;
        int bytesRead;
        
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (bytesRead < buffer.Length)
            {
                for (int i = bytesRead; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
            }
            
            int blockIndex = _freeBlocks.Allocate();
            if (blockIndex == -1)
            {
                throw new Exception("No free blocks available.");
            }
            
            _container.WriteBlock(blockIndex, buffer);
            
            meta.Blocks[blocksWritten++] = blockIndex;
        }

        meta.BlockCount = blocksWritten;
        _metadata.SaveEntry(freeIndex, meta);
    }
    
    public void Cpout(string internalName, string destinationPath)
    {
        int index = _metadata.FindByName(internalName);
        if (index == -1)
        {
            throw new FileNotFoundException(internalName);
        }
        
        var meta = _metadata.LoadEntry(index);
        
        using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write);
        
        byte[] buffer = new byte[_superblock.BlockSize];
        int remaining = meta.Size;
        
        for (int i = 0; i < meta.BlockCount; i++)
        {
            _container.ReadBlock(meta.Blocks[i], buffer);
            
            int toWrite = Math.Min(remaining, buffer.Length);
            fileStream.Write(buffer, 0, toWrite);
            remaining -= toWrite;
            
            if (remaining <= 0)
            {
                break;
            }
        }
    }
    
    public FileMetadata[] Ls()
    {
        return _metadata.ListFiles();
    }
    
    public void Rm(string internalName)
    {
        int index = _metadata.FindByName(internalName);
        if (index == -1)
        {
            throw new FileNotFoundException(internalName);
        }
        
        var meta = _metadata.LoadEntry(index);

        for (int i = 0; i < meta.BlockCount; i++)
        {
            _freeBlocks.Free(meta.Blocks[i]);
        }
        
        meta.IsUsed = false;
        _metadata.SaveEntry(index, meta);
    }
}