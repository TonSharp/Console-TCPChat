using System.IO;
using System.Linq;
using TCPChat.Extensions;

namespace TCPChat.Tools
{
    public static class VersionVerifier
    {
        public static bool Verify(byte[] remoteHash)
        {
            var localHash = GetHash();

            if (localHash.Length != remoteHash.Length)
                return false;

            return !localHash.Where((t, i) => t != remoteHash[i]).Any();
        }

        public static string GetStringHash() => GetHash().ToHexString();

        public static byte[] GetHash()
        {
            using var stream = new FileStream(Program.DllName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.GetMD5Hash();
        }
    }
}