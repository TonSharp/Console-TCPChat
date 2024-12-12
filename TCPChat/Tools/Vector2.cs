using System;

namespace TCPChat.Tools
{
    public class Vector2
    {
        public event Action PositionChanged;
        
        private int _x;
        private int _y;

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

        public Vector2(int x = 0, int y = 0)
        {
            _x = x;
            _y = y;
        }
    }
}
