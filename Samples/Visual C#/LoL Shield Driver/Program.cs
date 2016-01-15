using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://netmftoolbox.codeplex.com/)
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
namespace LoL_Shield_Driver
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G    W A R N I N G    W A R N I N G  !
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // ! This may technically draw more current than recommended by the microcontroller specifications, and !
    // ! may void your warranty. Have fun and keep the fire extinguisher nearby :-)                         !
    // ! See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.LolShield         !
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class Program
    {
        public static void Main()
        {
            // Defines a new LoL-shield connected to pins 2 to 13
            LolShield Shield = new LolShield(new Cpu.Pin[] {
                Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, 
                Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8, Pins.GPIO_PIN_D9, 
                Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D11, Pins.GPIO_PIN_D12, Pins.GPIO_PIN_D13
            });

            // Then, a test for every single LED:
            for (int LedNo = 0; LedNo < Shield.Width * Shield.Height; ++LedNo)
            {
                Shield.Set(LedNo, true);
                Thread.Sleep(100);
                Shield.Set(LedNo, false);
            }

            // Loop over all columns
            for (int Col = 0; Col < Shield.Width; ++Col)
            {
                Shield.VerticalLine(Col, true);
                Thread.Sleep(100);
                Shield.VerticalLine(Col, false);
            }

            // Loop over all rows
            for (int Row = 0; Row < Shield.Height; ++Row)
            {
                Shield.HorizontalLine(Row, true);
                Thread.Sleep(100);
                Shield.HorizontalLine(Row, false);
            }

            // Now turns on all LEDs for a few seconds
            Shield.Clear(true);
            Thread.Sleep(2000);
             
            // Draws a bitmap to the screen
            Shield.LoadBitmap(Bitmaps.lolshield_bmp.Data, Bitmaps.lolshield_bmp.Width);

            // And inverts that state after a few seconds, for 2 times
            Thread.Sleep(2000); Shield.Invert();
            Thread.Sleep(2000); Shield.Invert();

            // Pixel locations for the NP2 logo
            int[] Positions = new int[] { 
                84, 70, 56, 42, 29, 30, 45, 59, 73, 87,           // N
                117, 103, 89, 75, 61, 47, 34, 35, 50, 64, 77, 76, // P
                38, 25, 26, 41, 55, 68, 81, 94, 95, 96, 97        // 2
            };

            while (true)
            {
                // Draws the NP2 logo
                Shield.Clear();
                for (int pixno = 0; pixno < Positions.Length; ++pixno)
                {
                    Shield.Set(Positions[pixno], true);
                    Thread.Sleep(50);
                }

                for (int Repeat = 0; Repeat < 5; ++Repeat)
                {
                    // Clears the screen
                    Shield.Clear();
                    Thread.Sleep(250);

                    // Draws the logo again, but now faster
                    for (int pixno = 0; pixno < Positions.Length; ++pixno)
                        Shield.Set(Positions[pixno], true);
                    Thread.Sleep(250);
                }
            }
        }

    }
}
