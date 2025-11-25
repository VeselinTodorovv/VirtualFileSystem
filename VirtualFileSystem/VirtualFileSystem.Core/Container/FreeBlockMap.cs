namespace VirtualFileSystem.Core.Container;

public class FreeBlockMap(ContainerStream stream, int blockCount, int startBlock)
{
    private readonly byte[] _map = new byte[blockCount]; // 0- free; 1- used

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

    public void Free(int blockIndex)
    {
        _map[blockIndex] = 0;
        Save();
    }

    public void Load()
    {
        byte[] buffer = new byte[_map.Length];
        stream.ReadBytes(stream.OffsetOfBlock(startBlock), buffer, _map.Length);

        for (int i = 0; i < _map.Length; i++)
        {
            _map[i] = buffer[i];
        }
    }

    public void Save()
    {
        stream.WriteBytes(stream.OffsetOfBlock(startBlock), _map, _map.Length);
    }
}