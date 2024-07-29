namespace CanankaDebug;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using Medo.Device;

internal static class CanankaDebug {

    public static void Main(string[] args) {
        if (args.Length != 1) {
            Output.WriteErrorLine("Usage: canankadebug <serial-port>");
            var portNames = SerialPort.GetPortNames();
            if (portNames.Length > 0) {
                Output.WriteErrorLine("Available ports:");
                foreach (var portName in portNames) {
                    Output.WriteErrorLine("  " + portName);
                }
            } else {
                Output.WriteErrorLine("No ports available.");
            }
            Environment.Exit(255);
        }

        var serialPortName = args[0];

        var cananka = new Cananka(serialPortName);
        cananka.MessageArrived += OnMessageArrived;

        try {
            if (!cananka.Open()) {
                Output.WriteErrorLine("Not a SLCAN device (" + serialPortName + ").");
                Environment.Exit(1);
            }
        } catch (Exception ex) {
            Output.WriteErrorLine("Error setting up device: " + ex.Message);
            Environment.Exit(113);
        }

        Output.WriteOkLine("Connected to SLCAN device (" + serialPortName + ").");
        Loop(cananka);

        cananka.Close();
        Output.WriteOkLine("Disconnected from SLCAN device (" + serialPortName + ").");
    }

    private static readonly ConcurrentQueue<(CanankaMessage, DateTime)> MessagesIn = new();

    private static void Loop(Cananka cananka) {
        var inverse = false;

        while (true) {
            // handle keyboard
            if (Console.KeyAvailable) {
                var key = Console.ReadKey(intercept: true);

                switch (key.Key) {
                    case ConsoleKey.Q:  // <Q> quit
                        return;

                    case ConsoleKey.Enter:  // <Enter> empty line
                        Output.WriteLine();
                        break;

                    case ConsoleKey.T:  // <T> turn on termination  <Del><T> turn off termination
                        var newTerminationState = !inverse;
                        if (cananka.SetTermination(newTerminationState)) {
                            Output.WriteOkLine("Termination turned " + (newTerminationState ? "on" : "off"));
                        } else {
                            Output.WriteErrorLine("Cannot turn termination " + (newTerminationState ? "on" : "off"));
                        }
                        break;

                    case ConsoleKey.P:  // <P> turn on power  <Del><T> turn off power
                        var newPowerState = !inverse;
                        if (cananka.SetPower(newPowerState)) {
                            Output.WriteOkLine("Power turned " + (newPowerState ? "on" : "off"));
                        } else {
                            Output.WriteErrorLine("Cannot turn power " + (newPowerState ? "on" : "off"));
                        }
                        break;

                    case ConsoleKey.S:  // <S> send status
                        var statusMessage = new CanankaMessage(0, []);
                        cananka.SendMessage(statusMessage);
                        break;

                    case ConsoleKey.E:  // <E> edit mode
                        EditMode(cananka);
                        break;
                }
                if (key.Key == ConsoleKey.Delete) { inverse = !inverse; } else { inverse = false; }
            }

            // process incoming messages
            if (MessagesIn.TryDequeue(out var entry)) {
                Output.WriteMessageLine(entry.Item1, entry.Item2);
            }
        }
    }

    private static void EditMode(Cananka cananka) {
        var sb = new StringBuilder();

        Output.WriteLine("Entering edit mode; <Escape> to exit; <Enter> to apply, # splits data", ConsoleColor.Blue);
        while (true) {
            var keyE = Console.ReadKey(intercept: true);
            switch (keyE.Key) {
                case ConsoleKey.Escape:
                    Output.WriteLine();
                    return;

                case ConsoleKey.Enter:
                    Output.WriteLine();

                    var text = sb.ToString().Replace(" ", "");
                    if (text.Length > 0) {
                        var parts = text.ToString().Split('#');
                        if (parts.Length > 2) {
                            Output.WriteErrorLine("Invalid message format (too much data).");
                            return;
                        }

                        var idText = parts[0];
                        var dataText = (parts.Length == 2) ? parts[1] : "";

                        if (!uint.TryParse(idText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id) || (id >= 536870912)) {
                            Output.WriteErrorLine("ID outside of range.");
                            return;
                        }

                        if (dataText.Length % 2 != 0) {
                            Output.WriteErrorLine("Odd number of hexadecimal digits.");
                            return;
                        }

                        var dataBytes = new List<byte>();
                        for (var i = 0; i < dataText.Length; i += 2) {
                            if (!byte.TryParse(dataText.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var dataByte)) {
                                Output.WriteErrorLine("Invalid hexadecimal digit.");
                                return;
                            }
                            dataBytes.Add(dataByte);
                        }

                        if (dataBytes.Count > 8) {
                            Output.WriteErrorLine("Too much data.");
                            return;
                        }

                        var message = new CanankaMessage((int)id, dataBytes.ToArray());
                        cananka.SendMessage(message);
                    }

                    return;

                case ConsoleKey.Backspace:
                    if (sb.Length > 0) {
                        sb.Length -= 1;
                        Console.Write("\b \b");
                    }
                    break;

                default:
                    var c = keyE.KeyChar;
                    if (((c >= '0') && (c <= '9')) || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F')) || (c == '#')) {
                        sb.Append(c);
                        Console.Write(keyE.KeyChar);
                        break;
                    }
                    break;
            }
        }
    }

    private static void OnMessageArrived(object? sender, CanankaMessageEventArgs e) {
        MessagesIn.Enqueue((e.Message, DateTime.Now));
    }
}
