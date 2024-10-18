using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;

namespace eaztrialremove
{
    class Config
    {
        [ConfigHandler("Uses dnlib library instead of Mono.Cecil", "dnlib")]
        public bool dnlib { get; private set; } // whether to use dnlib or mono
        [ConfigHandler("Provides more detailed or extra output", "v", "verbose")]
        public static bool v { get; private set; } // verbose
        [ConfigHandler("Old way to remove/disable the trial check. should work with dlls too (haven't tested)", "l", "old")]
        public bool legacy { get; private set; }
        [ConfigHandler("Overwrite filename when writing", "ov", "overwrite")]
        public static bool overwrite { get; set; }
        public static bool writeAsm { get; set; }
        public List<string> path { get; private set; } // get path
        public Config() => path = new List<string>();

        public void Parse(string[] args)
        {
            var arg = new List<string>(args);

            foreach (var option in getOption())
            {
                foreach (var a in option.Attribute.Arg)
                {
                    if (arg.Remove(a))
                    {
                        if (option.Property.PropertyType == typeof(bool)) option.Property.SetValue(this, true);
                        break;
                    }
                }
            }

            path.AddRange(arg);
        }

        public void getHelp()
        {
            Logger.Log($"Usage: {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)} assembly.exe || assembly.dll", ConsoleColor.DarkCyan);
            Logger.Log("Other argument:", ConsoleColor.DarkCyan);

            foreach (var option in getOption()) Logger.Log($"{string.Join(" || ", option.Attribute.Arg)}: {option.Attribute.Desc}", ConsoleColor.Cyan);
        }

        private IEnumerable<dynamic> getOption()
        {
            return GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(prop => Attribute.IsDefined(prop, typeof(ConfigHandler)))
                .Select(prop => new
                {
                    Attribute = (ConfigHandler)Attribute.GetCustomAttribute(prop, typeof(ConfigHandler)),
                    Property = prop
                });
        }
    }
}
