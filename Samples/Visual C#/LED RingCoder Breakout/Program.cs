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
        // Defines the push button and LEDs in the knob
        public static AutoRepeatInputPort Button = new AutoRepeatInputPort(Pins.GPIO_PIN_D2, Port.ResistorMode.PullUp, true);
        public static OutputPort Red = new OutputPort(Pins.GPIO_PIN_D3, false);
        public static OutputPort Green = new OutputPort(Pins.GPIO_PIN_D4, true);

        // There are 16 LEDs in the circle around the knob
        public static IGPOPort[] Led = new IGPOPort[16];

        // We'll store an integer value of the knob
        public static int KnobValue = 0;

        public static void Main()
        {
            // Defines all 16 LEDs linked to two 74HC595 ICs in a chain
            Ic74hc595 IcChain = new Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 2);
            Led = IcChain.Pins;

            // Defines the rotary encoder
            RotaryEncoder Knob = new RotaryEncoder(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);
            Knob.Rotated += new NativeEventHandler(Knob_Rotated);

            // Links the event to the button
            Button.StateChanged += new AutoRepeatEventHandler(Button_StateChanged);

            // Wait infinitely
            Thread.Sleep(Timeout.Infinite);
        }

        static void Button_StateChanged(object sender, AutoRepeatEventArgs e)
        {
            // We will only change when the button gets pressed
            if (Button.Read()) return;
            // Toggles Red
            Red.Write(!Red.Read());
            // Makes green invert Red
            Green.Write(!Red.Read());
        }

        /// <summary>
        /// The value has been changed
        /// </summary>
        /// <param name="Unused">Not used</param>
        /// <param name="Value">The new value</param>
        /// <param name="Time">Time of the event</param>
        static void Knob_Rotated(uint Unused, uint Value, DateTime Time)
        {
            // Increase or decrease?
            if (Value == 1)
                ++KnobValue;
            else
                --KnobValue;

            // Makes it actually go round ;-)
            if (KnobValue < 0) KnobValue = 15;
            if (KnobValue > 15) KnobValue = 0;

            // Now we'll fill up led 0 to KnobValue to display the actual value
            for (int LedNo = 0; LedNo < Led.Length; ++LedNo)
            {
                Led[LedNo].Write(KnobValue >= LedNo);
            }
        }

    }

}
