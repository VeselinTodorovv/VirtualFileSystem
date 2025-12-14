namespace VirtualFileSystem.Core.Util;

public class PathResolver
{
    public int CurrentDir { get; private set; }
    
    public void ChangeDirectory(int newIndex) => CurrentDir = newIndex;
}