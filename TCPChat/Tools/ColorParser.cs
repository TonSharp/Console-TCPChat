using System;
using System.Drawing;

namespace TCPChat.Tools
{
    public static class ColorParser
    {
        public static Color GetColorFromString(string color)
        {
            try
            {
                var consoleColor = Color.White;
                
                if(color[0] != '#')
                {
                    var knownColor = Enum.Parse<KnownColor>(color, true);
                    consoleColor = Color.FromKnownColor(knownColor);
                }
                else if(color.Length == 7)
                {
                    consoleColor = ColorTranslator.FromHtml(color);
                    var byteColor = BitConverter.GetBytes(consoleColor.ToArgb());

                    if(byteColor[0] <= 48 && byteColor[1] <= 48 && byteColor[2] <= 48)
                        consoleColor = Color.White;
                }


                if (consoleColor == Color.Black)
                    consoleColor = Color.White; //If this is Black Color, then it will be White color, no racism)

                return consoleColor;
            }

            catch
            {
                return Color.White;
            }
        }
    }
}
