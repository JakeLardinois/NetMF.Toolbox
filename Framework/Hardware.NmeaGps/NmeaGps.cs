using System;
using System.IO.Ports; // Microsoft.SPOT.Hardware.SerialPort.dll
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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// NMEA compatible GPS
    /// </summary>
    public class NmeaGps
    {
        /// <summary>
        /// Reference to the serial port
        /// </summary>
        private SerialPort _Uart;

        /// <summary>
        /// Contains the serial buffer
        /// </summary>
        private string _Buffer = "";

        /// <summary>
        /// Initializes a new GPS module
        /// </summary>
        /// <param name="SerialPort">The serial port the module is connected to</param>
        /// <param name="BaudRate">The speed of the module</param>
        /// <remarks>Specs are taken from http://www.gpsinformation.org/dale/nmea.htm </remarks>
        public NmeaGps(string SerialPort = "COM2", int BaudRate = 4800)
        {
            this._Uart = new SerialPort(SerialPort, BaudRate);
            this._Uart.DataReceived += new SerialDataReceivedEventHandler(_Uart_DataReceived);
        }

        /// <summary>
        /// Checks if we're listening to the GPS module
        /// </summary>
        public bool Active { get { return this._Uart.IsOpen; } }

        /// <summary>
        /// Starts listening to the GPS module
        /// </summary>
        public void Start()
        {
            this._Uart.Open();
        }

        /// <summary>
        /// Stops listening to the GPS module
        /// </summary>
        public void Stop()
        {
            this._Uart.Close();
        }

        /// <summary>Event triggered when we gain a fix</summary>
        public event NativeEventHandler GotFix;
        /// <summary>Event triggered when we loose a fix</summary>
        public event NativeEventHandler LostFix;
        /// <summary>Event triggered when our position is changed</summary>
        public event NativeEventHandler PositionChanged;

        /// <summary>True when we have a fix</summary>
        public bool Fix { get { return this._Fix; } protected set {
            if (!this._Fix && value)
            {
                this._Fix = true;
                if (this.GotFix != null) this.GotFix(0, (uint)(this.Fix3D ? 3 : 2), this.GPSTime);
            }
            else if (this._Fix && !value)
            {
                this._Fix = false;
                if (this.LostFix != null) this.LostFix(0, 0, this.GPSTime);
            }
        } }
        /// <summary>True when we have a fix</summary>
        private bool _Fix;

        /// <summary>True when we have a 3D fix</summary>
        public bool Fix3D { get; protected set; }
        /// <summary>The amount of fixed satellites</summary>
        public int Satellites { get; protected set; }
        /// <summary>Time according to the satellites</summary>
        public DateTime GPSTime { get; protected set; }
        /// <summary>Speed over the ground in knots</summary>
        public float Knots { get; protected set; }
        /// <summary>Speed over the ground in kilometers per hour</summary>
        public float Kmh { get { return this.Knots * 1.952f; } }
        /// <summary>Track angle in degrees</summary>
        public float TrackAngle { get; protected set; }

        /// <summary>Latidude (in the format 4068.092,N)</summary>
        public string SLatitude { get; protected set; }
        /// <summary>Longitude (in the format 04704.045,W)</summary>
        public string SLongitude { get; protected set; }
        /// <summary>Altitude, Meters, above mean sea level (in the format 545.4,M)</summary>
        public string SAltitude { get; protected set; }

        /// <summary>Latest fix timestamp: Hour</summary>
        private int _Hour = 0;
        /// <summary>Latest fix timestamp: Minute</summary>
        private int _Minute = 0;
        /// <summary>Latest fix timestamp: Second</summary>
        private int _Second = 0;
        /// <summary>Latest fix timestamp: MilliSecond</summary>
        private int _MilliSecond = 0;
        /// <summary>Latest fix timestamp: Day</summary>
        private int _Day = 1;
        /// <summary>Latest fix timestamp: Month</summary>
        private int _Month = 1;
        /// <summary>Latest fix timestamp: Year</summary>
        private int _Year = 2000;

        /// <summary>True when we got an RMC reply</summary>
        private bool _RMC_Supported = false;
        /// <summary>True when we got an GGA reply</summary>
        private bool _GGA_Supported = false;

        /// <summary>Longitude as floating point value</summary>
        public float Longitude { get {
            if (this.SLongitude == null) return 0;
            string[] Parts = this.SLongitude.Split(new char[] { ',' } , 2);
            float Degrees = (float)double.Parse(Parts[0].Substring(0, 3));
            float Minutes = (float)double.Parse(Parts[0].Substring(3)) / 60f;
            float RetValue = Degrees + Minutes;
            if (Parts[1] == "W") RetValue *= -1;
            return RetValue;
        } }

        /// <summary>Latitude as floating point value</summary>
        public float Latitude { get {
            if (this.SLatitude == null) return 0;
            string[] Parts = this.SLatitude.Split(new char[] { ',' }, 2);
            float Degrees = (float)double.Parse(Parts[0].Substring(0, 2));
            float Minutes = (float)double.Parse(Parts[0].Substring(2)) / 60f;
            float RetValue = Degrees + Minutes;
            if (Parts[1] == "S") RetValue *= -1;
            return RetValue;
        } }

        /// <summary>Altitude in meters as floating point value</summary>
        public float Altitude { get {
            string[] Parts = this.SAltitude.Split(new char[] { ',' }, 2);
            return (float)double.Parse(Parts[0]);
        } }

        /// <summary>Last position as string</summary>
        private string _LastPosition = "";

        /// <summary>Checks if the position is changed. If so, the PositionChanged event it called</summary>
        private void _IsPositionChanged()
        {
            // No need to check it
            if (this.PositionChanged == null) return;
            // Current position
            string Position = this.SAltitude + "," + this.SLatitude + "," + this.SLongitude;
            // Same as last position?
            if (Position != this._LastPosition) this.PositionChanged(0, (uint)(this.Fix3D ? 3 : 2), this.GPSTime);
            // Stores current position
            this._LastPosition = Position;
        }

        /// <summary>
        /// Triggered when overall satellite data is received
        /// </summary>
        /// <param name="Params">The data</param>
        private void _GSA_DataReceived(string[] Params)
        {
            // 3D fix - values include: 1 = no fix, 2 = 2D fix, 3 = 3D gix
            this.Fix = Params[2] != "1";
            this.Fix3D = Params[2] == "3";

            // PRNs of satellites used for fix (space for 12)
            // We won't do that if GGA is supported, since that has a more accurate way of measuring
            if (!this._GGA_Supported)
            {
                int Satellites = 0;
                for (int Pos = 3; Pos < 15; ++Pos)
                    if (Params[Pos] != "") ++Satellites;
                this.Satellites = Satellites;
            }
        }

        /// <summary>
        /// Triggered when recommended minimum data for gps is received
        /// </summary>
        /// <param name="Params">The data</param>
        private void _RMC_DataReceived(string[] Params)
        {
            // Announces the GPS device supports RMC calls
            this._RMC_Supported = true;

            // Fix taken at UTC-time (HHMMSS format)
            if (Params[1].Length > 5)
            {
                this._Hour = int.Parse(Params[1].Substring(0, 2));
                this._Minute = int.Parse(Params[1].Substring(2, 2));
                this._Second = int.Parse(Params[1].Substring(4, 2));
                // Time field may contain milliseconds (HHMMSS.mmm format)
                if (Params[1].Length == 10)
                    this._MilliSecond = int.Parse(Params[1].Substring(7, 3));
            }

            // Date (DDMMYY format)
            if (Params[9].Length == 6)
            {
                this._Day = int.Parse(Params[9].Substring(0, 2));
                this._Month = int.Parse(Params[9].Substring(2, 2));
                this._Year = 2000 + int.Parse(Params[9].Substring(4, 2));
            }

            // Combines all date and time elements from this collection and, possibly, previous collections.
            // It's done like this since not all NMEA responses contain the date field for example.
            this.GPSTime = new DateTime(this._Year, this._Month, this._Day, this._Hour, this._Minute, this._Second, this._MilliSecond);

            // There's no float.parse ;-)
            if (Params[7] != "") this.Knots = (float)double.Parse(Params[7]);      // Speed over the ground in knots
            if (Params[8] != "") this.TrackAngle = (float)double.Parse(Params[8]); // Track angle in degrees

            // Latitude, longitude and megnetic variation
            this.SLatitude = Params[3] + "," + Params[4];
            this.SLongitude = Params[5] + "," + Params[6];

            // Some cleanup
            if (this.SLatitude == ",") this.SLatitude = null;
            if (this.SLongitude == ",") this.SLongitude = null;

            // See if the position is changed
            if (this.Fix) this._IsPositionChanged();
        }

        /// <summary>
        /// Triggered when fix information is received
        /// </summary>
        /// <param name="Params">The data</param>
        private void _GGA_DataReceived(string[] Params)
        {
            // Announces the GPS device supports GGA calls
            this._GGA_Supported = true;

            // RMC has a more precise clock signal
            if (!this._RMC_Supported)
            {
                // Fix taken at UTC-time (HHMMSS format)
                if (Params[1].Length > 5)
                {
                    this._Hour = int.Parse(Params[1].Substring(0, 2));
                    this._Minute = int.Parse(Params[1].Substring(2, 2));
                    this._Second = int.Parse(Params[1].Substring(4, 2));
                    // Time field may contain milliseconds (HHMMSS.mmm format)
                    if (Params[1].Length == 10)
                        this._MilliSecond = int.Parse(Params[1].Substring(7, 3));
                }

                // Combines all date and time elements from this collection and, possibly, previous collections.
                // It's done like this since not all NMEA responses contain the date field for example.
                this.GPSTime = new DateTime(this._Year, this._Month, this._Day, this._Hour, this._Minute, this._Second, this._MilliSecond);
            }

            // Fix quality
            this.Fix = Params[6] != "0";

            // Number of satellites being tracked
            this.Satellites = int.Parse(Params[7]);

            // Latitude, longitude and Altitude
            this.SLatitude = Params[2] + "," + Params[3];
            this.SLongitude = Params[4] + "," + Params[5];
            this.SAltitude = Params[9] + "," + Params[10];

            // Some cleanup
            if (this.SLatitude == ",") this.SLatitude = null;
            if (this.SLongitude == ",") this.SLongitude = null;
            if (this.SAltitude == ",") this.SAltitude = null;

            // See if the position is changed
            if (this.Fix) this._IsPositionChanged();
        }

        /// <summary>
        /// There's data received from the serial port
        /// </summary>
        /// <param name="sender">The sender of the data</param>
        /// <param name="e">Some event data</param>
        private void _Uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] Buffer = new byte[this._Uart.BytesToRead];
            this._Uart.Read(Buffer, 0, Buffer.Length);
            this._Buffer += new string(Tools.Bytes2Chars(Buffer));
            
            // Have we received a full line of data?
            int Pos = this._Buffer.IndexOf("\r\n");
            if (Pos >= 0)
            {
                string Data = this._Buffer.Substring(0, Pos);
                this._Buffer = this._Buffer.Substring(Pos + 2);
                // Lets validate the data
                if (Data.Substring(0, 1) != "$") return;
                if (Data.Substring(Data.Length-3 , 1) != "*") return;
                string RealData = Data.Substring(1, Data.Length - 4);
                string DataCheckSum = Data.Substring(Data.Length - 2, 2);
                string CheckSum = Tools.Dec2Hex(Tools.XorChecksum(RealData), 2);
                if (DataCheckSum != CheckSum) return;
                // Data is valid, lets see what kind of data this is
                string[] Params = RealData.Split(",".ToCharArray());
                // Trigger the right parsing method
                //Debug.Print(Data);
                if (Params[0] == "GPGSA") // Overall Satellite data 
                    this._GSA_DataReceived(Params);
                else if (Params[0] == "GPGGA") // Fix information
                    this._GGA_DataReceived(Params);
                else if (Params[0] == "GPRMC") // Recommended minimum data for gps 
                    this._RMC_DataReceived(Params);
            }

            // When we only receive invalid data we could get out of memory exceptions
            if (this._Buffer.Length > 255) this._Buffer = "";
        }
    }
}
