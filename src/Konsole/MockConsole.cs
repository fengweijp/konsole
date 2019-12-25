using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konsole.Internal;
using static System.ConsoleColor;

namespace Konsole
{
    /// <summary>
    /// MockConsole - is a default window with width of 120 and height of 60, White on Black background, that will not echo to real console
    /// that has window state, colors, cursor, text written, that will simulate (quite well) a real console.
    /// </summary>
    public class MockConsole : Window
    {
        public MockConsole(int width, int height, ConsoleColor foreground, ConsoleColor background)
            : base(new Settings(0, 0, width, height, foreground, background, false, new NullWriter(width, height)) { isMockConsole = true }) { }

        public MockConsole()
            : base(new Settings(0, 0, 120, 60, Gray, Black, false, new NullWriter(120, 60)) { isMockConsole = true }) { }

        public MockConsole(int width, int height) 
            : base( new Settings(0, 0, width, height, White, Black, false, new NullWriter(width, height)) { isMockConsole = true }) { }


        public override void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop,
            char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            for (int i = sourceTop-1; i < sourceTop + (sourceHeight-1); i++)
            {
                for (int x = sourceLeft; x < sourceLeft + sourceWidth; x++)
                {
                    _lines[i].Cells[x] = _lines[i + 1].Cells[x];
                }
            }
            for (int x = sourceLeft; x < sourceLeft + sourceWidth; x++)
            {
                _lines[sourceTop + sourceHeight-1].Cells[x] = new Cell(sourceChar,sourceForeColor, sourceBackColor);
            }
        }

    }
}
