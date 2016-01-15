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
 * Copyright 2012-2014 Stefan Thoolen (http://netmftoolbox.codeplex.com/)
 * 
 * This sample is based on the Light-and-Temp-logger from Adafruit:
 * https://github.com/adafruit/Light-and-Temp-logger
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
namespace Adafruit_Fridgelogger
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
            writer.WriteLine("ticks,datetime,light,temp");
#endif

            // LEDs
            OutputPort red = new OutputPort(Pins.GPIO_PIN_D2, false);
            OutputPort green = new OutputPort(Pins.GPIO_PIN_D3, false);

            // An analog light sensor
            IADCPort light = new Netduino.ADC(Pins.GPIO_PIN_A0);
            light.RangeSet(0, 1024);
            // An analog temperature sensor
            Tmp36 temperature = new Tmp36(new Netduino.ADC(Pins.GPIO_PIN_A1));

            // Time module (comment out SetTime once, to set the clock)
            DS1307 time = new DS1307();
            /*time.SetTime(
                Day: 11,
                Month: 8,
                Year: 2012,
                Hour: 12,
                Minute: 0,
                Second: 0
            );*/
            time.Synchronize();

            while (true)
            {
                // Green status LED ON
                green.Write(true);

                // Builds the output
                string output = "";
                output += DateTime.Now.Ticks.ToString() + ", ";
                output += DateTime.Now.ToString() + ", ";
                output += light.RangeRead().ToString() + ", ";
                output += temperature.Temperature.ToString();

                // Prints the output to the debugger
                Debug.Print(output);
#if SD_ENABLED
                // Writes the output to the SD buffer
                writer.WriteLine(output);
#endif

                // Green status LED OFF, Red status LED ON
                green.Write(false);
                red.Write(true);

#if SD_ENABLED
                // Flushes the buffers to the SD card
                writer.Flush();
                stream.Flush();
#endif

                // Red status LED OFF
                red.Write(false);

                // Sleeps for a second
                Thread.Sleep(1000);
            }
        }

    }
}
