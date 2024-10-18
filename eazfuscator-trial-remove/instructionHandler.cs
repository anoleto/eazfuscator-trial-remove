using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;

namespace eaztrialremove
{
    class instructionHandler
    {
        internal void RemoveInstrMono(Mono.Cecil.Cil.MethodBody methodBody) // entrypoint call check
        {
            var calledMethod = methodBody.Instructions
                .Where(instr => instr.OpCode == Mono.Cecil.Cil.OpCodes.Call)
                .Select(instr => instr.Operand as Mono.Cecil.MethodReference)
                .FirstOrDefault()?.Resolve();

            if (calledMethod != null && calledMethod.ReturnType.FullName == "System.Void")
            {
                Logger.LogVerbose($"Found method: {calledMethod.FullName} {calledMethod.MetadataToken}", ConsoleColor.Cyan);
                Logger.LogVerbose($"Total instructions in called method: {calledMethod.Body.Instructions.Count}", ConsoleColor.Cyan);
                rvMono(calledMethod.Body);
            }
            else
            {
                Logger.Log("No method call found. Removing trial call instructions directly...", ConsoleColor.Blue);
                rvMono(methodBody);
            }
        }

        internal void RemoveInstrDnlib(CilBody body)
        {
            var calledMethod = body.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call)
                .Select(instr => instr.Operand as dnlib.DotNet.MethodDef)
                .FirstOrDefault();

            if (calledMethod != null && calledMethod.ReturnType.FullName == "System.Void")
            {
                Logger.LogVerbose($"Found method: {calledMethod.FullName} 0x{calledMethod.MDToken}", ConsoleColor.Cyan);
                Logger.LogVerbose($"Total instructions in called method: {calledMethod.Body.Instructions.Count}", ConsoleColor.Cyan);
                rvDnlib(calledMethod.Body);
            }
            else
            {
                Logger.Log("No method call found. Removing trial call instructions directly...", ConsoleColor.Blue);
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
            Config.writeAsm = true;
        }

        private void rvDnlib(CilBody body)
        {
            var instructionsToRemove = body.Instructions
                .Where(instr => instr.OpCode == OpCodes.Call ||
                                instr.OpCode == OpCodes.Brtrue_S ||
                                instr.OpCode == OpCodes.Ret)
                .Take(3).ToList();

            foreach (var instruction in instructionsToRemove)
            {
                Logger.LogVerbose($"Removing instruction: {instruction.OpCode} at {instruction.Offset} ({instruction})", ConsoleColor.Yellow);
                body.Instructions.Remove(instruction);
            }

            Logger.Log("Instructions successfully removed.", ConsoleColor.Green);
            Config.writeAsm = true;
        }

        internal static void legacyinstrRemove(ModuleDefMD module)
        {
            var types = module.GetTypes().ToList();
            bool Found = false;

            foreach (var type in types)
            {
                if (!type.IsClass || !type.IsAbstract || !type.IsSealed || !type.IsNotPublic) continue; //Logger.LogVerbose($"Type {type.FullName} != internal static class.", ConsoleColor.Red);

                // != 4 i think for older version of eaz
                if (!type.HasMethods || type.Methods.Count != 7) {
                    Logger.LogVerbose($"Class {type.FullName} doesn't have 7 methods, found: {type.Methods.Count}.", ConsoleColor.Red);
                    continue;
                } else { Logger.LogVerbose($"Class check matched, {type.FullName} 0x{type.MDToken}", ConsoleColor.Cyan); }

                var methods = type.Methods.ToList();

                foreach (var method in methods)
                {
                    if (!method.HasBody) continue;
                    var instr = method.Body.Instructions;

                    if (instr.Count >= 3 && 
                        instr[instr.Count - 3].OpCode == OpCodes.Ldc_I4_0 &&
                        instr[instr.Count - 2].OpCode == OpCodes.Call && 
                        instr[instr.Count - 1].OpCode == OpCodes.Ret)
                    {
                        Logger.LogVerbose($"{method.FullName} 0x{method.MDToken} matched instruction.", ConsoleColor.Cyan);
                        instr.Clear();

                        instr.Add(OpCodes.Ldc_I4_1.ToInstruction()); Logger.LogVerbose("Adding Ldc.i4.1 instruction", ConsoleColor.Yellow);
                        instr.Add(OpCodes.Ret.ToInstruction()); Logger.LogVerbose("Adding Ret instruction", ConsoleColor.Yellow);

                        Logger.Log($"Instructions successfully initialized", ConsoleColor.Green);
                        Found = true;
                        Config.writeAsm = true;
                        break;
                    } else { Logger.LogVerbose($"Instructions not matched, {method.FullName} has {instr.Count} instructions.", ConsoleColor.Red); }
                }

                if (Found) return;
            }

            Logger.Log("Failed to change instructions!", ConsoleColor.Red);
        }
    }
}
