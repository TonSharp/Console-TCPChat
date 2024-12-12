using System;
using System.IO;
using TCPChat.Extensions;
using TCPChat.Network;

namespace TCPChat.Messages
{
    public class UserDataMessage : Message
    {
        public Method Method { get; private set; }
        public User Sender { get; private set; }
        public UserDataMessage(Method method, User sender = null)
        {
            PostCode = 6;
            Method = method;

            if (method == Method.Send) Sender = sender;
        }

        public UserDataMessage(byte[] data)
        {
            Deserialize(data);
        }
        
        public override byte[] Serialize()
        {
            byte[] data = null;
            var methodData = Method.ToString().Serialize();

            switch (Method)
            {
                case Method.Get:
                {
                    data = new byte[sizeof(int) + methodData.Length];
                    
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);

                    break;
                }

                case Method.Send:
                {
                    var userData = Sender.Serialize();
                    data = new byte[sizeof(int) + methodData.Length + userData.Length];
                    
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);
                    
                    writer.Write(PostCode);
                    writer.Write(methodData);
                    writer.Write(userData);

                    break;
                }
            }

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var code = reader.ReadInt32();

            if (code != 6) return;
            PostCode = code;

            var messageData = data.CopyFrom(sizeof(int));

            Method = Enum.Parse<Method>(messageData.DeserializeStrings(1, out var userData)[0]);

            if (PostCode == 12)
            {
                Sender.Deserialize(userData, out _);
            }
        }
    }
}