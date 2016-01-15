using System;
using System.Threading;
using Microsoft.SPOT;

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
    /// Authentication
    /// </summary>
    public class Auth
    {
        /// <summary>
        /// Stores the last username
        /// </summary>
        private string _Username = "";

        /// <summary>
        /// Set to true when we logged in
        /// </summary>
        private bool _Authorized = false;

        /// <summary>
        /// Amount of password retries (default: 3)
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// The shell
        /// </summary>
        private ShellCore _Shell;

        /// <summary>
        /// Creates a new authorization module
        /// </summary>
        /// <param name="Shell">Shell to bind to</param>
        public Auth(ShellCore Shell)
        {
            // Sets some default values
            this.Retries = 3;
            this.TimeToLogin = 0;
            this._Shell = Shell;

            // Binds to the shell
            Shell.OnConnected += new ShellCore.ConnectionStateChange(this._Shell_OnConnected);
            Shell.OnCommandReceived += new ShellCore.CommandReceived(this._Shell_OnCommandReceived);
        }

        /// <summary>
        /// Unbinds the shell
        /// </summary>
        public void Dispose()
        {
            // Unbinds from the shell
            this._Shell.OnConnected -= new ShellCore.ConnectionStateChange(this._Shell_OnConnected);
            this._Shell.OnCommandReceived -= new ShellCore.CommandReceived(this._Shell_OnCommandReceived);
        }

        /// <summary>
        /// Time to login in seconds (default: 0)
        /// </summary>
        public int TimeToLogin { get; set; }
        
        /// <summary>
        /// New connection has been made
        /// </summary>
        /// <param name="Shell"></param>
        /// <param name="RemoteAddress"></param>
        /// <param name="Time"></param>
        private void _Shell_OnConnected(ShellCore Shell, string RemoteAddress, DateTime Time)
        {
            if (this.OnAuthorize == null)
            {
                Shell.TelnetServer.Print("Auth.OnAuthorize is not defined, so authorization is inactive");
                return;
            }

            // Resets the current login
            this._Authorized = false;
            this._Username = "";

            // Adds the timer to check for a login timeout
            if (this.TimeToLogin > 0)
                new Timer(this._LoginTimedout, null, this.TimeToLogin * 1000, 0);

            for (int i = 0; i < Retries; ++i)
            {
                // Asks for the username
                Shell.TelnetServer.Print("Username: ", true);
                string Username = Shell.TelnetServer.Input().Trim();
                if (Username == "") continue;

                // Asks for the password (with echoing disabled!)
                Shell.TelnetServer.Print("Password: ", true);
                Shell.TelnetServer.EchoEnabled = false;
                string Password = Shell.TelnetServer.Input().Trim();
                Shell.TelnetServer.EchoEnabled = true;
                Shell.TelnetServer.Print("");

                // Verifies the details
                if (this.OnAuthorize(Shell, Username, Password, RemoteAddress))
                {
                    this._Username = Username;
                    this._Authorized = true;
                    return;
                }
                Shell.TelnetServer.Print("Username and/or password incorrect", false, true);
                Shell.TelnetServer.Print("");
            }
            // No valid login has been given
            Shell.TelnetServer.Print("Closing the connection");
            Thread.Sleep(100);
            Shell.TelnetServer.Close();
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        private void _Shell_OnCommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time)
        {
            if (Arguments[0].ToUpper() == "WHOAMI")
            {
                Shell.TelnetServer.Print("Logged in as " + this._Username);
                SuspressError = true;
            }
            if (Arguments[0].ToUpper() == "HELP")
            {
                if (Arguments.Length == 1 || Arguments[1].ToUpper() == "WHOAMI")
                {
                    Shell.TelnetServer.Print("WHOAMI                             Returns the current username");
                    SuspressError = true;
                }
            }
        }

        /// <summary>
        /// Triggered after this.TimeToLogin seconds to optionally close a connection
        /// </summary>
        /// <param name="Param"></param>
        private void _LoginTimedout(object Param)
        {
            if (this._Authorized) return ;
            this._Shell.TelnetServer.Print("", false, true);
            this._Shell.TelnetServer.Print("Login timeout");
            Thread.Sleep(100);
            this._Shell.TelnetServer.Close();
        }

        /// <summary>
        /// Asks if a user is authorized
        /// </summary>
        public event IsAuthorized OnAuthorize;

        /// <summary>
        /// Asks if a user is authorized
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Username">The username</param>
        /// <param name="Password">The password</param>
        /// <param name="RemoteAddress">The remote host address</param>
        /// <returns>Return true if authorization is successfull</returns>
        public delegate bool IsAuthorized(ShellCore Shell, string Username, string Password, string RemoteAddress);
    }
}
