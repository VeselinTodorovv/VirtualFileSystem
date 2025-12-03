namespace VirtualFileSystem.Core.Util;

public static class ByteUtil
{
    public static void WriteFixedString(string? value, byte[] buffer, int offset, int maxLength)
    {
        if (value == null)
        {
            return;
        }
        
        byte[] src = System.Text.Encoding.ASCII.GetBytes(value);
        
        int count = Math.Min(src.Length, maxLength);
        Array.Copy(src, 0, buffer, offset, count);
        
        int offset1 = offset + count;
        int length = offset + maxLength;
        
        for (int i = offset1; i < length; i++)
        {
            buffer[i] = 0;
        }
    }
    
    public static string ReadFixedString(byte[] buffer, int offset, int maxLength)
    {
        int length = 0;
        
        for (int i = 0; i < maxLength; i++)
        {
            if (buffer[offset + i] == 0)
            {
                break;
            }
            
            length++;
        }
        
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)buffer[offset + i];
        }
        
        return new string(chars);
    }
}