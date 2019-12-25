using System;

namespace Konsole
{
    public class HostSizer : IHostSizer
    {
        private int? _width;
        private int? _height;
        private int? _top;
        public int Width => _width ?? (_width = Console.WindowWidth).Value;
        public int Height => _height ?? (_height = Console.WindowHeight).Value;
        public int CursorTop => _top ?? (_top = Console.CursorTop).Value;
    }

    public class Sizer : IHostSizer
    {
        private readonly IConsole _console;

        public Sizer(IConsole console)
        {
            _console = console;
        }
        public int Width => _console.WindowWidth;
        public int Height => _console.WindowHeight;
        public int CursorTop => _console.CursorTop;

    }
}
