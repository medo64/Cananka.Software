namespace CanankaDebug;
using System;
using System.Globalization;
using System.Reflection;
using Medo.Device;

internal static class Output {

    private static readonly object Lock = new();

    public static void WriteLine() {
        lock (Lock) {
            Console.WriteLine();
        }
    }

    public static void WriteLine(string text, ConsoleColor color) {
        lock (Lock) {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    public static void WriteErrorLine(string text) {
        lock (Lock) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    public static void WriteOkLine(string text) {
        lock (Lock) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    public static void WriteInfoLine(string text) {
        lock (Lock) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }

    public static void WriteMessageLine(CanankaMessage message, DateTime timestamp) {
        lock (Lock) {
            var id = message.Id;
            var dataBytes = message.GetData() ?? [];

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            Console.Write(" ");
            if (message.IsExtended) {
                Console.Write(id.ToString("X8", CultureInfo.InvariantCulture));
            } else {
                Console.Write("     " + id.ToString("X3", CultureInfo.InvariantCulture));
            }
            Console.Write("#");
            for (var i = 0; i < dataBytes.Length; i++) {
                Console.Write(dataBytes[i].ToString("X2", CultureInfo.InvariantCulture));
            }
            if (dataBytes.Length < 8) { Console.Write(new string(' ', (8 - dataBytes.Length) * 2)); }

            Console.ForegroundColor = ConsoleColor.Cyan;
            if (message.IsExtended) {
                Console.Write(" " + id.ToString("X8", CultureInfo.InvariantCulture));
            } else {
                Console.Write(" " + id.ToString("X3", CultureInfo.InvariantCulture));
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(message.Length.ToString(" 0", CultureInfo.InvariantCulture));

            if (message.Length > 0) {
                Console.ForegroundColor = ConsoleColor.White;
                for (var i = 0; i < dataBytes.Length; i++) {
                    Console.Write(" ");
                    Console.Write(dataBytes[i].ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            if (message.IsRemoteRequest) {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(" R");
            }

            Console.ResetColor();
            Console.WriteLine();
        }
    }

}
