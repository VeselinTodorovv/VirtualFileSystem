using VirtualFileSystem.Core.Diagnostics;

namespace VirtualFileSystem.Core.Container;

public class ContainerStream(string path, int blockSize) : IDisposable
{
    private readonly FileStream _stream = new(
        path,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.None
    );
    
    public int BlockSize { get; } = blockSize;
    
    public long OffsetOfBlock(int blockIndex)
    {
        return (long)blockIndex * BlockSize;
    }
    
    public void ReadBytes(long offset, byte[] buffer, int count)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.ReadExactly(buffer, 0, count);
    }
    
    public void WriteBytes(long offset, byte[] buffer, int count)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.Write(buffer, 0, count);
        
        _stream.Flush(flushToDisk: true);
    }
    
    public void ReadBlock(int blockIndex, byte[] buffer)
    {
        var offsetOfBlock = OffsetOfBlock(blockIndex);
        
        FsLogger.Log($"ReadBlock block={blockIndex} off={offsetOfBlock} (0x{offsetOfBlock:X}) size={BlockSize}");
        ReadBytes(offsetOfBlock, buffer, BlockSize);
    }
    
    public void WriteBlock(int blockIndex, byte[] buffer)
    {
        var offsetOfBlock = OffsetOfBlock(blockIndex);
        
        FsLogger.Log($"WriteBlock block={blockIndex} off={offsetOfBlock} (0x{offsetOfBlock:X}) size={BlockSize}");
        WriteBytes(offsetOfBlock, buffer, BlockSize);
    }
    
    public void Dispose()
    {
        _stream.Dispose();
    }
}