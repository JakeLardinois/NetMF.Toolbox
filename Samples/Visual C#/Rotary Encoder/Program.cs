using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
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
namespace NetduinoApplication1
{
    public class Program
    {
        public static void Main()
        {
            // Initializes a new rotary encoder object
            RotaryEncoder Knob = new RotaryEncoder(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);
            // Bounds the event to the rotary encoder
            Knob.Rotated += new NativeEventHandler(Knob_Rotated);
            // Wait infinitely
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// The value has been changed
        /// </summary>
        /// <param name="Unused">Not used</param>
        /// <param name="Value">The new value</param>
        /// <param name="Time">Time of the event</param>
        static void Knob_Rotated(uint Unused, uint Value, DateTime Time)
        {
            Debug.Print(Value == 1 ? "Clockwise" : "Counter clockwise");
        }

    }

}
