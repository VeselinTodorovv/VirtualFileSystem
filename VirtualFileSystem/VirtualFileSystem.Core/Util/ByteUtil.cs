namespace VirtualFileSystem.Core.Util;

public static class ByteUtil
{
    public static void WriteFixedString(string? value, byte[] buffer, int offset, int maxLength)
    {
        // Null to empty string
        value ??= string.Empty;

        int count = value.Length;
        if (count > maxLength)
        {
            count = maxLength;
        }
        
        // ASCII bytes
        for (int i = 0; i < count; i++)
        {
            buffer[offset + i] = (byte)value[i];
        }

        for (int i = 0; i < maxLength; i++)
        {
            buffer[offset + i] = 0;
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