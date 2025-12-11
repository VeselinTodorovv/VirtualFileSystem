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
    private FileValidator _fileValidator = null!;
    
    private FileOperations _fileOperations = null!;
    private DirectoryOperations _directoryOperations = null!;
    private PathResolver _pathResolver = null!;

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
        
        _pathResolver = new PathResolver();
        
        _fileOperations = new FileOperations(_metadata, _freeBlocks, _fileWriter, _fileReader, _fileValidator);
        _directoryOperations = new DirectoryOperations(_metadata, _pathResolver);
    }
    
    private void FormatNew()
    {
        CreateSuperblock();
        Mount(_superblock);
        
        GrowContainerToFullSize();
        InitializeMetadataRegion();
        InitializeFreeBlockBitmap();
        
        ReserveSystemBlocks();
    }
    
    private void ReserveSystemBlocks()
    {
        // Superblock
        _freeBlocks.Allocate();
        
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
    
    private void InitializeFreeBlockBitmap()
    {
        var offsetOfBlock = _container.OffsetOfBlock(_layout.FreeBlockMapStartBlock);
        _container.WriteBytes(offsetOfBlock, new byte[_layout.TotalBlocks], _layout.TotalBlocks);
        
        _freeBlocks.Load();
    }
    
    private void InitializeMetadataRegion()
    {
        for (int i = 0; i < _layout.MetadataBlocks; i++)
        {
            _container.WriteBlock(_layout.MetadataStartBlock + i, new byte[_layout.BlockSize]);
        }
    }
    
    private void GrowContainerToFullSize()
    {
        _container.WriteBytes(_layout.ContainerSizeBytes - 1, new byte[1], 1);
    }
    
    private void CreateSuperblock()
    {
        _superblock = new Superblock(_layout.BlockSize, _layout.TotalBlocks, _layout.MetadataStartBlock);
        _superblock.WriteTo(_container);
    }
    
    public void Cpin(string externalPath, string internalName) => _fileOperations.Import(externalPath, internalName, _pathResolver.CurrentDir);
    
    public void Cpout(string internalName, string destinationPath) => _fileOperations.Export(internalName, destinationPath, _pathResolver.CurrentDir);
    
    public FileMetadata[] Ls() => _fileOperations.List(_pathResolver.CurrentDir);
    
    public void Rm(string internalName) => _fileOperations.Remove(internalName, _pathResolver.CurrentDir);
    
    public void Md(string name) => _directoryOperations.MakeDirectory(name);
    
    public void Cd(string name) =>  _directoryOperations.ChangeDirectory(name);
}