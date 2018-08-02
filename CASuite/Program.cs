using ConsoleHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CASuite
{
    class Program
    {
        static void Main(string[] args)
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
            while (true)
            {
                Console.WriteLine(CHelper.ASCIIArt("PSuite", CHelper.LoadASCIIFont(@"C:\Users\PeterHusman\Documents\FontFile.json")));
                switch (CHelper.SelectorMenu("Please select an action.", new string[] { "Open Assembler", "Open Emulator", "Assemble and emulate a file" }, true, ConsoleColor.Yellow, ConsoleColor.Gray, ConsoleColor.Magenta))
                {
                    case 0:
                        Console.Clear();
                        CAAssembler.Program.Main(new string[0]);
                        break;
                    case 1:
                        Console.Clear();
                        CAEmulator.Program.Main(new string[0]);
                        break;
                    case 2:
                        Console.WriteLine();
                        string[] options = new string[] { "None", "Debug" };
                        string extraParam = options[CHelper.SelectorMenu("Choose an extra parameter to provide to the emulator.", options,true, ConsoleColor.Yellow, ConsoleColor.Gray, ConsoleColor.Magenta)];
                        Console.WriteLine();
                        string filePath = CHelper.RequestInput("Please enter the file path of the file to assemble and emulate.",true,ConsoleColor.Yellow,ConsoleColor.Gray,recursiveFileSearch($@"C:\Users\{Environment.UserName}"));
                        CAAssembler.Program.Main(new string[] { filePath, filePath + ".temp", @"C:\Users\PeterHusman\Downloads\Assembly To Machine Code Chart - Sheet1 (4).csv", "fullassemble" });
                        if (extraParam == "None")
                        {
                            CAEmulator.Program.Main(new string[] { filePath + ".temp" });
                        }
                        else
                        {
                            CAEmulator.Program.Main(new string[] { filePath + ".temp", extraParam.ToLower() });
                        }
                        break;

                }
            }
        }
    }
}
