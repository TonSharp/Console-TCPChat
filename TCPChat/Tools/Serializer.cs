using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TCPChat.Tools
{
    public static class Serializer
    {
        public static byte[] CopyFrom(byte[] startData, int moveFrom)
        {
            var movedData = new byte[startData.Length - moveFrom];

            for (var i = 0; i < startData.Length; i++)
            {
                if (i < moveFrom)
                    continue;

                if (i - moveFrom < movedData.Length)
                    movedData[i - moveFrom] = startData[i];
            }

            return movedData;
        }
        
        public static byte[] JoinBytes(byte[] data1, byte[] data2)
        {
            var expandedData = new byte[data1.Length + data2.Length];

            data1.CopyTo(expandedData, 0);
            data2.CopyTo(expandedData, data1.Length);

            return expandedData;
        }
        
        public static int GetStringDataSize(params string[] str)
        {
            var size = 0;

            foreach (var s in str)
            {
                size += sizeof(int);
                size += sizeof(char) * s.Length;
            }

            return size;
        }
        
        public static byte[] SerializeString(params string[] str)
        {
            var data = new byte[GetStringDataSize(str)];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);

            foreach (var s in str)
            {
                writer.Write(s.Length);
                writer.Write(Encoding.Unicode.GetBytes(s));
            }

            return data;
        }
        
        public static string[] DeserializeString(byte[] data)
        {
            var deserialized = new List<string>();
            var bytes = 0;

            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream);

                while (bytes < data.Length - 1)
                {
                    var size = reader.ReadInt32();
                    
                    bytes += sizeof(int);
                    deserialized.Add(Encoding.Unicode.GetString(reader.ReadBytes(size)));
                    bytes += sizeof(char) * size;
                }
            }

            return deserialized.ToArray();
        }

        public static string[] DeserializeString(byte[] data, int count)
        {
            var deserialized = new string[count];

            using var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            for (var i = 0; i < count; i++)
            {
                var size = reader.ReadInt32();
                deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
            }

            return deserialized;
        }
        
        public static string[] DeserializeString(byte[] data, int count, out byte[] otherData)
        {
            var deserialized = new string[count];

            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream);

                for (var i = 0; i < count; i++)
                {
                    var size = reader.ReadInt32();
                    deserialized[i] = Encoding.Unicode.GetString(reader.ReadBytes(size * sizeof(char)));
                }
            }

            var stringDataSize = GetStringDataSize(deserialized);
            otherData = CopyFrom(data, stringDataSize);

            return deserialized;
        }
    }
}
