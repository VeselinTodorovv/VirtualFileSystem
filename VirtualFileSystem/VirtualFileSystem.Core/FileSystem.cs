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
    
    public FileSystem(string containerPath, int blockSize)
    {
        _layout = new FileSystemLayout(blockSize);
        _container = new ContainerStream(containerPath, blockSize);
        
        var sb = Superblock.LoadFrom(_container);
        if (sb == null)
        {
            FormatNew();
        }
        else
        {
            Mount(sb);
            _freeBlocks.Load();
        }
    }
    
    private void Mount(Superblock sb)
    {
        _superblock = sb;
        
        var metaLayout = _layout.MetadataLayout;
        var fbmLayout = _layout.FreeBlockMapLayout;
        
        _metadata = new MetadataTable(_container, metaLayout);
        _freeBlocks = new FreeBlockMap(_container, fbmLayout);
    }
    
    private void FormatNew()
    {
        _superblock = new Superblock(_layout.BlockSize, _layout.TotalBlocks, _layout.MetadataStartBlock);
        _superblock.WriteTo(_container);
        
        Mount(_superblock);
        
        _container.WriteBytes(_layout.ContainerSizeBytes - 1, new byte[1], 1);
        
        for (int i = 0; i < _layout.MetadataBlocks; i++)
        {
            _container.WriteBlock(_layout.MetadataStartBlock + i, new byte[_layout.BlockSize]);
        }
        
        var offsetOfBlock = _container.OffsetOfBlock(_layout.FreeBlockMapStartBlock);
        _container.WriteBytes(offsetOfBlock, new byte[_layout.TotalBlocks], _layout.TotalBlocks);
        
        _freeBlocks.Load();
        
        _freeBlocks.Allocate(); // superblock
        
        for (int i = 0; i < _layout.MetadataBlocks; i++)
        {
            _freeBlocks.Allocate();
        }
        
        for (int i = 0; i < _layout.BitmapBlocks; i++)
        {
            _freeBlocks.Allocate();
        }
        
        _freeBlocks.Save();
    }
    
    public void Cpin(string externalPath, string internalName)
    {
        if (_metadata.FindByName(internalName) != -1)
        {
            throw new InvalidOperationException($"A file with the name {internalName} already exists.");
        }
        
        if (!File.Exists(externalPath))
        {
            throw new FileNotFoundException(externalPath);
        }
        
        int freeIndex = _metadata.FindFreeIndex();
        if (freeIndex == -1)
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
        
        byte[] buffer = new byte[_superblock.BlockSize];
        
        int blocksWritten = 0;
        int bytesRead;
        
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (bytesRead < buffer.Length)
            {
                Array.Clear(buffer, bytesRead, buffer.Length - bytesRead);
            }
            
            int blockIndex = _freeBlocks.Allocate();
            if (blockIndex == -1)
            {
                throw new IOException("Filesystem out of space");
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
        
        using FileStream output = new(destinationPath, FileMode.Create, FileAccess.Write);
        
        byte[] buffer = new byte[_superblock.BlockSize];
        int remaining = meta.Size;
        
        for (int i = 0; i < meta.BlockCount; i++)
        {
            _container.ReadBlock(meta.Blocks[i], buffer);
            
            int toWrite = Math.Min(remaining, buffer.Length);
            output.Write(buffer, 0, toWrite);
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