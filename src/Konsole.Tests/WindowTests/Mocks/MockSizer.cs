using System;
using System.Collections.Generic;
using System.Text;

namespace Konsole.Tests.WindowTests.Mocks
{
    public class MockSizer : IHostSize
    {
        public MockSizer() : this(10, 10, 10) { }
        public MockSizer(int width, int height, int top)
        {
            Width = width;
            Height = height;
            CursorTop = top;
        }
        public int Width { get; }

        public bool CursorVisible { get; } = false;

        public int Height { get; }

        public int CursorTop { get; }
    }
}
