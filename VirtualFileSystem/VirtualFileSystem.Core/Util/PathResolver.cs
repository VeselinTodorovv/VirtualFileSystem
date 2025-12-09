namespace VirtualFileSystem.Core.Util;

public class PathResolver
{
    public int CurrentDir { get; private set; } = 0;
    
    public void ChangeDirectory(int newIndex) => CurrentDir = newIndex;
}