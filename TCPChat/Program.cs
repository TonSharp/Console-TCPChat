using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleContainer;
using McMaster.Extensions.CommandLineUtils;
using TCPChat.Tools;
using TCPChat.Network;
using TCPChat.Messages;
using TCPChat.AudioEngine;

namespace TCPChat;

internal class Program
{
    public const string DllName = "TCPChat.dll";
    
    private readonly Regex _hostRegex = new(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

    private NetworkManager _network;

    private IContainer _container;
    private AudioStorage _audioStorage;


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

    private Dictionary<string, Action<string[]>> _callbacks;

    private static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);


    // ReSharper disable once UnusedMember.Local
    private void OnExecute()
    {
        _callbacks = new Dictionary<string, Action<string[]>>
        {
            { "join", JoinCallback },
            { "connect", JoinCallback },
            { "create", RoomCallback },
            { "room", RoomCallback },
            { "disconnect", DisconnectCallback },
            { "clear", ClearCallback },
            { "clr", ClearCallback },
            { "color", ColorCallback },
            { "hash", HashCallback }
        };

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

    private void ParseCommand(string command)
    {
        var input = _network.GetInputType(command);

        switch (input)
        {
            case Input.Message:
                ParseInputMessage(command);
                break;
            case Input.Command:
            {
                ParseInputCommand(command);
                break;
            }
            case Input.Empty:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ParseInputMessage(string message) => _network.SendMessage(new SimpleMessage(_network.User, message));

    private void ParseInputCommand(string command)
    {
        var args = _network.GetCommandArgs(command);

        if(_callbacks.TryGetValue(args[0], out var callback))
            callback?.Invoke(args);
        else
            throw new Exception("Unknown command");
    }

    private void JoinCallback(string[] args)
    {
        if(_network.IsConnectedToServer())
        {
            _network.Cmd.WriteLine("You need to disconnect first");
            return;
        }

        if(_network.TryJoin(args))
            _audioStorage.PlaySound(AudioType.Connection);
    }

    private void RoomCallback(string[] args)
    {
        if(args.Length == 2 && _network.TryCreateRoom(args[1]))
            _audioStorage.PlaySound(AudioType.Connection);
    }

    private void DisconnectCallback(string[] args)
    {
        if(args.Length == 1)
            _network.TryDisconnect();
    }

    private void ClearCallback(string[] args)
    {
        if(args.Length == 1)
            _network.Cmd.Clear();
    }

    private void ColorCallback(string[] args)
    {
        if(args.Length != 2)
            return;

        _network.User.SetColor(ColorParser.GetColorFromString(args[1]));
        _network.SendMessage(new UserDataMessage(Method.Send, _network.User));
    }

    private void HashCallback(string[] args)
    {
        if(args.Length == 1)
            _network.Cmd.WriteLine(VersionVerifier.GetStringHash());
    }

    private void OnNotificationReceived() => _audioStorage.PlaySound(AudioType.Notification);
}