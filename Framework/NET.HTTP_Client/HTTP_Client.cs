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
    /// Simple HTTP Client
    /// </summary>
    public class HTTP_Client
    {
        /// <summary>
        /// HTTP Response container
        /// </summary>
        public class HTTP_Response
        {
            // Local header storage container
            private string[] _Headers = { };

            // HTTP Response container
            private string _ResponseBody = "";

            // Response code
            private int _ResponseCode = 0;

            /// <summary>Returns all response headers</summary>
            public string[] GetAllHeaders() { return this._Headers; }

            /// <summary>Returns the response body</summary>
            public string ResponseBody { get { return this._ResponseBody; } }

            /// <summary>Returns the response </summary>
            public int ResponseCode { get { return this._ResponseCode; } }

            /// <summary>Returns the webserver's response body</summary>
            /// <returns>The webserver's response body</returns>
            public override string ToString() { return this._ResponseBody; }

            /// <summary>
            /// Reads out a specific header
            /// </summary>
            /// <param name="Header">The name of the header</param>
            /// <returns>The value of the header</returns>
            public string ResponseHeader(string Header)
            {
                Header = Header.ToLower() + ":";
                for (int Counter = 0; Counter < this._Headers.Length; ++Counter)
                    if (this._Headers[Counter].Substring(0, Header.Length).ToLower() == Header)
                        return this._Headers[Counter].Substring(Header.Length + 1);
                return "";
            }

            /// <summary>
            /// Creates a new HTTP Response object based on some response data from the webserver
            /// </summary>
            /// <param name="ResponseData">The response data from the webserver</param>
            public HTTP_Response(string ResponseData)
            {
                // First, find the point that breaks between headers and content
                int BreakPoint = ResponseData.IndexOf("\r\n\r\n");
                if (BreakPoint > -1)
                {
                    // Lets set the response apart
                    this._ResponseBody = ResponseData.Substring(BreakPoint + 4);
                }
                else
                {
                    BreakPoint = ResponseData.IndexOf("\n\n");
                    if (BreakPoint > -1)
                        this._ResponseBody = ResponseData.Substring(BreakPoint + 2);
                    else
                        BreakPoint = this._ResponseBody.Length;
                }

                // The headers remain
                this._Headers = ResponseData.Substring(0, BreakPoint).Split('\n');
                // Trims all headers
                for (int Counter = 0; Counter < this._Headers.Length; ++Counter)
                    this._Headers[Counter] = this._Headers[Counter].Trim();
                // The first header also contains the ResponseCode
                if (this._Headers.Length > 0)
                {
                    string[] Parts = this._Headers[0].Split(" ".ToCharArray());
                    if (Parts.Length > 1)
                        this._ResponseCode = int.Parse(Parts[1]);
                }
            }
        }


        /// <summary>Reference to the socket</summary>
        private SimpleSocket _Socket;

        /// <summary>Contains all cookies</summary>
        private string[] _Cookies = new string[0];

        /// <summary>When using HTTP Authentication, the username is stored here</summary>
        private string _Username = "";
        /// <summary>When using HTTP Authentication, the password is stored here</summary>
        private string _Password = "";

        /// <summary>Returns the hostname of the webserver</summary>
        public string Hostname { get { return this._Socket.Hostname; } }
        /// <summary>Returns the port the webserver should listen to</summary>
        public ushort Port { get { return this._Socket.Port; } }

        /// <summary>The referrer for the next HTTP request (will automaticly be filled when a page is requested)</summary>
        public string Referrer { get; set; }

        /// <summary>The user agent header we send with the HTTP request</summary>
        public string UserAgent { get; set; }

        /// <summary>The Accept-header we send with the HTTP request</summary>
        public string Accept { get; set; }

        /// <summary>The Accept Language-header we send with the HTTP request</summary>
        public string AcceptLanguage { get; set; }

        /// <summary>
        /// Initializes a web session
        /// </summary>
        /// <param name="Socket">The socket to use (default TCP port for HTTP is 80)</param>
        public HTTP_Client(SimpleSocket Socket)
        {
            this._Socket = Socket;
            this.Accept = "*/*";
            this.AcceptLanguage = "en";
            // What kind of device do we run on?
            string[] SocketProvider = Socket.ToString().Split(new char[] { '.' });
            string[] Client = this.ToString().Split(new char[] { '.' });
            // User Agent
            this.UserAgent = "NETMFToolbox/0.1 (textmode; " + Tools.HardwareProvider + "; " + SocketProvider[SocketProvider.Length - 1] + "; " + Client[Client.Length - 1] + ")";
            // No referrer yet
            this.Referrer = "";
        }

        /// <summary>
        /// When a Username is specified, HTTP Authentication will be used
        /// </summary>
        /// <param name="Username">The username</param>
        /// <param name="Password">The password</param>
        public void Authenticate(string Username, string Password)
        {
            this._Username = Username;
            this._Password = Password;
        }

        /// <summary>
        /// Saves cookie data to the memory
        /// </summary>
        /// <param name="Key">The cookie name</param>
        /// <param name="Value">The cookie value (Needs to be URL Encoded!)</param>
        public void SetCookie(string Key, string Value)
        {
            // Array that fits all current cookies plus one
            string[] NewCookie = new string[this._Cookies.Length + 1];
            // Checks if the cookie already exists
            for (int Counter = 0; Counter < this._Cookies.Length; ++Counter)
            {
                if (this._Cookies[Counter].Substring(0, Key.Length + 1) == Key + "=")
                {
                    // Replaces an existing cookie
                    this._Cookies[Counter] = Key + "=" + Value;
                    return;
                }
                NewCookie[Counter] = this._Cookies[Counter];
            }
            // Appairently it's a new cookie, yum!
            NewCookie[NewCookie.Length - 1] = Key + "=" + Value;
            this._Cookies = NewCookie;
        }

        /// <summary>
        /// Executes a GET request and returns the HTTP response
        /// </summary>
        /// <param name="Url">Path to request</param>
        /// <param name="Arguments">Request arguments</param>
        /// <returns>HTTP response</returns>
        public HTTP_Response Get(string Url, string Arguments = "")
        {
            // Compiles the GET request
            string RequestData = "GET " + Url;
            if (Arguments != "") RequestData += "?" + Arguments;
            RequestData += " HTTP/1.1\r\n";
            RequestData += "Accept: " + this.Accept + "\r\n";
            RequestData += "Accept-Language: " + this.AcceptLanguage + "\r\n";
            if (this.Referrer != "") RequestData += "Referer: " + this.Referrer + "\r\n"; // Funny thing, the Referer-typo: http://en.wikipedia.org/wiki/HTTP_referrer#Origin_of_the_term_referer
            RequestData += "User-Agent: " + this.UserAgent + "\r\n";
            RequestData += "Host: " + this.Hostname + "\r\n";
            for (int Counter = 0; Counter < this._Cookies.Length; ++Counter)
                RequestData += "Cookie: " + this._Cookies[Counter] + "\r\n";
            if (this._Username != "")
                RequestData += "Authorization: Basic " + Tools.Base64Encode(this._Username + ":" + this._Password) + "\r\n";
            RequestData += "Connection: Close\r\n";
            RequestData += "\r\n";

            // Sets the referrer for the next request
            this.Referrer = "http://" + this.Hostname + (this.Port == 80 ? "" : ":" + this.Port.ToString()) + Url + (Arguments == "" ? "" : "?" + Arguments);

            return this._DoRequest(RequestData);
        }

        /// <summary>
        /// Executes a POST request and returns the HTTP response
        /// </summary>
        /// <param name="Url">Path to request</param>
        /// <param name="Arguments">Request arguments</param>
        /// <returns>HTTP response</returns>
        public HTTP_Response Post(string Url, string Arguments = "")
        {
            // Compiles the POST request
            string RequestData = "POST " + Url + " HTTP/1.1\r\n";
            RequestData += "Accept: " + this.Accept + "\r\n";
            RequestData += "Accept-Language: " + this.AcceptLanguage + "\r\n";
            if (this.Referrer != "") RequestData += "Referer: " + this.Referrer + "\r\n"; // Funny thing, the Referer-typo: http://en.wikipedia.org/wiki/HTTP_referrer#Origin_of_the_term_referer
            RequestData += "User-Agent: " + this.UserAgent + "\r\n";
            RequestData += "Host: " + this.Hostname + "\r\n";
            for (int Counter = 0; Counter < this._Cookies.Length; ++Counter)
                RequestData += "Cookie: " + this._Cookies[Counter] + "\r\n";
            if (this._Username != "")
                RequestData += "Authorization: Basic " + Tools.Base64Encode(this._Username + ":" + this._Password) + "\r\n";
            RequestData += "Content-Type: application/x-www-form-urlencoded\r\n";
            RequestData += "Content-Length: " + Arguments.Length.ToString() + "\r\n";
            RequestData += "Connection: Close\r\n";
            RequestData += "\r\n";
            RequestData += Arguments + "\r\n";
            RequestData += "\r\n";

            // Sets the referrer for the next request
            this.Referrer = "http://" + this.Hostname + (this.Port == 80 ? "" : ":" + this.Port.ToString()) + Url + (Arguments == "" ? "" : "?" + Arguments);

            return this._DoRequest(RequestData);
        }

        /// <summary>
        /// Actually executes a request
        /// </summary>
        /// <param name="RequestData">The Request Data</param>
        /// <returns>The HTTP Response</returns>
        private HTTP_Response _DoRequest(string RequestData)
        {

            // Opens the connection
            this._Socket.Connect();
            // Sends out the request
            this._Socket.Send(RequestData);
            // Fetches the returned data
            string ResponseData = "";
            while (this._Socket.IsConnected || this._Socket.BytesAvailable > 0)
                ResponseData += this._Socket.Receive();
            // Closes the connection
            this._Socket.Close();

            // Parses the response data
            HTTP_Response RetVal = new HTTP_Response(ResponseData);
            // Parses cookies and such
            this._ParseHeaders(RetVal.GetAllHeaders());

            return RetVal;
        }

        /// <summary>
        /// Parses the headers from a response and processes all cookies
        /// </summary>
        /// <param name="HTTP_Headers">The response headers</param>
        private void _ParseHeaders(string[] HTTP_Headers)
        {
            // Lets read out all cookies
            for (int Counter = 0; Counter < HTTP_Headers.Length; ++Counter)
            {
                if (HTTP_Headers[Counter].ToLower().IndexOf("set-cookie:") == 0)
                {
                    string CookieData = HTTP_Headers[Counter].Substring(11).Trim();
                    int ValuePos = CookieData.LastIndexOf(';');
                    if (ValuePos < 0) ValuePos = CookieData.Length;
                    string Key = CookieData.Substring(0, CookieData.IndexOf('='));
                    string Value = CookieData.Substring(Key.Length + 1, ValuePos - Key.Length - 1);
                    this.SetCookie(Key, Value);
                }
            }
        }
    }
}
