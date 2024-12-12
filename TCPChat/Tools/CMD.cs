using System.Drawing;
using TCPChat.Messages;
using TCPChat.Network;
using Console = Colorful.Console;

namespace TCPChat.Tools
{
    public class Cmd
    {
        private readonly Vector2 _messagePos, _promptPos, _defaultPromptPos, _currentPromptPos;
        
        private void CheckBufferArea()
        {
            if (_messagePos.Y >= _promptPos.Y)
            {
                Console.SetCursorPosition(_promptPos.X, _promptPos.Y);
                Console.Write("  ");
                
                _promptPos.Y++;
                
                Console.SetCursorPosition(_promptPos.X, _promptPos.Y);
            }
        }

        public Cmd()
        {
            _messagePos = new Vector2();
            _defaultPromptPos = new Vector2(0, 15);
            _currentPromptPos = new Vector2(0, _defaultPromptPos.Y);
            _promptPos = new Vector2(0, _defaultPromptPos.Y);
            _messagePos.PositionChanged += CheckBufferArea;
        }
        
        public void WriteLine<T>(T message)
        {
            _currentPromptPos.X = Console.CursorLeft;
            _currentPromptPos.Y = _promptPos.Y;

            Console.SetCursorPosition(_messagePos.X, _messagePos.Y);
            Console.WriteLine(message, Color.White);
            _messagePos.Y++;
        }
        
        public void UserWriteLine<T>(T message, User sender)
        {
            _currentPromptPos.X = Console.CursorLeft;
            _currentPromptPos.Y = _promptPos.Y;

            Console.SetCursorPosition(_messagePos.X, _messagePos.Y);
            Console.Write(sender.UserName, sender.Color);
            Console.Write(": ", Color.White);
            Console.WriteLine(message, Color.White);
            _messagePos.Y++;
        }
        
        public void ConnectionMessage(User sender, string str)
        {
            _currentPromptPos.X = Console.CursorLeft;
            _currentPromptPos.Y = _promptPos.Y;

            Console.SetCursorPosition(_messagePos.X, _messagePos.Y);
            Console.Write(sender.UserName, sender.Color);
            Console.WriteLine(" " + str, Color.White);
            _messagePos.Y++;
        }
        
        public string ReadLine(User user)
        {
            Console.SetCursorPosition(_promptPos.X, _promptPos.Y);
            Console.Write(user.UserName, user.Color);
            Console.Write("> ", Color.White);
            
            var input = Console.ReadLine();
            
            Console.SetCursorPosition(_promptPos.X, _promptPos.Y);
            Console.Write(new string(' ',user.UserName.Length + input.Length + 2));

            return input;
        }

        public void SwitchToPrompt()
        {
            Console.SetCursorPosition(_currentPromptPos.X, _currentPromptPos.Y);
        }
        
        public void ParseMessage(Message message)
        {
            switch (message.PostCode)
            {
                case <= 4 and >= 1:
                {
                    var msg = message as SimpleMessage;
                    UserWriteLine(msg?.SendData, msg?.Sender);
                    
                    break;
                }
                
                case 7:
                {
                    var msg = message as ConnectionMessage;
                    
                    if(msg?.Connection == Connection.Connect)
                        UserWriteLine("Has joined", msg.Sender);
                    
                    else UserWriteLine("Has disconnected", msg?.Sender);
                    
                    break;
                }
                
                default: return;
            }

            CheckBufferArea();
            SwitchToPrompt();
        }
        
        public void Clear()
        {
            Console.Clear();
            _messagePos.Y = 0;
            _promptPos.Y = _defaultPromptPos.Y;
        }
    }
}
