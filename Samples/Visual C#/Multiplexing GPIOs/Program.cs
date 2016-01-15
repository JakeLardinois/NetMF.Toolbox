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
namespace Multiplexing_GPIOs
{
    public class Program
    {
        // In the global scope so we can call the leds from the events
        public static IGPOPort[] Leds = new IGPOPort[16];

        public static void Main()
        {
            // Defining two 74HC165s daisychained on the SPI bus, pin 10 as latchpin
            Ic74hc165 IcInChain = new Ic74hc165(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 2);

            // Defining two 74HC595s daisychained on the SPI bus, pin 9 as latchpin
            Ic74hc595 IcOutChain = new Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D9, 2);

            // Defines all 16 leds
            for (uint Counter = 0; Counter < 16; ++Counter)
            {
                Leds[Counter] = IcOutChain.Pins[Counter];
            }

            // Defines all 16 buttons
            IIRQPort[] Buttons = new IIRQPort[16];
            for (uint Counter = 0; Counter < 16; ++Counter)
            {
                Buttons[Counter] = IcInChain.Pins[Counter];
                Buttons[Counter].OnStateChange += new StateChange(Program_OnStateChange);
                Buttons[Counter].ID = Counter.ToString();
            }

            // Enables interrupts
            IcInChain.EnableInterrupts();

            // Wait infinite; let the events to their jobs
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Event triggered when a state changes
        /// </summary>
        /// <param name="Object">The pin that triggered the change</param>
        /// <param name="State">The current value</param>
        /// <param name="Time">Time and date of the event</param>
        static void Program_OnStateChange(IIRQPort Object, bool State, DateTime Time)
        {
            Debug.Print("State changed of the 74HC165 port " + Object.ID.ToString() + " to " + State.ToString());
            Leds[int.Parse(Object.ID)].Write(State);
        }

    }
}
