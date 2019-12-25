using System;
using static System.ConsoleColor;

namespace Konsole
{
    public class Settings
    {
        public Settings() {
        // these are the default settings when you create a new Window() without a console
        // it's not a mock console so it's echo = true, and echoConsole = null which will cause echo all actions to the writer not to echoConsole! TBC.

            // X = 0
            // Y = 0
            // Width = 1000
            // Height = 1000
            // White
            // Black
            // Echo = true
            // echConsole = null
        }

        public Settings(IConsole echoConsole, int x, int y)
        {
            X = x;
            Y = y;
            EchoConsole = echoConsole;
        }

        public Settings(IConsole echoConsole)
        {
            EchoConsole = echoConsole;
        }

        public Settings(int x, int y, int width, int height, IConsole echoConsole)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Echo = true;
            EchoConsole = echoConsole;
        }

        public Settings(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background, bool echo, IConsole echoConsole)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Foreground = foreground;
            Background = background;
            Echo = echo;
            EchoConsole = echoConsole;
        }

        public Settings(int x, int y, int width, int height, ConsoleColor foreground, ConsoleColor background, bool echo, IConsole echoConsole, bool transparent, bool clipping, bool scrolling)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Foreground = foreground;
            Background = background;
            Echo = echo;
            EchoConsole = echoConsole;
            Transparent = transparent;
            Clipping = clipping;
            Scrolling = scrolling;
        }

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        /// <summary>
        /// if no width is provided then the host's width is used. The default is 1000 which will cause the width to be clipped by the host.
        /// provided you do not create a window largers than 300 x 300
        /// </summary>
        public int Width { get; set; } = 300;

        /// <summary>
        /// if no height is provided then the host's height is used. The default is 1000 which will cause the height to be clipped by the host.
        /// provided you do not create a window largers than 300 x 300
        /// </summary>
        public int Height { get; set; } = 1000;
        public ConsoleColor Foreground { get; set; } = White;
        public ConsoleColor Background { get; set; } = Black;
        public bool Echo { get; set; } = true;
        public IConsole EchoConsole { get; set; } = null;
        public bool Transparent { get; set; } = false;

        /// <summary>
        /// Is this an inline window. i.e. a small window that lives side by side (inline) with the existing system console. host console
        /// Cursor is moved below the newly created window so that the existing thread can continue. Useful for two short parallel tasks,
        /// or one parallel backgroun task that will take an unknown time, that we need to report data about.
        /// </summary>
        public bool Inline { get; set; } = false;

        private bool _clipping = false;
        public bool Clipping { 
            get { return _clipping; }
            set { _clipping = value; _scrolling = !value; }
        }

        private bool _scrolling = true;
        public bool Scrolling { 
            get { return _scrolling; }
            set { _scrolling = value; _clipping = !value; }
        } 

        public IHostSizer HostSizer { get; set; }

        /// <summary>
        /// this is set to true by the mock console so that we can tell when we need to get the host consolesize from the operating system directly.
        /// each window only knows about it's container's size (height and width) via the "echoconsole" property 
        /// except for a root window that has no parent container. There are two ways a root window can be created, either as 
        /// mockConsole or as a simple parentless window() that doesnt take an Iconsole as a property.
        /// </summary>
        public bool isMockConsole { get; set; } = false;

        public bool IsRealRoot
        {
            get
            {
                return Echo && EchoConsole == null;
            }
        }

        public void Validate()
        {
            if (Clipping && Scrolling)
            {
                throw new ArgumentOutOfRangeException("Cannot specify Clipping as well as Scrolling; pick 1, or leave both out. Scrolling is default.");
            }
        }
    }
}
