using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Feli.AntiSizeofMutations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Anti sizeof mutations, by Feli (https://github.com/01-Feli/AntiSizeofMutations)");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: drag and drop you file with sizeof mutations");
                Console.ReadKey();
                return;
            }

            string path = args[0].Replace("\"", "");

            if (!File.Exists(path))
            {
                Console.WriteLine("The file: " + path + " doesn't exist");
                Console.ReadKey();
                return;
            }

            int sizeofCount = 0;

            ModuleDef module = ModuleDefMD.Load(path);

            foreach (var type in module.GetTypes())
            {
                foreach (var method in type.Methods)
                {
                    // Checks if the method has a body
                    if (!method.HasBody)
                        continue;

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var instruction = method.Body.Instructions[i];

                        // check if it is sizeof opCode
                        if (instruction.OpCode != OpCodes.Sizeof)
                            continue;

                        //gets the value of the sizeof using the operand type
                        int value = GetSize(instruction.Operand.ToString());

                        //here i replace the opCode to the int opCode and i set the new value
                        instruction.OpCode = OpCodes.Ldc_I4;
                        instruction.Operand = value;

                        method.Body.Instructions.OptimizeMacros();
                        sizeofCount++;
                    }
                }
            }

            module.Write(path + ".removedSizeof.exe");

            Console.WriteLine("Replaced " + sizeofCount + " mutations");
            Console.ReadKey();
        }

        static int GetSize(string tipo)
        {
            switch (tipo)
            {
                case "System.SByte":
                    return 1;
                case "System.Boolean":
                    return 1;
                case "System.Byte":
                    return 1;
                case "System.Int16":
                    return 2;
                case "System.Int32":
                    return 4;
                case "System.Single":
                    return 4;
                case "System.Int64":
                    return 8;
                case "System.Decimal":
                    return 16;
            }
            return 0;
        }
    }
}
