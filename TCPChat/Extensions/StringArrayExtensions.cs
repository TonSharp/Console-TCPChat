using System.IO;
using System.Linq;
using System.Text;

namespace TCPChat.Extensions;

public static class StringArrayExtensions
{
    public static int GetBytesDataSize(this string[] str) => str.Sum(s => str.GetBytesDataSize());
    
    public static byte[] Serialize(this string[] str)
    {
        var data = new byte[str.GetBytesDataSize()];

        using var stream = new MemoryStream(data);
        var writer = new BinaryWriter(stream);

        foreach (var s in str)
        {
            writer.Write(s.Length);
            writer.Write(Encoding.Unicode.GetBytes(s));
        }

        return data;
    }
}