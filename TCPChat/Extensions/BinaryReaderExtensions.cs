using System.IO;
using System.Text;

namespace TCPChat.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadUnicodeString(this BinaryReader binaryReader, int count) =>
        Encoding.Unicode.GetString(binaryReader.ReadBytes(count));
}