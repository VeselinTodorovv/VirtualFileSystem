namespace VirtualFileSystem.Core.Diagnostics;

public static class FsLogger
{
    private static bool _enabled;
    
    public static void Enable() => _enabled = true;
    public static void Disable() => _enabled = false;
    
    public static void Log(string message)
    {
        if (!_enabled)
        {
            return;
        }
        
        Console.WriteLine($"[FS:DEBUG] {message}");
    }
}