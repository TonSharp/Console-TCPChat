using System;

namespace TCPChat.Messages
{
    public class PostCodeMessage : Message
    {
        public PostCodeMessage(int postCode)
        {
            PostCode = postCode;
        }

        public PostCodeMessage(byte[] data)
        {
            Deserialize(data);
        }
        
        public override byte[] Serialize()
        {
            var data = BitConverter.GetBytes(PostCode);

            return data;
        }

        public override void Deserialize(byte[] data)
        {
            PostCode = BitConverter.ToInt32(data);
        }
    }
}