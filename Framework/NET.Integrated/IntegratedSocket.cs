using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Toolbox.NETMF;

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
    /// Simplifies usage of sockets in .NETMF
    /// </summary>
    public class IntegratedSocket : SimpleSocket
    {
        /// <summary>Contains a reference to the socket</summary>
        private Socket _Sock;
        /// <summary>Stores the hostname connected to</summary>
        private string _Hostname;
        /// <summary>Stores the TCP port connected to</summary>
        private ushort _Port;
        /// <summary>Contains the buffer of the read data</summary>
        private string _Buffer = "";

        /// <summary>Set to true when Close() is called</summary>
        private bool _Closed = false;

        /// <summary>
        /// Creates a new socket based on the integrated .NETMF socket TCP stack
        /// </summary>
        /// <param name="Hostname">The hostname to connect to</param>
        /// <param name="Port">The port to connect to</param>
        public IntegratedSocket(string Hostname, ushort Port)
        {
            // Stores the values to the memory
            this._Hostname = Hostname;
            this._Port = Port;
            // Default line ending values
            this.LineEnding = "";
            // Creates a new socket
        }

        /// <summary>
        /// Listens on the port instead of connecting remotely
        /// </summary>
        public override void Listen()
        {
            Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new IPEndPoint(IPAddress.Any, this._Port));
            Listener.Listen(1);
            // Accepts the first connection
            this._Sock = Listener.Accept();
            this._Closed = false;
            this._Hostname = ((IPEndPoint)this._Sock.RemoteEndPoint).Address.ToString();
            // Stops further listening
            Listener.Close();
        }

        /// <summary>
        /// Requests the amount of bytes available in the buffer
        /// </summary>
        public override uint BytesAvailable { get {
            return (uint)this._Buffer.Length;
        } }

        /// <summary>
        /// Connects to the remote host
        /// </summary>
        /// <param name="Protocol">The protocol to be used</param>
        public override void Connect(SocketProtocol Protocol = SocketProtocol.TcpStream)
        {
            // Creates a new socket object
            if (Protocol == SocketProtocol.TcpStream)
                this._Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            else
                this._Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // Resolves the hostname to an IP address
            IPHostEntry address = Dns.GetHostEntry(this._Hostname);
            // Creates the new IP end point
            EndPoint Destination = new IPEndPoint(address.AddressList[0], (int)this._Port);
            // Connects to the socket
            this._Sock.Connect(Destination);
            this._Closed = false;
        }

        /// <summary>
        /// Closes the connection
        /// </summary>
        public override void Close()
        {
            this._Closed = true;
            this._Sock.Close();
        }

        /// <summary>
        /// Sends binary data to the socket
        /// </summary>
        /// <param name="Data">The binary data to send</param>
        public override void SendBinary(byte[] Data)
        {
            this._Sock.Send(Data);

            // I implemented this sleep here for an issue I encountered.
            // For more details, read http://forums.netduino.com/index.php?/topic/4555-socket-error-10055-wsaenobufs/
            if (Data.Length < 32) Thread.Sleep(Data.Length * 10);
        }

        /// <summary>
        /// Returns true when connected, otherwise false
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (this._Closed) return false;
                // SelectRead returns true if:
                // - Listen has been called and a connection is pending; -or-
                // - if data is available for reading; -or-
                // - if the connection has been closed, reset, or terminated
                // We're not listening, so we only need to check if data is available for reading. If not, the connection is a goner.
                if (this._Sock.Poll(1, SelectMode.SelectRead) && this._Sock.Available == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>Returns the hostname this socket is configured for</summary>
        public override string Hostname { get { return this._Hostname; } }
        /// <summary>Returns the port number this socket is configured for</summary>
        public override ushort Port { get { return this._Port; } }

        /// <summary>
        /// Receives data from the socket
        /// </summary>
        /// <param name="Block">When true, this function will wait until there is data to return</param>
        /// <returns>The received data (may be empty)</returns>
        public override string Receive(bool Block = false)
        {
            string RetValue = "";
            do
            {
                // Do we need to read data?
                if (this._Sock.Available > 0)
                {
                    // Do we already have enough data for now? (saves us from early out of memory exceptions)
                    if (this.LineEnding.Length == 0 || this._Buffer.IndexOf(this.LineEnding) < 0) 
                    {

                        // There is data, lets read it!
                        byte[] ReadBuffer = new byte[this._Sock.Available];
                        this._Sock.Receive(ReadBuffer);
                        // Lets add the data to the buffer
                        this._Buffer += new string(Tools.Bytes2Chars(ReadBuffer));
                    }
                }

                if (this.LineEnding.Length > 0)
                {
                    // We're going to do buffering
                    int Pos = this._Buffer.IndexOf(this.LineEnding);
                    // Appairently there's a line ending found, lets split the data up
                    if (Pos > -1)
                    {
                        RetValue = this._Buffer.Substring(0, Pos + this.LineEnding.Length);
                        this._Buffer = this._Buffer.Substring(Pos + this.LineEnding.Length);
                    }
                }
                else
                {
                    // We don't do buffering at this moment. We just send all data back.
                    RetValue = this._Buffer;
                    this._Buffer = "";
                }
            } while (Block && RetValue == "");

            return RetValue;
        }

        /// <summary>
        /// Receives binary data from the socket (line endings aren't used with this method)
        /// </summary>
        /// <param name="Length">The amount of bytes to receive</param>
        /// <returns>The binary data</returns>
        public override byte[] ReceiveBinary(int Length)
        {
            byte[] RetValue = new byte[Length];
            this._Sock.Receive(RetValue);

            return RetValue;
        }

        /// <summary>
        /// Checks if a feature is implemented
        /// </summary>
        /// <param name="Feature">The feature to check for</param>
        /// <returns>True if the feature is implemented</returns>
        public override bool FeatureImplemented(SocketFeatures Feature)
        {
            switch (Feature)
            {
                case SocketFeatures.TcpStream:
                case SocketFeatures.UdpDatagram:
                case SocketFeatures.TcpListener:
                    return true;
                default:
                    return false;
            }
        }
    }
}
