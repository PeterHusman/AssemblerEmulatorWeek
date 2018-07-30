using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleHelper
{
    public enum AutocompleteOptions
    {
        None,
        FilePath
    }
    public abstract class CHelper
    {
        //Dictionary<char, string> dictionary
        //{
        //    get;
        //    set;
        //}
        public static string RequestInput(string prompt, bool inputOnNewLine, ConsoleColor promptColor, ConsoleColor answerColor, AutocompleteOptions options)
        {
            Console.ForegroundColor = promptColor;
            Console.Write(prompt + (inputOnNewLine ? '\n' : ' '));
            Console.ForegroundColor = answerColor;
            if (options == AutocompleteOptions.None)
            {
                return Console.ReadLine();
            }
            else
            {
                throw new NotImplementedException();
            }

        }
        public static void CenteredWriteLine(string text, ConsoleColor color, int row)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(Console.BufferWidth / 2 - text.Length / 2, row);
            Console.WriteLine(text);
        }
        public static string ASCIIArt(string input, string asciiFontFile)
        {
            Dictionary<char, string> dictionary = JsonConvert.DeserializeObject<Dictionary<char, string>>(File.ReadAllText(asciiFontFile));
            StringBuilder builder = new StringBuilder();
            //for (int j = 0; j < dictionary.Values[0].Split('\n').Length; j++)
            //{
                for (int i = 0; i < input.Length; i++)
                {
                    builder.Append(dictionary[input[i]]);
                }
            //}
            return builder.ToString();
        }
    }
}
