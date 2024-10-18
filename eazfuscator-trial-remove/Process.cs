using dnlib.DotNet;
using System;
using System.IO;

namespace eaztrialremove
{
    internal class ProcessAsm
    {
        static instructionHandler instructionHandler = new instructionHandler();

        internal static void ProcessMono(string assemblyPath)
        {
            Logger.Log($"Processing with Mono.Cecil: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Config.overwrite ? assemblyPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_cecil{Path.GetExtension(assemblyPath)}");
            var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyPath);
            var entryPoint = assembly.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Logger.LogVerbose($"Entry point method: {entryPoint.FullName} {entryPoint.MetadataToken}", ConsoleColor.Green);
            instructionHandler.RemoveInstrMono(entryPoint.Body);

            WriteMono(assembly, modifiedAssemblyPath);
        }

        internal static void ProcessDnlib(string assemblyPath)
        {
            Logger.Log($"Processing with dnlib: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Config.overwrite ? assemblyPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_dnlib{Path.GetExtension(assemblyPath)}");
            var module = ModuleDefMD.Load(assemblyPath);
            var entryPoint = module.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Logger.LogVerbose($"Entry point method: {entryPoint.FullName} 0x{entryPoint.MDToken}", ConsoleColor.Green);
            instructionHandler.RemoveInstrDnlib(entryPoint.Body);

            WriteDnlib(module, modifiedAssemblyPath);
        }

        internal static void ProcessLegacy(string assemblyPath)
        {
            Logger.Log($"Processing with dnlib: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Config.overwrite ? assemblyPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_l{Path.GetExtension(assemblyPath)}");
            var module = ModuleDefMD.Load(assemblyPath);

            instructionHandler.legacyinstrRemove(module);

            WriteDnlib(module, modifiedAssemblyPath);
        }

        private static void WriteMono(Mono.Cecil.AssemblyDefinition assembly, string path) => Write(() => assembly.Write(getTmp(path)), path);
        private static void WriteDnlib(ModuleDefMD module, string path) => Write(() => module.Write(getTmp(path)), path);
        private static string getTmp(string path) => Path.Combine(Path.GetTempPath(), $"{Path.GetFileName(path)}.tmp");

        private static void Write(Action writeAction, string path)
        {
            if (!Config.writeAsm) return;

            string tmp = getTmp(path);

            try
            {
                writeAction();
                if (Config.overwrite) File.Replace(tmp, path, null);
                else
                {
                    if (File.Exists(path)) File.Delete(path);
                    File.Move(tmp, path);
                }

                Logger.Log($"Modified assembly saved to: {path}", ConsoleColor.Green);
            }
            catch (Exception ex) { HandleErr(tmp, ex); }
        }

        private static void HandleErr(string path, Exception ex)
        {
            Logger.Log($"Failed to write assembly: {ex.Message}", ConsoleColor.Red);
            Logger.LogVerbose($"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
