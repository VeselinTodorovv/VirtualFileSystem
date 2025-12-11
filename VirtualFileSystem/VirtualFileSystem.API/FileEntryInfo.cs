namespace VirtualFileSystem.API;

public class FileEntryInfo(string name, int size, bool isDir)
{
    public string Name { get; } = name;
    public int Size { get; } = size;
    public bool IsDir { get; } = isDir;
}