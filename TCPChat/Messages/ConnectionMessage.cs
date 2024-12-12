using System;
using System.IO;
using TCPChat.Extensions;
using TCPChat.Network;
using TCPChat.Tools;

namespace TCPChat.Messages
{
    public class ConnectionMessage : Message
    {
        public User Sender { get; private set; }
        public byte[] Hash { get; private set; }
        public Connection Connection { get; private set; }

        public ConnectionMessage(Connection connection, User sender)
        {
            Sender = sender;
            Connection = connection;
            PostCode = 7;
        }

        public ConnectionMessage(byte[] data)
        {
            Deserialize(data);
        }
        
        public override byte[] Serialize()
        {
            byte[] data = null;
            var connectionData = Connection.ToString().Serialize();
            var userData = Sender.Serialize();

            switch (Connection)
            {
                case Connection.Connect:
                {
                    data = new byte[sizeof(int) + connectionData.Length + userData.Length + VersionVerifier.GetHash().Length];

                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);

                    writer.Write(PostCode);
                    writer.Write(connectionData);
                    writer.Write(userData);
                    writer.Write(VersionVerifier.GetHash());

                    break;
                }
                case Connection.Disconnect:
                {
                    data = new byte[sizeof(int) + connectionData.Length + userData.Length];
                    
                    using var stream = new MemoryStream(data);
                    using var writer = new BinaryWriter(stream);

                    writer.Write(PostCode);
                    writer.Write(connectionData);
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

            if (code != 7) return;
            PostCode = code;

            var messageData = data.CopyFrom(sizeof(int));

            Connection = Enum.Parse<Connection>(messageData.DeserializeStrings(1, out var userAndHashData)[0], true);

            Sender = new User(userAndHashData, out var hashData);

            if (Connection == Connection.Connect) Hash = hashData;
        }
    }
}