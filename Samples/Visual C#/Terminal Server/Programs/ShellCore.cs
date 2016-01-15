using System;
using System.Threading;
using Microsoft.SPOT;
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
    /// Shell core
    /// </summary>
    public class ShellCore
    {
        /// <summary>
        /// Reference to the TelnetServer object
        /// </summary>
        public TelnetServer TelnetServer { get; protected set; }

        /// <summary>
        /// Command line prompt prefix
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// The last enterred command line
        /// </summary>
        public string LastCommandline { get; protected set; }

        /// <summary>
        /// When running in the background, this thread is used
        /// </summary>
        private Thread _BgThread;

        /// <summary>
        /// Initializes a new Shell core
        /// </summary>
        /// <param name="Socket">Socket to use</param>
        public ShellCore(SimpleSocket Socket)
        {
            this.TelnetServer = new TelnetServer(Socket);
            this.Prompt = ">";
        }

        /// <summary>
        /// Starts listening
        /// </summary>
        public void Start() { this.Start(false); }

        /// <summary>
        /// Starts listening
        /// </summary>
        /// <param name="Background">Set to true to start the shell in the background</param>
        public void Start(bool Background)
        {
            if (Background)
            {
                // Creates a background thread
                ThreadStart BgThread = new ThreadStart(this.Start);
                this._BgThread = new Thread(BgThread);
                this._BgThread.Start();
                return;
            }

            this.TelnetServer.Listen();
            if (this.OnConnected != null) this.OnConnected(this, this.TelnetServer.RemoteAddress, DateTime.Now);

            while (this.TelnetServer.IsConnected)
            {
                this.TelnetServer.Color(TelnetServer.Colors.Yellow);
                this.TelnetServer.Print(this.Prompt + " ", true);
                this.TelnetServer.Color(TelnetServer.Colors.White);
                string Command = this.TelnetServer.Input();
                if (Command != "") this._ExecuteCommand(Command);
            }

            if (this.OnDisconnected != null) this.OnDisconnected(this, this.TelnetServer.RemoteAddress, DateTime.Now);
        }

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="CommandLine">The command line</param>
        private void _ExecuteCommand(string CommandLine)
        {
            this.LastCommandline = CommandLine;
            if (this.OnCommandReceived != null)
            {
                // Splits up the command line in arguments
                string[] Arguments = CommandLine.Split(new char[] { ' ' });
                // By default, we don't suspress the error "Command unknown"
                bool SuspressError = false;
                // Tries to execute the command
                try
                {
                    this.OnCommandReceived(this, Arguments, ref SuspressError, DateTime.Now);
                }
                // If we fail executing the command, an error message will show up
                catch (Exception Error)
                {
                    this._PrintError("An exception has been triggered: " + Error.Message);
                    SuspressError = true;
                }
                // If SuspressError hasn't been set to True, we show the error "Command unknown"
                if (!SuspressError)
                    this._PrintError("Command " + Arguments[0].ToUpper() + " unknown");
            }
        }

        /// <summary>
        /// Prints out an error
        /// </summary>
        /// <param name="Text">The error to show</param>
        private void _PrintError(string Text)
        {
            this.TelnetServer.Color(TelnetServer.Colors.LightRed);
            this.TelnetServer.Print(Text);
            this.TelnetServer.Color(TelnetServer.Colors.White);
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        public event CommandReceived OnCommandReceived;

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        public delegate void CommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time);

        /// <summary>
        /// Triggered when the connection has been made
        /// </summary>
        public event ConnectionStateChange OnConnected;
        /// <summary>
        /// Triggered when the connection has been lost
        /// </summary>
        public event ConnectionStateChange OnDisconnected;

        /// <summary>
        /// Triggered when the connection state changes
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="RemoteAddress">Hostname of the user</param>
        /// <param name="Time">Timestamp of the event</param>
        public delegate void ConnectionStateChange(ShellCore Shell, string RemoteAddress, DateTime Time);
    }
}
