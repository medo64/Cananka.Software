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
        Loop(cananka);

        cananka.Close();
        Output.WriteOkLine("Disconnected from SLCAN device (" + serialPortName + ").");
    }


    private static void Loop(Cananka cananka) {
        var inverse = false;

        while (true) {
            var key = Console.ReadKey(intercept: true);
            switch (key.Key) {
                case ConsoleKey.Escape:  // exit
                case ConsoleKey.Q:
                    return;
                case ConsoleKey.Enter:  // empty line
                    Output.WriteLine();
                    break;
                case ConsoleKey.T:  // <T> turn on termination  <Del><t> turn off termination
                    var newTerminationState = !inverse;
                    if (cananka.SetTermination(newTerminationState)) {
                        Output.WriteOkLine("Termination turned " + (newTerminationState ? "on" : "off"));
                    } else {
                        Output.WriteErrorLine("Cannot turn termination on" + (newTerminationState ? "on" : "off"));
                    }
                    break;
                case ConsoleKey.P:  // <P> turn on power  <Del><t> turn off power
                    var newPowerState = !inverse;
                    if (cananka.SetPower(newPowerState)) {
                        Output.WriteOkLine("Power turned " + (newPowerState ? "on" : "off"));
                    } else {
                        Output.WriteErrorLine("Cannot turn power on" + (newPowerState ? "on" : "off"));
                    }
                    break;
            }
            if (key.Key == ConsoleKey.Delete) { inverse = !inverse; } else { inverse = false; }
        }

    }

    private static void OnMessageArrived(object? sender, CanankaMessageEventArgs e) {
        Output.WriteMessageLine(e.Message);
    }

}
