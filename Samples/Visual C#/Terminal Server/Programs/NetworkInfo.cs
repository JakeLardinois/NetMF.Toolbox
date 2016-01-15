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
    /// Returns networking info
    /// </summary>
    public static class NetworkInfo
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
            if (Arguments[0].ToUpper() == "NETWORKINFO")
            {
                NetworkInfo.Start(Shell.TelnetServer);
                SuspressError = true;
            }
            if (Arguments[0].ToUpper() == "HELP")
            {
                if (Arguments.Length == 1 || Arguments[1].ToUpper() == "NETWORKINFO")
                {
                    Shell.TelnetServer.Print("NETWORKINFO                        Shows details about all network interfaces");
                    SuspressError = true;
                }
            }
        }

        /// <summary>
        /// Actually starts the demo
        /// </summary>
        /// <param name="Server">Reference to the Telnet Server</param>
        private static void Start(TelnetServer Server)
        {
            NetworkInterface[] Ips = NetworkInterface.GetAllNetworkInterfaces();
            for (int IpCnt = 0; IpCnt < Ips.Length; ++IpCnt)
            {
                Server.Print("Network interface " + IpCnt.ToString() + ":");
                Server.Print("MAC Address: " + NetworkInfo.MacToString(Ips[IpCnt].PhysicalAddress));
                Server.Print("- IP Address: " + Ips[IpCnt].IPAddress.ToString() + " (" + Ips[IpCnt].SubnetMask.ToString() + ")");
                Server.Print("- Gateway: " + Ips[IpCnt].GatewayAddress.ToString());
                for (int DnsCnt = 0; DnsCnt < Ips[IpCnt].DnsAddresses.Length; ++DnsCnt)
                    Server.Print("- DNS-server " + DnsCnt.ToString() + ": " + Ips[IpCnt].DnsAddresses[DnsCnt].ToString());
            }
            Server.Print("Connected to: " + Server.RemoteAddress);
        }

        /// <summary>
        /// Converts a PhysicalAddress array to a MAC address
        /// </summary>
        /// <param name="PhysicalAddress">The PhysicalAddress</param>
        /// <returns>The MAC Address as string</returns>
        private static string MacToString(byte[] PhysicalAddress)
        {
            if (PhysicalAddress.Length == 0) return "00:00:00:00:00:00";
            string RetVal = "";
            for (int i = 0; i < PhysicalAddress.Length; ++i)
                RetVal += ":" + Tools.Dec2Hex(PhysicalAddress[i], 2);
            return RetVal.Substring(1);
        }
    }
}
