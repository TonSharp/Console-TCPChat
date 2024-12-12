using System.IO;
using System.Security.Cryptography;

namespace TCPChat.Extensions;

public static class StreamExtensions
{
    public static byte[] GetMD5Hash(this Stream stream)
    {
        var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);

        return hash;
    }
}