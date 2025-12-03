namespace VirtualFileSystem.API;

public class FileEntryInfo(string name, int size)
{
    public string Name { get; } = name;
    public int Size { get; } = size;
}