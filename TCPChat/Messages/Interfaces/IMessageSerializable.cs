namespace TCPChat.Messages
{
    public interface IMessageSerializable
    {
        public byte[] Serialize();
    }
}