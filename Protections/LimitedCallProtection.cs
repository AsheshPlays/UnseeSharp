﻿using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

public class LimitedCallProtection
{
    public static void Process(ModuleDefMD module)
    {
        foreach (TypeDef type in module.Types.ToArray())
        {
            foreach (MethodDef method in type.Methods.ToArray())
            {
                if (method.FullName != module.EntryPoint.FullName)
                {
                    continue;
                }

                if (!method.HasBody)
                {
                    continue;
                }

                if (!method.Body.HasInstructions)
                {
                    continue;
                }

                int steps = 1;

                for (int i = 0; i < method.Body.Instructions.Count - 1; i++)
                {
                    if (method.Body.Instructions[i].ToString().Contains("ISupportInitialize") || method.Body.Instructions[i].OpCode != OpCodes.Call && method.Body.Instructions[i].OpCode != OpCodes.Callvirt && method.Body.Instructions[i].OpCode != OpCodes.Ldloc_S)
                    {
                        continue;
                    }

                    if (method.Body.Instructions[i].ToString().Contains("Object") || method.Body.Instructions[i].OpCode != OpCodes.Call && method.Body.Instructions[i].OpCode != OpCodes.Callvirt && method.Body.Instructions[i].OpCode != OpCodes.Ldloc_S)
                    {
                        continue;
                    }

                    if (steps > 1)
                    {
                        try
                        {
                            MemberRef calliMember = (MemberRef)method.Body.Instructions[i].Operand;
                            method.Body.Instructions[i].OpCode = OpCodes.Calli;
                            method.Body.Instructions[i].Operand = calliMember.MethodSig;
                            method.Body.Instructions.Insert(i, Instruction.Create(OpCodes.Ldftn, calliMember));
                        }
                        catch
                        {

                        }
                    }

                    steps++;
                }
            }
        }
    }
}