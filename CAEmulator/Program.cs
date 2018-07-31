using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CALib;

namespace CAEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort[] registers = new ushort[0x100];
            registers[0] = 0x0000;
            Span<byte> registerSpan = MemoryMarshal.AsBytes(registers.AsSpan());
            byte[] program = File.ReadAllBytes(args[0]);
            Random rand = new Random();
            MemoryMap memoryMap = new MemoryMap(program);
            while (memoryMap[0x0000] == 0)
            {
                memoryMap[0x0003] = (ushort)(rand.NextDouble() * ushort.MaxValue);
                if (memoryMap[0x0002] != 0)
                {
                    memoryMap[0x0004] = ushort.Parse(Console.ReadLine());
                    memoryMap[0x0002] = 0;
                }
                if (memoryMap[0x0008] != 0)
                {
                    Console.WriteLine(memoryMap[0x0007]);
                    memoryMap[0x0008] = 0;
                }
                if (memoryMap[0x0006] != 0)
                {
                    Console.Write((char)(memoryMap[0x0005]));
                    memoryMap[0x0006] = 0;
                }




                Span<byte> instruction = memoryMap.GetInstruction(registers[0] * 2 + 0x8000);
                switch ((OpCodes)instruction[0])
                {
                    case OpCodes.nop:
                        break;
                    case OpCodes.add:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] + registers[instruction[3]]);
                        break;
                    case OpCodes.sub:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] - registers[instruction[3]]);
                        break;
                    case OpCodes.mul:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] * registers[instruction[3]]);
                        break;
                    case OpCodes.div:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] / registers[instruction[3]]);
                        break;
                    case OpCodes.mod:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] % registers[instruction[3]]);
                        break;
                    case OpCodes.and:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] & registers[instruction[3]]);
                        break;
                    case OpCodes.orx:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] | registers[instruction[3]]);
                        break;
                    case OpCodes.not:
                        registers[instruction[1]] = (ushort)(~registers[instruction[2]]);
                        break;
                    case OpCodes.xor:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] ^ registers[instruction[3]]);
                        break;
                    case OpCodes.jmp:
                        byte[] inst = instruction.Slice(2).ToArray();
                        inst = inst.Reverse().ToArray();
                        registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(inst)[0] - 1);
                        break;
                    case OpCodes.jiz:
                        if (registers[instruction[1]] == 0)
                        {
                            byte[] inst2 = instruction.Slice(2).ToArray();
                            inst2 = inst2.Reverse().ToArray();
                            registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(inst2)[0] - 1);
                        }
                        break;
                    case OpCodes.jnz:
                        if (registers[instruction[1]] != 0)
                        {
                            byte[] inst3 = instruction.Slice(2).ToArray();
                            inst3 = inst3.Reverse().ToArray();
                            registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(inst3)[0] - 1);
                        }
                        break;
                    case OpCodes.set:
                        byte[] inst7 = instruction.Slice(2).ToArray();
                        inst7 = inst7.Reverse().ToArray();
                        registers[instruction[1]] = (ushort)(MemoryMarshal.Cast<byte, ushort>(inst7)[0]);
                        break;
                    case OpCodes.mov:
                        registers[instruction[1]] = registers[instruction[2]];
                        break;
                    case OpCodes.equ:
                        registers[instruction[1]] = Convert.ToUInt16(registers[instruction[2]] == registers[instruction[3]]);
                        break;
                    case OpCodes.lth:
                        registers[instruction[1]] = Convert.ToUInt16(registers[instruction[2]] < registers[instruction[3]]);
                        break;
                    case OpCodes.leq:
                        registers[instruction[1]] = Convert.ToUInt16(registers[instruction[2]] <= registers[instruction[3]]);
                        break;
                    case OpCodes.lod:
                        byte[] inst5 = instruction.Slice(2).ToArray();
                        inst5 = inst5.Reverse().ToArray();
                        registers[instruction[1]] = memoryMap[(ushort)(MemoryMarshal.Cast<byte, ushort>(inst5)[0])];
                        break;
                    case OpCodes.str:
                        byte[] inst6 = instruction.Slice(2).ToArray();
                        inst6 = inst6.Reverse().ToArray();
                        memoryMap[(ushort)(MemoryMarshal.Cast<byte, ushort>(inst6)[0])] = registers[instruction[1]];
                        break;
                    case OpCodes.psh:
                        registers[0x20]--;
                        memoryMap[registers[0x20]] = registers[instruction[1]];
                        break;
                    case OpCodes.pop:
                        registers[instruction[1]] = memoryMap[registers[0x20]];
                        registers[0x20]++;
                        break;
                    case OpCodes.pek:
                        registers[instruction[1]] = memoryMap[registers[0x20] + (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0])];
                        break;
                    case OpCodes.cal:
                        registers[0x20]--;
                        memoryMap[registers[0x20]] = registers[0];
                        byte[] inst4 = instruction.Slice(2).ToArray();
                        inst4 = inst4.Reverse().ToArray();
                        registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(inst4)[0] - 1);
                        break;
                    case OpCodes.ret:
                        registers[0] = (ushort)(memoryMap[registers[0x20]] - 1);
                        registers[0x20]++;
                        break;
                    case OpCodes.jmi:
                        registers[0] = (ushort)(registers[instruction[3]] - 1);
                        break;
                    case OpCodes.jzi:
                        if (registers[instruction[1]] == 0)
                        {
                            registers[0] = (ushort)(registers[instruction[3]] - 1);
                        }
                        break;
                    case OpCodes.jni:
                        if (registers[instruction[1]] != 0)
                        {
                            registers[0] = (ushort)(registers[instruction[3]] - 1);
                        }
                        break;
                    case OpCodes.sti:
                        memoryMap[registers[instruction[3]]] = registers[instruction[1]];
                        break;
                    case OpCodes.ldi:
                        registers[instruction[1]] = memoryMap[registers[instruction[2]] - instruction[3]];
                        break;
                    case OpCodes.cli:
                        registers[0x20]--;
                        memoryMap[registers[0x20]] = registers[0];
                        registers[0] = (ushort)(registers[instruction[3]] - 1);
                        break;
                    case OpCodes.shl:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] << registers[instruction[3]]);
                        break;
                    case OpCodes.shr:
                        registers[instruction[1]] = (ushort)(registers[instruction[2]] >> registers[instruction[3]]);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                registers[0]++;
            }
            Console.ReadKey();
        }
    }
}
