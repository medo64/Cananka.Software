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

    public static void WriteMessageLine(CanankaMessage message) {
        lock (Lock) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
            Console.Write(" ");
            Console.Write(message.ToString());

            Console.ForegroundColor = ConsoleColor.Cyan;
            if (message.IsExtended) {
                Console.Write(" " + message.Id.ToString("X8", CultureInfo.InvariantCulture));
            } else {
                Console.Write(" " + message.Id.ToString("X3", CultureInfo.InvariantCulture));
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(message.Length.ToString(" 0", CultureInfo.InvariantCulture));

            if (message.Length > 0) {
                Console.ForegroundColor = ConsoleColor.White;
                var dataBytes = message.GetData() ?? [];
                for (var i = 0; i < dataBytes.Length; i++) {
                    Console.Write(" ");
                    Console.Write(dataBytes[i].ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            Console.ResetColor();
            Console.WriteLine();
        }
    }

}
