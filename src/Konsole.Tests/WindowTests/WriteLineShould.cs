﻿using System;
using FluentAssertions;
using NUnit.Framework;
using static System.ConsoleColor;

namespace Konsole.Tests.WindowTests
{
    class WriteLineShould
    {



        [Test]
        public void write_relative_to_the_window_being_printed_to_not_the_parent()
        {
            var c = new MockConsole(6, 4);
            c.WriteLine("------");
            c.WriteLine("------");
            c.WriteLine("------");
            c.Write("------");
            var w = new Window(new Settings(1, 1, 4, 2, c) { Transparent = true });
            w.WriteLine("X");
            w.Write("Y");
            var expected = new[]
            {
                "------",
                "-X----",
                "-Y----",
                "------"
            };
            Console.WriteLine(c.BufferWrittenString);
            c.Buffer.Should().BeEquivalentTo(expected);
        }


        [Test]
        public void write_using_the_currently_set_fore_and_background_colors()
        {
            var console = new MockConsole(3, 3);
            console.ForegroundColor = ConsoleColor.Red;
            console.BackgroundColor = ConsoleColor.White;
            console.PrintAt(0, 0, "X");

            var expectedBefore = new[]
            {
                "Xrw wk wk",
                " wk wk wk",
                " wk wk wk"
            };

            Assert.AreEqual(expectedBefore, console.BufferWithColor);

            var w = new Window(new Settings(1, 1, 2, 2, console) { Foreground = Yellow, Background = Black, Transparent = true });
            w.WriteLine("Y");
            w.Write("Z");

            var expectedAfter = new[]
            {
                "Xrw wk wk",
                " wkYyk wk",
                " wkZyk wk"
            };

            Assert.AreEqual(expectedAfter, console.BufferWithColor);
        }

        [Test]
        public void increment_cursortop_position_of_calling_window()
        {
            var console = new MockConsole(80, 20);
            Assert.AreEqual(0, console.CursorTop);
            console.WriteLine("line1");
            Assert.AreEqual(1, console.CursorTop);
            console.Write("This ");
            Assert.AreEqual(1, console.CursorTop);
            console.Write("is ");
            Assert.AreEqual(1, console.CursorTop);
            console.WriteLine("a test line.");
            Assert.AreEqual(2, console.CursorTop);
            console.WriteLine("line 3");
            Assert.AreEqual(3, console.CursorTop);
        }

        [Test]
        public void not_change_state_of_parent_console()
        {
            var parent = new MockConsole(80, 20);
            var state = parent.State;

            var console = new Window(parent);

            console.WriteLine("This");
            state.Should().BeEquivalentTo(parent.State);
        }

    }
}
