using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CALib;
using ConsoleHelper;

namespace AssemblerEmulatorWeek
{
    enum Operation
    {
        assemble = 0,
        dissassemble = 1,
        passemble = 2
    }
    class Program
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
                    catch (Exception e)
                    {

                    }
                }
                foreach (string s in Directory.EnumerateDirectories(pth))
                {
                    try
                    {
                        files.AddRange(recursiveFileSearch(s));
                    }
                    catch (Exception e)
                    {

                    }
                }
                return files.ToArray();
            }
            //CHelper.ASCIIArt("Hello World!", Console.ReadLine()));
            Operation op;
            string path;
            string pathOut;
            string csvPath;
            if (args.Length >= 4)
            {
                path = args[0];
                pathOut = args[1];
                csvPath = args[2];
                op = (Operation)Enum.Parse(typeof(Operation), args[3], true);
            }
            else
            {
                Console.WriteLine(CHelper.ASCIIArt("PASM ASSEMBLER", CHelper.LoadASCIIFont(@"C:\Users\PeterHusman\Documents\FontFile.json")));//, @"C:\Users\PeterHusman\Documents\FontFile.json"));
                Console.WriteLine(@"                                  _     _           
     /\                          | |   | |          
    /  \   ___ ___  ___ _ __ ___ | |__ | | ___ _ __ 
   / /\ \ / __/ __|/ _ \ '_ ` _ \| '_ \| |/ _ \ '__|
  / ____ \\__ \__ \  __/ | | | | | |_) | |  __/ |   
 /_/    \_\___/___/\___|_| |_| |_|_.__/|_|\___|_|  ");
                int choice = CHelper.SelectorMenu("Please select an action.", new string[] { "Assemble", "Dissassemble", "Pseudoassemble" }, true, ConsoleColor.Yellow, ConsoleColor.Gray, ConsoleColor.Magenta);
                Console.WriteLine();
                string[] recFiles = recursiveFileSearch($@"C:\Users\{Environment.UserName}");
                path = CHelper.RequestInput("What is the input file path?", true, ConsoleColor.Yellow, ConsoleColor.Gray, recFiles);
                pathOut = CHelper.RequestInput("What is the output file path?", true, ConsoleColor.Yellow, ConsoleColor.Gray, recFiles);
                csvPath = CHelper.RequestInput("What is the .csv file path?", true, ConsoleColor.Yellow, ConsoleColor.Gray, recFiles);
                op = (Operation)choice;
            }
            var csv = GetOpCodesFromCSV(csvPath);
            switch (op)
            {
                case Operation.assemble:
                    Assemble(path, csv, pathOut, true);
                    break;
                case Operation.dissassemble:
                    Dissassemble(path, csv, pathOut);
                    break;
                case Operation.passemble:
                    HelperToAsm(path, pathOut);
                    break;
            }
            //var csv2 = GetOpCodesFromCSV(csvPath);
            //HelperToAsm(path, pathOut);
            //Assemble(pathOut, csv, pathOut, true);

            //string[] lines = File.ReadAllLines(path);
            //Dictionary<string, uint> labels = new Dictionary<string, uint>();
            //List<byte> output = new List<byte>();
            //for(int i = 0; i < lines.Length; i++)
            //{
            //    lines[i] = lines[i].Split(';')[0];
            //    lines[i] = lines[i].Replace("r", "");
            //    if(lines[i].EndsWith(":"))
            //    {
            //        labels.Add(lines[i].Remove(lines[i].Length-1,1), (uint)i);
            //    }
            //}
            //for(int i = 0; i < lines.Length; i++)
            //{

            //}
            //File.WriteAllBytes(pathOut, output.ToArray());
            //Console.ReadKey();
        }

        public static void Dissassemble(string fileInPath, Dictionary<string, int[]> opCodes, string fileOutPath)
        {
            byte[] bytes = File.ReadAllBytes(fileInPath);
            string[] outLines = new string[bytes.Length / 4];
            int ptr = 0;
            Span<byte> instructions = (bytes.AsSpan());
            for (int i = 0; i < bytes.Length / 4; i++)
            {
                ptr = 4 * i;
                for (int j = 0; j < (opCodes.Values.ToArray().Length); j++)
                {
                    if (instructions[ptr] == opCodes.Values.ToArray()[j][0])
                    {
                        outLines[i] = opCodes.Keys.ToArray()[j];
                        int ptrIncr = ptr + 1;
                        for (int k = 1; k < opCodes.Values.ToArray()[j].Length; k++)
                        {
                            switch (opCodes.Values.ToArray()[j][k])
                            {
                                case -1:
                                    ptrIncr++;
                                    break;
                                case 0:
                                    ptrIncr++;
                                    break;
                                case 1:
                                    outLines[i] += " " + instructions[ptrIncr].ToString("X2");
                                    ptrIncr++;
                                    break;
                                case 2:
                                    byte[] inst = instructions.Slice(ptrIncr, 2).ToArray();
                                    inst = inst.Reverse().ToArray();
                                    outLines[i] += " " + MemoryMarshal.Cast<byte, ushort>(inst.AsSpan())[0].ToString("X5");
                                    ptrIncr++;
                                    break;
                            }
                        }
                        break;
                    }
                }
            }
            File.WriteAllLines(fileOutPath, outLines);
        }


        public static void HelperToAsm(string fileInPath, string fileOutPath)
        {
            string[] lines = File.ReadAllLines(fileInPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] subParams = lines[i].Split(' ');
                if (subParams.Length < 2)
                {
                    continue;
                }
                if (subParams[1] == "=")
                {
                    if (subParams.Length <= 3)
                    {
                        switch (subParams[2])
                        {
                            default:
                                if (subParams[0].StartsWith("*r"))
                                {
                                    lines[i] = $"sti {subParams[2]} {subParams[0].Remove(0, 1)}";
                                }
                                else if (subParams[0].StartsWith("*"))
                                {
                                    lines[i] = $"str {subParams[2]} {subParams[0].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("*r"))
                                {
                                    lines[i] = $"ldi {subParams[0]} {subParams[2].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("*"))
                                {
                                    lines[i] = $"lod {subParams[0]} {subParams[2].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("r"))
                                {
                                    lines[i] = $"mov {subParams[0]} {subParams[2]}";
                                }
                                else
                                {
                                    lines[i] = $"set {subParams[0]} {subParams[2]}";
                                }
                                break;
                        }

                        continue;
                    }
                    switch (subParams[3])
                    {
                        case "+":
                            lines[i] = $"add {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "-":
                            lines[i] = $"sub {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "*":
                            lines[i] = $"mul {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "/":
                            lines[i] = $"div {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                        case "%":
                            lines[i] = $"mod {subParams[0]} {subParams[2]} {subParams[4]}";
                            break;
                    }
                }
            }
            File.WriteAllLines(fileOutPath, lines);
        }

        public static Dictionary<string, int[]> GetOpCodesFromCSV(string filePath)
        {
            Dictionary<string, int[]> output = new Dictionary<string, int[]>();
            Dictionary<string, int> phraseToBytes = new Dictionary<string, int>() { ["DEST REG"] = 1, ["REG TO CHECK"] = 1, ["SRC REG"] = 1, ["SRC1"] = 1, ["SRC2"] = 1, ["CONS"] = 2, ["ADD"] = 2, ["SRC ADD"] = 2, ["DEST ADD"] = 2, ["0"] = -1, ["NUM TO POP"] = 2, ["OFF"] = 2, ["PTR REG"] = 1, ["OFFSET"] = 1 };
            string[] rows = File.ReadAllLines(filePath);
            for (int k = 0; k < rows.Length; k++)
            {
                string[] cells = rows[k].Split(',');
                ushort opCode = 0;
                int[] columns;
                bool success = false;
                try
                {
                    opCode = ushort.Parse(cells[1], NumberStyles.HexNumber);
                    success = true;
                }
                catch
                {

                }
                if (success)
                {
                    columns = new int[4];
                    columns[0] = opCode;
                    for (int i = 0; i < 3; i++)
                    {
                        columns[1 + i] = phraseToBytes[cells[4 + i]];
                        if (columns[i + 1] == 2)
                        {
                            columns[i + 2] = 0;
                            break;
                        }
                    }
                    output.Add(cells[0], columns);
                }
            }
            return output;
        }

        public static void Assemble(string filePathIn, Dictionary<string, int[]> opCodes, string filePathOut, bool outputAsBytes)
        {
            string[] lines = File.ReadAllText(filePathIn).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            uint[] outlines = new uint[lines.Length];
            Dictionary<string, ushort> labels = new Dictionary<string, ushort>();
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Split(';')[0];
                lines[i] = lines[i].Replace("\r", "");
                lines[i] = lines[i].Trim(' ');
                if (lines[i].EndsWith(":"))
                {
                    labels.Add(lines[i].Remove(lines[i].Length - 1), (ushort)i);
                    lines[i] = "nop";
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace(",", "");
                foreach (string label in labels.Keys.ToArray())
                {
                    lines[i] = lines[i].Replace(label, labels[label].ToString("X"));
                }
                string[] subParams = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (subParams.Length <= 0)
                {
                    continue;
                }
                for (int k = 1; k < subParams.Length; k++)
                {
                    subParams[k] = subParams[k].Replace("r", "");
                }
                foreach (string o in opCodes.Keys.ToArray())
                {
                    if (subParams[0] == o)
                    {
                        int[] cols = opCodes[o];
                        outlines[i] = (uint)cols[0];
                        int subParamsI = 1;
                        bool brk = false;
                        for (int j = 1; j <= 3; j++)
                        {
                            outlines[i] = (uint)(outlines[i] << 8);
                            switch (cols[j])
                            {
                                case -1:
                                    break;
                                case 0:
                                    brk = true;
                                    break;
                                case 1:
                                    outlines[i] += uint.Parse(subParams[subParamsI], NumberStyles.HexNumber);
                                    subParamsI++;
                                    break;
                                case 2:
                                    outlines[i] = (uint)(outlines[i] << 8);
                                    outlines[i] += uint.Parse(subParams[subParamsI], NumberStyles.HexNumber);
                                    subParamsI++;
                                    brk = true;
                                    break;


                            }
                            if (brk)
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (!outputAsBytes)
            {
                string[] outputLines = new string[outlines.Length];
                for (int i = 0; i < outlines.Length; i++)
                {
                    outputLines[i] = outlines[i].ToString("X8");
                }
                File.WriteAllLines(filePathOut, outputLines);
            }
            else
            {
                byte[] outputBytes = new byte[outlines.Length * 4];
                for (int i = 0; i < outlines.Length; i++)
                {
                    outputBytes[i * 4] = (byte)(outlines[i] >> 24);
                    outputBytes[i * 4 + 1] = (byte)(outlines[i] >> 16);
                    outputBytes[i * 4 + 2] = (byte)(outlines[i] >> 8);
                    outputBytes[i * 4 + 3] = (byte)(outlines[i]);
                }
                File.WriteAllBytes(filePathOut, outputBytes);
            }
            //File.WriteAllText(filePathOut, allText);
        }
    }
}
