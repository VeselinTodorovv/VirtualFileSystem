using VirtualFileSystem.Core.Diagnostics;
using VirtualFileSystem.Core.Layout;

namespace VirtualFileSystem.Core.Container;

public class FreeBlockMap
{
    private readonly ContainerStream _stream;
    private readonly FreeBlockMapLayout _layout;
    private readonly byte[] _map;
    
    public FreeBlockMap(ContainerStream stream, FreeBlockMapLayout layout)
    {
        _stream = stream;
        _layout = layout;
        _map = new byte[_layout.BlockCount];
    }
    
    public void Save()
    {
        var offsetOfBlock = _stream.OffsetOfBlock(_layout.StartBlock);
        _stream.WriteBytes(offsetOfBlock, _map, _map.Length);
    }
    
    public int Allocate()
    {
        for (int i = 0; i < _map.Length; i++)
        {
            if (_map[i] != 0)
            {
                continue;
            }
            
            MarkUsed(i);
            FsLogger.Log($"Allocating block {i}");
            
            return i;
        }
        
        return -1;
    }
    
    public void MarkFree(int index)
    {
        FsLogger.Log($"Freeing block {index}");
        
        _map[index] = 0;
    }
    
    public void MarkUsed(int index)
    {
        _map[index] = 1;
    }
    
    public void ClearAll()
    {
        Array.Clear(_map, 0, _map.Length);
    }
}