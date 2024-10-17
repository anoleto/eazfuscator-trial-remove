using System;
using System.IO;
using System.Linq;

namespace eaztrialremove
{
    class Program
    {
        static Config configHandler = new Config();
        static void Main(string[] args)
        {
            configHandler.Parse(args);
            if (args.Contains("--h") || args.Contains("--help")) { configHandler.getHelp(); return; }

            if (configHandler.path.Count == 0)
            {
                Logger.Log("Please drop an assembly or paste assembly path to modify. add --h arg to get help", ConsoleColor.Red);
                return;
            }

            foreach (var assemblyPath in configHandler.path)
            {
                if (!File.Exists(assemblyPath) || !assemblyPath.EndsWith("exe") && !assemblyPath.EndsWith("dll"))
                {
                    Logger.Log($"The specified assembly file {assemblyPath} does not exist. add --h arg to get help", ConsoleColor.Red);
                    continue;
                }

                if (configHandler.dnlib) ProcessAsm.ProcessDnlib(assemblyPath); else ProcessAsm.ProcessMono(assemblyPath);
            }

            Logger.Log("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
