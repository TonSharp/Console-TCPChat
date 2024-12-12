using SimpleContainer;
using TCPChat.AudioEngine;
using TCPChat.Network;
using TCPChat.Tools;

namespace TCPChat;

public class DependencyInstaller : IDependencyInstaller
{
    public IContainer Install()
    {
        var container = new Container();
        
        container.Bind<IContainer>().To<Container>().AsSingle().FromInstance(container);

        container.Bind<NetworkManager>().AsSingle();
        container.Bind<AudioStorage>().AsSingle();
        container.Bind<Cmd>().AsSingle();
        
        container.Bind<Server>().AsTransient();
        container.Bind<Connector>().AsTransient();
        
        
        return container;
    }
}