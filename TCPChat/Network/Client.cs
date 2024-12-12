using System;
using System.Net.Sockets;
using System.Text;
using TCPChat.Messages;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Client
    {
        public event Action Notification;
        
        private readonly TcpClient _client;
        private readonly Server _server;
        
        private User _user;
        
        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; private set; }

        public Client(TcpClient tcpClient, Server server)
        {
            Id = Guid.NewGuid().ToString();
            
            _client = tcpClient;
            _server = server;
            
            server.AddConnection(this);
        }

        public void Process()
        {  
            try
            {
                Stream = _client.GetStream();
                InitializeUserData();

                while (true)
                {
                    try
                    {
                        var message = GetMessage();
                        
                        if (message.Length > 0)
                        {
                            var msg = IMessageDeserializable.Parse(message);

                            switch(msg.PostCode)
                            {
                                case >= 1 and <= 4:
                                {
                                    Notification?.Invoke();
                                    _server.BroadcastMessage(msg, Id);
                                    
                                    break;
                                }
                                case 6:
                                {
                                    var userDataMessage = msg as UserDataMessage;
                                    if(userDataMessage?.Method == Method.Send)
                                        _user = new User(userDataMessage.Sender.UserName, userDataMessage.Sender.Color);
                                    
                                    break;
                                }
                                case 7:
                                {
                                    var idMessage = msg as IDMessage;
                                    if (idMessage?.Method == Method.Get)
                                    {
                                        var sendMessage = new IDMessage(Method.Send, Id);
                                        Stream.Write(sendMessage.Serialize());
                                    }

                                    break;
                                }
                                case 9:
                                {
                                    _server.BroadcastMessage(msg, Id);
                                    _server.RemoveConnection(Id);
                                    break;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    catch
                    {
                        var disconnectionMsg = new ConnectionMessage(Connection.Disconnect, _user);
                        _server.BroadcastMessage(disconnectionMsg, Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _server.RemoveConnection(Id);
                Close();
            }
        }

        private void InitializeUserData()
        {
            var messageData = GetMessage();
            var msg = new ConnectionMessage(messageData);

            if (!VersionVerifier.Verify(msg.Hash))
            {
                Stream.Write(new PostCodeMessage(11).Serialize());
                _server.RemoveConnection(Id);

                return;
            }

            SendId();
            _server.BroadcastMessage(msg, Id);
        }

        private void SendId()
        {
            var msg = new IDMessage(Method.Send, Id);
            Stream.Write(msg.Serialize());
        }

        private byte[] GetMessage()
        {
            var data = new byte[64];
            var builder = new StringBuilder();
            do
            {
                var bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return Encoding.Unicode.GetBytes(builder.ToString());
        }

        protected internal void Close()
        {
            Stream?.Close();
            _client?.Close();
        }
    }
}