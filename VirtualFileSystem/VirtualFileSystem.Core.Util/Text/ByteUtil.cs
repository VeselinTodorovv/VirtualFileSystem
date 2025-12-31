using System.Text;
using VirtualFileSystem.Core.Layout.Constants;

namespace VirtualFileSystem.Core.Util.Text;

public static class ByteUtil
{
    public static void WriteFixedString(string? value, byte[] buffer, int offset, int maxLength)
    {
        if (value == null)
        {
            return;
        }
        
        byte[] src = Encoding.ASCII.GetBytes(value);
        
        int count = Math.Min(src.Length, maxLength);
        Array.Copy(src, 0, buffer, offset, count);
        
        for (int i = offset + count; i < offset + maxLength; i++)
        {
            buffer[i] = 0;
        }
    }
    
    public static string ReadFixedString(byte[] buffer)
    {
        string chars = Encoding.ASCII.GetString(buffer, FileMetadataLayout.NameOffset, FileMetadataLayout.NameMaxLength);
        
        return Trim(chars);
    }
    
    private static string Trim(string value)
    {
        int length = 0;
        foreach (var c in value)
        {
            if (c == '\0')
            {
                break;
            }
            
            length++;
        }
        
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = value[i];
        }
        
        return new string(result);
    }
}