using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CALib;
using ConsoleHelper;
using System.Diagnostics;

namespace CAEmulator
{
    public class Program
    {

        public static void Main(string[] args)
        {
            string[] recursiveFileSearch(string pth)
            {
                List<string> files = new List<string>();
                foreach (string s in Directory.EnumerateFiles(pth))
                {
                    try
                    {
                        files.Add(s);
                    }
                    catch
                    {

                    }
                }
                foreach (string s in Directory.EnumerateDirectories(pth))
                {
                    try
                    {
                        files.AddRange(recursiveFileSearch(s));
                    }
                    catch
                    {

                    }
                }
                return files.ToArray();
            }
            ushort flipEndian(ushort input)
            {
                byte[] inst9 = MemoryMarshal.AsBytes<ushort>(new ushort[] { input }).ToArray();
                inst9 = inst9.Reverse().ToArray();
                return MemoryMarshal.Cast<byte, ushort>(inst9)[0];
            }
            ushort flipEndianBytes(byte[] input)
            {
                byte[] inst9 = input.ToArray();
                inst9 = inst9.Reverse().ToArray();
                return MemoryMarshal.Cast<byte, ushort>(inst9)[0];
            }
            ushort[] registers = new ushort[0x100];
            registers[0] = 0x0000;
            Span<byte> registerSpan = MemoryMarshal.AsBytes(registers.AsSpan());
            string filePath;
            bool debug = false;
            if (args.Length > 0)
            {
                filePath = args[0];
                if (args.Length > 1)
                {
                    debug = args[1] == "debug";
                }
            }
            else
            {
                Console.WriteLine(CHelper.ASCIIArt("PEmulator", CHelper.LoadASCIIFont(@"C:\Users\PeterHusman\Documents\FontFile.json")));
                filePath = CHelper.RequestInput("Please enter the file path of the program to emulate.", true, ConsoleColor.Yellow, ConsoleColor.Gray, recursiveFileSearch($@"C:\Users\{Environment.UserName}"));
                debug = CHelper.SelectorMenu("Please choose an extra parameter.", new string[] { "None", "Debug" }, true, ConsoleColor.Yellow, ConsoleColor.Gray, ConsoleColor.Magenta) == 1;
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            if (debug)
            {
                Console.WriteLine("Debug mode entered. Enter a blank line to step through the program and enter Q followed by a register number to query the value of the register.");
            }
            byte[] program = File.ReadAllBytes(filePath);
            Random rand = new Random();
            MemoryMap memoryMap = new MemoryMap(program);
            while (memoryMap[0x0000] == 0)
            {
                if (debug)
                {
                    string s = "a";
                    do
                    {
                        s = Console.ReadLine();
                        if (s.ToLower().StartsWith("q"))
                        {
                            Console.WriteLine($"r{s.Remove(0, 1)} = {registers[int.Parse(s.Remove(0, 1), System.Globalization.NumberStyles.HexNumber)]:X}");
                        }
                    } while (s != "");
                    Span<byte> instr = memoryMap.GetInstruction(registers[0] * 2 + 0x8000);
                    Console.Write($"{((OpCodes)instr[0]).ToString()} {instr[1]:X2} {instr[2]:X2} {instr[3]:X2}");
                }
                memoryMap[0x0003] = (ushort)(rand.NextDouble() * ushort.MaxValue);
                if (memoryMap[0x0002] != 0)
                {
                    memoryMap[0x0004] = ushort.Parse(Console.ReadLine());
                    memoryMap[0x0002] = 0;
                }
                if (memoryMap[0x0008] != 0)
                {
                    Console.WriteLine(memoryMap[0x0007].ToString("X"));
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
                        registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] - 1);
                        break;
                    case OpCodes.jiz:
                        if (registers[instruction[1]] == 0)
                        {
                            registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] - 1);
                        }
                        break;
                    case OpCodes.jnz:
                        if (registers[instruction[1]] != 0)
                        {
                            registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] - 1);
                        }
                        break;
                    case OpCodes.set:
                        registers[instruction[1]] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0]);
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
                        registers[instruction[1]] = memoryMap[(ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0])];
                        break;
                    case OpCodes.str:
                        memoryMap[(ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0])] = registers[instruction[1]];
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
                        registers[0] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] - 1);
                        break;
                    case OpCodes.ret:
                        registers[0] = (ushort)(memoryMap[registers[0x20]]);
                        registers[0x20] += (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] + 1);
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
                        registers[instruction[1]] = (ushort)(memoryMap[registers[instruction[2]] - instruction[3]]);
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
                    case OpCodes.lpm:
                        registers[instruction[1]] = (ushort)(MemoryMarshal.Cast<byte, ushort>(instruction.Slice(2))[0] * 2 + 0x8000);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                registers[0]++;
            }
            if (args.Length <= 0)
            {
                Console.ReadKey();
            }
        }
    }
}
