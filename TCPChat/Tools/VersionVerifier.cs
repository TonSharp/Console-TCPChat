using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace TCPChat.Tools
{
    public static class VersionVerifier
    {
        public static bool Verify(byte[] remoteHash)
        {
            var localHash = GetHash();

            if (localHash.Length != remoteHash.Length) return false;

            return !localHash.Where((t, i) => t != remoteHash[i]).Any();
        }

        public static void PrintHash() => Console.WriteLine(GetStringHash());

        public static string GetStringHash()
        {
            var data = GetHash();
            var builder = new StringBuilder(data.Length);
            
            foreach (var b in data)
            {
                builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
        }

        public static byte[] GetHash()
        {
            using var stream = new FileStream("TCPChat.dll", FileMode.Open, FileAccess.Read, FileShare.Read);
            var hash = new MD5CryptoServiceProvider().ComputeHash(stream);

            return hash;
        }
    }
}