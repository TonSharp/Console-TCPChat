using System.Collections.Generic;

namespace TCPChat.AudioEngine;

public class AudioStorage
{
    private readonly Dictionary<AudioType, AudioFile> _sounds = new()
    {
        { AudioType.Startup, new AudioFile("Audio/Startup.mp3") },
        { AudioType.Connection, new AudioFile("Audio/Connection.mp3") },
        { AudioType.Notification, new AudioFile("Audio/MessageNotification.mp3") }
    };

    public void PlaySound(AudioType audioType) => _sounds[audioType].Sound.TryPlay();
}

public class AudioFile(string path)
{
    private readonly string _path = path;
    public CachedSound Sound { get; } = new(path);
}

public enum AudioType
{
    Startup,
    Connection,
    Notification
}