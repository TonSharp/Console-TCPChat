using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TCPChat.AudioEngine;
using TCPChat.Messages;
using TCPChat.Tools;

namespace TCPChat.Network
{
    public class Server(Cmd cmd)
    {
        public event Action NotificationReceived;
        
        private static TcpListener _tcpListener;
        private readonly List<Client> _clients = [];

        private int _port;

        public void SetPort(int port) => _port = port;

        protected internal void AddConnection(Client clientObject)
        {
            clientObject.Notification += OnNotificationReceived;
            _clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            var client = _clients.FirstOrDefault(c => c.Id == id);

            if(client == null)
                return;
            
            client.Notification -= OnNotificationReceived;
            _clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, _port);
                _tcpListener.Start();
                cmd.WriteLine("Server started, waiting for connections...");
                cmd.SwitchToPrompt();

                while (true)
                {
                    var tcpClient = _tcpListener.AcceptTcpClient(); //If we get a new connection
                    var clientObject = new Client(tcpClient, this); //Lets create new client

                    var clientThread = new Thread(clientObject.Process); //And start new thread
                    clientThread.Start();
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
                cmd.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(Message msg, string id)
        {
            var data = msg.Serialize();
            cmd.ParseMessage(msg); //If we want to broadcast message, lets write it in the server

            foreach (var t in _clients.Where(t => t.Id != id))
            {
                t.Stream.Write(data, 0, data.Length); //And then if it isn a sender, send rhis message to client
            }
        }

        protected internal void BroadcastFromServer(Message msg)
        {
            var data = msg.Serialize();
            foreach (var t in _clients)
            {
                t.Stream.Write(data, 0, data.Length); //Send this message for all clients
            }
        }

        protected internal void Disconnect()
        {
            _tcpListener.Stop();
            cmd.WriteLine("Server was stopped");

            foreach (var t in _clients)
            {
                var msg = new PostCodeMessage(10);
                t.Stream.Write(msg.Serialize());
                t.Close(); //And then close connection
            }
        }

        private void OnNotificationReceived() => NotificationReceived?.Invoke();
    }
}