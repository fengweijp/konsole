using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Konsole.Drawing;
using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    class OpenConcurrentShould
    {
        [Test]
        public void open_a_window_with_border_using_default_values()
        {
            var c = new MockConsole(10,5);
            var w = Window.OpenConcurrent(0, 0, 10, 5,"title", LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black, c);
            w.WriteLine("one");
            w.WriteLine("two");
            w.Write("three");
            Console.WriteLine(c.BufferString);
            var expected = new[]
            {
                "╔═ title ╗",
                "║one     ║",
                "║two     ║",
                "║three   ║",
                "╚════════╝"
            };
            c.BufferWritten.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void return_an_inside_scrollable_window_that_exactly_fits_inside_the_box_with_the_title()
        {
            var c = new MockConsole(10, 8);
            var w = Window.OpenConcurrent(0, 0, 8, 6, "title", LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black, c);
            w.WindowHeight.Should().Be(4);
            w.WindowWidth.Should().Be(6);
            w.AbsoluteX.Should().Be(1);
            w.AbsoluteY.Should().Be(1);
        }

        [Test]
        public void open_a_window_that_can_be_scrolled()
        {
            var c = new MockConsole(15, 6);
            var w = Window.OpenConcurrent(5, 1, 10, 5, "title", LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black, c);
            w.WriteLine("one");
            w.WriteLine("two");
            w.WriteLine("three");
            w.WriteLine("four");
            var expected = new[]
            {
                "               ",
                "     ╔═ title ╗",
                "     ║three   ║",
                "     ║four    ║",
                "     ║        ║",
                "     ╚════════╝"
            };

            var actual = c.BufferWritten;
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        [TestCase(LineThickNess.Double)]
        [TestCase(LineThickNess.Single)]
        public void draw_a_box_around_the_scrollable_window(LineThickNess thickness)
        {
            var c = new MockConsole(10, 8);
            var w = Window.OpenConcurrent(0, 0, 10, 5, "title", thickness, ConsoleColor.White, ConsoleColor.Black, c);
            var expected = new string[0];
            switch (thickness)
            {
                case LineThickNess.Single:
                    expected = new[]
                    {
                        "┌─ title ┐",
                        "│        │",
                        "│        │",
                        "│        │",
                        "└────────┘"
                    };
                    break;
                case LineThickNess.Double:
                    expected = new[]
                    {
                        "╔═ title ╗",
                        "║        ║",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;

            }
            c.BufferWritten.Should().BeEquivalentTo(expected);
        }

        [Test]
        [TestCase("title")]
        [TestCase("titles")]
        [TestCase("catsandDogs is a long title")]
        [TestCase(null)]
        [TestCase("")]
        public void text_should_be_centered_or_clipped(string title)
        {
            var c = new MockConsole(10, 4);
            var w = Window.OpenConcurrent(0, 0, 10, 4, title, LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black, c);

            string[] expected  = new string[0];
            switch (title)
            {
                case "title":
                    expected = new[]
                    {
                        "╔═ title ╗",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;
                case "catsandDogs is a long title":
                    expected = new[]
                    {
                        "╔ catsand╗",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;

                case "titles":
                    expected = new[]
                    {
                        "╔ titles ╗",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;
                case null:
                    expected = new[]
                    {
                        "╔════════╗",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;

                case "":
                    expected = new[]
                    {
                        "╔════════╗",
                        "║        ║",
                        "║        ║",
                        "╚════════╝"
                    };
                    break;
            }
            c.BufferWritten.Should().BeEquivalentTo(expected);
        }


        [Test]
        public void draw_a_box_around_the_scrollable_window_with_a_centered_title_()
        {
            var c = new MockConsole(10, 8);
            var w = Window.OpenConcurrent(0, 0, 10, 5, "title", LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black, c);
            var expected = new[]
            {
                "╔═ title ╗",
                "║        ║",
                "║        ║",
                "║        ║",
                "╚════════╝"
            };
            c.BufferWritten.Should().BeEquivalentTo(expected);
        }


    }
}
