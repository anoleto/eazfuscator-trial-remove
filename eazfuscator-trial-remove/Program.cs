using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace eaztrialremove
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine("Please drop an assembly or paste assembly path to modify."); return; }

            string assemblyPath = args[0];

            if (!File.Exists(assemblyPath)) { Console.WriteLine("The specified assembly file does not exist."); return; }

            string modifiedAssemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Path.GetFileNameWithoutExtension(assemblyPath)}_{Path.GetExtension(assemblyPath)}");
            Console.WriteLine($"Original Assembly Path: {assemblyPath}\nModified Assembly Path: {modifiedAssemblyPath}");

            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var entryPoint = assembly.EntryPoint ?? throw new InvalidOperationException("No entry point found in the assembly.");

            Console.WriteLine($"Entry point method: {entryPoint.FullName}");

            MethodDefinition calledMethod = entryPoint.Body.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call)
                .Select(instr => instr.Operand as MethodReference)
                .FirstOrDefault()?.Resolve();

            if (calledMethod != null) { Console.WriteLine($"Found called method: {calledMethod.FullName}"); RemoveInstructions(calledMethod.Body); } // BRO
            else Console.WriteLine("No method call found in the entry point.");

            try { assembly.Write(modifiedAssemblyPath); Console.WriteLine($"Modified assembly saved to: {modifiedAssemblyPath}"); }
            catch (Exception ex) { Console.WriteLine($"Failed to write assembly: {ex.Message}"); }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void RemoveInstructions(MethodBody methodBody)
        {
            var ilProcessor = methodBody.GetILProcessor();
            var instructionsToRemove = methodBody.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Brtrue_S || instr.OpCode == OpCodes.Ret)
                .Take(3).ToList();

            foreach (var instruction in instructionsToRemove)
            {
                Console.WriteLine($"Removing instruction: {instruction.OpCode}");
                ilProcessor.Remove(instruction);
            }
        }
    }
}
