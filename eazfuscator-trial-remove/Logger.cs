using System;

namespace eaztrialremove
{
    internal class Logger
    {
        private static void WriteLine(string prefix, string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{prefix}{text}");
            Console.ResetColor();
        }
        public static void Log(string text, ConsoleColor color = ConsoleColor.White) => WriteLine("[L]", text, color);
        public static void LogVerbose(string text, ConsoleColor color)
        {
            if (!Config.v) return;

            WriteLine("[V]", text, color);
        }
    }
}
