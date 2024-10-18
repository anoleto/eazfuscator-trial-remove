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
        public bool legacy { get; private set; }
        public static bool overwrite { get; set; }
        public static bool writeAsm { get; set; }
        public List<string> path { get; private set; } // get path
        public Config() => path = new List<string>();

        public void Parse(string[] args)
        {
            var arg = new List<string>(args);

            if (arg.Remove("--v") || arg.Remove("--verbose"))
                v = true;

            if (arg.Remove("--dnlib"))
                dnlib = true;

            if (arg.Remove("--l"))
                legacy = true;

            if (arg.Remove("--ov") || arg.Remove("--overwrite"))
                overwrite = true;

            path.AddRange(arg);
        }

        public void getHelp()
        {
            Logger.Log($"Usage: {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)} assembly.exe || assembly.dll", ConsoleColor.DarkCyan);
            Logger.Log("Other argument:", ConsoleColor.DarkCyan);
            Logger.Log("--v || --verbose: Provides more detailed or extra output", ConsoleColor.Cyan);
            Logger.Log("--dnlib: Uses dnlib library instead of Mono.Cecil", ConsoleColor.Cyan);
            Logger.Log("--l: Old way to remove/disable the trial check. should work with dlls too (haven't tested)", ConsoleColor.Cyan);
            Logger.Log("--ov || --overwrite: Overwrite filename when writing", ConsoleColor.Cyan);
        }
    }
}
