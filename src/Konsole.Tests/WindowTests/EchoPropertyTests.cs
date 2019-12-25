using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    public class EchoPropertyTests
    {
        [Test]
        public void When_WriteLine_SHOULD_translate_wrapped_lines_to_parent()
        {
            var parent = new MockConsole(4, 5);
            var window = new Window(1, 1, 2, 3, parent);
            window.WriteLine("12345");

            var expected = new[]    // what we're currently getting ?? bizarrely
            {
                "    ",             // " 1  ",
                " 34 ",             // " 32 ",
                " 5  ",             // "  4 ",
                "    ",             // " 5  "
                "    "              // 
            };
            Assert.AreEqual(expected, parent.Buffer);
        }

        [Test]
        public void When_Write_SHOULD_translate_wrapped_lines_to_parent()
        {
            var parent = new MockConsole(4, 5);
            var window = new Window(1, 1, 2, 3, parent);
            window.Write("12345");

            var expected = new[]
            {
                    "    ",
                    " 12 ",
                    " 34 ",
                    " 5  ",
                    "    "
                };
            Assert.AreEqual(expected, parent.Buffer);
        }


        [Test]
        public void translate_all_writes_to_the_parent()
        {
            // the only reason this test is so important, because it's how we simulate writing to the real Console.
            var parent = new MockConsole(4, 4);
            var window = new Window(1, 1, 2, 2, parent);
            window.WriteLine("12");
            window.Write("34");

            var expected = new[]
            {
                "    ",
                " 12 ",
                " 34 ",
                "    "
            };
            Assert.AreEqual(expected, parent.Buffer);
        }

        [Test]
        public void EchoFalseShould_not_translate_all_writes_to_the_parent()
        {
            // the only reason this test is so important, because it's how we simulate writing to the real Console.
            var parent = new MockConsole(6, 4);
            parent.WriteLine("one");
            parent.WriteLine("two");
            parent.WriteLine("three");
            var window = new Window(new Settings(parent) { X = 0, Y = 1, Height = 2, Echo = false });
            window.WriteLine("12");
            window.Write("34");

            var expected = new[]
            {
                "one   ",
                "two   ",
                "three ",
                "      ",
                };
            Assert.AreEqual(expected, parent.Buffer);
        }

        [Test]
        public void EchoTrueShould2_translate_all_writes_to_the_parent()
        {
            // the only reason this test is so important, because it's how we simulate writing to the real Console.
            var parent = new MockConsole(6, 4);
            parent.WriteLine("one");

            var expected = new[]
            {
                    "one   ",
                    "      ",
                    "      ",
                    "      ",
                    };
            Assert.AreEqual(expected, parent.Buffer);

            var window = new Window(parent, 2);
            window.WriteLine("12");
            window.Write("34");


            expected = new[]
            {
                    "one   ",
                    "12    ",
                    "34    ",
                    "      ",
                    };

            Assert.AreEqual(expected, parent.Buffer);
        }
    }
}