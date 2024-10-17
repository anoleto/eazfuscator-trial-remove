using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace eaztrialremove
{
    class Config
    {
        public bool dnlib { get; private set; } // whether to use dnlib or mono
        public static bool v { get; private set; } // verbose
        public List<string> path { get; private set; } // get path
        public Config() => path = new List<string>();

        public void Parse(string[] args)
        {
            if (args.Contains("--v") || args.Contains("--verbose"))
                v = true;
                args = args.Where(arg => arg != "--v" && arg != "--verbose").ToArray();

            if (args.Contains("--dnlib"))
                dnlib = true;
                args = args.Where(arg => arg != "--dnlib").ToArray();

            path.AddRange(args);
        }

        public void getHelp()
        {
            Logger.Log($"Usage: {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)} assembly.exe", ConsoleColor.DarkCyan);
            Logger.Log("Other argument:", ConsoleColor.DarkCyan);
            Logger.Log("--v || --verbose: Provides more detailed or extra output", ConsoleColor.Cyan);
            Logger.Log("--dnlib: Uses dnlib library instead of Mono.Cecil", ConsoleColor.Cyan);
        }
    }
}
