using FluentAssertions;
using NUnit.Framework;
using System;

namespace Konsole.Tests.WindowTests
{
    public class SplitColumnsShould
    {

        [Test]
        public void return_one_console_per_split_with_size_matching_split()
        {
            var con = new MockConsole(30, 4);
            var window = new Window(con);
            var consoles = window.SplitColumns(
                    new Split(8, "col1", LineThickNess.Single),
                    new Split(10, "col2", LineThickNess.Single),
                    new Split(12, "col3", LineThickNess.Single)
            );

            var col1 = consoles[0];
            var col2 = consoles[1];
            var col3 = consoles[2];

            col1.Write("my headline");
            col2.Write("content goes here");
            col3.Write("I get clipped & wrap");

            var expected = new[]
            {
                    "┌ col1 ┐┌─ col2 ─┐┌── col3 ──┐",
                    "│my hea││goes her││I get clip│",
                    "│dline ││e       ││ped & wrap│",
                    "└──────┘└────────┘└──────────┘"
            };

            con.Buffer.Should().BeEquivalentTo(expected);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WhenNotUsingABorder_validate_the_requested_column_size(bool wildcard)
        {
            var con = new MockConsole(30, 4);

            var cons = con.SplitColumns(
                    new Split(8),
                    wildcard ? new Split(0) : new Split(10),
                    new Split(12)
            );
            cons[0].WindowWidth.Should().Be(8);
            cons[1].WindowWidth.Should().Be(10);
            cons[2].WindowWidth.Should().Be(12);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WhenUsingABorder_validate_the_requested_column_size(bool wildcard)
        {
            var con = new MockConsole(30, 4);
            var col2 = wildcard ? new Split(0, "col2", LineThickNess.Single) : new Split(10, "col2", LineThickNess.Single);
            var cons = con.SplitColumns(
                    new Split(8, "col1", LineThickNess.Single),
                    col2,
                    new Split(12, "col3", LineThickNess.Single)
            );
            // widths are 2 chars narrower because there's a border
            cons[0].WindowWidth.Should().Be(6);
            cons[1].WindowWidth.Should().Be(8);
            cons[2].WindowWidth.Should().Be(10);
        }


        [Test]
        public void throw_OutOfRangeException_WhenNoWildCardAndColumnsTooSmall()
        {
            var con = new MockConsole(30, 4);
            IConsole[] cons;
            Action action = () => cons = con.SplitColumns(
                    new Split(12, "col1", LineThickNess.Single),
                    new Split(12, "col2", LineThickNess.Single)
            );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void throw_OutOfRangeException_WhenNoWildCardAndColumnsTooLarge()
        {
            var con = new MockConsole(30, 4);
            IConsole[] cons;
            Action action = () => cons = con.SplitColumns(
                    new Split(18, "col1", LineThickNess.Single),
                    new Split(18, "col2", LineThickNess.Single)
            );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }


        [Test]
        public void WhenSplitIsStart_return_one_console_per_numbered_input_and_one_console_for_the_zero_containing_the_balance()
        {
            var con = new MockConsole(30, 4);
            var window = new Window(con);
            var consoles = window.SplitColumns(
                    new Split(10, "col1", LineThickNess.Single),
                    new Split(0, "col2", LineThickNess.Single),
                    new Split(10, "col3", LineThickNess.Single)
            );

            var col1 = consoles[0];
            var col2 = consoles[1];
            var col3 = consoles[2];

            col1.Write("my headline");
            col2.Write("content goes here");
            col3.Write("I get clipped & wrap.");

            var expected = new[]
            {
                    "┌─ col1 ─┐┌─ col2 ─┐┌─ col3 ─┐",
                    "│my headl││goes her││ipped & │",
                    "│ine     ││e       ││wrap.   │",
                    "└────────┘└────────┘└────────┘"
            };

            con.Buffer.Should().BeEquivalentTo(expected);
        }

        // min  width should be 3
        // heading should be clipped 

    }
}
