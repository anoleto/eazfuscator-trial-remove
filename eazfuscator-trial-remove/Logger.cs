using System;

namespace eaztrialremove
{
    internal class Logger
    {
        public static void Log(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[L]{text}");
            Console.ResetColor();
        }

        public static void LogVerbose(string text, ConsoleColor color)
        {
            if (!Config.v) return;

            Console.ForegroundColor = color;
            Console.WriteLine($"[V]{text}");
            Console.ResetColor();
        }
    }
}
