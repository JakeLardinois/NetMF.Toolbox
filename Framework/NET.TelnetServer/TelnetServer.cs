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
namespace Toolbox.NETMF.NET
{
    /// <summary>
    /// A telnet server with minimal TELNET and ANSI support
    /// </summary>
    /// <remarks>
    /// I found these links very useful:
    /// ANSI escapes: http://isthe.com/chongo/tech/comp/ansi_escapes.html
    /// Telnet commands: http://www.networksorcery.com/enp/protocol/telnet.htm
    /// </remarks>
    public class TelnetServer
    {
        private const char IAC = (char)255;   // Interpret as Command 
        private const char WILL = (char)251;
        private const char WONT = (char)252;
        private const char DO = (char)253;
        private const char DONT = (char)254;
        private const char ECHO = (char)1;
        private const char SUPPRESSGOAHEAD = (char)3;

        /// <summary>When true, user inputted data gets echoed</summary>
        public bool EchoEnabled { get; set; }

        /// <summary>Reference to the socket</summary>
        private SimpleSocket _Sock;

        /// <summary>Local data buffer</summary>
        private string _Buffer = "";

        /// <summary>
        /// Returns the amount of bytes waiting for this.Input()
        /// </summary>
        public int InputBuffer { get { this._Read(); return this._Buffer.Length; } }

        /// <summary>The client address</summary>
        public string RemoteAddress { get { return this._Sock.Hostname; } }

        /// <summary>Some commands, like color, will be buffered to avoid small data packets. This actually speeds up the server.</summary>
        private string _WriteBuffer = "";
        
        /// <summary>
        /// Creates a new telnet server
        /// </summary>
        /// <param name="Socket">The socket</param>
        public TelnetServer(SimpleSocket Socket)
        {
            this._Sock = Socket;
            this.EchoEnabled = true;
            // We handle our line endings ourself
            this._Sock.LineEnding = "";
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Close()
        {
            this._Sock.Close();
        }

        /// <summary>
        /// True when there's a connection
        /// </summary>
        public bool IsConnected { get { return this._Sock.IsConnected; } }

        /// <summary>
        /// Listens and waits until we have a connection
        /// </summary>
        public void Listen()
        {
            this._Sock.Listen();
            // We handle echoing ourselves
            this._Write(new char[] { IAC, DONT, ECHO }, true);
            this._Write(new char[] { IAC, WILL, ECHO }, true);
            // We want to receive every byte when it's received, don't wait for it!
            this._Write(new char[] { IAC, DO, SUPPRESSGOAHEAD }, true);
            this._Write(new char[] { IAC, WILL, SUPPRESSGOAHEAD });
            Thread.Sleep(100);
            this._Read();
        }

        /// <summary>
        /// Writes data to the client
        /// </summary>
        /// <param name="Data">Data to write</param>
        /// <param name="Buffered">When true, data will be sent in front of the next packet</param>
        private void _Write(string Data, bool Buffered = false)
        {
            if (Buffered)
            {
                this._WriteBuffer += Data;
            }
            else
            {
                string Out = this._WriteBuffer + Data;
                this._WriteBuffer = "";
                if (this.IsConnected) this._Sock.Send(Out);
            }
        }

        /// <summary>
        /// Writes data to the client
        /// </summary>
        /// <param name="Data">Data to write</param>
        /// <param name="Buffered">When true, data will be sent in front of the next packet</param>
        private void _Write(char[] Data, bool Buffered = false)
        {
            this._Write(new string(Data), Buffered);
        }

        /// <summary>
        /// Sends a beep to the client
        /// </summary>
        public void Beep()
        {
            this._Write("\x07");
        }

        /// <summary>
        /// Reads data from the client
        /// </summary>
        private void _Read()
        {
            string NewData = this._Sock.Receive();
            if (NewData == "") return;
            for (int i = 0; i < NewData.Length; ++i)
            {
                if (NewData[i] == IAC)
                {
                    char Command = NewData[++i];
                    // Lets only support DO, DONT, WILL and WONT for now
                    if (Command == DO || Command == DONT || Command == WILL || Command == WONT)
                    {
                        // Lets see what the other party wants to tell us
                        char Option = NewData[++i];
                        // Server will echo
                        if (Command == DO && Option == ECHO) this._Write(new char[] { IAC, WILL, ECHO });
                        // Client shouldn't echo
                        else if (Command == WILL && Option == ECHO) this._Write(new char[] { IAC, DONT, ECHO });
                        // We will suppress go-aheads
                        else if (Command == DO && Option == SUPPRESSGOAHEAD) this._Write(new char[] { IAC, WILL, SUPPRESSGOAHEAD });
                        // Fine that you will ;-)
                        else if (Command == WILL) { /* Ignored */ }
                        // But I won't do the rest!
                        else this._Write(new char[] { IAC, WONT, Option });
                    }
                }
                else
                {
                    // Backspace
                    if (NewData[i] == '\x08')
                    {
                        if (this._Buffer.Length > 0)
                        {
                            this._Buffer = this._Buffer.Substring(0, this._Buffer.Length - 1);
                            if (EchoEnabled) this._Write("\x08 \x08");
                        }
                    }
                    // Escape
                    else if (NewData[i] == '\x1b')
                    {
                        if (this._Buffer.Length > 0)
                        {
                            if (EchoEnabled)
                                for (int Cnt = 0; Cnt < this._Buffer.Length; ++Cnt)
                                    this._Write("\x08 \x08");
                            this._Buffer = "";
                        }
                    }
                    // All other characters
                    else
                    {
                        this._Buffer += NewData[i];
                        if (EchoEnabled) this._Write(NewData.Substring(i, 1));
                    }
                }
            }
        }

        /// <summary>
        /// Reads out input from the terminal
        /// </summary>
        /// <param name="Length">The amount of bytes to read, if 0, it reads until a carriage return</param>
        /// <param name="Blocking">When set to false, it won't wait for data, it will just return empty if there's no data</param>
        public string Input(int Length = 0, bool Blocking = true)
        {
            while (true)
            {
                if (!this.IsConnected) return "";

                this._Read();

                // Amount of bytes?
                if (Length > 0 && this._Buffer.Length >= Length)
                {
                    // Gets the data
                    string Data = this._Buffer.Substring(0, Length);
                    // Removes the data from the buffer
                    this._Buffer = this._Buffer.Substring(Length);
                    return Data;
                }

                // Carriage return?
                int Pos = this._Buffer.IndexOf('\r');
                if (Length == 0 && Pos >= 0)
                {
                    // Gets the data
                    string Data = this._Buffer.Substring(0, Pos);
                    // Removes the data from the buffer
                    this._Buffer = this._Buffer.Substring(Pos + 1);
                    // Removes additional newline if sent
                    if (this._Buffer.Substring(0, 1) == "\n") this._Buffer = this._Buffer.Substring(1);
                    return Data;
                }

                // Ctrl+C ?
                Pos = this._Buffer.IndexOf('\x03');
                if (Length == 0 && Pos >= 0)
                {
                    if (this.EchoEnabled) this.Print("\x08^C\r\n");
                    this._Buffer = "";
                    return "\x03";
                }

                // Do we disable blocking?
                if (!Blocking) return "";

                Thread.Sleep(100);
            }
        }
        
        /// <summary>
        /// Prints a text to the screen
        /// </summary>
        /// <param name="Text">Text to print</param>
        /// <param name="NoNewLine">Normally a Print call will print the text and go to the next line. Set this to true to avoid that behaviour.</param>
        /// <param name="Buffered">When true, data won't be sent immediately but stored in a buffer.</param>
        public void Print(string Text, bool NoNewLine = false, bool Buffered = false)
        {
            if (NoNewLine)
                this._Write(Text, Buffered);
            else
                this._Write(Text + "\r\n", Buffered);
        }

        /// <summary>Clears the terminal's screen</summary>
        public void ClearScreen()
        {
            this._Write("\x1b[2J");
        }

        /// <summary>Available colors</summary>
        public enum Colors
        {
            /// <summary>Black</summary>
            Black = 0,
            /// <summary>Blue</summary>
            Blue = 1,
            /// <summary>Green</summary>
            Green = 2,
            /// <summary>Cyan</summary>
            Cyan = 3,
            /// <summary>Red</summary>
            Red = 4,
            /// <summary>Magenta</summary>
            Magenta = 5,
            /// <summary>Brown/orange</summary>
            Brown = 6,
            /// <summary>White</summary>
            White = 7,
            /// <summary>Gray</summary>
            Gray = 8,
            /// <summary>Light blue</summary>
            LightBlue = 9,
            /// <summary>Light green</summary>
            LightGreen = 10,
            /// <summary>Light cyan</summary>
            LightCyan = 11,
            /// <summary>Light red</summary>
            LightRed = 12,
            /// <summary>Light magenta</summary>
            LightMagenta = 13,
            /// <summary>Yellow</summary>
            Yellow = 14,
            /// <summary>High-intensity white</summary>
            HighIntensityWhite = 15,
            /// <summary>The default of the terminal</summary>
            TerminalDefault = 255,
        }

        /// <summary>
        /// Sets the terminal colors
        /// </summary>
        /// <param name="Foreground">Forground color</param>
        public void Color(Colors Foreground)
        {
            this.Color(Foreground, (Colors)17);
        }

        /// <summary>
        /// Sets the terminal colors
        /// </summary>
        /// <param name="Foreground">Forground color</param>
        /// <param name="Background">Background color</param>
        public void Color(Colors Foreground, Colors Background)
        {
            string Data = "";
            // Foreground color parameters
            if ((int)Foreground == 0) Data += ";00;30";  // black
            if ((int)Foreground == 1) Data += ";00;34";  // blue
            if ((int)Foreground == 2) Data += ";00;32";  // green
            if ((int)Foreground == 3) Data += ";00;36";  // cyan
            if ((int)Foreground == 4) Data += ";00;31";  // red
            if ((int)Foreground == 5) Data += ";00;35";  // magenta
            if ((int)Foreground == 6) Data += ";00;33";  // brown
            if ((int)Foreground == 7) Data += ";00;37";  // white
            if ((int)Foreground == 8) Data += ";01;30";  // gray
            if ((int)Foreground == 9) Data += ";01;34";  // light blue
            if ((int)Foreground == 10) Data += ";01;32"; // light green
            if ((int)Foreground == 11) Data += ";01;36"; // light cyan
            if ((int)Foreground == 12) Data += ";01;31"; // light red
            if ((int)Foreground == 13) Data += ";01;35"; // light magenta
            if ((int)Foreground == 14) Data += ";01;33"; // yellow
            if ((int)Foreground == 15) Data += ";01;37"; // high-intensity white
            if ((int)Foreground == 255) Data += ";00;39";// Default color
            // Background color parameters
            if ((int)Background == 0) Data += ";40";     // black
            if ((int)Background == 1) Data += ";44";     // blue
            if ((int)Background == 2) Data += ";42";     // green
            if ((int)Background == 3) Data += ";46";     // cyan
            if ((int)Background == 4) Data += ";41";     // red
            if ((int)Background == 5) Data += ";45";     // magenta
            if ((int)Background == 6) Data += ";43";     // brown
            if ((int)Background == 7) Data += ";47";     // white
            if ((int)Background == 8) Data += ";40";     // black
            if ((int)Background == 9) Data += ";44";     // blue
            if ((int)Background == 10) Data += ";42";    // green
            if ((int)Background == 11) Data += ";46";    // cyan
            if ((int)Background == 12) Data += ";41";    // red
            if ((int)Background == 13) Data += ";45";    // magenta
            if ((int)Background == 14) Data += ";43";    // brown
            if ((int)Background == 15) Data += ";47";    // white
            if ((int)Background == 255) Data += ";49";   // Default color

            if (Data != "")
                this._Write("\x1b[" + Data.Substring(1) + "m", true);
        }

        /// <summary>
        /// Moves the cursor position
        /// </summary>
        /// <param name="Line">Line (starts at 1 instead of 0!)</param>
        /// <param name="Column">Column (starts at 1 instead of 0!)</param>
        /// <param name="Buffered">When true, data won't be sent immediately but stored in a buffer.</param>
        public void Locate(int Line, int Column, bool Buffered = false)
        {
            this._Write("\x1b[" + Line.ToString() + ";" + Column.ToString() + "f", Buffered);
        }
    }
}
