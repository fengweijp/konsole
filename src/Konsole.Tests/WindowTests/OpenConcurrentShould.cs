using System;
using FluentAssertions;
using NUnit.Framework;
using static System.ConsoleColor;

namespace Konsole.Tests.WindowTests
{
    class OpenConcurrentShould
    {
        [Test]
        public void open_a_window_with_border_using_default_values()
        {
            var c = new MockConsole(10,5);
            var w = Window.OpenConcurrent(0, 0, 10, 5,"title", LineThickNess.Double, White, Black, c);
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
            var w = Window.OpenConcurrent(0, 0, 8, 6, "title", LineThickNess.Double, White, Black, c);
            w.WindowHeight.Should().Be(4);
            w.WindowWidth.Should().Be(6);
            w.AbsoluteX.Should().Be(1);
            w.AbsoluteY.Should().Be(1);
        }

        [Test]
        public void open_a_window_that_can_be_scrolled()
        {
            var c = new MockConsole(15, 6);
            var w = Window.OpenConcurrent(5, 1, 10, 5, "title", LineThickNess.Double, White, Black, c);
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
            var w = Window.OpenConcurrent(0, 0, 10, 5, "title", thickness, White, Black, c);
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
            var w = Window.OpenConcurrent(0, 0, 10, 4, title, LineThickNess.Double, White, Black, c);

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
            var w = Window.OpenConcurrent(0, 0, 10, 5, "title", LineThickNess.Double, White, Black, c);
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


        [Test]
        public void WhenNested_draw_a_box_around_the_scrollable_window_with_a_centered_title_and_return_a_live_window_at_the_correct_screen_location()
        {
            var con = new MockConsole(20, 8);
            var w = Window.OpenConcurrent(0, 0, 20, 8, "title", LineThickNess.Double, White, Black, con);
            w.WriteLine("line1");
            w.WriteLine("line2");
            var child = Window.OpenConcurrent(7, 2, 8, 4, "c1", LineThickNess.Single, White, Black, w);
            var expected = new[]
            {
                "╔══════ title ═════╗",
                "║line1             ║",
                "║line2             ║",
                "║       ┌─ c1 ─┐   ║",
                "║       │      │   ║",
                "║       │      │   ║",
                "║       └──────┘   ║",
                "╚══════════════════╝"
            };

            con.BufferWritten.Should().BeEquivalentTo(expected);

            child.WriteLine("cats");
            child.Write("dogs");
            expected = new[]
            {
                "╔══════ title ═════╗",
                "║line1             ║",
                "║line2             ║",
                "║       ┌─ c1 ─┐   ║",
                "║       │cats  │   ║",
                "║       │dogs  │   ║",
                "║       └──────┘   ║",
                "╚══════════════════╝"
            };

            con.BufferWritten.Should().BeEquivalentTo(expected);

            // should not interfere with original window cursor position so should still be able to continue writing as 
            // if no new child window had been created.

            w.WriteLine("line3");
            w.WriteLine("line4");

            expected = new[]
{
                "╔══════ title ═════╗",
                "║line1             ║",
                "║line2             ║",
                "║line3  ┌─ c1 ─┐   ║",
                "║line4  │cats  │   ║",
                "║       │dogs  │   ║",
                "║       └──────┘   ║",
                "╚══════════════════╝"
            };

            con.BufferWritten.Should().BeEquivalentTo(expected);
        }


    }
}
