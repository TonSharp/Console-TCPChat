using System;
using System.Drawing;
using System.IO;
using TCPChat.Extensions;

namespace TCPChat.Network
{
    public class User
    {
        private int UserDataSize
        {
            get
            {
                var size = UserName.GetBytesDataSize();
                size += sizeof(int);

                return size;
            }
        }

        public string UserName
        {
            get;
            private set;
        }
        
        public Color Color
        {
            get;
            private set;
        }
        
        public void SetColor(Color color)
        {
            Color = color;
        }

        public User(string name, Color color)
        {
            UserName = name;
            Color = color;
        }
        
        public User(byte[] data, out byte[] otherData)
        {
            UserName = data.DeserializeStrings(1)[0];
            
            var userDataSize = UserName.GetBytesDataSize();
            var colorData = data.CopyFrom(userDataSize);

            using (var stream = new MemoryStream(colorData))
            {
                var reader = new BinaryReader(stream);
                Color = Color.FromArgb(reader.ReadInt32());
                userDataSize += sizeof(int);
            }

            otherData = data.CopyFrom(userDataSize);        
        }
        
        public User(byte[] data)
        {
            UserName = data.DeserializeStrings(1)[0];
            var nameDataSize = UserName.GetBytesDataSize();

            var colorData = data.CopyFrom(nameDataSize);

            using var stream = new MemoryStream(colorData);
            var reader = new BinaryReader(stream);

            Color = Color.FromArgb(reader.ReadInt32());
        }
        
        public void Deserialize(byte[] data, out byte[] otherData)
        {
            var userData = new byte[UserDataSize];

            otherData = data.CopyFrom(UserDataSize);

            var userDataStrings = userData.DeserializeStrings(1, out var colorData);

            UserName = userDataStrings[0];
            Color = Color.FromArgb(Convert.ToInt32(colorData));
        }
        
        public byte[] Serialize()
        {
            var data = new byte[UserDataSize];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);

            writer.Write(UserName.Serialize());
            writer.Write(Color.ToArgb());

            return data;
        }
    }
}
