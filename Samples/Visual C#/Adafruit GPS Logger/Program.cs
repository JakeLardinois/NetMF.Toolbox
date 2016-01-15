#define SD_ENABLED
using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF;
using Toolbox.NETMF.Hardware;
#if SD_ENABLED
using System.IO;
using SecretLabs.NETMF.IO;
#endif

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
namespace Adafruit_GPS_Logger
{
    public class Program
    {
        public static void Main()
        {
#if SD_ENABLED
            // If your Netduino can't execute the next line of code, make sure you got at least firmware 4.1.1 beta 1
            // See also: http://forums.netduino.com/index.php?/topic/1592-netduino-firmware-v411-beta-1/
            StorageDevice.MountSD("SD", SPI_Devices.SPI1, Pins.GPIO_PIN_D10);

            // Determines the filename
            string filename = "";
            int index = 0;
            do
            {
                filename = @"\SD\LOGGER" + Tools.ZeroFill(index, 2) + ".CSV";
                ++index;
            }
            while (File.Exists(filename));

            // Starts writing to the file
            FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);

            // Writes file headers
            writer.WriteLine("ticks,datetime,fix,latitude,longitude,altitude,satellites,kmh");
#endif

            // The GPS unit is connected to COM2
            NmeaGps gps = new NmeaGps("COM2");

            // Lets power on the GPS unit (active: LOW)
            OutputPort gps_enable = new OutputPort(Pins.GPIO_PIN_D4, false);

            // LEDs are connected to pins 5 and 6
            OutputPort led1 = new OutputPort(Pins.GPIO_PIN_D5, false);
            OutputPort led2 = new OutputPort(Pins.GPIO_PIN_D6, false);

            // Starts the GPS unit
            gps.Start();

            // Keeps on looping
            while (true)
            {
                // LED1 status ON
                led1.Write(true);

                // Builds the output
                string output = "";
                output += DateTime.Now.Ticks.ToString() + ", ";
                output += gps.GPSTime.ToString() + ", ";
                if (!gps.Fix)
                    output += "0, 0, 0, 0, 0, 0";
                else
                {
                    output += "1, ";
                    output += gps.Latitude.ToString() + ", ";
                    output += gps.Longitude.ToString() + ", ";
                    output += gps.Altitude.ToString() + ", ";
                    output += gps.Satellites.ToString() + ", ";
                    output += gps.Kmh.ToString();
                }

                // Prints the output to the debugger
                Debug.Print(output);
#if SD_ENABLED
                // Writes the output to the SD buffer
                writer.WriteLine(output);
#endif

                // LED1 status OFF, LED1 status ON
                led1.Write(false);
                led2.Write(true);

#if SD_ENABLED
                // Flushes the buffers to the SD card
                writer.Flush();
                stream.Flush();
#endif

                // LED2 status OFF
                led2.Write(false);

                // Sleeps for a second
                Thread.Sleep(1000);
            }
        }

    }
}
