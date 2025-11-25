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
        // block 1 = metadata entries (large region)
        // block 2 = free-block map
        const int metadataStartBlock = 1;
        
        _superblock = new Superblock(blockSize, containerBlocks, metadataStartBlock);
        _superblock.WriteTo(_container);

        _metadata = new MetadataTable(_container, _superblock.MetadataStartBlock);
        _freeBlocks = new FreeBlockMap(_container, containerBlocks, metadataStartBlock + 1);
        
        // Block 0 (superblock), 1 (metadata), 2 (bitmap) - mark as used
        _freeBlocks.Load();
        _freeBlocks.Allocate(); // block 0
        _freeBlocks.Allocate(); // block 1
        _freeBlocks.Allocate(); // block 2
        _freeBlocks.Save();
    }
    
    // TODO: Implement Cpin, Cpout, Ls, Rm
}