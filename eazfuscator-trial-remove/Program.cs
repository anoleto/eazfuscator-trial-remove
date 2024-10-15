using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace eaztrialremove
{
    class Program
    {
        static bool v = false; // verbose

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Log("Please drop an assembly or paste assembly path to modify. --v for more output.", ConsoleColor.Red);
                return;
            }

            if (args.Contains("--v"))
                v = true;
                args = args.Where(arg => arg != "--v").ToArray();

            foreach (var assemblyPath in args)
            {
                if (!File.Exists(assemblyPath) || !assemblyPath.EndsWith("exe") && !assemblyPath.EndsWith("dll"))
                {
                    Log($"The specified assembly file {assemblyPath} does not exist.", ConsoleColor.Red);
                    continue;
                }

                Process(assemblyPath);
            }

            Log("Press any key to exit...");
            Console.ReadKey();
        }

        static void Process(string assemblyPath)
        {
            string modifiedAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_{Path.GetExtension(assemblyPath)}");

            LogVerbose($"Original Assembly Path: {assemblyPath}", ConsoleColor.Green);
            LogVerbose($"Modified Assembly Path: {modifiedAssemblyPath}", ConsoleColor.Green);

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var entryPoint = assembly.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            LogVerbose($"Entry point method: {entryPoint.FullName} {entryPoint.MetadataToken}", ConsoleColor.Green);
            LogVerbose($"Total instructions in entry point: {entryPoint.Body.Instructions.Count}", ConsoleColor.Green);

            MethodDefinition calledMethod = entryPoint.Body.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call)
                .Select(instr => instr.Operand as MethodReference)
                .FirstOrDefault()?.Resolve();

            if (calledMethod != null && calledMethod.ReturnType.FullName == "System.Void")
            {
                LogVerbose($"Found called method: {calledMethod.FullName} {calledMethod.MetadataToken}", ConsoleColor.Green);
                LogVerbose($"Total instructions in called method: {calledMethod.Body.Instructions.Count}", ConsoleColor.Green);
                RemoveInstructions(calledMethod.Body);
            }
            else
            {
                LogVerbose("No method call found in the entry point. Trying to remove trial call instructions directly...", ConsoleColor.Cyan);
                RemoveInstructions(entryPoint.Body);
            }

            try
            {
                assembly.Write(modifiedAssemblyPath);
                Log($"Modified assembly saved to: {modifiedAssemblyPath}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Log($"Failed to write assembly: {ex.Message}", ConsoleColor.Red);
                LogVerbose($"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);
                File.Delete(modifiedAssemblyPath);
            }
        }

        static void RemoveInstructions(MethodBody methodBody)
        {
            var ilProcessor = methodBody.GetILProcessor();
            var instructionsToRemove = methodBody.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Brtrue_S || instr.OpCode == OpCodes.Ret)
                .Take(3).ToList();

            foreach (var instruction in instructionsToRemove)
            {
                LogVerbose($"Removing instruction: {instruction.OpCode} at {instruction.Offset} ({instruction})", ConsoleColor.Yellow);
                ilProcessor.Remove(instruction);
            }

            LogVerbose("Instructions successfully removed.", ConsoleColor.Green);
        }

        static void Log(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void LogVerbose(string text, ConsoleColor color)
        {
            if (!v) return;

            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
