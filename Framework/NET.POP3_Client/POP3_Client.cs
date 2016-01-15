using System;
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
    /// .NETMF POP3 Client
    /// </summary>
    public class POP3_Client
    {
        /// <summary>Reference to the POP3 username</summary>
        private string _POP3_User = "";
        /// <summary>Reference to the POP3 password</summary>
        private string _POP3_Pass = "";
        /// <summary>Reference to the socket wrapper</summary>
        private SimpleSocket _Socket = null;
        /// <summary>The amount of messages on the server</summary>
        private uint _MessageCount = 0;
        /// <summary>The size of the mailbox on the server in bytes</summary>
        private uint _BoxSize = 0;

        /// <summary>The amount of messages on the server</summary>
        public uint MessageCount { get { return this._MessageCount; } }
        /// <summary>The size of the mailbox on the server in bytes</summary>
        public uint BoxSize { get { return this._BoxSize; } }

        /// <summary>
        /// Initializes a mail client
        /// </summary>
        /// <param name="Socket">The socket to use (default TCP port for POP3 is 110)</param>
        /// <param name="Username">Username for the POP3 server</param>
        /// <param name="Password">Password for the POP3 server</param>
        public POP3_Client(SimpleSocket Socket, string Username, string Password)
        {
            // Copies all parameters to the global scope
            this._Socket = Socket;
            this._POP3_User = Username;
            this._POP3_Pass = Password;
            // Defines that we use line endings with the linefeed character
            this._Socket.LineEnding = "\n";
        }

        /// <summary>Connects to the POP3 server</summary>
        public void Connect() 
        {
            // Used for the answers of the POP3 server
            string Text = "";
            // Makes the actual connection
            this._Socket.Connect();

            string Error = "";
            do
            { // We will use break, also when an error occures, so we always close the connection nicely
                // Reads and validates the first line of data
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "+OK") { Error = Text; break; }
                // Sends the username
                this._Socket.Send("USER " + this._POP3_User + "\r\n");
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "+OK") { Error = Text; break; }
                // Sends the password
                this._Socket.Send("PASS " + this._POP3_Pass + "\r\n");
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "+OK") { Error = Text; break; }
            } while (false);

            if (Error != "")
            {
                throw new SystemException(Error);
            }

            this._Stat();
        }

        /// <summary>
        /// Updates the current mailbox statistics
        /// </summary>
        private void _Stat()
        {
            // Requests the amount of messages and the box size
            this._Socket.Send("STAT\r\n");
            string Text = this._Socket.Receive(true);
            if (Text.Substring(0, 3) != "+OK")
                throw new SystemException(Text);
            string[] Values = Text.TrimEnd("\r\n".ToCharArray()).Split(" ".ToCharArray(), 4);
            this._MessageCount = uint.Parse(Values[1]);
            this._BoxSize = uint.Parse(Values[2]);
        }

        /// <summary>
        /// Fetches the headers of a mail message
        /// </summary>
        /// <param name="Id">The message Id</param>
        /// <returns>The headers as one big string</returns>
        private string _Top(uint Id)
        {
            // Requests the list of mails
            this._Socket.Send("TOP " + Id.ToString() + " 0\r\n");
            string Text = this._Socket.Receive(true);
            if (Text.Substring(0, 3) != "+OK")
                throw new SystemException(Text);

            // Reads until we get a dot
            string Return = "";
            while (Text.TrimEnd("\r\n".ToCharArray()) != ".")
            {
                Text = this._Socket.Receive(true);
                Return += Text;
            }

            return Return.TrimEnd(".\r\n".ToCharArray());
        }

        /// <summary>
        /// Fetches the body of a mail message
        /// </summary>
        /// <param name="Id">The message Id</param>
        /// <param name="IncludeHeaders">When true, mail headers will also be included</param>
        /// <returns></returns>
        public string FetchBody(uint Id, bool IncludeHeaders = false)
        {
            // If we want to include the headers, we need to start parsing immediately
            bool StartParsing = IncludeHeaders;

            // Requests the mail
            this._Socket.Send("RETR " + Id.ToString() + "\r\n");
            string Text = this._Socket.Receive(true);
            if (Text.Substring(0, 3) != "+OK")
                throw new SystemException(Text);

            // Reads until we get a dot
            string Return = "";
            while (Text.TrimEnd("\r\n".ToCharArray()) != ".")
            {
                Text = this._Socket.Receive(true);
                if (Text.Trim() == "" && StartParsing == false)
                {
                    Text = "";
                    StartParsing = true;
                }
                if (StartParsing) 
                    Return += Text;
            }

            return Return.TrimEnd(".\r\n".ToCharArray());
        }

        /// <summary>
        /// Fetches the headers of a mail message
        /// </summary>
        /// <param name="Id">The message Id</param>
        /// <returns>An array with a header in each index</returns>
        public string[] FetchHeaders(uint Id)
        {
            // Fetches the actual headers
            string[] Headers = this._Top(Id).Split("\n".ToCharArray());
            
            // Migrates headers that are on multiple lines
            int LastIndex = 0;
            int HeaderCount = Headers.Length;
            for (int Index = 0; Index < Headers.Length; ++Index)
            {
                // Adds the removed whitespace back for now
                Headers[Index] += "\n";
                if (Headers[Index].Substring(0, 1) != " " && Headers[Index].Substring(0, 1) != "\t")
                {
                    LastIndex = Index;
                }
                else
                {
                    Headers[LastIndex] += Headers[Index];
                    Headers[Index] = "";
                    --HeaderCount;
                }
            }

            // Removes empty headers
            string[] CleanHeaders = new string[HeaderCount];
            LastIndex = 0;
            for (int Index = 0; Index < Headers.Length; ++Index)
            {
                if (Headers[Index].Trim() != "")
                {
                    CleanHeaders[LastIndex] = Headers[Index].Trim();
                    ++LastIndex;
                }
            }

            return CleanHeaders;
        }

        /// <summary>
        /// Fetches specific headers of a mail message
        /// </summary>
        /// <param name="Id">The message Id</param>
        /// <param name="Headers">The names of the headers</param>
        /// <returns>An array with a header in each index</returns>
        public string[] FetchHeaders(uint Id, string[] Headers)
        {
            // Fetches all headers
            string[] FetchedHeaders = this.FetchHeaders(Id);

            // Sorts out which headers we want
            string[] ReturnHeaders = new string[Headers.Length];
            for (int FetchIndex = 0; FetchIndex < FetchedHeaders.Length; ++FetchIndex)
            {
                for (int ReturnIndex = 0; ReturnIndex < ReturnHeaders.Length; ++ReturnIndex)
                {
                    string search = Headers[ReturnIndex].ToLower() + ":";
                    if (FetchedHeaders[FetchIndex].Substring(0, search.Length).ToLower() == search)
                        ReturnHeaders[ReturnIndex] = FetchedHeaders[FetchIndex].Substring(search.Length).Trim();
                }
            }

            return ReturnHeaders;
        }

        /// <summary>
        /// Fetches a list of emails from the server
        /// </summary>
        /// <param name="Id">The unique ID of the mail (unique for the current session)</param>
        /// <param name="Size">The size of the mail in bytes</param>
        public void ListMails(out uint[] Id, out uint[] Size)
        {
            // Expands the arrays to the correct dimentions
            Id = new uint[this._MessageCount];
            Size = new uint[this._MessageCount];

            // Requests the list of mails
            this._Socket.Send("LIST\r\n");
            string Text = this._Socket.Receive(true);
            if (Text.Substring(0, 3) != "+OK")
                throw new SystemException(Text);

            // Reads all rows
            for (uint Index = 0; Index < this._MessageCount; ++Index)
            {
                Text = this._Socket.Receive(true);
                string[] Values = Text.TrimEnd("\r\n".ToCharArray()).Split(" ".ToCharArray(), 3);
                Id[Index] = uint.Parse(Values[0]);
                Size[Index] = uint.Parse(Values[1]);
            }

            // We should end with a single .
            Text = this._Socket.Receive(true).TrimEnd("\r\n".ToCharArray());
            if (Text != ".") throw new IndexOutOfRangeException();
        }

        /// <summary>Closes the connection</summary>
        public void Close() 
        {
            this._Socket.Send("QUIT\r\n");
            this._Socket.Close();
        }
        /// <summary>Returns true when connected</summary>
        public bool IsConnected { get { return this._Socket.IsConnected; } }
    }
}
