using System;
using System.Collections.Generic;
using System.Linq;
using Konsole.Internal;
using static System.ConsoleColor;

namespace Konsole
{
    public class Window : IConsole
    {
     
        public string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        public bool OverflowBottom => CursorTop >= _height;

        // these two fields made mutable to avoid overcomplicating the constructor overloads.
        // perhaps there's a simpler way to do this?
        private int _absoluteX;
        private int _absoluteY;

        private readonly int _x;
        private readonly int _y;
        private readonly int _width;
        private readonly int _height;
        private readonly bool _echo;
        internal readonly bool _isMockConsole;
        internal readonly bool _isChildWindow;
        internal bool _isRealRoot = false; // this is root window, and it's not a mock console, this window needs to talk to OS!
        internal IHostSize _hostSizer = null;

        // Echo console is a default wrapper around the real Console, that we can swap out during testing. single underscore indicating it's not for general usage.
        private IConsole _echoConsole { get; set; }


        private bool _transparent = false;

        public bool Clipping
        {
            get { return _clipping; }
        }

        private bool _clipping = false;

        public bool Scrolling
        {
            get { return _scrolling; }
        }

        private bool _scrolling = true;

        public bool Transparent { get; private set; } = false;

        private readonly ConsoleColor _startForeground;
        private readonly ConsoleColor _startBackground;

        protected readonly Dictionary<int, Row> _lines = new Dictionary<int, Row>();

        private XY _cursor;
        private int _lastLineWrittenTo = -1;



        public Cell this[int x, int y]
        {
            get
            {
                int row = y > (_height - 1) ? (_height - 1) : y;
                int col = x > (_width - 1) ? (_width - 1) : x;
                return _lines[row].Cells[col];
            }
        }

        private XY Cursor
        {
            get { return _cursor; }
            set
            {
                int x = value.X >= _width ? (_width - 1) : value.X;
                int y = value.Y > _height ? _height : value.Y;
                _cursor = new XY(x, y);

                if (_cursor.Y > _lastLineWrittenTo && _cursor.X != 0) _lastLineWrittenTo = _cursor.Y;
                if (_cursor.Y > _lastLineWrittenTo && _cursor.X == 0) _lastLineWrittenTo = _cursor.Y - 1;
            }
        }

        public Window() 
            : this(new Settings())
        {
            
        }

        public Window(int width, int height)
            : this(new Settings() {  Width = width, Height = height })
        {
        }

        public Window(int width, int height, ConsoleColor foreground, ConsoleColor background)
            : this(new Settings(0, 0, width, height, foreground, background, true, null))
        {
        }

        public Window(IConsole console, int width, int height, ConsoleColor foreground, ConsoleColor background)
            : this(new Settings(0, 0, width, height, foreground, background, true, console))
        {
        }

        public Window(IConsole echoConsole, int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background)
            : this(new Settings(x, y, width, height, foreground, background, true, echoConsole))
        {
        }

        public Window(IConsole console, int height) 
            : this(new Settings(console.CursorLeft, console.CursorTop, console.WindowWidth, height, White, Black, true, console) { Inline = true })
        {

        }
        public Window(IConsole console, int width, int height) 
            : this(new Settings(console.CursorLeft, console.CursorTop, width, height, White, Black, true, console) { Inline = true } )
        {
        }

        public Window(IConsole echoConsole, int x, int y, int width, int height)
            : this(x, y, width, height, White, ConsoleColor.Black, true, echoConsole)
        {
        }

        // TODO: fix the window constructors, second parameter is sometimes height, and sometimes not!
        public Window(IConsole echoConsole, int height, ConsoleColor foreground, ConsoleColor background)
            : this(0, 0, echoConsole.WindowWidth, height, foreground, background, true, echoConsole)
        {
        }

        /// <summary>
        /// open a window Inline. Creates a window at the current cursor position of 'height' rows, and moves the OS's cursor down to underneath the window
        /// so that your build script or whatever is using the console can carry on writing to the console oblivious that there's a window that will 
        /// be written to.
        /// </summary>
        public static IConsole OpenInlineClipped(IConsole echoConsole, int padLeft, int width, int height, ConsoleColor foreground, ConsoleColor background)
        {
            lock (_staticLocker)
            {
                var w = new Window(
                    new Settings(padLeft, echoConsole.CursorTop, width, height, foreground, background, true, echoConsole)
                    { 
                        Clipping = true 
                    }  
                );
                echoConsole.CursorTop += height;
                return w.Concurrent();
            }
        }

        public static IConsole OpenInline(IConsole echoConsole, int padLeft, int width, int height, ConsoleColor foreground, ConsoleColor background)
        {
            lock (_staticLocker)
            {
                var w = new Window(padLeft, echoConsole.CursorTop, width, height, foreground, background, true, echoConsole);
                echoConsole.CursorTop += height;
                return w.Concurrent();
            }
        }

        public static IConsole OpenInline(IConsole echoConsole, int height)
        {
            lock (_staticLocker)
            {
                var w = new Window(0, echoConsole.CursorTop, echoConsole.WindowWidth, height, ConsoleColor.White, ConsoleColor.Black, true, echoConsole);
                echoConsole.CursorTop += height;
                echoConsole.CursorLeft = 0;
                return w.Concurrent();
            }
        } 

        public Window(IConsole echoConsole) : this(new Settings(echoConsole)) 
        { 
        }

        public Window(int x, int y, int width, int height, IConsole echoConsole = null)
            : this(new Settings(x, y, width, height, White, Black, true, echoConsole))
        {
        }

        protected Window(int x, int y, int width, int height, bool echo = true, IConsole echoConsole = null)
            : this(x, y, width, height, ConsoleColor.White, ConsoleColor.Black, echo, echoConsole)
        {
        }

        internal static object _staticLocker = new object();

        [Obsolete("please use OpenFloating. This method will be removed in the next version.")]
        public static IConsole Open(int x, int y, int width, int height, string title,
            LineThickNess thickNess = LineThickNess.Double, ConsoleColor foregroundColor = ConsoleColor.Gray,
            ConsoleColor backgroundColor = ConsoleColor.Black, IConsole console = null
            )
        {
            return OpenConcurrent(x, y, width, height, title, thickNess, foregroundColor, backgroundColor, console);
        }

        /// <summary>
        /// This is the the only threadsafe way to create a window at the moment.
        /// </summary>
        public static IConsole OpenConcurrent(int x, int y, int width, int height, string title,
        LineThickNess thickNess = LineThickNess.Double, ConsoleColor foregroundColor = ConsoleColor.Gray,
        ConsoleColor backgroundColor = ConsoleColor.Black, IConsole console = null)
        {
            lock (_staticLocker)
            {
                var echoConsole = console ?? new Writer();
                var window = new Window(x + 1, y + 1, width - 2, height - 2, foregroundColor, backgroundColor, true,
                    echoConsole);
                var state = echoConsole.State;
                try
                {
                    echoConsole.ForegroundColor = foregroundColor;
                    echoConsole.BackgroundColor = backgroundColor;
                    new Draw(echoConsole).Box(x, y, x + (width - 1), y + (height - 1), title, thickNess);

                }
                finally
                {
                    echoConsole.State = state;
                }
                return window.Concurrent();
            }
        }

        public Window(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background, IConsole echoConsole)
            : this(new Settings(x, y, width, height, foreground, background, true, echoConsole))
        {

        }

        public Window(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background) 
            : this(new Settings(x, y, width, height, foreground, background, true, null))
        {

        }

        internal static IConsole _CreateFloatingWindow(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background, bool echo = true, IConsole echoConsole = null)        
        {
            lock (_staticLocker)
            {
                var w = new Window(new Settings(x, y, width, height, foreground, background, echo, echoConsole));
                return w.Concurrent();
            }
        }

        protected internal Window(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background, bool echo = true, IConsole echoConsole = null) 
            : this(new Settings(x, y, width, height, foreground, background, echo, echoConsole, false, false, true))
        {

        }

        public bool isInline { get; private set; } = false;

        // This is the main constructor, all the others overload to this.
        protected internal Window(Settings settings)
        {
            settings.Validate();

            lock (_staticLocker)
            {
                _x = 0;
                _y = 0;
                Cursor = new XY(_x, _y);
                _isMockConsole = settings.isMockConsole;
                isInline = settings.Inline;
                _isRealRoot = settings.IsRealRoot;
                _echo = settings.Echo;
                _echoConsole = _isRealRoot ? new Writer() : settings.EchoConsole;
                _isChildWindow = !_isMockConsole;
                _absoluteX = (settings.EchoConsole?.AbsoluteX ?? 0 ) + settings.X;
                _absoluteY = (settings.EchoConsole?.AbsoluteY ?? 0) + settings.Y;
                _startForeground = settings.Foreground;
                _startBackground = settings.Background;
                _transparent = settings.Transparent;
                _clipping = settings.Clipping;
                _scrolling = settings.Scrolling;

                // _hostSizer determines when we read the real width and height from Console (potentially throwing exception in unit tests with "invalid IO"
                // and when we get the size from the echo console.
                _hostSizer = settings.HostSizer ?? (_isRealRoot ? (IHostSize)new OSSizer() : new ConsoleSizerWrapper(_echoConsole));

                (_width, _height) = ClipChildWindowToNotExceedHostBoundaries(settings);

                init();
                // if we're creating an inline window
                if (isInline)
                {
                    _echoConsole.CursorTop += _height;
                    _echoConsole.CursorLeft = 0;
                }


            }
        }

        private (int width, int height) ClipChildWindowToNotExceedHostBoundaries(Settings settings)
        {
            // TEST: ConstructorShould.clip_child_window_to_not_exceed_parent_boundaries
            // take the min of requested height or requested Height - OffsetY

            // if echo console is null, then we need to get the height and width from the Operating system because
            // theres no parent window, this is a user creating a new window() with no mock console,
            // that's never done in tests. 

            // we know a mock console will never have an offset, theres no logical use for that.
            // and in those cases the height and width is simply the height and width of the mock console itself.
            if(settings.isMockConsole)
            {
                return (settings.Width, settings.Height);
            }

            int hostHeight = _hostSizer.Height;
            int hostWidth = _hostSizer.Width;

            var height = (settings.Height + settings.Y > hostHeight) ? hostHeight - settings.Y : settings.Height;
            var width = (settings.Width + settings.X > hostWidth) ? hostWidth - settings.X : settings.Width;
            return (width, height);
        }



        private void init(ConsoleColor? background = null)
        {
            ForegroundColor =  _startForeground;
            BackgroundColor = background ?? _startBackground;
            _lastLineWrittenTo = -1;
            _lines.Clear();
            for (int i = 0; i < _height; i++)
            {
                _lines.Add(i, new Row(_width, ' ', ForegroundColor, BackgroundColor));
                if (!_transparent) PrintAt(0, i, new string(' ', _width));
            }
            Cursor = new XY(0, 0);
            _lastLineWrittenTo = -1;
        }

        /// <summary>
        /// use this method to return an 'approve-able' text buffer representing the background color of the buffer
        /// </summary>
        /// <param name="highliteColor">the background color to look for that indicates that text has been hilighted</param>
        /// <param name="hiChar">the char to use to indicate a highlight</param>
        /// <param name="normal">the chart to use for all other</param>
        /// <returns></returns>
        public string[] BufferHighlighted(ConsoleColor highliteColor, char hiChar = '#', char normal = ' ')
        {
            var buffer = new HiliteBuffer(highliteColor, hiChar, normal);
            var rows = _lines.Select(l => l.Value).ToArray();
            var texts = buffer.ToApprovableText(rows);
            return texts;
        }

        public string BufferHighlightedString(ConsoleColor highliteColor, char hiChar = '#', char normal = ' ')
        {
            var buffer = new HiliteBuffer(highliteColor, hiChar, normal);
            var rows = _lines.Select(l => l.Value).ToArray();
            var text = buffer.ToApprovableString(rows);
            return text;
        }

        /// <summary>
        /// returns the buffer with additional 2 characters representing the background color and foreground color
        /// colors rendered using the `ColorMapper.cs`
        /// </summary>
        /// <returns></returns>
        public string[] BufferWithColor
        {
            get
            {
                var buffer = _lines.Select(l => l.Value.ToStringWithColorChars());
                return buffer.ToArray();
            }
        }

        private string ColorString(Row row)
        {
            var chars = row.Cells.SelectMany(r => r.Value.ToChars()).ToArray();
            return new string(chars);
        }


        /// <summary>
        /// get the entire buffer (all the lines for the whole console) regardless of whether they have been written to or not, untrimmed.
        /// </summary>
        public string[] Buffer => _lines.Values.Take(_height).Select(b => b.ToString()).ToArray();

        /// <summary>
        /// get the entire buffer (all the lines for the whole console) regardless of whether they have been written to or not, untrimmed. as a single `crln` concatenated string.
        /// </summary>
        public string BufferString => string.Join("\r\n", Buffer);

        /// <summary>
        /// get all the lines written to for the whole console, untrimmed
        /// </summary>
        public string[] BufferWritten // should be buffer written
        {
            get { return _lines.Values.Take(_lastLineWrittenTo + 1).Select(b => b.ToString()).ToArray(); }
        }

        /// <summary>
        /// get all the lines written to for the whole console - bufferWrittenString
        /// </summary>
        public string BufferWrittenString => string.Join("\r\n", BufferWritten);


        /// <summary>
        /// get all the lines written to for the whole console, all trimmed.
        /// </summary>
        public string[] BufferWrittenTrimmed
        {
            get
            {
                return
                    _lines.Values.Take(_lastLineWrittenTo + 1).Select(b => b.ToString().TrimEnd(new[] {' '})).ToArray();
            }
        }

        //TODO: convert everything to redirect all calls to PrintAt, so that writing to parent works flawlessly!
        private void _write(string text)
        {
            if (_clipping && OverflowBottom)
            {
                return;
            }
                
            DoCommand(_echoConsole, () =>
            {
                var overflow = "";
                while (overflow != null)
                {
                    if (!_lines.ContainsKey(Cursor.Y)) return;
                    var result = _lines[Cursor.Y].WriteToRowBufferReturnWrittenAndOverflow(ForegroundColor, BackgroundColor, Cursor.X, text);
                    overflow = result.Overflow;
                    if (_echo && _echoConsole != null)
                    {
                        _echoConsole.ForegroundColor = ForegroundColor;
                        _echoConsole.BackgroundColor = BackgroundColor;
                        _echoConsole.PrintAt(CursorLeft + _absoluteX, CursorTop + AbsoluteY, result.Written);
                    }
                    if (overflow == null)
                    {
                        Cursor = Cursor.IncX(text.Length);
                    }
                    else
                    {
                        Cursor = new XY(0, Cursor.Y + 1);
                        if (_clipping && OverflowBottom) break;
                        if (OverflowBottom)
                            ScrollDown();
                    }
                    text = overflow;
                }
            });
        }

        public void WriteLine(string format, params object[] args)
        {
            if (_clipping && OverflowBottom)
            {
                return;
            }

            if (OverflowBottom)
            {
                ScrollDown();
                Write(format, args);
                Cursor = new XY(0, Cursor.Y + 1);
                return;
            }

            Write(format, args);
            Cursor = new XY(0, Cursor.Y + 1);
            if (OverflowBottom && !_clipping)
            {
                ScrollDown();
            }
        }

        public void WriteLine(ConsoleColor color, string format, params object[] args)
        {
            var foreground = ForegroundColor;
            try
            {
                ForegroundColor = color;
                WriteLine(format, args);
            }
            finally
            {
                ForegroundColor = foreground;
            }
        }

        public void Write(ConsoleColor color, string format, params object[] args)
        {
            var foreground = ForegroundColor;
            try
            {
                ForegroundColor = color;
                Write(format, args);
            }
            finally
            {
                ForegroundColor = foreground;
            }
;
        }

        public void Write(string format, params object[] args)
        {
            var text = string.Format(format, args);
            Write(text);
        }

        public void Clear()
        {
            Clear(null);
        }

        public void Clear(ConsoleColor? background)
        {
            init(background);
        }

        public virtual void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop,
            char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            if (!_echo) return;
            if (_echoConsole!=null)
                _echoConsole.MoveBufferArea(sourceLeft  + AbsoluteX,sourceTop + AbsoluteY,sourceWidth,sourceHeight,targetLeft + AbsoluteX, targetTop + AbsoluteY, sourceChar, sourceForeColor, sourceBackColor);
                
            else
            {
                throw new Exception("Should never get here, something gone wrong in the logic, possibly in the constructor checks?");
            }

        }

        public void Write(string text)
        {
            _write(text);
        }

        /// <summary>
        /// scroll the screen up 1 line, and pop the top line off the buffer, and fill the bottom 
        /// line of the window with background char and colour
        /// </summary>
        public void ScrollDown()
        {
            for (int i = 0; i < (_height-1); i++)
            {
                _lines[i] = _lines[i+1];
            }
            _lines[_height-1] = new Row(_width, ' ', ForegroundColor, BackgroundColor);
            Cursor = new XY(0, _height-1);
            if (_echoConsole != null)
            {
                _echoConsole.MoveBufferArea(AbsoluteX, AbsoluteY + 1, _width, _height - 1, AbsoluteX, AbsoluteY, ' ', ForegroundColor, BackgroundColor);
            }
        }






        public int WindowHeight
        {
            get
            {
                return _height;
            }
        }

        public int CursorTop
        {
            get { return Cursor.Y; }
            set { Cursor = Cursor.WithY(value); }
        }

        public int CursorLeft
        {
            get { return Cursor.X; }
            set { Cursor = Cursor.WithX(value); }
        }

        public Colors Colors
        {
            get
            {
                return new Colors(ForegroundColor, BackgroundColor);
            }
            set
            {
                ForegroundColor = value.Foreground;
                BackgroundColor = value.Background;
            }
        }

        public int AbsoluteY => _absoluteY;
        public int AbsoluteX => _absoluteX;
        public int WindowWidth => _width;

        public ConsoleColor BackgroundColor { get; set; }

        private bool _noEchoCursorVisible = true;

        public bool CursorVisible
        {
            get { return _echoConsole?.CursorVisible ?? _noEchoCursorVisible; }
            set
            {
                if(_echoConsole==null)
                    _noEchoCursorVisible = value;
                else
                    _echoConsole.CursorVisible = value;
            }
        }



        public ConsoleColor ForegroundColor { get; set; }

        
        /// <summary>
        /// prints text at x and y location, without affecting the current window or parent state
        /// </summary>
        public void PrintAt(int x, int y, string format, params object[] args)
        {
            var text = string.Format(format, args);
            PrintAt(x,y,text);
        }

        public void PrintAt(int x, int y, string text)
        {
            DoCommand(this, () =>
            {
                Cursor = new XY(x, y);
                Write(text);
            });

        }

        public void PrintAtColor(ConsoleColor foreground, int x, int y, string text, ConsoleColor? background = null)
        {
            DoCommand(_echoConsole, () =>
            {
                DoCommand(this, () =>
                {
                    State = new ConsoleState(foreground, background ?? BackgroundColor, y, x, CursorVisible);
                    Write(text);
                });
            });
        }


        public ConsoleState State
        {
            get
            {
                return new ConsoleState(ForegroundColor, BackgroundColor, CursorTop, CursorLeft, _hostSizer?.CursorVisible ?? CursorVisible);                
            }

            set
            {
                CursorLeft = value.Left;
                CursorTop = value.Top;
                ForegroundColor = value.ForegroundColor;
                BackgroundColor = value.BackgroundColor;
                // this might be very slow on every write, don't change this all the time
                if (CursorVisible != value.CursorVisible) CursorVisible = value.CursorVisible;
            }

        }

        public void PrintAt(int x, int y, char c)
        {
            Cursor = new XY(x,y);
            Write(c.ToString());
        }


        /// <summary>
        /// Run command and preserve the state, i.e. restore the console state after running command.
        /// </summary>
        public  void DoCommand(IConsole console, Action action)
        {
            //TODO write test that proves we need to lock right here!
            //lock(_staticLocker)
            if (console == null)
            {
                action();
                return;
            }
            var state = console.State;
            try
            {
                GotoEchoCursor(console);                
                action();
            }
            finally
            {
                console.State = state;
            }
        }

        private void GotoEchoCursor(IConsole console)
        {
            console.CursorTop = _cursor.Y + _y;
            console.CursorLeft = (_cursor.X + _x);
        }

        public void Fill(ConsoleColor color, int sx, int sy, int width, int height)
        {
            DoCommand(this, () =>
            {
                ForegroundColor = color;
                var line = new String(' ', width);
                for (int y = sy; y < height; y++)
                {
                    PrintAt(sx, y, line);
                }
            });
        }

    }
}
