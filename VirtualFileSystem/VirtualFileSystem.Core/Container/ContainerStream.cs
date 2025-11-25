namespace VirtualFileSystem.Core.Container;

public class ContainerStream : IDisposable
{
    private readonly FileStream _stream;
    
    public string Path { get; }
    public int BlockSize { get; }

    public ContainerStream(string path, int blockSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(blockSize);

        Path = path;
        BlockSize = blockSize;
        
        _stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }
    
    public long Length => _stream.Length;
    public bool IsEmpty => _stream.Length == 0;

    public void Flush()
    {
        _stream.Flush();
    }
    
    public void ReadAt(long offset, byte[] buffer, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (count < 0 || count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _stream.Seek(offset, SeekOrigin.Begin);

        int readTotal = 0;
        while (readTotal < count)
        {
            int read = _stream.Read(buffer, readTotal, count - readTotal);
            if (read == 0)
            {
                throw new EndOfStreamException("Unexpected end of container while reading.");
            }
            
            readTotal += read;
        }
    }
    
    public void WriteAt(long offset, byte[] buffer, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (count < 0 || count > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _stream.Seek(offset, SeekOrigin.Begin);
        _stream.Write(buffer, 0, count);
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}