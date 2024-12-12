using System.IO;

namespace TCPChat.Messages
{
    public interface IMessageDeserializable
    {
        public void Deserialize(byte[] data);

        public static int GetPostCode(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            return reader.ReadInt32();
        }

        public static Message Parse(byte[] data)
        {
            var code = GetPostCode(data);

            switch (code)
            {
                    case { } i when(i >= 1 && i <= 4):
                    {
                        return new SimpleMessage(data);
                    }
                    case 5:
                    {
                        return new IDMessage(data);
                    }
                    case 6:
                    {
                        return new UserDataMessage(data);
                    }
                    case 7:
                    {
                        return new ConnectionMessage(data);
                    }
                    case { } i when (i >= 10 && i <= 11):
                    {
                        return new PostCodeMessage(data);
                    }
                    
                    default: return null;
            }
        }
    }
}