using System.Collections.Generic;
using System.Linq;

namespace eaztrialremove
{
    class Config
    {
        public bool dnlib { get; private set; } // whether use dnlib or mono
        public static bool v { get; private set; } // verbose
        public List<string> Path { get; private set; }
        public Config() => Path = new List<string>();

        public void Parse(string[] args)
        {
            if (args.Contains("--v"))
                v = true;
                args = args.Where(arg => arg != "--v").ToArray();

            if (args.Contains("--dnlib"))
                dnlib = true;
                args = args.Where(arg => arg != "--dnlib").ToArray();

            Path.AddRange(args);
        }
    }
}
