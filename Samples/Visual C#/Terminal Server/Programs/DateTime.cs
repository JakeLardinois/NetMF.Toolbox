using System;
using Toolbox.NETMF;
using Toolbox.NETMF.NET;
using Microsoft.SPOT.Hardware;

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
    /// Access to the DateTime object
    /// </summary>
    public static class Time
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
                case "TIME":
                    if (Arguments.Length == 1) Shell.TelnetServer.Print("Current time: " + DisplayTime());
                    else ChangeTime(Shell.TelnetServer, Arguments[1]);
                    SuspressError = true;
                    break;
                case "DATE":
                    if (Arguments.Length == 1) Shell.TelnetServer.Print("Current date: " + DisplayDate());
                    else ChangeDate(Shell.TelnetServer, Arguments[1]);
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
        /// Changes the current time
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Time">New time</param>
        private static void ChangeTime(TelnetServer Server, string Time)
        {
            // Splits up time parts
            string[] Parts = Time.Split(new char[] { ':' });
            if (Parts.Length != 3) throw new ArgumentException("Invalid TIME format. See HELP TIME for more.");
            // Validates data
            int Hours = int.Parse(Parts[0]);
            int Minutes = int.Parse(Parts[1]);
            int Seconds = int.Parse(Parts[2]);
            if (Hours > 23 || Hours < 0) throw new ArgumentException("Invalid TIME format. See HELP TIME for more.");
            if (Minutes > 59 || Minutes < 0) throw new ArgumentException("Invalid TIME format. See HELP TIME for more.");
            if (Seconds > 59 || Seconds < 0) throw new ArgumentException("Invalid TIME format. See HELP TIME for more.");

            DateTime NewTime = new DateTime(
                year: DateTime.Now.Year,
                month: DateTime.Now.Month,
                day: DateTime.Now.Day,
                hour: Hours,
                minute: Minutes,
                second: Seconds
            );
            Utility.SetLocalTime(NewTime);

            Server.Print("New time: " + DisplayTime());
        }


        /// <summary>
        /// Changes the current date
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Date">New date</param>
        private static void ChangeDate(TelnetServer Server, string Date)
        {
            // Splits up date parts
            string[] Parts = Date.Split(new char[] { '-' });
            if (Parts.Length != 3) throw new ArgumentException("Invalid TIME format. See HELP DATE for more.");
            // Validates data
            int Years = int.Parse(Parts[0]);
            int Months = int.Parse(Parts[1]);
            int Days = int.Parse(Parts[2]);
            if (Years < 2000 || Years > 3000) throw new ArgumentException("Invalid DATE format. See HELP DATE for more.");
            if (Months > 12 || Months < 1) throw new ArgumentException("Invalid DATE format. See HELP DATE for more.");
            if (Days > 31 || Days < 1) throw new ArgumentException("Invalid DATE format. See HELP DATE for more.");

            DateTime NewTime = new DateTime(
                year: Years,
                month: Months,
                day: Days,
                hour: DateTime.Now.Hour,
                minute: DateTime.Now.Minute,
                second: DateTime.Now.Second
            );
            Utility.SetLocalTime(NewTime);

            Server.Print("New date: " + DisplayDate());
        }

        /// <summary>
        /// Returns the time
        /// </summary>
        /// <returns>Time in hh:mm:ss format</returns>
        private static string DisplayTime()
        {
            return Tools.ZeroFill(DateTime.Now.Hour, 2) + ":" + Tools.ZeroFill(DateTime.Now.Minute, 2) + ":" + Tools.ZeroFill(DateTime.Now.Second, 2);
        }

        /// <summary>
        /// Returns the date
        /// </summary>
        /// <returns>Date in yyyy:mm:dd format</returns>
        private static string DisplayDate()
        {
            return DateTime.Now.Year.ToString() + "-" + Tools.ZeroFill(DateTime.Now.Month, 2) + "-" + Tools.ZeroFill(DateTime.Now.Day, 2);
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
                    Server.Print("TIME [TIME]                        Displays or changes the current time");
                    Server.Print("DATE [DATE]                        Displays or changes the current date");
                    return true;
                case "TIME":
                    Server.Print("TIME [TIME]                        Displays or changes the current time");
                    Server.Print("- [TIME]  If set, will change the time (Format: hh:mm:ss)");
                    return true;
                case "DATE":
                    Server.Print("DATE [DATE]                        Displays or changes the current date");
                    Server.Print("- [DATE]  If set, will change the date (Format: yyyy-mm-dd)");
                    return true;
                default:
                    return false;
            }
        }
    }
}
