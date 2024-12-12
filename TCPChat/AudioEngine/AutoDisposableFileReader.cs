using NAudio.Wave;

namespace TCPChat.AudioEngine
{
    internal class AutoDisposableFileReader : ISampleProvider
    {
        private readonly AudioFileReader _reader;
        
        private bool _isDisposed;
        
        public WaveFormat WaveFormat { get; }

        public AutoDisposableFileReader(AudioFileReader reader)
        {
            _reader = reader;
            WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (_isDisposed)
                return 0;
            
            var read = _reader.Read(buffer, offset, count);
            
            if (read != 0)
                return read;
            
            _reader.Dispose();
            _isDisposed = true;
            
            return read;
        }
    }
}
