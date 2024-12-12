using System;
using System.Text;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;
using SimpleContainer;
using TCPChat.AudioEngine;
using TCPChat.Messages;
using TCPChat.Network;
using TCPChat.Tools;

namespace TCPChat;

internal class Program
{
    private static NetworkManager _network;

    private static IContainer _container;
    private static AudioStorage _audioStorage;

    private readonly Regex _hostRegex = new(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

    [Option("-n|--name", CommandOptionType.SingleValue)]
    private string Name { get; } = null;

    [Option("-c|--color", CommandOptionType.SingleValue)]
    private string Color { get; } = null;

    [Option("-C|--client", CommandOptionType.SingleValue)]
    private bool UseClient { get; } = false;
    
    [Option("-S|--server", CommandOptionType.SingleValue)]
    private bool UseServer { get; } = false;
    
    [Option("-h|--host", CommandOptionType.SingleValue)]
    private string Host { get; } = null;
    
    [Option("-p|--port", CommandOptionType.SingleValue)]
    private int Port { get; } = 0;

    private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);


    // ReSharper disable once UnusedMember.Local
    private void OnExecute()
    {
        var dependencyInstaller = new DependencyInstaller();
        _container = dependencyInstaller.Install();

        _audioStorage = _container.Resolve<AudioStorage>();
        _network = _container.Resolve<NetworkManager>();

        _network.NotificationReceived += OnNotificationReceived;
        Console.OutputEncoding = Encoding.Unicode;
        
        _audioStorage.PlaySound(AudioType.Startup);

        if(UseServer || UseClient)
        {
            if(Host == null || !_hostRegex.IsMatch(Host))
                throw new Exception("Invalid host name");
            
            if(Port == 0)
                throw new Exception("Invalid port number");
            
            if(UseServer && UseClient)
                throw new Exception("Use server or client only");
            
            _network.Connector.Port = Port;
        }

        if(Name != null && Color != null)
        {
            _network.User = new User(Name, ColorParser.GetColorFromString(Color));
            _network.Cmd.Clear();
        }
        else
            _network.RegisterUser();

        if(UseClient)
        {
            _network.Connector.Host = Host;
            
            if(_network.StartClient())
                _audioStorage.PlaySound(AudioType.Connection);
            
            _network.Cmd.SwitchToPrompt();
        }
        else if(UseServer)
        {
            if(_network.StartServer())
                _audioStorage.PlaySound(AudioType.Connection);
            
            _network.Cmd.SwitchToPrompt();
        }
        
        while (true)
            ParseCommand(_network.Process());
    }

    private static void ParseCommand(string command)
    {
        var input = _network.GetInputType(command);

        switch (input)
        {
            //If this is not a command
            case Input.Message:
                _network.SendMessage(new SimpleMessage(_network.User, command));
                break;
            case Input.Command:
            {
                string[] args = _network.GetCommandArgs(command);

                switch (args[0])
                {
                    case { } s when (s == "join" || s == "connect"):
                    {
                        if(_network.IsConnectedToServer()) //So if you try reconnect and you already have session
                        {
                            _network.Cmd.WriteLine("You need to disconnect first"); //You need ro disconnect)

                            return;
                        }

                        if(_network.TryJoin(args))
                            _audioStorage.PlaySound(AudioType.Connection);

                        break;
                    }
                    case { } s when (s == "create" || s == "room"):
                    {
                        if(args.Length == 2)
                        {
                            if(_network.TryCreateRoom(args[1]))
                                _audioStorage.PlaySound(AudioType.Connection);
                        }

                        break;
                    }
                    // ReSharper disable once StringLiteralTypo
                    case { } s when (s == "disconnect" || s == "dconnect"):
                    {
                        if(args.Length == 1)
                        {
                            _network.TryDisconnect();
                        }

                        break;
                    }
                    case { } s when (s == "clear" || s == "clr"):
                    {
                        if(args.Length == 1)
                        {
                            _network.Cmd.Clear();
                        }

                        break;
                    }

                    case { } s when (s == "color"):
                    {
                        if(args.Length != 2) return;
                        _network.User.SetColor(ColorParser.GetColorFromString(args[1]));
                        _network.SendMessage(new UserDataMessage(Method.Send, _network.User));

                        break;
                    }

                    case { } s when (s == "hash"):
                    {
                        if(args.Length == 1)
                        {
                            _network.Cmd.WriteLine(VersionVerifier.GetStringHash());
                        }

                        break;
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void OnNotificationReceived() => _audioStorage.PlaySound(AudioType.Notification);
}