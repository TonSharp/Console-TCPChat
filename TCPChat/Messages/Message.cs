namespace TCPChat.Messages
{
    public abstract class Message : IMessage
    {
        public int PostCode { get; protected set; }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data);
    }
}

//POST CODES:
//1 - Send default message +
//2 - Send warning message +-
//3 - Send error message +-
//4 - Send notification message +-
//5 - ID message (request from client or send from server) +
//6 - UserData message (request from server or send from client) +
//7 - Connection message (join or disconnect) +
//8 - reserved
//9 - reserved
//10 - Server Disconnect message + (PostCodeMessage)
//11 - Unsupported version message + (PostCodeMessage)
//12 - reserved
//13 - reserved
//14 - reserved
//15 - reserved
//16 - reserved
//17 - reserved
//18 - reserved
//19 - reserved