using System;
using Microsoft.SPOT.Hardware;
using Toolbox.NETMF.NET;

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
    /// Simple Network Time Protocol client
    /// </summary>
    public class SNTP_Client
    {
        /// <summary>Reference to the socket</summary>
        private SimpleSocket _Socket;

        /// <summary>Returns the hostname of the NTP server</summary>
        public string Hostname { get { return this._Socket.Hostname; } }
        /// <summary>Returns the UDP port number of the NTP server</summary>
        public ushort Port { get { return this._Socket.Port; } }

        /// <summary>
        /// Creates a connection to a NTP server
        /// </summary>
        /// <param name="Socket">The socket to use (default UDP port for NTP is 123)</param>
        public SNTP_Client(SimpleSocket Socket)
        {
            this._Socket = Socket;
        }

        /// <summary>
        /// The amount of seconds since 1 jan. 1900
        /// </summary>
        public Double Timestamp
        {
            get
            {
                double RetValue = 0;

                if (this._Socket.FeatureImplemented(SimpleSocket.SocketFeatures.UdpDatagram))
                {
                    // Connects to the NTP-server over UDP
                    this._Socket.Connect(SimpleSocket.SocketProtocol.UdpDatagram);

                    // Sends the request for 48 bytes
                    byte[] Request = new byte[48];
                    Request[0] = 0x1b; // Identifies as a protocol version v3 compatible NTP-client
                    this._Socket.SendBinary(Request);

                    // Reads out the response of 48 bytes
                    byte[] Response = this._Socket.ReceiveBinary(48);

                    // Closes the connection
                    this._Socket.Close();

                    // Reading out the NTP response (see also RFC2030 chapter 4: NTP Message Format)
                    // The first 40 bytes can safely be ignored, just keeping it simple NTP.
                    // After 40 bytes, we have 4 bytes (32-bit number) containing the integer part (seconds)
                    // After 44 bytes, we have 4 bytes (32-bit number) containing the fraction part (miliseconds)
                    // We need to have those values!
                    uint IntegerPart = 0;
                    uint FractionPart = 0;
                    for (int Counter = 0; Counter < 4; ++Counter)
                    {
                        IntegerPart = IntegerPart * 0x100 + Response[40 + Counter];
                        FractionPart = FractionPart * 0x100 + Response[44 + Counter];
                    }

                    // Combines both the integer and fraction parts to form the amount of seconds from 1 jan. 1900
                    RetValue = double.Parse(IntegerPart.ToString() + "." + FractionPart.ToString());
                }
                else if (this._Socket.FeatureImplemented(SimpleSocket.SocketFeatures.NtpClient))
                {
                    // Requests the time
                    RetValue = this._Socket.NtpLookup();
                }
                else
                {
                    throw new NotImplementedException("No UDP nor NTP client is implemented in the Socket class");
                }

                return RetValue;
            }
        }

        /// <summary>
        /// The UTC time as DateTime object
        /// </summary>
        public DateTime UTCDate
        {
            get
            {
                // Queries the NTP-server
                double Timestamp = this.Timestamp;

                // The amount of ticks this would be for the microprocessor
                long Ticks = (long)(Timestamp * 1000 * TimeSpan.TicksPerMillisecond);

                // Creates a new DateTime object referring to the UTC date and time, and adds the right amount of ticks
                DateTime RetValue = new DateTime(1900, 1, 1, 0, 0, 0, 0);
                RetValue += TimeSpan.FromTicks(Ticks);

                return RetValue;
            }
        }

        /// <summary>
        /// The local time as DateTime object
        /// </summary>
        public DateTime LocalDate
        {
            get
            {
                // Fetches the UTC Date
                DateTime UTCDate = this.UTCDate;

                // Applies the local time difference
                TimeSpan UTCOffset = TimeZone.CurrentTimeZone.GetUtcOffset(UTCDate);
                DateTime RetValue = UTCDate + UTCOffset;

                return RetValue;
            }
        }

        /// <summary>
        /// Synchronizes the MCU with the NTP-server
        /// </summary>
        public void Synchronize()
        {
            // Sets the time on the MCU
            Utility.SetLocalTime(this.LocalDate);
        }
    }
}
