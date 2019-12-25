﻿using System;
using FluentAssertions;
using Konsole.Tests.Helpers;
using NUnit.Framework;
using static System.ConsoleColor;

namespace Konsole.Tests.WindowTests
{
    public class PrintAtShould
    {
        [Test]
        public void print_relative_to_the_window_being_printed_to_not_the_parent()
        {
            var c = new MockConsole(6,4);
            //c.WriteLine("aaaaaa");
            //c.WriteLine("bbbbbb");
            //c.WriteLine("cccccc");
            //c.Write("dddddd");

            var w = new Window(c, 1, 1, 4, 2);

            w.PrintAt(0,0,"X");
            w.PrintAt(1,1,"Y");
            var expected = new[]
            {
                "      ",
                " X    ",
                "  Y   ",
                "      "
            };
            Console.WriteLine(c.BufferWrittenString);
            c.Buffer.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Not_change_cursor_position_when_printing()
        {
            var c = new MockConsole(5, 3);
            c.PrintAt(4, 1, "O");
            c.WriteLine("one");

            var expected = new[]
            {
                "one  ",
                "    O",
                "     "
            };
            Assert.AreEqual(expected, c.Buffer);
        }


        [Test]
        public void print_the_text_at_the_required_x_y_coordinate()
        {
            var console = new MockConsole(5, 5);
            console.PrintAt(0, 0, "*");
            console.PrintAt(2, 2, "*");
            console.PrintAt(4, 4, "*");
            // all lines are trimmed
            var trimmed = new[]
            {
                "*",
                "",
                "  *",
                "",
                "    *",
            };

            var buffer = new[]
            {
                "*    ",
                "     ",
                "  *  ",
                "     ",
                "    *"
            };
            Assert.That(console.Buffer, Is.EqualTo(buffer));
        }


        [Test]
        public void overflow_any_overflow_text_to_next_line()
        {
            var console = new MockConsole(5, 5);
            console.PrintAt(2, 2, "12345");

            var expected = new[]
            {
                "     ",
                "     ",
                "  123",
                "45   ",
                "     "
            };
            Assert.AreEqual(expected, console.Buffer);
        }


        [Test]
        public void print_to_the_parent()
        {
            var console = new MockConsole(5, 3);
            console.ForegroundColor = ConsoleColor.Red;
            console.BackgroundColor = ConsoleColor.White;
            console.PrintAt(0, 0, "X");
            // if the window was not transparent, then this window would overwrite (blank out) the 'X' just printed above, and test would fail.
            // setting the window to transparent, keeps the underlying text visible. (showing through all non printed areas).
            var w = new Window(new Settings(console){ Transparent = true });

            w.PrintAt(3, 1, "123");

            var expectedAfter = new[]
            {
                "X    ",
                "   12",
                "3    "
            };

            Assert.AreEqual(expectedAfter, console.Buffer);
        }


        [Test]
        public void echo_printing_to_parent_in_the_right_fore_and_back_colors()
        {
            var console = new MockConsole(3, 3);
            console.ForegroundColor = ConsoleColor.Red;
            console.BackgroundColor = ConsoleColor.White;

            var expectedBefore1 = new[]
            {
                " wk wk wk",
                " wk wk wk",
                " wk wk wk"
            };

            Precondition.Check(() => expectedBefore1.Should().BeEquivalentTo(console.BufferWithColor));

            console.PrintAt(0, 0, "X");

            var expectedBefore2 = new[]
            {
                "Xrw wk wk",
                " wk wk wk",
                " wk wk wk"
            };

            Precondition.Check( ()=> expectedBefore2.Should().BeEquivalentTo(console.BufferWithColor));

            var w = new Window(new Settings(){
                EchoConsole = console,
                Transparent = true,
                Foreground = DarkGreen,
                Background = DarkCyan
            });
            w.PrintAt(1, 1, "YY");

            var expectedAfter = new[]
            {
                "Xrw wk wk",
                " wkYGCYGC",
                " wk wk wk"
            };

            Assert.AreEqual(expectedAfter, console.BufferWithColor);
        }


        [Test]
        public void not_change_the_parent_state()
        {
            var console = new MockConsole(3, 3);
            console.WriteLine("X");
            var state = console.State;

            var w = new Window(new Settings()
            {
                EchoConsole = console,
                Foreground = DarkGreen,
                Background = DarkCyan
            });
            w.PrintAt(2, 2, "Y");
            console.State.Should().BeEquivalentTo(state);
        }




    }
}
