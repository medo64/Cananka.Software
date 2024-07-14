namespace CanankaDebug;
using System;
using System.IO.Ports;
using System.Runtime.InteropServices.Marshalling;
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
        Loop();

        cananka.Close();
        Output.WriteOkLine("Disconnected from SLCAN device (" + serialPortName + ").");
    }


    private static void Loop() {
        while (true) {
            var key = Console.ReadKey(intercept: true);
            switch (key.Key) {
                case ConsoleKey.Escape:
                case ConsoleKey.Q:
                    return;
            }
        }

    }

    private static void OnMessageArrived(object? sender, CanankaMessageEventArgs e) {
        Output.WriteMessageLine(e.Message);
    }

}
