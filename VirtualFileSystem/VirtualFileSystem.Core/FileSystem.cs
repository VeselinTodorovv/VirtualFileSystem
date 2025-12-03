using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;

namespace VirtualFileSystem.Core;

public class FileSystem
{
    private readonly ContainerStream _container;
    private readonly FileSystemLayout _layout;
    
    private Superblock _superblock = null!;
    private MetadataTable _metadata = null!;
    private FreeBlockMap _freeBlocks = null!;
    
    private const int EntrySize = 256;
    private const int EntryCount = 256;
    private const int BlockCount = 4096;
    
    public FileSystem(string containerPath, int blockSize)
    {
        _layout = new FileSystemLayout(
            blockSize,
            EntrySize,
            EntryCount,
            BlockCount
        );
        
        _container = new ContainerStream(containerPath, blockSize);
        
        var loaded = Superblock.LoadFrom(_container);
        
        if (loaded == null)
        {
            FormatNewContainer();
        }
        else
        {
            MountExistingContainer(loaded);
        }
    }
    
    private void MountExistingContainer(Superblock superblock)
    {
        _superblock = superblock;
        
        _metadata = new MetadataTable(_container, _layout.MetadataStartBlock);
        _freeBlocks = new FreeBlockMap(_container, _layout.BlockCount, _layout.FreeBlockMapStartBlock);
        
        _freeBlocks.Load();
    }
    
    private void FormatNewContainer()
    {
        _superblock = new Superblock(
            _layout.BlockSize,
            _layout.BlockCount,
            _layout.MetadataStartBlock
        );
        
        _superblock.WriteTo(_container);
        
        _metadata = new MetadataTable(_container, _layout.MetadataStartBlock);
        _freeBlocks = new FreeBlockMap(_container, _layout.BlockCount, _layout.FreeBlockMapStartBlock);
        
        byte[] emptyBlock = new byte[_layout.BlockSize];
        
        for (int i = 0; i < _layout.MetadataBlocks; i++)
        {
            int layoutMetadataStartBlock = _layout.MetadataStartBlock + i;
            _container.WriteBlock(layoutMetadataStartBlock, emptyBlock);
        }
        
        byte[] zeroBitmap = new byte[_layout.BlockCount];
        long offsetOfBlock = _container.OffsetOfBlock(_layout.MetadataStartBlock);
        
        _container.WriteBytes(
            offsetOfBlock, zeroBitmap, zeroBitmap.Length
        );
        
        _container.WriteBytes(_layout.ContainerSizeBytes - 1, [0], 1);
        
        _freeBlocks.Load();
        _freeBlocks.Allocate(); // Superblock
        
        // Metadata
        for (int i = 0; i < _layout.MetadataBlocks; i++)
        {
            _freeBlocks.Allocate();
        }
        
        // Bitmap
        for (int i = 0; i < _layout.BitmapBlocks; i++)
        {
            _freeBlocks.Allocate();
        }
        
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