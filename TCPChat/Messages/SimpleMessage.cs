using TCPChat.Network;
using System.IO;
using TCPChat.Extensions;

namespace TCPChat.Messages
{
    public class SimpleMessage : Message
    {
        public User Sender { get; private set; }
        public string SendData { get; private set; }
        public Method Method { get; private set; }
        public SimpleMessage(User sender, string message)
        {
            Sender = sender;
            PostCode = 1;
            SendData = message;
            Method = Method.Send;
        }

        public SimpleMessage(byte[] data)
        {
            Deserialize(data);
        }
        
        public override byte[] Serialize()
        {
            var userData = Sender.Serialize();
            var messageData = SendData.Serialize();
            var data = new byte[sizeof(int) + userData.Length + messageData.Length];

            using var stream = new MemoryStream(data);
            var writer = new BinaryWriter(stream);
            writer.Write(PostCode);
            writer.Write(userData);
            writer.Write(messageData);

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            var code = reader.ReadInt32();
            if (code != 1) return;
            PostCode = code;
            
            var userData = data.CopyFrom(sizeof(int));
            Sender = new User(userData, out var messageData);

            SendData = messageData.DeserializeStrings(1)[0];
        }
    }
}