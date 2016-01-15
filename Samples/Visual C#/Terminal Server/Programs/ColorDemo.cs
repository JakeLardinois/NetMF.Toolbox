using System;
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
    /// Color dmeo
    /// </summary>
    public static class ColorDemo
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
            if (Arguments[0].ToUpper() == "COLORDEMO")
            {
                ColorDemo.Start(Shell.TelnetServer);
                SuspressError = true;
            }
            if (Arguments[0].ToUpper() == "HELP")
            {
                if (Arguments.Length == 1 || Arguments[1].ToUpper() == "COLORDEMO")
                {
                    Shell.TelnetServer.Print("COLORDEMO                          Shows an ANSI Color demo");
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
            for (int f = 0; f < 16; ++f)
            {
                for (int b = 0; b < 8; ++b)
                {
                    Server.Color((TelnetServer.Colors)f, (TelnetServer.Colors)b);
                    Server.Print(" " + Tools.ZeroFill(f, 2) + "," + Tools.ZeroFill(b, 2) + " ", true, true);
                }
                Server.Color(TelnetServer.Colors.White, TelnetServer.Colors.Black);
                Server.Print("");
            }
        }
    }
}
