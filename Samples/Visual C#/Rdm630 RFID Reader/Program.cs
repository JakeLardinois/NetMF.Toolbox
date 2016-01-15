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
namespace Rdm630_RFID_Reader
{
    public class Program
    {
        /// <summary>
        /// Makes the RFID class public so all methods can reach it
        /// </summary>
        public static Rdm630 RFID;

        public static void Main()
        {
            /* At the top right a set of 5 pins, from left to right:
             * 1. +5V
             * 2. GND
             * 3. Unused
             * 4. RX (Unused as well since it's read-only)
             * 5. TX (to Netduino pin 0 for COM1 or 2 for COM2)
             * 
             * At the bottom left a set of two pins, this is where you should connect the antenna.
             * Polarity doesn't matter in this case.
             * 
             * At the bottorm right there is:
             * 1. GND (connected on-board to Pin 2 of the 5-set)
             * 2. +5V (connected on-board to pin 1 of the 5-set)
             * 3. LED
             * You can connect a led to both pin 1 or 2, depending if you want to have it emitting when reading
             * data or when not reading data. Don't forget to add a resistor between the led and power supply.
             */
            RFID = new Rdm630("COM1");

            // Binds the event
            RFID.DataReceived += new NativeEventHandler(RFID_DataReceived);

            // Waits infinite for RFID tags
            Thread.Sleep(Timeout.Infinite);
        }

        /// <summary>
        /// Triggered when an RFID tag is scanned
        /// </summary>
        /// <param name="Unused1">Not used</param>
        /// <param name="Unused2">Not used</param>
        /// <param name="Time">Date and time of the event</param>
        static void RFID_DataReceived(uint Unused1, uint Unused2, DateTime Time)
        {
            Debug.Print("RFID tag " + RFID.Tag + " scanned");
        }
    }
}
