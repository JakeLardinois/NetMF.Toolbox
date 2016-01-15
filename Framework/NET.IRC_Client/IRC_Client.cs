using System;
using System.Threading;

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
namespace Toolbox.NETMF.NET
{
    /// <summary>
    /// IRC Client
    /// </summary>
    /// <remarks>
    /// This client contains the most basic features to stay connected to an IRC server.
    /// It can also reply to the CTCP commands VERSION, TIME and PING.
    /// To disable or change the CTCP replies, just create your own OnCtcpRequest method.
    /// </remarks>
    public class IRC_Client : IDisposable
    {
        #region "Core"
        /// <summary>Contains the socket wrapper</summary>
        private SimpleSocket _Socket;

        /// <summary>Main loop thread</summary>
        private Thread _LoopThread;

        /// <summary>The nickname of the client</summary>
        private string _Nickname;
        /// <summary>The username of the client</summary>
        private string _Username;
        /// <summary>The full name of the client</summary>
        private string _Fullname;
        /// <summary>Optional password</summary>
        private string _Password;

        /// <summary>Contains the clientversion</summary>
        private string _ClientVersion = "";

        /// <summary>Returns the clientversion</summary>
        public string ClientVersion { get { return this._ClientVersion; } }

        /// <summary>True when the user is authenticated by the remote server</summary>
        private bool _Authenticated = false;
        /// <summary>True when the user is authenticated by the remote server</summary>
        public bool Authenticated { get { return this._Authenticated; } }

        /// <summary>Contains the name of the IRC server</summary>
        private string _ServerName = "";
        /// <summary>Returns the name of the IRC server</summary>
        public string ServerName { get { return this._ServerName; } }

        /// <summary>
        /// Connects to an IRC server
        /// </summary>
        /// <param name="Socket">The socket to use</param>
        /// <param name="Nickname">Nickname</param>
        /// <param name="Username">Username (optional)</param>
        /// <param name="Fullname">Full name (optional)</param>
        /// <param name="Password">Password to connect to the server (optional)</param>
        public IRC_Client(SimpleSocket Socket, string Nickname, string Username = "", string Fullname = "", string Password = "")
        {
            // What kind of device do we run on?
            string[] SocketProvider = Socket.ToString().Split(new char[] { '.' });
            string[] Client = this.ToString().Split(new char[] { '.' });
            // Version
            this._ClientVersion = "NETMFToolbox/0.1 (" + Tools.HardwareProvider + "; " + SocketProvider[SocketProvider.Length - 1] + "; " + Client[Client.Length - 1] + ")";

            // Stores all fields
            this._Socket = Socket;
            this._Nickname = Nickname;
            this._Username = (Username == "") ? Nickname : Username;
            this._Fullname = (Fullname == "") ? this._ClientVersion : Fullname;
            this._Password = Password;
            this._ServerName = Socket.Hostname;

            // Configures the socket
            this._Socket.LineEnding = "\n";

            // Creates a new background thread
            this._LoopThread = new Thread(new ThreadStart(this._Loop));
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        /// <summary>
        /// Connects to the IRC server
        /// </summary>
        public void Connect()
        {
            this._Socket.Connect();
            if (this._Password != "") this.SendRaw("PASS " + this._Password);
            this.SendRaw("NICK " + this._Nickname);
            this.SendRaw("USER " + this._Username + " 0 " + this._Socket.Hostname + " :" + this._Fullname);
            this._LoopThread.Start();
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Disconnect()
        {
            if (this._Socket.IsConnected)
            {
                this.SendRaw("QUIT :Client disconnected");
                this._Socket.Close();
            }
            this._LoopThread.Abort();
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void _Loop()
        {
            // Infinite loop, we keep trying to read data
            while (true)
            {
                string Data = this._Socket.Receive();
                if (Data != "")
                    this._DataReceived(Data.TrimEnd("\r\n".ToCharArray()));
                else
                    // Give other threads some time as well
                    Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Gets or sets the nickname
        /// </summary>
        public string Nickname
        {
            get { return this._Nickname; }
            set { this.SendRaw("NICK :" + value); }
        }

        #endregion

        #region "Receive methods"
        /// <summary>Event triggered when string data is received</summary>
        /// <param name="Sender">The sender of the data</param>
        /// <param name="Target">The target of the data</param>
        /// <param name="Data">The data</param>
        /// <remarks>A very generic method, can be used for a lot of events</remarks>
        public delegate void OnStringReceived(string Sender, string Target, string Data);

        /// <summary>Event triggered when raw data is received from the remote server</summary>
        public event OnStringReceived OnRawReceived;
        /// <summary>Event triggered when a notice is received</summary>
        public event OnStringReceived OnNotice;
        /// <summary>Event triggered when a message is received</summary>
        public event OnStringReceived OnMessage;
        /// <summary>Event triggered when an action is received</summary>
        public event OnStringReceived OnAction;
        /// <summary>Event triggered when a CTCP-request is received</summary>
        public event OnStringReceived OnCtcpRequest;
        /// <summary>Event triggered when a CTCP-reply is received</summary>
        public event OnStringReceived OnCtcpReply;
        /// <summary>Event triggered when the user is fully logged in</summary>
        public event OnStringReceived OnAuthenticated;
        /// <summary>Event triggered when a user joins a channel</summary>
        public event OnStringReceived OnJoin;
        /// <summary>Event triggered when a user parts a channel</summary>
        public event OnStringReceived OnPart;
        /// <summary>Event triggered when a user quits the server</summary>
        public event OnStringReceived OnQuit;
        /// <summary>Event triggered when a user is kicked from a channel</summary>
        public event OnStringReceived OnKick;
        /// <summary>Event triggered when a user changes its name</summary>
        public event OnStringReceived OnNick;

        /// <summary>Triggered for every line of data received by the server</summary>
        /// <param name="Data">The received line of data</param>
        private void _DataReceived(string Data)
        {
            // Triggers the OnRawReceived event for all data
            if (this.OnRawReceived != null) this.OnRawReceived("", "", Data);

            // Splits on spaces to detect the command
            string[] DataSplit = SplitRawData(Data);

            // Default ping/pong, just send back the full response
            if (DataSplit[0] == "PING") {
                this.SendRaw("PONG " + Data.Substring(5));
                return;
            }

            // There are two commands, commands directly from the server,
            // and commands from another party
            if (DataSplit[0].Substring(0, 1) == ":")
            {
                // We got a command from another party
                DataSplit[0] = DataSplit[0].Substring(1);
            }
            else
            {
                // We got data from the server, lets shift some stuff
                string[] NewSplit = new string[DataSplit.Length + 1];
                DataSplit.CopyTo(NewSplit, 1);
                DataSplit = NewSplit;
            }

            // Now we can safely assume the 1st value is empty or the sender,
            // and the 2nd one is the command.
            switch (DataSplit[1])
            {
                case "001": // Welcome message, confirms the current nickname
                    this._ServerName = DataSplit[0];
                    this._Nickname = DataSplit[2];
                    this._Authenticated = true;
                    if (this.OnAuthenticated != null) this.OnAuthenticated(DataSplit[0], DataSplit[2], DataSplit[3]);
                    break;
                case "433": // Nickname already in use
                    int Counter = 0;
                    if (DataSplit[3] == this._Nickname)
                        Counter = 1;
                    else
                        Counter = int.Parse(DataSplit[3].Substring(this._Nickname.Length)) + 1;
                    this.SendRaw("NICK " + this._Nickname + Counter.ToString());
                    break;
                case "NICK": // Someone changes his name
                    // Is it my own name?
                    if (this._Nickname == SplitName(DataSplit[0])[0])
                        this._Nickname = DataSplit[2];
                    // Do we need to send back an event?
                    if (this.OnNick != null)
                        this.OnNick(DataSplit[0], DataSplit[2], "");
                    break;
                case "ACTION":
                    if (this.OnAction != null) this.OnAction(DataSplit[0], DataSplit[2], DataSplit[3]);
                    break;
                case "NOTICE":
                    if (DataSplit[3].Substring(0, 1) == "\x01" && DataSplit[3].Substring(DataSplit[3].Length - 1, 1) == "\x01")
                    {
                        // CTCP Reply
                        DataSplit[3] = DataSplit[3].Trim(new char[] { (char)1 });
                        if (this.OnCtcpReply != null) this.OnCtcpReply(DataSplit[0], DataSplit[2], DataSplit[3]);
                    }
                    else
                    {
                        // Notice
                        if (this.OnNotice != null) this.OnNotice(DataSplit[0], DataSplit[2], DataSplit[3]);
                    }
                    break;
                case "PRIVMSG":
                    if (DataSplit[3].Substring(0, 1) == "\x01" && DataSplit[3].Substring(DataSplit[3].Length - 1, 1) == "\x01")
                    {
                        // CTCP Request
                        DataSplit[3] = DataSplit[3].Trim(new char[] { (char)1 }).ToUpper();
                        if (this.OnCtcpRequest == null)
                        {
                            // No CTCP Request event programmed, we're going to do it ourselves
                            if (DataSplit[3] == "VERSION")
                                this.CtcpResponse(SplitName(DataSplit[0])[0], "VERSION " + this.ClientVersion);
                            if (DataSplit[3] == "TIME")
                                this.CtcpResponse(SplitName(DataSplit[0])[0], "TIME " + IRC_Client.Time);
                            if (DataSplit[3].Substring(0, 4) == "PING")
                                this.CtcpResponse(SplitName(DataSplit[0])[0], "PING" + DataSplit[3].Substring(4));
                        }
                        else
                        {
                            this.OnCtcpRequest(DataSplit[0], DataSplit[2], DataSplit[3]);
                        }
                    }
                    else
                    {
                        // Message
                        if (this.OnMessage != null) this.OnMessage(DataSplit[0], DataSplit[2], DataSplit[3]);
                    }
                    break;
                case "JOIN": // User joins a channel
                    if (this.OnJoin != null) this.OnJoin(DataSplit[0], DataSplit[2], "");
                    break;
                case "PART": // User parts a channel
                    if (this.OnPart != null) this.OnPart(DataSplit[0], DataSplit[2], DataSplit.Length > 3 ? DataSplit[3] : "");
                    break;
                case "KICK": // User has been kicked from a channel
                    if (this.OnKick != null) this.OnKick(DataSplit[0], DataSplit[3], DataSplit[2] + " " + DataSplit[4]);
                    break;
                case "QUIT": // User leaves the server
                    if (this.OnQuit != null) this.OnQuit(DataSplit[0], DataSplit[2], DataSplit[3]);
                    // Its us! Lets close the rest
                    if (this._Username == IRC_Client.SplitName(DataSplit[0])[0]) this.Disconnect();
                    break;
            }
        }
        #endregion

        #region "Send methods"
        /// <summary>Sends a CTCP Response</summary>
        /// <param name="Recipient">The recipient (may be a user or a channel)</param>
        /// <param name="Data">Data to send</param>
        public void CtcpResponse(string Recipient, string Data)
        {
            this.Notice(Recipient, "\x01" + Data + "\x01");
        }

        /// <summary>Sends a CTCP Request</summary>
        /// <param name="Recipient">The recipient (may be a user or a channel)</param>
        /// <param name="Data">Data to send</param>
        public void CtcpRequest(string Recipient, string Data)
        {
            this.Message(Recipient, "\x01" + Data + "\x01");
        }

        /// <summary>Sends an action</summary>
        /// <param name="Recipient">The recipient (may be a user or a channel)</param>
        /// <param name="Data">Data to send</param>
        public void Action(string Recipient, string Data)
        {
            this.SendRaw("ACTION " + Recipient + " :" + Data);
        }

        /// <summary>Sends a notice</summary>
        /// <param name="Recipient">The recipient (may be a user or a channel)</param>
        /// <param name="Data">Data to send</param>
        public void Notice(string Recipient, string Data)
        {
            this.SendRaw("NOTICE " + Recipient + " :" + Data);
        }

        /// <summary>Sends a message</summary>
        /// <param name="Recipient">The recipient (may be a user or a channel)</param>
        /// <param name="Data">Data to send</param>
        public void Message(string Recipient, string Data)
        {
            this.SendRaw("PRIVMSG " + Recipient + " :" + Data);
        }

        /// <summary>
        /// Joins one or more channels
        /// </summary>
        /// <param name="Channels">The channel to join (multiple can be comma seperated)</param>
        /// <param name="Passwords">Optional, the password(s) to join the channel(s)</param>
        public void Join(string Channels, string Passwords = "")
        {
            this.SendRaw("JOIN " + Channels + (Passwords != "" ? " " + Passwords : ""));
        }

        /// <summary>
        /// Parts a channel
        /// </summary>
        /// <param name="Channel">The channel to leave</param>
        /// <param name="Reason">The reason to leave the channel (optional)</param>
        public void Part(string Channel, string Reason = "")
        {
            this.SendRaw("PART " + Channel + (Reason == "" ? "" : " :" + Reason));
        }

        /// <summary>
        /// Sends raw data to the remote server
        /// </summary>
        /// <param name="Data">Data to send</param>
        public void SendRaw(string Data)
        {
            this._Socket.Send(Data + "\n");
        }
        #endregion

        #region "Static string modifiers"
        /// <summary>Gets the current local time</summary>
        /// <returns>The local time as string</returns>
        public static string Time
        {
            get
            {
                string[] Days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

                DateTime Now = DateTime.Now;
                return Days[(int)Now.DayOfWeek - 1] + " " + Months[Now.Month - 1] + " " + Now.Day.ToString() + " " + Now.TimeOfDay.ToString().Substring(0, 8) + " " + Now.Year.ToString();
            }
        }

        /// <summary>
        /// Splits data according to the IRC protocol
        /// </summary>
        /// <param name="Data">Input data</param>
        /// <returns>Output data</returns>
        public static string[] SplitRawData(string Data)
        {
            // Checks for longer string values
            int BreakPoint = Data.IndexOf(" :");
            // No longer string values
            if (BreakPoint < 0) return Data.Split(new char[] { ' ' });

            // Splits all regular values
            string[] Values = Data.Substring(0, BreakPoint).Split(new char[] { ' ' });
            // Creates a new array and copies the regular values
            string[] RetValue = new string[Values.Length + 1];
            Values.CopyTo(RetValue, 0);
            // Adds the longer string value
            RetValue[RetValue.Length - 1] = Data.Substring(BreakPoint + 2);

            return RetValue;
        }

        /// <summary>
        /// Returns the username splitted (many IRCds send "[nickname]![username]@[hostname]")
        /// </summary>
        /// <param name="Name">The full name</param>
        /// <returns>An array with 3 values: nickname, username, hostname</returns>
        public static string[] SplitName(string Name)
        {
            // First the nickname
            string[] Split1 = Name.Split(new char[] { '!' }, 2);
            if (Split1.Length == 1) return new string[3] { Name, "", "" };
            // Now the username/host
            string[] Split2 = Split1[1].Split(new char[] { '@' }, 2);
            if (Split2.Length == 1) return new string[3] { Split1[0], Split1[1], "" };
            // We return everything
            return new string[3] { Split1[0], Split2[0], Split2[1] };
        }
        #endregion
    }
}
