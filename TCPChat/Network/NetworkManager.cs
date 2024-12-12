using System;
using System.Text;
using System.Threading;
using SimpleContainer;
using TCPChat.Tools;
using TCPChat.Messages;

namespace TCPChat.Network
{
    public class NetworkManager
    {
        public event Action NotificationReceived;
        
        public User User;
        
        public readonly Cmd Cmd;
        public readonly Connector Connector;

        private Thread _listenThread;
        private Thread _receiveThread;

        private string _id;


        private bool _receiveMessage;

        public NetworkManager(IContainer container)
        {
            Cmd = container.Resolve<Cmd>();
            Connector = container.Resolve<Connector>();
        }

        public string Process() => Cmd.ReadLine(User);

        protected internal void RegisterUser()
        {
            var nameIsCorrect = false;
            var userName = string.Empty;

            while (!nameIsCorrect)
            {
                Console.Write("Enter your name: ");
                userName = Console.ReadLine();

                if(userName is { Length: > 16 })
                    Console.WriteLine("Name is too long");
                else
                    nameIsCorrect = true;
            }

            Console.Title = userName!;

            Console.Write("Enter your color (white): ");
            var color = Console.ReadLine();

            Console.Clear();

            User = new User(userName, ColorParser.GetColorFromString(color));
        }

        public bool StartClient()
        {
            Connector.StartClient(_receiveThread, _listenThread);

            var joiningMessage = new ConnectionMessage(Connection.Connect, User);
            SendMessage(joiningMessage);

            Cmd.WriteLine("Successfully connected to the server");

            _receiveMessage = true;

            _receiveThread = new Thread(ReceiveMessage);
            _receiveThread.Start();

            return true;
        }

        public bool StartServer()
        {
            try
            {
                if(Connector.ConnectionType == ConnectionType.Server)
                    _listenThread?.Interrupt();

                Connector.StartServer();

                _listenThread = new Thread(Connector.Server.Listen);
                _listenThread.Start();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReceiveMessage()
        {
            while (_receiveMessage)
            {
                try
                {
                    var message = GetMessage();

                    if(message is not { Length: > 0 })
                        continue;

                    var msg = IMessageDeserializable.Parse(message);
                    ParseMessage(msg);
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                catch (System.IO.IOException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Can't receive message: " + e.Message);
                }
            }
        }

        private byte[] GetMessage()
        {
            try
            {
                var data = new byte[64];
                var builder = new StringBuilder();

                if(Connector.Stream == null)
                    return null;

                do
                {
                    var bytes = Connector.Stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (Connector.Stream.DataAvailable);

                return Encoding.Unicode.GetBytes(builder.ToString());
            }
            catch (NullReferenceException)
            {
                return null;
            }

            catch (System.IO.IOException)
            {
                Cmd.WriteLine("You are disconnected");
                Cmd.SwitchToPrompt();

                Connector.Disconnect(_receiveThread, _listenThread);
                return null;
            }

            catch (Exception e)
            {
                Cmd.WriteLine("Can't get message from thread: " + e.Message);

                return null;
            }
        }

        public Input GetInputType(string input)
        {
            if(input.Trim().Length < 1)
                return Input.Empty;

            return input[0] == '/' ? Input.Command : Input.Message;
        }

        public string[] GetCommandArgs(string input)
        {
            if(GetInputType(input) != Input.Command)
                return [];

            var lower = input.ToLower();
            var args = lower.Split(" ");
            args[0] = args[0][1..];

            return args;
        }

        public bool IsConnectedToServer()
        {
            return (Connector.Client != null || Connector.Stream != null) &&
                   Connector.ConnectionType == ConnectionType.Client;
        }

        public bool TryCreateRoom(string port)
        {
            try
            {
                Connector.Port = Convert.ToInt32(port);

                return StartServer();
            }
            catch
            {
                return false;
            }
        }

        public bool TryJoin(params string[] joinCommand)
        {
            try
            {
                switch (joinCommand.Length)
                {
                    case 2:
                    {
                        var data = joinCommand[1].Split(":");

                        Connector.Port = Convert.ToInt32(data[1]);
                        Connector.Host = data[0];

                        break;
                    }
                    case 3:
                        Connector.Host = joinCommand[0];
                        Connector.Port = Convert.ToInt32(joinCommand[1]);

                        break;

                    default:
                        return false;
                }

                return StartClient();
            }
            catch
            {
                return false;
            }
        }

        public void TryDisconnect()
        {
            Connector.Disconnect(_receiveThread, _listenThread);
        }

        public void SendMessage(Message msg)
        {
            try
            {
                if(Connector.ConnectionType == ConnectionType.None)
                {
                    Connector.Disconnect(_receiveThread, _listenThread);
                    return;
                }

                var data = msg.Serialize();

                if(data.Length > 0)
                {
                    Connector.Stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Cmd.WriteLine("Can't send message: " + e.Message);
            }
        }

        private void SendServerMessage(Message message)
        {
            try
            {
                if(message.PostCode is >= 1 and <= 4)
                {
                    var msg = message as SimpleMessage;
                    Cmd.UserWriteLine(msg?.SendData, User);
                }

                Connector.Server.BroadcastFromServer(message);
            }
            catch (Exception e)
            {
                Cmd.WriteLine("Can't send message from the server: " + e.Message);
            }
        }

        private void StopClient()
        {
            if(_receiveMessage)
            {
                _receiveMessage = false;

                _receiveThread.Interrupt();
                _receiveThread = null;
            }
        }

        private void DisconnectClient()
        {
            var msg = new ConnectionMessage(Connection.Disconnect, User);
            Connector.Stream.Write(msg.Serialize());

            StopClient();
        }

        private void ParseMessage(Message message)
        {
            switch (message.PostCode)
            {
                case >= 1 and <= 4:
                {
                    var simpleMessage = message as SimpleMessage;
                    Cmd.UserWriteLine(simpleMessage?.SendData, simpleMessage?.Sender);
                    
                    NotificationReceived?.Invoke();

                    break;
                }
                case 5:
                {
                    var idMessage = message as IDMessage;
                    if(idMessage?.Method == Method.Send)
                    {
                        _id = idMessage.SendData;
                        Cmd.WriteLine($"Your id is: {_id}");
                    }

                    break;
                }
                case 7:
                {
                    var connectionMessage = message as ConnectionMessage;
                    if(connectionMessage?.Connection == Connection.Connect)
                        Cmd.ConnectionMessage(connectionMessage.Sender, "has joined");
                    else
                        Cmd.ConnectionMessage(connectionMessage?.Sender, "has disconnected");

                    break;
                }
                case 10:
                {
                    DisconnectClient();
                    Cmd.WriteLine("Server was stopped");

                    break;
                }
                case 11:
                {
                    DisconnectClient();
                    Cmd.WriteLine("Hash sum is not correct");

                    break;
                }
                default: return;
            }

            Cmd.SwitchToPrompt();
        }
    }
}