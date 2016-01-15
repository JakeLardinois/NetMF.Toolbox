using System;
using Microsoft.SPOT;

/*
 * Copyright 2011-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
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
    /// .NETMF SMTP Client
    /// </summary>
    public class SMTP_Client
    {
        /// <summary>
        /// Small container for mail contacts
        /// </summary>
        public class MailContact
        {
            /// <summary>The full name of the person</summary>
            public string Name { get; set; }
            /// <summary>The mail address of the person</summary>
            public string MailAddress { get; set; }
            /// <summary>Returns the name and mail address as a string</summary>
            /// <returns>Returns the name and mail address as a string</returns>
            public override string ToString()
            {
                if (this.Name == "") return "<" + this.MailAddress + ">";
                return "\"" + this.Name + "\" <" + this.MailAddress + ">";
            }
            /// <summary>Creates a new mail contact</summary>
            /// <param name="MailAddress">The mail address of the person</param>
            /// <param name="Name">The full name of the person</param>
            public MailContact(string MailAddress, string Name = "") { this.MailAddress = MailAddress; this.Name = Name; }
        }

        /// <summary>
        /// Container for mail messages
        /// </summary>
        public class MailMessage
        {
            /// <summary>The subject of the mail</summary>
            public string Subject { get; set; }
            /// <summary>The body of the mail</summary>
            public string Body { get; set; }
            /// <summary>Content type of the mail (default: text/plain)</summary>
            public string ContentType { get; set; }
            /// <summary>Character set of the mail (default: us-ascii)</summary>
            public string CharacterSet { get; set; }

            /// <summary>Defines the priority of the mail (1 to 5, default: 3)</summary>
            public int Priority {
                get
                {
                    return this._Priority;
                }
                set
                {
                    if (value < 1 || value > 5) throw new IndexOutOfRangeException();
                    this._Priority = value;
                }
            }

            /// <summary>Contains the priority</summary>
            private int _Priority;

            /// <summary>Returns the mailbody</summary>
            /// <returns>Returns the mailbody</returns>
            public override string ToString() { return this.Body; }
            /// <summary>Creates a new mail message</summary>
            /// <param name="Subject">The subject of the mail</param>
            /// <param name="Body">The body of the mail</param>
            public MailMessage(string Subject, string Body = "")
            {
                this.Subject = Subject;
                this.Body = Body;
                this.Priority = 3;
                this.ContentType = "text/plain";
                this.CharacterSet = "us-ascii";
            }
        }
        
        /// <summary>
        /// Supported authentication types
        /// </summary>
        public enum AuthenticationTypes
        {
            /// <summary>No authentication is used</summary>
            None = 0,
            /// <summary>Login authentication is used</summary>
            Login = 1
            // I want to support Plain authentication as well, but a bug got in the way: http://netmf.codeplex.com/workitem/1321
            //Plain = 2
        }

        /// <summary>Reference to the SMTP authentication type</summary>
        private AuthenticationTypes _SMTP_Auth;
        /// <summary>Reference to the SMTP authentication username</summary>
        private string _SMTP_User;
        /// <summary>Reference to the SMTP authentication password</summary>
        private string _SMTP_Pass;

        /// <summary>Local host name, used for identifying the mail client</summary>
        private string _LocalHostname;

        /// <summary>Reference to the socket</summary>
        private SimpleSocket _Socket;

        /// <summary>
        /// Initializes a mail sender
        /// </summary>
        /// <param name="Socket">The socket to use (default TCP port for SMTP is 25)</param>
        /// <param name="AuthenticationType">The form of SMTP Authentication (default: no authentication)</param>
        /// <param name="Username">Username for the SMTP server (when using authentication)</param>
        /// <param name="Password">Password for the SMTP server (when using authentication)</param>
        public SMTP_Client(SimpleSocket Socket, AuthenticationTypes AuthenticationType = AuthenticationTypes.None, string Username = "", string Password = "")
        {
            // Copies all parameters to the global scope
            this._Socket = Socket;
            this._SMTP_User = Username;
            this._SMTP_Pass = Password;
            this._SMTP_Auth = AuthenticationType;
            // By default we fill the hostname with the name of the hardware itself. We need this to identify ourself
            string Hostname = Tools.HardwareProvider.ToString();
            int LastDot = Hostname.LastIndexOf('.');
            if (LastDot > 0)
                this._LocalHostname = Hostname.Substring(0, LastDot);
            else
                this._LocalHostname = Hostname;
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="Message">The message to send</param>
        /// <param name="From">The sender (From: header)</param>
        /// <param name="To">A list of recipients (To: header)</param>
        /// <param name="CC">A list of recipients (CC: header)</param>
        /// <param name="BCC">A list of recipients (in no header at all)</param>
        public void Send(MailMessage Message, MailContact From, MailContact[] To, MailContact[] CC = null, MailContact[] BCC = null)
        {
            // CC and BCC can be defined null
            if (CC == null) CC = new MailContact[0];
            if (BCC == null) BCC = new MailContact[0];
            // This array will be filled with all recipients
            string[] Recipients = new string[To.Length + CC.Length + BCC.Length];
            int RecipientIndex = 0;
            // Subject
            string MailHeaders = "Subject: " + Message.Subject + "\r\n";
            // From
            MailHeaders += "From: " + From.ToString() + "\r\n";
            // Reply-to
            MailHeaders += "Reply-to: <" + From.MailAddress + ">\r\n";
            // To
            MailHeaders += "To: ";
            for (int Counter = 0; Counter < To.Length; ++Counter)
            {
                MailHeaders += To[Counter].ToString() + ";";
                Recipients[RecipientIndex] = To[Counter].MailAddress;
                ++RecipientIndex;
            }
            MailHeaders = MailHeaders.Substring(0, MailHeaders.Length - 1) + "\r\n";
            // BCC
            if (CC.Length != 0)
            {
                // CC
                MailHeaders += "CC: ";
                for (int Counter = 0; Counter < CC.Length; ++Counter)
                {
                    MailHeaders += CC[Counter].ToString() + ";";
                    Recipients[RecipientIndex] = CC[Counter].MailAddress;
                    ++RecipientIndex;
                }
                MailHeaders = MailHeaders.Substring(0, MailHeaders.Length - 1) + "\r\n";
            }
            // BCC
            if (BCC.Length != 0)
            {
                for (int Counter = 0; Counter < BCC.Length; ++Counter)
                {
                    Recipients[RecipientIndex] = BCC[Counter].MailAddress;
                    ++RecipientIndex;
                }
            }
            // Date; below 2011, the date is probably not set and sending it will only give the message a higher spam score in spam filters
            if (DateTime.Now.Year > 2010)
                MailHeaders += "Date: " + this._RFC2822_Date() + "\r\n";
            // Message-ID
            MailHeaders += "Message-ID: <" + DateTime.Now.Ticks.ToString() + "@" + this._LocalHostname + ">\r\n";
            // Priority
            if (Message.Priority != 3)
            {
                MailHeaders += "X-Priority: " + Message.Priority.ToString() + "\r\n";
            }
            // Content Type
            MailHeaders += "Content-Type: " + Message.ContentType + "; charset=" + Message.CharacterSet + "\r\n";
            // Mailer
            MailHeaders += "X-Mailer: NETMFToolbox/0.1\r\n";

            // Actually sends the mail
            this._Send(From.MailAddress, Recipients, MailHeaders + "\r\n" + Message.Body);
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="Message">The message to send</param>
        /// <param name="From">The sender (From: header)</param>
        /// <param name="To">The recipient (To: header)</param>
        public void Send(MailMessage Message, MailContact From, MailContact To)
        {
            this.Send(Message, From, new MailContact[] { To }, null, null);
        }

        /// <summary>
        /// Actually sends a message
        /// </summary>
        /// <param name="From">The mail address of the sender</param>
        /// <param name="Recipients">The mail addresses of all recipients (To, CC and BCC)</param>
        /// <param name="Data">The data, including headers</param>
        private void _Send(string From, string[] Recipients, string Data)
        {
            // Used for the answers of the SMTP server
            string Text = "";
            // Creates the socket
            // Defines that we use line endings with the linefeed character
            this._Socket.LineEnding = "\n";
            // Connects to the socket
            this._Socket.Connect();

            string Error = "";
            do
            { // We will use break, also when an error occures, so we always close the connection nicely

                // Reads and validates the first line of data
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "220") { Error = Text; break; }

                // Say "hello!"
                this._Socket.Send("HELO " + this._LocalHostname + "\r\n");
                // Validates the response. Some servers send multiple 220 replies. We require a 250 reply. All other replies are invalid.
                while (Text.Substring(0, 3) == "220")
                    Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "250") { Error = Text; break; }

                // Login Authentication Support
                if (this._SMTP_Auth == AuthenticationTypes.Login)
                {
                    // Authenticates with the LOGIN authentication type
                    this._Socket.Send("AUTH LOGIN\r\n");
                    Text = this._Socket.Receive(true);
                    if (Text.Substring(0, 3) != "334") { Error = Text; break; }

                    // Sends the username, base64 encoded
                    this._Socket.Send(Tools.Base64Encode(this._SMTP_User) + "\r\n");
                    Text = this._Socket.Receive(true);
                    if (Text.Substring(0, 3) != "334") { Error = Text; break; }

                    // Sends the password, base64 encoded
                    this._Socket.Send(Tools.Base64Encode(this._SMTP_Pass) + "\r\n");
                    Text = this._Socket.Receive(true);
                    if (Text.Substring(0, 3) != "235") { Error = Text; break; }
                }

                // Specifies the sender and validates the response
                this._Socket.Send("MAIL FROM:<" + From + ">\r\n");
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "250") { Error = Text; break; }

                // Specifies the receiver and validates the response
                for (int Counter = 0; Counter < Recipients.Length; ++Counter)
                {
                    this._Socket.Send("RCPT TO:<" + Recipients[Counter] + ">\r\n");
                    Text = this._Socket.Receive(true);
                    if (Text.Substring(0, 3) != "250") { Error = Text; break; }
                }

                // Initializes data transfer
                this._Socket.Send("DATA\r\n");
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "354") { Error = Text; break; }

                // Sends the actual data
                this._Socket.Send(Data + "\r\n.\r\n");
                Text = this._Socket.Receive(true);
                if (Text.Substring(0, 3) != "250") { Error = Text; break; }

            } while (false);

            // Closes the connection
            this._Socket.Send("QUIT\r\n");
            this._Socket.Close();

            if (Error != "")
            {
                throw new SystemException(Error);
            }
        }

        /// <summary>
        /// Returns the date formatted for mail headers
        /// </summary>
        /// <returns>The date formatted for mail headers</returns>
        private string _RFC2822_Date()
        {
            // All days in a week
            string[] Days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            // All months in a year
            string[] Months = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            // Current date/time
            DateTime dt = DateTime.Now;
            // Timezone
            string tz = Tools.ZeroFill(DateTime.UtcNow.CompareTo(dt) * 100, 4);
            if (tz.Substring(0, 1) != "-") tz = "+" + tz;
            // Start building the output
            string Output = "";
            Output += Days[(int)dt.DayOfWeek] + ", ";
            Output += dt.Day + " ";
            Output += Months[(int)dt.Month] + " ";
            Output += dt.Year + " ";
            Output += Tools.ZeroFill(dt.Hour, 2) + ":";
            Output += Tools.ZeroFill(dt.Minute, 2) + ":";
            Output += Tools.ZeroFill(dt.Second, 2) + " ";
            Output += tz;

            return Output;
        }
    }
}
