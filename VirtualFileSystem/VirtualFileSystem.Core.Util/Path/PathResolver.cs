namespace VirtualFileSystem.Core.Util.Path;

public class PathResolver
{
    public int CurrentDir { get; private set; }
    
    public void ChangeDirectory(int newIndex) => CurrentDir = newIndex;
}