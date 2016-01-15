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
namespace Matrix_KeyPad
{
    public class Program
    {
        public static void Main()
        {
	        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	        // !   W A R N I N G    W A R N I N G     W A R N I N G     W A R N I N G    W A R N I N G   !
	        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	        // ! There are a lot of different keypads with different pin sets.                           !
	        // ! See http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.MatrixKeyPad !
	        // ! for more about these keypads.                                                           !
			// ! The pins used below probably won't work on your specific keypad.                        !
	        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // Row pins. The keypad exists out of 4 rows.
            Cpu.Pin[] RowPins = { Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7 };
            // Col pins. The keypad exists out of 3 columns.
            Cpu.Pin[] ColPins = { Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3 };
            // Initializes the new keypad
            MatrixKeyPad kb = new MatrixKeyPad(RowPins, ColPins);

            // Bind both events
            kb.OnKeyDown += new NativeEventHandler(kb_OnKeyDown);
            kb.OnKeyUp += new NativeEventHandler(kb_OnKeyUp);

            // Lets wait forever for events to occure
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Triggered when a key is released
        /// </summary>
        /// <param name="KeyCode">The key code</param>
        /// <param name="Unused">Not used</param>
        /// <param name="time">Date and time of the event</param>
        static void kb_OnKeyUp(uint KeyCode, uint Unused, DateTime time)
        {
            Debug.Print("Key released: " + KeyCode.ToString());
        }

        /// <summary>
        /// Triggered when a key is pressed
        /// </summary>
        /// <param name="KeyCode">The key code</param>
        /// <param name="Unused">Not used</param>
        /// <param name="time">Date and time of the event</param>
        static void kb_OnKeyDown(uint KeyCode, uint Unused, DateTime time)
        {
            Debug.Print("Key pressed: " + KeyCode.ToString());
        }
    }
}
