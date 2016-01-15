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
namespace Rotary_DIP_Switch
{
    public class Program
    {
        public static void Main()
        {
            // The switch is connected to pins 1, 2, 4 and 8, so a 4-bit switch (define them in the correct sequence)
            Cpu.Pin[] RotaryPins = { Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D8 };
            RotaryDIPSwitch Switch = new RotaryDIPSwitch(RotaryPins);
            Switch.OnInterrupt += new NativeEventHandler(Switch_OnInterrupt);

            // Let the events do the rest
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// The switch' state is changed
        /// </summary>
        /// <param name="Unused">Unused</param>
        /// <param name="State">New state</param>
        /// <param name="Time">Time of the event</param>
        static void Switch_OnInterrupt(uint Unused, uint State, DateTime Time)
        {
            Debug.Print(State.ToString());
        }
    }
}
