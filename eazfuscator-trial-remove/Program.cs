using System;
using System.IO;

namespace eaztrialremove
{
    class Program
    {
        static Config configHandler = new Config();
        static instructionHandler instructionHandler = new instructionHandler();
        static void Main(string[] args)
        {
            configHandler.Parse(args);

            if (configHandler.Path.Count == 0)
            {
                Logger.Log("Please drop an assembly or paste assembly path to modify. --v for more output. --dnlib to use dnlib instead of mono.", ConsoleColor.Red);
                return;
            }

            foreach (var assemblyPath in configHandler.Path)
            {
                if (!File.Exists(assemblyPath) || !assemblyPath.EndsWith("exe") && !assemblyPath.EndsWith("dll"))
                {
                    Logger.Log($"The specified assembly file {assemblyPath} does not exist.", ConsoleColor.Red);
                    continue;
                }

                if (configHandler.dnlib) ProcessDnlib(assemblyPath); else ProcessMono(assemblyPath);
            }

            Logger.Log("Press any key to exit...");
            Console.ReadKey();
        }

        // shoulda make this two to a new class so it look lil clean
        static void ProcessMono(string assemblyPath)
        {
            Logger.LogVerbose($"Processing with Mono.Cecil: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_cecil{Path.GetExtension(assemblyPath)}");

            var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
            var entryPoint = assembly.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Logger.LogVerbose($"Entry point method: {entryPoint.FullName} {entryPoint.MetadataToken}", ConsoleColor.Green);
            instructionHandler.RemoveInstrMono(entryPoint.Body);

            try
            {
                assembly.Write(modifiedAssemblyPath);
                Logger.Log($"Modified assembly saved to: {modifiedAssemblyPath}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write assembly: {ex.Message}", ConsoleColor.Red);
                Logger.LogVerbose($"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);
                File.Delete(modifiedAssemblyPath);
            }
        }

        static void ProcessDnlib(string assemblyPath)
        {
            Logger.LogVerbose($"Processing with dnlib: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_dnlib{Path.GetExtension(assemblyPath)}");

            var module = dnlib.DotNet.ModuleDefMD.Load(assemblyPath);
            var entryPoint = module.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Logger.LogVerbose($"Entry point method: {entryPoint.FullName} {entryPoint.MDToken}", ConsoleColor.Green);
            instructionHandler.RemoveInstrDnlib(entryPoint.Body);

            try
            {
                module.Write(modifiedAssemblyPath);
                Logger.Log($"Modified assembly saved to: {modifiedAssemblyPath}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to write assembly: {ex.Message}", ConsoleColor.Red);
                Logger.LogVerbose($"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);
                File.Delete(modifiedAssemblyPath);
            }
        }
    }
}
