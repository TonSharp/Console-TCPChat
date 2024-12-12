using System;
using System.Collections.Generic;
using System.IO;

namespace TCPChat.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] CopyFrom(this byte[] byteArray, int startIndex)
    {
        var copiedData = new byte[byteArray.Length - startIndex];

        for (var i = 0; i < byteArray.Length; i++)
        {
            if (i < startIndex)
                continue;

            if (i - startIndex < copiedData.Length)
                copiedData[i - startIndex] = byteArray[i];
        }

        return copiedData;
    }

    public static byte[] Join(this byte[] first, byte[] second)
    {
        var expandedData = new byte[first.Length + second.Length];

        first.CopyTo(expandedData, 0);
        second.CopyTo(expandedData, first.Length);

        return expandedData;
    }
    
    public static string[] DeserializeStrings(this byte[] data)
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
                deserialized.Add(reader.ReadUnicodeString(size));
                bytes += sizeof(char) * size;
            }
        }

        return deserialized.ToArray();
    }
    
    public static string[] DeserializeStrings(this byte[] data, int stringsCount)
    {
        var deserialized = new string[stringsCount];

        using var stream = new MemoryStream(data);
        var reader = new BinaryReader(stream);

        for (var i = 0; i < stringsCount; i++)
        {
            var size = reader.ReadInt32();
            deserialized[i] = reader.ReadUnicodeString(size * sizeof(char));
        }

        return deserialized;
    }
    
    public static string[] DeserializeStrings(this byte[] data, int stringsCount, out byte[] otherData)
    {
        var deserialized = new string[stringsCount];

        using (var stream = new MemoryStream(data))
        {
            var reader = new BinaryReader(stream);

            for (var i = 0; i < stringsCount; i++)
            {
                var size = reader.ReadInt32();
                deserialized[i] = reader.ReadUnicodeString(size * sizeof(char));
            }
        }

        var stringDataSize = deserialized.GetBytesDataSize();
        otherData = data.CopyFrom(stringDataSize);

        return deserialized;
    }

    public static string ToHexString(this byte[] data) => Convert.ToHexString(data);
}