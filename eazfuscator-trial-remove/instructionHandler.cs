using dnlib.DotNet.Emit;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace eaztrialremove
{
    class instructionHandler
    {
        public void RemoveInstrMono(Mono.Cecil.Cil.MethodBody methodBody) // entrypoint call check
        {
            var calledMethod = methodBody.Instructions
                .Where(instr => instr.OpCode == Mono.Cecil.Cil.OpCodes.Call)
                .Select(instr => instr.Operand as Mono.Cecil.MethodReference)
                .FirstOrDefault()?.Resolve();

            if (calledMethod != null && calledMethod.ReturnType.FullName == "System.Void")
            {
                Logger.LogVerbose($"Found method: {calledMethod.FullName}", ConsoleColor.Green);
                Logger.LogVerbose($"Total instructions in called method: {calledMethod.Body.Instructions.Count}", ConsoleColor.Green);
                rvMono(calledMethod.Body);
            }
            else
            {
                Logger.Log("No method call found. Removing trial call instructions directly...", ConsoleColor.Cyan);
                rvMono(methodBody);
            }
        }

        public void RemoveInstrDnlib(CilBody body)
        {
            var calledMethod = body.Instructions
                .Where(instr => instr.OpCode == dnlib.DotNet.Emit.OpCodes.Call)
                .Select(instr => instr.Operand as dnlib.DotNet.MethodDef)
                .FirstOrDefault();

            if (calledMethod != null && calledMethod.ReturnType.FullName == "System.Void")
            {
                Logger.LogVerbose($"Found method: {calledMethod.FullName}", ConsoleColor.Green);
                Logger.LogVerbose($"Total instructions in called method: {calledMethod.Body.Instructions.Count}", ConsoleColor.Green);
                rvDnlib(calledMethod.Body);
            }
            else
            {
                Logger.Log("No method call found. Removing trial call instructions directly...", ConsoleColor.Cyan);
                rvDnlib(body);
            }
        }

        private void rvMono(Mono.Cecil.Cil.MethodBody methodBody) // now this two the method that removes the trial call
        {
            var ilProcessor = methodBody.GetILProcessor();
            var instructionsToRemove = methodBody.Instructions
                .Where(instr => instr.OpCode == Mono.Cecil.Cil.OpCodes.Call ||
                                instr.OpCode == Mono.Cecil.Cil.OpCodes.Brtrue_S ||
                                instr.OpCode == Mono.Cecil.Cil.OpCodes.Ret)
                .Take(3).ToList();

            foreach (var instruction in instructionsToRemove)
            {
                Logger.LogVerbose($"Removing instruction: {instruction.OpCode} at {instruction.Offset} ({instruction})", ConsoleColor.Yellow);
                ilProcessor.Remove(instruction);
            }

            Logger.Log("Instructions successfully removed.", ConsoleColor.Green);
        }

        private void rvDnlib(CilBody body)
        {
            var instructionsToRemove = body.Instructions
                .Where(instr => instr.OpCode == dnlib.DotNet.Emit.OpCodes.Call ||
                                instr.OpCode == dnlib.DotNet.Emit.OpCodes.Brtrue_S ||
                                instr.OpCode == dnlib.DotNet.Emit.OpCodes.Ret)
                .Take(3).ToList();

            foreach (var instruction in instructionsToRemove)
            {
                Logger.LogVerbose($"Removing instruction: {instruction.OpCode} at {instruction.Offset} ({instruction})", ConsoleColor.Yellow);
                body.Instructions.Remove(instruction);
            }

            Logger.Log("Instructions successfully removed.", ConsoleColor.Green);
        }
    }
}
