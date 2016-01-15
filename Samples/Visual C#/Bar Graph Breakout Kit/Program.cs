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
namespace Bar_Graph_Breakout_Kit
{
    public class Program
    {
        public static void Main()
        {
            // We got 4 74HC595's in a chain
            Ic74hc595 IcChain = new Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 4);

            // Led loop back and forward
            while (true)
            {
                for (int Counter = 0; Counter < 30; ++Counter)
                {
                    IcChain.Pins[Counter].Write(true);
                    Thread.Sleep(50);
                    IcChain.Pins[Counter].Write(false);
                }
                for (int Counter = 28; Counter > 0; --Counter)
                {
                    IcChain.Pins[Counter].Write(true);
                    Thread.Sleep(50);
                    IcChain.Pins[Counter].Write(false);
                }
            }
        }

    }
}
