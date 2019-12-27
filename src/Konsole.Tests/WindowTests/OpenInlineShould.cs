using FluentAssertions;
using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    public class OpenInlineShould
    {
        [Test]
        public void set_cursor_position_to_below_the_window()
        {
            var con = new MockConsole(10, 4);
            con.WriteLine("line1");
            var win = Window.OpenInline(con, 2);
            //var win = new Window(con, 2);
            win.WriteLine("moo");
            win.WriteLine("cats");
            win.WriteLine("dogs");
            win.WriteLine("fruit");
            con.Write("line2");
            var expected = new[]
            {
                "line1     ",
                "fruit     ",
                "line2     ",
                "          "

            };
            con.Buffer.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void clip_the_height_to_fit_within_parent()
        {
            var c = new MockConsole(10, 10);
            c.CursorLeft = 5;
            c.CursorTop = 5;
            var w = Window.OpenInline(c, 10);
            w.WindowHeight.Should().Be(5);
        }

        [Test]
        public void use_the_full_screen_width_if_no_width_provided_and_move_cursor_of_host_to_below_inline_window_and_reset_x_position_to_left()
        {
            var c = new MockConsole(12, 10);
            c.CursorLeft = 5;
            c.CursorTop = 5;
            var w = Window.OpenInline(c, 2);
            w.WindowWidth.Should().Be(12);
            c.CursorTop.Should().Be(7);
            c.CursorLeft.Should().Be(0);
        }


        //[Test]
        //[TestCase(0, 0, 4, 10, 4)]
        //[TestCase(0, 0, 5, 10, 5)]
        //[TestCase(0, 0, 15, 10, 10)] // clip the height to 10
        //[TestCase(0, 5, 15, 10, 10)] // clip the width to 5, clip the height to 10
        //[TestCase(5, 0, 15, 5, 10)] // clip the width to 5 and clip the height to 10
        //public void use_balance_of_parent_height_and_width_as_defaults(int parentCurrentX, int parentCurrentY, int heightRows, int expectedWidth, int expectedHeight)
        //{
        //    var c = new MockConsole(10, 10);
        //    c.CursorLeft = parentCurrentX;
        //    c.CursorTop = parentCurrentY;
        //    var w = Window.OpenInline(c, heightRows);
        //    w.WindowWidth.Should().Be(expectedWidth);
        //    w.WindowHeight.Should().Be(expectedHeight);
        //}


    }
}
