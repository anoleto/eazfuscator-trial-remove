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

            if (configHandler.path.Count == 0)
            {
                configHandler.getHelp();
                return;
            }

            foreach (var assemblyPath in configHandler.path)
            {
                if (!File.Exists(assemblyPath) || !assemblyPath.EndsWith("exe") && !assemblyPath.EndsWith("dll"))
                {
                    configHandler.getHelp();
                    continue;
                }

                if (configHandler.dnlib) ProcessAsm.ProcessDnlib(assemblyPath); else ProcessAsm.ProcessMono(assemblyPath);
            }

            Logger.Log("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
