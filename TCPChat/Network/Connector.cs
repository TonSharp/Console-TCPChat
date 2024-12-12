using System;
using System.Net.Sockets;
using System.Threading;
using SimpleContainer;

namespace TCPChat.Network
{
    public class Connector(IContainer container)
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 23;
        
        public Server Server { get; private set; }
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public ConnectionType ConnectionType { get; private set; } = ConnectionType.None;

        public void StartServer()
        {
            if (Server != null)
            {
                Server.Disconnect();
                Server = null;
            }

            try
            {
                Server = container.Resolve<Server>();
                Server.SetPort(Port);
                
                ConnectionType = ConnectionType.Server;
            }
            catch
            {
                Server = null;
            }
        }

        public void StartClient(Thread receiveThread, Thread listenThread)
        {
            if (Stream != null || Client != null)
            {
                StopClient(receiveThread);
            }

            if (ConnectionType == ConnectionType.Server)
            {
                StopServer(listenThread);
            }

            try
            {
                Client = new TcpClient(Host, Port);
                ConnectionType = ConnectionType.Client;
                Stream = Client.GetStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void StopClient(Thread receiveThread)
        {
            try
            {
                if (Stream != null)
                {
                    Stream.Close();
                    Stream.Dispose();
                    Stream = null;
                }

                if (Client != null)
                {
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                }

                ConnectionType = ConnectionType.None;
                receiveThread.Interrupt();
            }
            catch
            {
                // ignored
            }
        }

        private void StopServer(Thread listenThread)
        {
            try
            {
                Server.Disconnect();
                ConnectionType = ConnectionType.None;
                listenThread.Interrupt();
            }
            catch
            {
                // ignored
            }
        }

        public void Disconnect(Thread receiveThread, Thread listenThread)
        {
            switch (ConnectionType)
            {
                case ConnectionType.Client: StopClient(receiveThread); 
                    break;
                case ConnectionType.Server: StopServer(listenThread);
                    break;
                case ConnectionType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}