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
namespace RGB_Led
{
    public class Program
    {
        public static void Main()
        {
            // Defines an RGB Led connected to three PWM pins, common cathode
            // Some RGB-leds are common anode, in those cases, set the last boolean on true
            RgbLed Led = new RgbLed(new Netduino.PWM(Pins.GPIO_PIN_D9), new Netduino.PWM(Pins.GPIO_PIN_D6), new Netduino.PWM(Pins.GPIO_PIN_D5), false);

            while (true)
            {
                // Fade to red
                for (byte Red = 0; Red < 255; ++Red)
                {
                    Led.Write(Red, 0, 0);
                    Thread.Sleep(25);
                }
                // Fade to green
                for (byte Green = 0; Green < 255; ++Green)
                {
                    Led.Write(0, Green, 0);
                    Thread.Sleep(25);
                }
                // Fade to blue
                for (byte Blue = 0; Blue < 255; ++Blue)
                {
                    Led.Write(0, 0, Blue);
                    Thread.Sleep(25);
                }

                Led.Write(0x0000ff); Thread.Sleep(500); // Blue
                Led.Write(0x00ff00); Thread.Sleep(500); // Green
                Led.Write(0x00ffff); Thread.Sleep(500); // Turquoise
                Led.Write(0xff0000); Thread.Sleep(500); // Red
                Led.Write(0xff00ff); Thread.Sleep(500); // Purple
                Led.Write(0xffff00); Thread.Sleep(500); // Yellow
                Led.Write(0xffffff); Thread.Sleep(500); // White
                Led.Write(0x000000); Thread.Sleep(500); // Off
            }
        }

    }
}
