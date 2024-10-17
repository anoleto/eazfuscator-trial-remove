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

        internal static void ProcessDnlib(string assemblyPath)
        {
            Logger.Log($"Processing with dnlib: {assemblyPath}", ConsoleColor.Green);

            string modifiedAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_dnlib{Path.GetExtension(assemblyPath)}");

            var module = dnlib.DotNet.ModuleDefMD.Load(assemblyPath);
            var entryPoint = module.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Logger.LogVerbose($"Entry point method: {entryPoint.FullName} 0x{entryPoint.MDToken}", ConsoleColor.Green);
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
