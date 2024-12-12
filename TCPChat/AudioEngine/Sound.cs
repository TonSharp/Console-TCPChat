namespace TCPChat.AudioEngine
{
    public static class Sound
    {
        public static CachedSound TryLoadCached(string path) => new(path);
    }
}