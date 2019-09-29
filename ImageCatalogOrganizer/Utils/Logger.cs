using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgCli
{
    public static class Logger
    {
        public static void LogError(string log)
        {
            var consoleForeground = Console.ForegroundColor;
            var consoleBackground = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine($"{DateTime.Now:HH:mm:ss}\t{log}");

            Console.ForegroundColor = consoleForeground;
            Console.BackgroundColor = consoleBackground;
        }

        public static void LogWarning(string log)
        {
            var consoleForeground = Console.ForegroundColor;
            var consoleBackground = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine($"{DateTime.Now:HH:mm:ss}\t{log}");

            Console.ForegroundColor = consoleForeground;
            Console.BackgroundColor = consoleBackground;
        }

        public static void Log(string log)
        {
            var consoleForeground = Console.ForegroundColor;
            var consoleBackground = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine($"{DateTime.Now:HH:mm:ss}\t{log}");

            Console.ForegroundColor = consoleForeground;
            Console.BackgroundColor = consoleBackground;
        }
    }
}
