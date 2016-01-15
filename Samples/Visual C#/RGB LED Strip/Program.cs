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
namespace RGB_LED_Strip
{
    public class Program
    {
        public static void Main()
        {
            // There's a 1M strip (32 LEDs) connected to the first SPI bus on the Netduino
            RgbLedStrip Chain = new RgbLedStrip(RgbLedStrip.Chipsets.LPD8806, 32, SPI_Devices.SPI1);

            // Repeats all demos infinitely
            while (true)
            {
                // Shows a red, green and blue LED
                Chain.SetColor(0, 0xff0000);
                Chain.SetColor(1, 0x00ff00);
                Chain.SetColor(2, 0x0000ff);
                Chain.Write();
                Thread.Sleep(5000);

                // Loops R, G and B for one minute
                Chain.SetColorAll(0, false);
                for (int Seconds = 0; Seconds < 30; ++Seconds)
                {
                    Chain.InsertColorAtBack(0xff0000, false); Thread.Sleep(333);
                    Chain.InsertColorAtBack(0x00ff00, false); Thread.Sleep(333);
                    Chain.InsertColorAtBack(0x0000ff, false); Thread.Sleep(334);
                }
                Chain.SetColorAll(0, false);
                for (int Seconds = 0; Seconds < 30; ++Seconds)
                {
                    Chain.InsertColorAtFront(0x0000ff, false); Thread.Sleep(334);
                    Chain.InsertColorAtFront(0x00ff00, false); Thread.Sleep(333);
                    Chain.InsertColorAtFront(0xff0000, false); Thread.Sleep(333);
                }

                // Just changing colors
                Chain.SetColorAll(0xff0000, false); Thread.Sleep(1000);
                Chain.SetColorAll(0x00ff00, false); Thread.Sleep(1000);
                Chain.SetColorAll(0x0000ff, false); Thread.Sleep(1000);

                // Loops nicely through all colors
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(Brightness, 0, 0, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(255, Brightness, 0, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll((byte)(255 - Brightness), 255, 0, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(0, 255, Brightness, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(0, (byte)(255 - Brightness), 255, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(Brightness, 0, 255, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll(255, Brightness, 255, false); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.SetColorAll((byte)(255 - Brightness), (byte)(255 - Brightness), (byte)(255 - Brightness), false); Thread.Sleep(1);
                }
            }
        }

    }
}
