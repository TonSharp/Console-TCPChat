using System;

namespace TCPChat.Tools
{
    public class Vector2(int x = 0, int y = 0)
    {
        public event Action PositionChanged;
        
        private int _x = x;
        private int _y = y;

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                PositionChanged?.Invoke();
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                PositionChanged?.Invoke();
            }
        }
    }
}
