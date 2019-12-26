using System;
using FluentAssertions;
using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    public class SplitTests
    {

        //TODO: need tests without borders
        public class NestedTests
        {
            [Test]
            public void when_with_border_and_title_singleLine_nesting_split_windows_should_split_correctly()
            {
                var con = new MockConsole(20, 12);

                var left = con.SplitLeft("left");
                var right = con.SplitRight("right");
                var nestedTop = left.SplitTop("ntop");
                var nestedBottom = left.SplitBottom("nbot");

                var expected = new[]
                {
                "┌─ left ─┐┌─ right ┐",
                "│┌ ntop ┐││        │",
                "││      │││        │",
                "││      │││        │",
                "││      │││        │",
                "│└──────┘││        │",
                "│┌ nbot ┐││        │",
                "││      │││        │",
                "││      │││        │",
                "││      │││        │",
                "│└──────┘││        │",
                "└────────┘└────────┘"
                };

                // recieved
                // -------------------
                //┌─ left ─┐┌─ right ┐
                //│┌ ntop ┐││        │
                //││      │││        │
                //││       ││        │
                //││       ││        │
                //│└─      ││        │
                //│┌ nbot ┐││        │
                //││      │││        │
                //││       ││        │
                //││       ││        │
                //│└─      ││        │
                //└────────┘└────────┘

                con.Buffer.Should().BeEquivalentTo(expected);
            }

            [Test]
            public void when_nesting_with_border_and_title_doubleLine_split_windows_should_split_correctly()
            {
                var con = new MockConsole(20, 12);

                var left = con.SplitLeft("left", LineThickNess.Double);
                var right = con.SplitRight("right", LineThickNess.Double);
                var nestedTop = left.SplitTop("ntop", LineThickNess.Double);
                var nestedBottom = left.SplitBottom("nbot", LineThickNess.Double);

                var expected = new[]
                {
                "╔═ left ═╗╔═ right ╗",
                "║╔ ntop ╗║║        ║",
                "║║      ║║║        ║",
                "║║      ║║║        ║",
                "║║      ║║║        ║",
                "║╚══════╝║║        ║",
                "║╔ nbot ╗║║        ║",
                "║║      ║║║        ║",
                "║║      ║║║        ║",
                "║║      ║║║        ║",
                "║╚══════╝║║        ║",
                "╚════════╝╚════════╝"
                };

                // recieved
                // -------------------
                //╔═ left ═╗╔═ right ╗
                //║╔ ntop ╗║║        ║
                //║║      ║║║        ║
                //║║       ║║        ║
                //║║       ║║        ║
                //║╚═      ║║        ║
                //║╔ nbot ╗║║        ║
                //║║      ║║║        ║
                //║║       ║║        ║
                //║║       ║║        ║
                //║╚═      ║║        ║
                //╚════════╝╚════════╝

                con.Buffer.Should().BeEquivalentTo(expected);
            }


            [Test]
            public void when_nesting_without_borders_split_windows_should_split_correctly()
            {
                var con = new MockConsole(20, 12);

                var left = con.SplitLeft();
                var right = con.SplitRight();
                var nestedTop = left.SplitTop();
                var nestedBottom = left.SplitBottom();
                right.WriteLine("**********this-is-the-right-window");
                nestedTop.WriteLine("XXXXXXXXX|top-goes-|here     |");
                nestedBottom.WriteLine("bottom-go|es-here");

                var expected = new[]
                {
                 "XXXXXXXXX|**********",
                 "top-goes-|this-is-th",
                 "here     |e-right-wi",
                 "          ndow      ",
                 "                    ",
                 "                    ",
                 "bottom-go|          ",
                 "es-here             ",
                 "                    ",
                 "                    ",
                 "                    ",
                 "                    "
                };

                con.Buffer.Should().BeEquivalentTo(expected);
            }

        }


        public class LeftRightTests
        {
            [Test]
            [TestCase(1, 19)]
            [TestCase(2, 20)]
            [TestCase(3, 21)]
            public void LeftHalf_and_RightHalf_ShouldFillTheParentConsole(int test, int width)
            {
                // test to show how uneven lines are split between left and right windows.
                var c = new MockConsole(width, 5);
                var left = c.SplitLeft("left");
                var right = c.SplitRight("right");
                left.WriteLine("one");
                left.WriteLine("two");
                left.Write("three");

                right.WriteLine("four");
                right.WriteLine("five");
                right.Write("six");
                Console.WriteLine(c.BufferString);

                var _19Cols = new[]
                {
                    "┌ left ─┐┌─ right ┐",
                    "│one    ││four    │",
                    "│two    ││five    │",
                    "│three  ││six     │",
                    "└───────┘└────────┘"
            };

                var _20Cols = new[]
                {
                    "┌─ left ─┐┌─ right ┐",
                    "│one     ││four    │",
                    "│two     ││five    │",
                    "│three   ││six     │",
                    "└────────┘└────────┘"
            };

                var _21Cols = new[]
                {
                    "┌─ left ─┐┌─ right ─┐",
                    "│one     ││four     │",
                    "│two     ││five     │",
                    "│three   ││six      │",
                    "└────────┘└─────────┘"

            };

                var expecteds = new[]
                {
                _19Cols, _20Cols, _21Cols
            };
                c.Buffer.Should().BeEquivalentTo(expecteds[test - 1]);
            }


            [Test]
            [TestCase(1, 19)]
            [TestCase(2, 20)]
            [TestCase(3, 21)]
            public void LeftHalf_and_RightHalf_WithoutBorder_ShouldFillTheParentConsole(int test, int width)
            {
                // test to show how uneven lines are split between left and right windows.
                var c = new MockConsole(width, 5);
                var left = c.SplitLeft();
                var right = c.SplitRight();
                left.WriteLine("one");
                left.WriteLine("two");
                left.WriteLine("three");

                right.WriteLine("four");
                right.WriteLine("five");
                right.Write("six");
                Console.WriteLine(c.BufferString);

                var _19Cols = new[]
                {
                    "one      four      ",
                    "two      five      ",
                    "three    six       ",
                    "                   ",
                    "                   ",
            };

                var _20Cols = new[]
                {
                    "one       four      ",
                    "two       five      ",
                    "three     six       ",
                    "                    ",
                    "                    "
            };

                var _21Cols = new[]
                {
                    "one       four       ",
                    "two       five       ",
                    "three     six        ",
                    "                     ",
                    "                     ",

            };

                var expecteds = new[]
                {
                _19Cols, _20Cols, _21Cols
            };
                c.Buffer.Should().BeEquivalentTo(expecteds[test - 1]);
            }


            [Test]
            public void WhenScrolling_ShouldScroll()
            {
                // dammit? this is working with them mock console but not the real console????

                var c = new MockConsole(20, 5);
                var left = c.SplitLeft("left");
                var right = c.SplitRight("right");
                left.WriteLine("one");
                left.WriteLine("two");
                left.WriteLine("three");
                left.WriteLine("four");
                // used write here so that last line does not add aditional scroll
                left.Write("five");

                right.WriteLine("foo");
                right.WriteLine("cats");
                right.WriteLine("dogs");
                // last line is already scrolling ie at the bottom of the screen so this adds an additional scroll
                right.WriteLine("dots");
                Console.WriteLine(c.BufferString);
                var expectedParent = new[]
                {
                    "┌─ left ─┐┌─ right ┐",
                    "│three   ││dogs    │",
                    "│four    ││dots    │",
                    "│five    ││        │",
                    "└────────┘└────────┘"
            };

                c.Buffer.Should().BeEquivalentTo(expectedParent);
            }



        }


        public class TopBottomTests
        {
            [Test]
            [TestCase(1, 10)]
            [TestCase(2, 11)]
            [TestCase(3, 12)]
            public void TopHalf_and_BottomHalf_ShouldFillTheParentConsole(int test, int height)
            {
                // test to show how uneven lines are split between top and bottom windows.
                var c = new MockConsole(10, height);
                var top = c.SplitTop("top");
                var bottom = c.SplitBottom("bot");
                top.WriteLine("one");
                top.WriteLine("two");
                top.Write("three");


                bottom.WriteLine("four");
                bottom.WriteLine("five");
                bottom.Write("six");
                Console.WriteLine(c.BufferString);

                var _10Rows = new[]
                {
                "┌── top ─┐",
                "│one     │",
                "│two     │",
                "│three   │",
                "└────────┘",
                "┌── bot ─┐",
                "│four    │",
                "│five    │",
                "│six     │",
                "└────────┘"
            };

                var _11Rows = new[]
                {
                "┌── top ─┐",
                "│one     │",
                "│two     │",
                "│three   │",
                "└────────┘",
                "┌── bot ─┐",
                "│four    │",
                "│five    │",
                "│six     │",
                "│        │",
                "└────────┘"
            };

                var _12Rows = new[]
    {
                "┌── top ─┐",
                "│one     │",
                "│two     │",
                "│three   │",
                "│        │",
                "└────────┘",
                "┌── bot ─┐",
                "│four    │",
                "│five    │",
                "│six     │",
                "│        │",
                "└────────┘"
            };

                var expecteds = new[]
                {
                _10Rows, _11Rows, _12Rows
            };
                c.Buffer.Should().BeEquivalentTo(expecteds[test - 1]);
            }

            [Test]
            public void WhenScrolling_ShouldScroll()
            {
                var c = new MockConsole(10, 10);
                var top = c.SplitTop("top");
                var bottom = c.SplitBottom("bot");
                top.WriteLine("one");
                top.WriteLine("two");
                top.WriteLine("three");
                top.WriteLine("four");
                top.Write("five");

                bottom.WriteLine("cats");
                bottom.WriteLine("dogs");
                bottom.Write("dots");
                Console.WriteLine(c.BufferString);
                var expectedParent = new[]
                {
                    "┌── top ─┐",
                    "│three   │",
                    "│four    │",
                    "│five    │",
                    "└────────┘",
                    "┌── bot ─┐",
                    "│cats    │",
                    "│dogs    │",
                    "│dots    │",
                    "└────────┘",
                };
                c.Buffer.Should().BeEquivalentTo(expectedParent);
            }
        }
    }
}
