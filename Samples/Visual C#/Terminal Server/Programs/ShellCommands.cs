using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
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
    /// Some generic shell commands
    /// </summary>
    public static class ShellCommands
    {
        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Bind(ShellCore Shell)
        {
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
            Shell.OnConnected += new ShellCore.ConnectionStateChange(Shell_OnConnected);
        }

        /// <summary>
        /// Unbinds from the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Unbind(ShellCore Shell)
        {
            Shell.OnConnected -= new ShellCore.ConnectionStateChange(Shell_OnConnected);
            Shell.OnCommandReceived -= new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Triggered when the connection has been made
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="RemoteAddress">Hostname of the user</param>
        /// <param name="Time">Timestamp of the event</param>
        private static void Shell_OnConnected(ShellCore Shell, string RemoteAddress, DateTime Time)
        {
            Shell.TelnetServer.Color(TelnetServer.Colors.White, TelnetServer.Colors.Black);
            Shell.TelnetServer.ClearScreen();
            _SendMotd(Shell.TelnetServer);
        }

        /// <summary>
        /// Sends the MOTD
        /// </summary>
        private static void _SendMotd(TelnetServer Server)
        {
            Server.Color(TelnetServer.Colors.HighIntensityWhite);
            Server.Print("Welcome to the Netduino Telnet Server, " + Server.RemoteAddress);
            Server.Print("Copyright 2012 by Stefan Thoolen (http://www.netmftoolbox.com/)");
            Server.Print("Type HELP to see a list of all supported commands");
            Server.Print("");
            Server.Color(TelnetServer.Colors.White);
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
                case "CLS":
                    Shell.TelnetServer.ClearScreen();
                    SuspressError = true;
                    break;
                case "MOTD":
                    _SendMotd(Shell.TelnetServer);
                    SuspressError = true;
                    break;
                case "ECHO":
                    Shell.TelnetServer.Print(Shell.LastCommandline.Substring(5));
                    SuspressError = true;
                    break;
                case "REBOOT":
                    Shell.TelnetServer.Print("Rebooting...");
                    Thread.Sleep(100);
                    Shell.TelnetServer.Close();
                    PowerState.RebootDevice(false);
                    SuspressError = true;
                    break;
                case "QUIT":
                    Shell.TelnetServer.Print("Bye!");
                    Thread.Sleep(100);
                    Shell.TelnetServer.Close();
                    SuspressError = true;
                    break;
                case "INFO":
                    Shell.TelnetServer.Print("Manufacturer: " + SystemInfo.OEMString);
                    Shell.TelnetServer.Print("Firmware version: " + SystemInfo.Version.ToString());
                    Shell.TelnetServer.Print("Memory available: " + Tools.MetricPrefix(Debug.GC(false), true) + "B");
                    if (PowerState.Uptime.Days == 0)
                        Shell.TelnetServer.Print("Uptime: " + PowerState.Uptime.ToString());
                    else
                        Shell.TelnetServer.Print("Uptime: " + PowerState.Uptime.Days.ToString() + " days, " + PowerState.Uptime.ToString());
                    Shell.TelnetServer.Print("Hardware provider: " + Tools.HardwareProvider);
                    Shell.TelnetServer.Print("System clock: " + Tools.MetricPrefix(Cpu.SystemClock) + "hz");
                    Shell.TelnetServer.Print("Endianness: " + (SystemInfo.IsBigEndian ? "Big Endian" : "Little Endian"));
                    Shell.TelnetServer.Print("Debugger: " + (System.Diagnostics.Debugger.IsAttached ? "attached" : "not attached"));
                    SuspressError = true;
                    break;
                case "VER":
                    System.Reflection.Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    for (int i = 0; i < Assemblies.Length; ++i)
                        Shell.TelnetServer.Print(Assemblies[i].FullName);
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
                    Server.Print("CLS                                Clears the screen");
                    Server.Print("ECHO [TEXT]                        Prints out the text");
                    Server.Print("MOTD                               Shows the message of the day");
                    Server.Print("QUIT                               Closes the connection");
                    Server.Print("VER                                Shows the version of all loaded assemblies");
                    Server.Print("INFO                               Shows some system info");
                    Server.Print("REBOOT                             Restarts the device");
                    return true;
                case "VER":
                    Server.Print("VER                                Shows the version of all loaded assemblies");
                    return true;
                case "REBOOT":
                    Server.Print("REBOOT                             Restarts the device");
                    return true;
                case "MOTD":
                    Server.Print("MOTD                               Shows the message of the day");
                    return true;
                case "INFO":
                    Server.Print("INFO                               Shows some system info");
                    return true;
                case "CLS":
                    Server.Print("CLS                                Clears the screen");
                    return true;
                case "ECHO":
                    Server.Print("ECHO [TEXT]                        Prints out the text");
                    Server.Print("- [TEXT]  Text to print out");
                    return true;
                case "QUIT":
                    Server.Print("QUIT                               Closes the connection");
                    return true;
                default:
                    return false;
            }
        }
    }
}
