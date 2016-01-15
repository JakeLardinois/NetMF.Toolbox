using System;
using System.IO.Ports; // Microsoft.SPOT.Hardware.SerialPort.dll
using Microsoft.SPOT;
using Toolbox.NETMF;
using Toolbox.NETMF.NET;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Programs
{
    /// <summary>
    /// Serial Port app
    /// </summary>
    public static class Serial
    {
        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Bind(ShellCore Shell)
        {
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Unbinds from the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Unbind(ShellCore Shell)
        {
            Shell.OnCommandReceived -= new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        private static void Shell_OnCommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time)
        {
            switch (Arguments[0].ToUpper())
            {
                case "SERIAL":
                    if (Arguments.Length == 1) throw new ArgumentException("COM-port required, see HELP SERIAL for more.");
                    else if (Arguments.Length == 2) Serial.Start(Shell.TelnetServer, Arguments[1]);
                    else if (Arguments.Length == 3) Serial.Start(Shell.TelnetServer, Arguments[1], Arguments[2]);
                    else if (Arguments.Length == 4) Serial.Start(Shell.TelnetServer, Arguments[1], Arguments[2], Arguments[3]);
                    else if (Arguments.Length == 5) Serial.Start(Shell.TelnetServer, Arguments[1], Arguments[2], Arguments[3], Arguments[4]);
                    else Serial.Start(Shell.TelnetServer, Arguments[1], Arguments[2], Arguments[3], Arguments[4], Arguments[5]);
                    SuspressError = true;
                    break;
                case "HELP":
                    bool PageFound = false;
                    if (Arguments.Length == 1) PageFound = DoHelp(Shell.TelnetServer, "");
                    else PageFound = DoHelp(Shell.TelnetServer, Arguments[1].ToUpper());
                    if (PageFound) SuspressError = true;
                    break;
            }
        }

        /// <summary>
        /// Shows a specific help page
        /// </summary>
        /// <param name="Server">The telnet server object</param>
        /// <param name="Page">The page</param>
        /// <returns>True when the page exists</returns>
        private static bool DoHelp(TelnetServer Server, string Page)
        {
            switch (Page)
            {
                case "":
                    Server.Print("SERIAL [COM]                       Opens a serial connection");
                    return true;
                case "SERIAL":
                    Server.Print("SERIAL [COM]                       Opens a serial connection");
                    Server.Print("- [COM]       The COM-port to connect to");
                    Server.Print("Additional parameters:");
                    Server.Print("- [BAUD]      Connection speed (default: 9600)");
                    Server.Print("- [PARITY]    Parity (default: none)");
                    Server.Print("- [DATABITS]  The amount of databits (default: 8)");
                    Server.Print("- [STOPBITS]  The amount of stopbits (default: 1)");
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Starts the serial client
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Com">COM-port</param>
        /// <param name="Baud">Baudrate</param>
        /// <param name="SParity">Parity</param>
        /// <param name="Databits">Databits</param>
        /// <param name="Stopbits">Stopbits</param>
        private static void Start(TelnetServer Server, string Com, string Baud = "9600", string SParity = "NONE", string Databits = "8", string Stopbits = "1")
        {
            // Parses parity
            Parity PParity;
            switch (SParity.ToUpper())
            {
                case "EVEN": PParity = Parity.Even; break;
                case "MARK": PParity = Parity.Mark; break;
                case "NONE": PParity = Parity.None; break;
                case "ODD": PParity = Parity.Odd; break;
                case "SPACE": PParity = Parity.Space; break;
                default:
                    throw new ArgumentException("Parity " + SParity + " unknown. Known values are EVEN, MARK, NONE, ODD, SPACE");
            }

            // Parses Stopbits
            StopBits Stop;
            switch (Stopbits.ToUpper())
            {
                case "0": Stop = StopBits.None; break;
                case "1": Stop = StopBits.One; break;
                case "1.5": Stop = StopBits.OnePointFive; break;
                case "2": Stop = StopBits.Two; break;
                default:
                    throw new ArgumentException("Stopbits " + Stopbits + " unknown. Known values are 0, 1, 1.5, 2");
            }

            // Configures the serial port
            SerialPort Port = new SerialPort(
                portName: Com.ToUpper(),
                baudRate: int.Parse(Baud),
                parity: PParity,
                dataBits: int.Parse(Databits),
                stopBits: Stop
            );

            Server.Print("Connecting to " + Com + "...");
            Port.DiscardInBuffer();
            Port.Open();
            Server.Print("Connected. Press Ctrl+C to close the connection");

            Server.EchoEnabled = false;
            while (true)
            {
                byte[] Data = Tools.Chars2Bytes(Server.Input(1, false).ToCharArray());
                if (Data.Length > 0)
                {
                    if (Data[0] == 3) break; // Ctrl+C
                    Port.Write(Data, 0, Data.Length);
                }
                if (Port.BytesToRead > 0)
                {
                    byte[] Buffer = new byte[Port.BytesToRead];
                    Port.Read(Buffer, 0, Buffer.Length);
                    Server.Print(new string(Tools.Bytes2Chars(Buffer)), true);
                }
            }
            Server.EchoEnabled = true;

            Port.Close();
            Server.Print("", false, true);
            Server.Print("Connection closed");
        }

    }
}
