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
using Newtonsoft.Json;

namespace CAAssembler
{
    enum Operation
    {
        assemble = 0,
        dissassemble = 1,
        passemble = 2,
        fullassemble = 3
    }
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
                Console.WriteLine(CHelper.ASCIIArt("PAssembler", CHelper.LoadASCIIFont(@"C:\Users\PeterHusman\Documents\FontFile.json")));//, @"C:\Users\PeterHusman\Documents\FontFile.json"));
                                                                                                                                          //               Console.WriteLine(@"                                  _     _           
                                                                                                                                          //    /\                          | |   | |          
                                                                                                                                          //   /  \   ___ ___  ___ _ __ ___ | |__ | | ___ _ __ 
                                                                                                                                          //  / /\ \ / __/ __|/ _ \ '_ ` _ \| '_ \| |/ _ \ '__|
                                                                                                                                          // / ____ \\__ \__ \  __/ | | | | | |_) | |  __/ |   
                                                                                                                                          ///_/    \_\___/___/\___|_| |_| |_|_.__/|_|\___|_|  ");
                int choice = CHelper.SelectorMenu("Please select an action.", new string[] { "Assemble", "Dissassemble", "Pseudoassemble", "Pseudoassemble and assemble" }, true, ConsoleColor.Yellow, ConsoleColor.Gray, ConsoleColor.Magenta);
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
                case Operation.fullassemble:
                    HelperToAsm(path, pathOut);
                    Assemble(pathOut, csv, pathOut, true);
                    break;
            }
            if (args.Length < 4)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Finished");
                Console.ReadKey(true);
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
            bool progmem = false;
            Span<byte> instructions = (bytes.AsSpan());
            for (int i = 0; i < bytes.Length / 4; i++)
            {
                ptr = 4 * i;
                if (instructions[ptr] == 0xFF && instructions[ptr + 1] == 0xFF && instructions[ptr + 2] == 0xFF && instructions[ptr + 3] == 0xFF)
                {
                    progmem = true;
                    outLines[i] = "PROGMEM";
                }
                else if (progmem)
                {
                    byte[] subset = instructions.Slice(ptr, 4).ToArray();
                    subset = subset.Reverse().ToArray();
                    outLines[i] = MemoryMarshal.Cast<byte, uint>(subset)[0].ToString("X4");
                }
                else
                {
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
                                        outLines[i] += " " + MemoryMarshal.Cast<byte, ushort>(instructions.Slice(ptrIncr, 2))[0].ToString("X5");
                                        ptrIncr++;
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            File.WriteAllLines(fileOutPath, outLines);
        }


        public static void HelperToAsm(string fileInPath, string fileOutPath)
        {
            int lineNumberForEndOfScope(string[] lns, int startingLine)
            {
                int scope = 0;
                for (int j = startingLine; j < lns.Length; j++)
                {
                    if (lns[j].StartsWith("{"))
                    {
                        scope++;
                    }
                    else if (lns[j].StartsWith("}"))
                    {
                        scope--;
                        if (scope == 0)
                        {
                            return j;
                        }
                    }
                }
                return -1;
            }
            string[] lines = File.ReadAllLines(fileInPath);
            Dictionary<string, ushort> variables = new Dictionary<string, ushort>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("="))
                {
                    string s = lines[i].Split('=')[0];
                    uint temp = 0;
                    if (!uint.TryParse(s.Remove(0, 1), NumberStyles.HexNumber, CultureInfo.CurrentCulture.NumberFormat, out temp))
                    {
                        variables.Add(s.Remove(0,1), (ushort)(variables.Count + 128));
                    }
                }
            }
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Split(';')[0].Trim(' ');
                string[] subParams = lines[i].Split(' ');
                if (subParams.Length <= 2)
                {
                    switch (subParams[0])
                    {
                        case "write":
                            lines[i] = $"set r21 1\nstr {subParams[1]} 5\nstr r21 6";
                            break;
                        case "writeI":
                            lines[i] = $"set r21 1\nstr {subParams[1]} 7\nstr r21 8";
                            break;
                    }
                    continue;
                }
                if ((subParams[0] == "while" && subParams.Length == 4) || (subParams[0] == "do" && subParams[1] == "while" && subParams.Length == 5))
                {
                    int zero = subParams.Length - 4;
                    int endScopeLine = lineNumberForEndOfScope(lines, i);
                    string startLabel = "whileStart" + i;
                    string endLabel = "while" + endScopeLine;
                    lines[endScopeLine] = endLabel + ":\n";
                    lines[i] = "";
                    if (subParams[0] == "do")
                    {
                        lines[i] = $"jmp {endLabel}\n";
                    }
                    lines[i] += $"{startLabel}:";
                    switch (subParams[2])
                    {
                        case "<":
                            lines[endScopeLine] += $"lth r21 {subParams[zero + 1]} {subParams[zero + 3]}\njnz r21 {startLabel}";
                            break;
                        case ">":
                            lines[endScopeLine] += $"leq r21 {subParams[zero + 1]} {subParams[zero + 3]}\njiz r21 {startLabel}";
                            break;
                        case "<=":
                            lines[endScopeLine] += $"leq r21 {subParams[zero + 1]} {subParams[zero + 3]}\njnz r21 {startLabel}";
                            break;
                        case ">=":
                            lines[endScopeLine] += $"lth r21 {subParams[zero + 1]} {subParams[zero + 3]}\njiz r21 {startLabel}";
                            break;
                        case "==":
                            lines[endScopeLine] += $"lth r21 {subParams[zero + 1]} {subParams[zero + 3]}\njnz r21 {startLabel}";
                            break;
                        case "!=":
                            lines[endScopeLine] += $"lth r21 {subParams[zero + 1]} {subParams[zero + 3]}\njnz r21 {startLabel}\nleq r21 {subParams[zero + 1]} {subParams[zero + 3]}\njiz r21 {startLabel}";
                            break;
                    }
                }
                else if (subParams[0] == "if" && subParams.Length == 4)
                {
                    int endScopeLine = lineNumberForEndOfScope(lines, i);
                    string endLabel = "if" + endScopeLine;
                    lines[endScopeLine] = endLabel + ":";
                    int endScopeLineElse = 0;
                    if (lines.Length > endScopeLine + 1 && lines[endScopeLine + 1] == "else")
                    {
                        endScopeLineElse = lineNumberForEndOfScope(lines, endScopeLine + 1);
                        string elseEndLabel = $"elseEnd{endScopeLineElse}";
                        lines[endScopeLineElse] = $"{elseEndLabel}:";
                        lines[endScopeLine] = $"jmp {elseEndLabel}\n" + lines[endScopeLine];
                        lines[endScopeLine + 1] = "";
                        lines[endScopeLine + 2] = "";
                    }
                    switch (subParams[2])
                    {
                        case "<":
                            lines[i] = $"lth r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jiz r21 {endLabel}";
                            break;
                        case ">":
                            lines[i] = $"leq r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jnz r21 {endLabel}";
                            break;
                        case "<=":
                            lines[i] = $"leq r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jiz r21 {endLabel}";
                            break;
                        case ">=":
                            lines[i] = $"lth r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jnz r21 {endLabel}";
                            break;
                        case "==":
                            lines[i] = $"lth r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jnz r21 {endLabel}\nleq r21 {subParams[1]} {subParams[3]}\njiz r21 {endLabel}";
                            break;
                        case "!=":
                            string startLabel = "ifStart" + i;
                            lines[i] = $"lth r21 {subParams[1]} {subParams[3]}";
                            lines[i + 1] = $"jnz r21 {startLabel}\nleq r21 {subParams[1]} {subParams[3]}\njiz r21 {startLabel}\njmp {endLabel}\n{startLabel}:";
                            break;
                    }

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
                                    //if (!subParams[2].Contains("r"))
                                    //{
                                    //    lines[i] = $"";
                                    //}
                                    lines[i] = $"str {subParams[2]} {subParams[0].Remove(0, 1)}";
                                }
                                else if (subParams[2].StartsWith("*r"))
                                {
                                    lines[i] = $"ldi {subParams[0]} {subParams[2].Remove(0, 1)} 0";
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
            List<uint> outlines = new List<uint>();

            bool progmem = false;
            Dictionary<string, ushort> labels = new Dictionary<string, ushort>();
            Dictionary<string, ushort> ptrs = new Dictionary<string, ushort>();
            int ptrTemp = 0;
            int progmemInd = ushort.MaxValue;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Split(';')[0];
                lines[i] = lines[i].Replace("\r", "");
                lines[i] = lines[i].Trim(' ');
                if (lines[i] == "PROGMEM")
                {
                    progmemInd = i;
                }
                if (lines[i].Split(' ')[0].EndsWith(":") && progmemInd != ushort.MaxValue)
                {
                    ptrs.Add(lines[i].Split(' ')[0].Remove(lines[i].Split(' ')[0].Length - 1), (ushort)ptrTemp);
                    lines[i] = lines[i].Remove(0, lines[i].Split(' ')[0].Length + 1);

                    if (lines[i].Contains('"'))
                    {
                        ptrTemp += (int)Math.Ceiling(((double)lines[i].Length - 2));
                    }
                    else
                    {
                        lines[i] = lines[i].Replace(" ", "");
                        for (int m = 0; m < lines[i].Length / 4; m++)
                        {
                            lines[i] = lines[i].Insert(m * 4 + m, " ");
                        }
                        ptrTemp += (int)Math.Ceiling(((double)lines[i].TrimStart('0').Length) / 2);
                    }
                    continue;
                }
                if (lines[i].EndsWith(":"))
                {
                    labels.Add(lines[i].Remove(lines[i].Length - 1), (ushort)i);
                    lines[i] = "nop";
                }
                ptrTemp++;
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (i < progmemInd)
                {
                    lines[i] = lines[i].Replace(",", "");
                    foreach (string label in labels.Keys.ToArray())
                    {
                        for (int j = 0; j < lines[i].Split(' ').Length; j++)
                        {
                            if (lines[i].Split(' ')[j] == label)
                            {
                                lines[i] = lines[i].Replace(label, labels[label].ToString("X"));
                            }

                        }
                    }
                    if (lines[i].StartsWith("lpm"))
                    {
                        foreach (string ptr in ptrs.Keys.ToArray())
                        {
                            for (int j = 0; j < lines[i].Split(' ').Length; j++)
                            {
                                if (lines[i].Split(' ')[j] == ptr)
                                {
                                    lines[i] = lines[i].Replace(ptr, ptrs[ptr].ToString("X"));
                                }

                            }
                        }
                    }
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
                if (subParams[0] == "PROGMEM")
                {
                    outlines.Add(0xFFFF_FFFF);
                    progmem = true;
                }
                else if (progmem)
                {
                    if (lines[i].Contains('"'))
                    {
                        string temp = JsonConvert.DeserializeObject<string>(lines[i]);
                        List<uint> list = new List<uint>();
                        for (int j = 0; j < temp.Length; j++)
                        {
                            list.Add((uint)temp[j] << 8);
                        }
                        list.Add(0);
                        outlines.AddRange(list.ToArray());
                    }
                    else
                    {
                        string temp = lines[i];
                        string[] lns = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < lns.Length; j++)
                        {
                            ushort s = ushort.Parse(lns[j], NumberStyles.HexNumber);
                            uint toAdd = 0;
                            toAdd += (byte)(s);
                            toAdd <<= 8;
                            toAdd += (byte)(s >> 8);
                            outlines.Add(toAdd);
                        }
                    }
                }
                else
                {
                    foreach (string o in opCodes.Keys.ToArray())
                    {
                        if (subParams[0] == o)
                        {
                            int[] cols = opCodes[o];
                            outlines.Add((uint)cols[0]);
                            int subParamsI = 1;
                            bool brk = false;
                            if (outlines.Count <= i)
                            {
                                outlines.Add(0);
                            }
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
                                        outlines[i] += (byte)(uint.Parse(subParams[subParamsI], NumberStyles.HexNumber));
                                        outlines[i] = (uint)(outlines[i] << 8);
                                        outlines[i] += (byte)(uint.Parse(subParams[subParamsI], NumberStyles.HexNumber) >> 8);
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
            }
            if (!outputAsBytes)
            {
                string[] outputLines = new string[outlines.Count];
                for (int i = 0; i < outlines.Count; i++)
                {
                    outputLines[i] = outlines[i].ToString("X8");
                }
                File.WriteAllLines(filePathOut, outputLines);
            }
            else
            {
                byte[] outputBytes = new byte[outlines.Count * 4];
                for (int i = 0; i < outlines.Count; i++)
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
