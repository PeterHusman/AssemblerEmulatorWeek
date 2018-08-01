using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleHelper
{
    public class Trie
    {
        class Node
        {
            List<Node> children;
            
        }
    }

    public abstract class CHelper
    {
        //Dictionary<char, string> dictionary
        //{
        //    get;
        //    set;
        //}
        public static int SelectorMenu(string prompt, string[] options, bool wraps, ConsoleColor promptColor, ConsoleColor notSelectedColor, ConsoleColor selectedColor)
        {
            Console.ForegroundColor = promptColor;
            Console.WriteLine(prompt);
            int row = Console.CursorTop;
            int selectedIndex = 0;
            while (true)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    Console.ForegroundColor = i == selectedIndex ? selectedColor : notSelectedColor;
                    Console.SetCursorPosition(0, row + i);
                    Console.Write(options[i]);
                }
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (wraps)
                    {
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                    }
                    else
                    {
                        if (selectedIndex > 0)
                        {
                            selectedIndex--;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (wraps)
                    {
                        selectedIndex = (selectedIndex + 1) % options.Length;
                    }
                    else
                    {
                        if (selectedIndex < options.Length - 1)
                        {
                            selectedIndex--;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    return selectedIndex;
                }
                
            }
        }

        public static string RequestInput(string prompt, bool inputOnNewLine, ConsoleColor promptColor, ConsoleColor answerColor)
        {
            Console.ForegroundColor = promptColor;
            Console.Write(prompt + (inputOnNewLine ? '\n' : ' '));
            Console.ForegroundColor = answerColor;
            return Console.ReadLine();
        }
        public static string RequestInput(string prompt, bool inputOnNewLine, ConsoleColor promptColor, ConsoleColor answerColor, string[] autoCompleteOptions)
        {
            Console.ForegroundColor = promptColor;
            Console.Write(prompt + (inputOnNewLine ? '\n' : ' '));
            Console.ForegroundColor = answerColor;
            string str = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return str;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (str.Length > 0)
                    {
                        Console.Write("\x08");
                        Console.Write(" ");
                        Console.Write("\x08");
                        str = str.Remove(str.Length - 1, 1);
                    }
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    List<string> autoComplete = new List<string>();
                    for (int i = 0; i < autoCompleteOptions.Length; i++)
                    {
                        if (autoCompleteOptions[i].Contains(str))
                        {
                            autoComplete.Add(autoCompleteOptions[i]);
                        }
                    }
                    if(autoComplete.Count <= 0)
                    {
                        continue;
                    }
                    erase();
                    for (int i = 0; i < autoComplete[0].Length; i++)
                    {
                        bool x = false;
                        bool y = false;
                        foreach (string s in autoComplete)
                        {
                            if(i >= s.Length)
                            {
                                y = true;
                                break;
                            }
                            if(autoComplete[0][i] != s[i])
                            {
                                y = true;
                                x = true;
                                break;
                            }
                        }
                        if(y)
                        {
                            break;
                        }
                        if(x)
                        {
                            continue;
                        }
                        Console.Write(autoComplete[0][i]);
                        str += autoComplete[0][i];
                    }
                }
                else
                {
                    char c = key.KeyChar;
                    str += c;
                    Console.Write(c);

                }
                void erase()
                {
                    for(int i = 0; i < str.Length; i++)
                    {
                        Console.Write("\x08");
                        Console.Write(" ");
                        Console.Write("\x08");
                    }
                    str = "";
                }
            }
        }

        public static void CenteredWriteLine(string text, ConsoleColor color, int row)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(Console.BufferWidth / 2 - text.Length / 2, row);
            Console.WriteLine(text);
        }
        public static Dictionary<char, string> LoadASCIIFont(string asciiFontFile)
        {
            return JsonConvert.DeserializeObject<Dictionary<char, string>>(File.ReadAllText(asciiFontFile));
        }

        public static string ASCIIArt(string input, Dictionary<char, string> asciiFont)
        {
            //Dictionary<char, string> dictionary = JsonConvert.DeserializeObject<Dictionary<char, string>>(File.ReadAllText(asciiFontFile));
            StringBuilder builder = new StringBuilder();
            string[] vals = new string[asciiFont.Count];
            asciiFont.Values.CopyTo(vals, 0);
            for (int j = 0; j < vals[0].Split('\n').Length; j++)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (j < asciiFont[input[i]].Split('\n').Length)
                    {
                        builder.Append(asciiFont[input[i]].Split('\n')[j].Replace("\r", ""));
                    }
                }
                builder.AppendLine();

            }
            return builder.ToString();
        }
    }
}
