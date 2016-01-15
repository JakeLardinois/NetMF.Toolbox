using System;
using Microsoft.SPOT.Net.NetworkInformation;
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
    /// Synchronizes time
    /// </summary>
    public static class Ntp
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
                case "NTPSYNC":
                    if (Arguments.Length != 2)
                        throw new ArgumentException("Need 1 parameter, see HELP NTPSYNC for more info.");
                    else
                        Ntp.Sync(Shell.TelnetServer, Arguments[1]);
                    SuspressError = true;
                    break;
                case "HELP":
                    bool PageFound = false;
                    if (Arguments.Length == 1) PageFound = Ntp.DoHelp(Shell.TelnetServer, "");
                    else PageFound = Ntp.DoHelp(Shell.TelnetServer, Arguments[1].ToUpper());
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
                    Server.Print("NTPSYNC [HOSTNAME]                 Synchronizes with a timeserver");
                    return true;
                case "NTPSYNC":
                    Server.Print("NTPSYNC [HOSTNAME]                 Synchronizes with a timeserver");
                    Server.Print("- [HOSTNAME]  The hostname of an NTP-server (example: pool.ntp.org)");
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Synchronizes with an NTP-server
        /// </summary>
        /// <param name="Server">The telnet server object</param>
        /// <param name="NtpServer">Hostname of the NTP server</param>
        private static void Sync(TelnetServer Server, string NtpServer)
        {
            SNTP_Client Client = new SNTP_Client(new IntegratedSocket(NtpServer, 123));
            Client.Synchronize();
            Server.Print("Current time: " + DateTime.Now.ToString());
        }
    }
}
