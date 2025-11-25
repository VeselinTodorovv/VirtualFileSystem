namespace VirtualFileSystem.Core.Container;

public class ContainerStream
{
    private readonly FileStream _stream;
    
    public int BlockSize { get; }
    public long Length => _stream.Length;

    public ContainerStream(string path, int blockSize)
    {
        BlockSize = blockSize;
        
        _stream = new FileStream(
        path,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.None
        );
    }
    
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
        _stream.Flush();
    }
    
    public void ReadBlock(int blockIndex, byte[] buffer)
    {
        ReadBytes(OffsetOfBlock(blockIndex), buffer, BlockSize);
    }

    public void WriteBlock(int blockIndex, byte[] buffer)
    {
        WriteBytes(OffsetOfBlock(blockIndex), buffer, BlockSize);
    }
}