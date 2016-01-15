using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

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
namespace _7_Segment_counter
{
    public class Program
    {
        public static void Main()
        {
            // Initializes a 7-segment display over a bitshift IC using a single IC
            Ic74hc595 Mux = new Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D10);
            SevenSegment Display = new SevenSegment(Mux.CreateParallelOut());

            while (true)
            {
                for (byte Value = 0; Value < 11; ++Value)
                {
                    // Displays all values for 0,5 sec. (0-9 = 0-9, 10=blank)
                    Display.SetDigit(Value);
                    // Toggles the dot
                    Display.SetDot(!Display.GetDot());
                    // Wait for 0,5 sec
                    Thread.Sleep(500);
                }
            }
        }

    }
}
