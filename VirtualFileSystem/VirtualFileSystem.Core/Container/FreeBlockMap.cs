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
    
    public void Load()
    {
        var offsetOfBlock = _stream.OffsetOfBlock(_layout.StartBlock);
        _stream.ReadBytes(offsetOfBlock, _map, _map.Length);
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
            
            _map[i] = 1;
            Save();
            
            return i;
        }
        
        return -1;
    }
    
    public void Free(int index)
    {
        _map[index] = 0;
        Save();
    }
}