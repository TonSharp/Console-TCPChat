using System.IO;

namespace TCPChat.Extensions;

public static class StringExtensions
{
    public static int GetBytesDataSize(this string str) => sizeof(int) + str.Length * sizeof(char);

    public static byte[] Serialize(this string str)
    {
        var data = new byte[GetBytesDataSize(str)];

        using var stream = new MemoryStream(data);
        var writer = new BinaryWriter(stream);
        
        writer.Write(str.Length);
        writer.Write(str);

        return data;
    }
}