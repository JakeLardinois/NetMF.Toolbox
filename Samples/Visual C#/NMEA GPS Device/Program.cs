using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF;
using Toolbox.NETMF.Hardware;

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
namespace NMEA_GPS_Device
{
    public class Program
    {
        public static NmeaGps Gps = new NmeaGps();
        public static OutputPort Led = new OutputPort(Pins.ONBOARD_LED, false);

        public static void Main()
        {
            // Binds all events to the GPS device
            Gps.GotFix += new NativeEventHandler(Gps_GotFix);
            Gps.LostFix += new NativeEventHandler(Gps_LostFix);
            Gps.PositionChanged += new NativeEventHandler(Gps_PositionChanged);
            // Starts the GPS device
            Debug.Print("Trying to get a fix...");
            Gps.Start();

            // Nice blinking LED effect when we have a fix
            while (true)
            {
                Led.Write(Gps.Fix);
                Thread.Sleep(450);
                Led.Write(!Gps.Fix);
                Thread.Sleep(50);
            }
        }

        static void Gps_PositionChanged(uint Unused, uint FixType, DateTime GPSTime)
        {
            string Outp = "";
            Outp += "3D-Fix: " + Gps.Fix3D.ToString();
            Outp += ", Sattellites: " + Gps.Satellites.ToString();
            Outp += ", Time: " + Gps.GPSTime.ToString();
            Outp += ", Latitude: " + Gps.SLatitude;
            Outp += ", Longitude: " + Gps.SLongitude;
            Outp += ", Altitude: " + Gps.SAltitude;
            Outp += ", Knots: " + Gps.Knots.ToString() + " (" + Gps.Kmh.ToString() + " km/h)";
            Debug.Print(Outp);

            // If you want to translate this to a Bing Maps URL, try this:
            Debug.Print("http://www.bing.com/maps/?q=" + Tools.RawUrlEncode(Gps.Latitude.ToString() + " " + Gps.Longitude.ToString()));
        }

        static void Gps_GotFix(uint Unused, uint FixType, DateTime GPSTime)
        {
            Debug.Print("We got a fix, yay!!");
        }

        static void Gps_LostFix(uint Unused, uint FixType, DateTime GPSTime)
        {
            Debug.Print("We lost our GPS fix :(");
        }
    }
}
