using VirtualFileSystem.Core.Container;
using VirtualFileSystem.Core.IO;
using VirtualFileSystem.Core.Layout;
using VirtualFileSystem.Core.Metadata;
using VirtualFileSystem.Core.Services;
using VirtualFileSystem.Core.Util;

namespace VirtualFileSystem.Core;

public class FileSystem
{
    private readonly ContainerStream _container;
    private readonly FileSystemLayout _layout;
    
    private Superblock _superblock = null!;
    private MetadataTable _metadata = null!;
    private FreeBlockMap _freeBlocks = null!;
    
    private FileWriter _fileWriter = null!;
    private FileReader _fileReader = null!;
    
    private FileOperations _ops = null!;
    private FileValidator _fileValidator = null!;
    
    
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
        
        _fileWriter = new FileWriter(_container, _freeBlocks, _layout.BlockSize);
        _fileReader = new FileReader(_container, _layout.BlockSize);
        
        _fileValidator = new FileValidator(_metadata);
        _ops = new FileOperations(_metadata, _freeBlocks, _fileWriter, _fileReader, _fileValidator);
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
    
    public void Cpin(string externalPath, string internalName) => _ops.Import(externalPath, internalName);
    
    public void Cpout(string internalName, string destinationPath) => _ops.Export(internalName, destinationPath);
    
    public FileMetadata[] Ls() => _ops.List();
    
    public void Rm(string internalName) => _ops.Remove(internalName);
}