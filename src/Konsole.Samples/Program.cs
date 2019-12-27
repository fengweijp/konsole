using System;

namespace Konsole.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var w = Window.OpenConcurrent(0, 0, 10, 5, "title", LineThickNess.Double, ConsoleColor.White, ConsoleColor.Black);
            w.WriteLine("one");
            w.WriteLine("two");
            w.WriteLine("three");
            w.WriteLine("four");

            using (var writer = new HighSpeedWriter())
            {

                var window = new Window(writer);
                Diagnostics.SelfTest.Test(window, () => writer.Flush());
            }
        }
    }
}
