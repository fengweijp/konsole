using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    class ConstructorsShould
    {
        // Need test for default constructor when passing a size provider


        [Test]
        /// <summary>
        /// An "INLINE" (non floating window) is a window that does not have a top and left property set
        /// and will be created at the current cursor y + 1, and left set to 0
        /// and the cursor should be moved to below the newly created window
        /// </summary>
        public void WhenCreatingInlineWindows_cursor_should_be_moved_to_below_the_newly_created_window()
        {
            IConsole _window;
            IConsole _inline;

            _window = new MockConsole(20,6);
            _window.WriteLine("line1");
            _window.Write("1234");
            Assert.AreEqual(1, _window.CursorTop);
            Assert.AreEqual(4, _window.CursorLeft);
            // create an inline window by only specifying a width and a height.
            _inline = new Window(_window,5,2);
            Assert.AreEqual(3, _window.CursorTop);
            Assert.AreEqual(0, _window.CursorLeft);
            _window.WriteLine("foo");
        }

        [Test]
        public void not_allow_start_x_y_values_outside_of_parent_window()
        {
            Assert.Inconclusive();
        }


        [Test]
        public void not_allow_negative_values()
        {
            Assert.Inconclusive("new requirements");
        }

        [Test]
        public void Not_change_parent_state()
        {
            var c = new MockConsole();
            var state = c.State;

            var w1 = new Window(c);
            state.Should().BeEquivalentTo(c.State);

            var w2 = new Window(c, 0, 0);
            state.Should().BeEquivalentTo(c.State);

            var w3 = new Window(0,0,10,10,c);
            state.Should().BeEquivalentTo(c.State);
        }

        [Test]
        [TestCase(0, 0, 10, 10)]
        [TestCase(5, 5, 10, 10)]
        [TestCase(0, 5, 10, 10)]
        [TestCase(5, 0, 10, 10)]
        public void when_no_values_set_should_use_parent_whole_screen_defaults_and_set_x_y_to_0_0(int parentCurrentX, int parentCurrentY, int expectedWidth, int expectedHeight)
        {
            var c = new MockConsole(10,10);
            c.CursorLeft = parentCurrentX;
            c.CursorTop = parentCurrentY;
            var w = new Window(c);
            w.WindowWidth.Should().Be(expectedWidth);
            w.WindowHeight.Should().Be(expectedHeight);
            w.CursorLeft.Should().Be(0);
            w.CursorTop.Should().Be(0);
        }

        [Test]
        public void clip_child_window_to_not_exceed_parent_boundaries()
        {
            var c = new MockConsole(20, 10);
            var w2 = new Window(c, 10, 5, 20, 10, ConsoleColor.Red, ConsoleColor.White);
            Assert.AreEqual(10, w2.WindowWidth);
            Assert.AreEqual(5, w2.WindowHeight);
        }

        [Test]
        public void set_correct_height_and_width()
        {
            var c = new MockConsole(20, 20);
            var w = new Window(c, 10, 8, 6, 4);
            w.WindowWidth.Should().Be(6);
            w.WindowHeight.Should().Be(4);
        }

        [Test]
        public void not_change_host_cursor_position()
        {
            var c = new MockConsole(20, 20);
            var w = new Window(c, 10, 8, 6, 4);
            c.CursorLeft.Should().Be(0);
            c.CursorTop.Should().Be(0);
        }

        [Test]
        public void offset_the_new_window()
        {
            var c = new MockConsole(20, 20);
            var w = new Window(c, 10, 8, 6, 4);
            w.AbsoluteX.Should().Be(10);
            w.AbsoluteY.Should().Be(8);
        }

        [Test]
        public void set_scrolling_if_specified()
        {
            var c = new MockConsole();
            var w = new Window(c, 10, 10);
            Assert.True(w.Scrolling);
            Assert.False(w.Clipping);
        }

        [Test]
        public void set_clipping_if_specified()
        {
            var c = new MockConsole();
            var w = new Window(new Settings(c) { Clipping = true });
            Assert.True(w.Clipping);
            Assert.False(w.Scrolling);
        }

        [Test]
        public void set_scrolling_as_default_if_nothing_specified()
        {
            var c = new MockConsole();
            var w = new Window(new Settings(c));
            Assert.True(w.Scrolling);
            Assert.False(w.Clipping);
        }

    }
}
