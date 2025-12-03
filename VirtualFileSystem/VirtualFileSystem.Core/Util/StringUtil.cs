namespace VirtualFileSystem.Core.Util;

public static class StringUtil
{
    public static bool Equals(string? a, string? b)
    {
        if (a == null || b == null)
        {
            return false;
        }
        
        if (a.Length != b.Length)
        {
            return false;
        }
        
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }
        
        return true;
    }
}