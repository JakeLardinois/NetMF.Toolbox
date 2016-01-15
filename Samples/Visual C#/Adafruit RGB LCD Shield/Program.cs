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
namespace Adafruit_RGB_LCD_Shield
{
    public class Program
    {
        public static void Main()
        {
            // The Adafruit LCD Shield uses a MCP23017 IC as multiplex chip
            Mcp23017 Mux = new Mcp23017();

            // Pins 0 to 4 on the Mux-chip are connected to the buttons
            IGPIPort ButtonSelect = Mux.Pins[0];
            IGPIPort ButtonRight = Mux.Pins[1];
            IGPIPort ButtonDown = Mux.Pins[2];
            IGPIPort ButtonUp = Mux.Pins[3];
            IGPIPort ButtonLeft = Mux.Pins[4];
            // Enables pull-ups for all the buttons
            for (int i = 0; i < 5; ++i)
            {
                Mux.EnablePullup(i, true);
                Mux.Pins[i].InvertReadings = true;
            }

            // Pins 6 to 8 on the Mux-chip are for the backlight
            Mux.Pins[6].Write(false); // Red backlight
            Mux.Pins[7].Write(true);  // Green backlight
            Mux.Pins[8].Write(true);  // Blue backlight

            // Pins 9 to 15 are connected to the HD44780 LCD
            Hd44780Lcd Display = new Hd44780Lcd(
                Data: Mux.CreateParallelOut(9, 4),
                ClockEnablePin: Mux.Pins[13],
                ReadWritePin: Mux.Pins[14],
                RegisterSelectPin: Mux.Pins[15]
            );

            // Pressing the Select-button will shift through these colors
            bool[][] Colors = new bool[][] {
                new bool[3] { false,  true,  true },
                new bool[3] {  true, false,  true },
                new bool[3] {  true,  true, false },
                new bool[3] { false, false,  true },
                new bool[3] { false,  true, false },
                new bool[3] {  true, false, false },
            };
            int ColorIndex = 0;

            // Fills up the display
            Display.ClearDisplay();
            Display.Write("Left:  ? Down: ?");
            Display.ChangePosition(1, 0); 
            Display.Write("Right: ? Up:   ?");

            // Loops infinitely
            bool SelectPressed = false;
            while (true)
            {
                Display.ChangePosition(0, 7); Display.Write(ButtonLeft.Read() ? "1" : "0");
                Display.ChangePosition(1, 7); Display.Write(ButtonRight.Read() ? "1" : "0");
                Display.ChangePosition(0, 15); Display.Write(ButtonDown.Read() ? "1" : "0");
                Display.ChangePosition(1, 15); Display.Write(ButtonUp.Read() ? "1" : "0");

                // Handles the Select button
                if (ButtonSelect.Read())
                {
                    if (!SelectPressed)
                    {
                        SelectPressed = true;
                        ++ColorIndex;
                        if (ColorIndex == Colors.Length) ColorIndex = 0;
                        Mux.Pins[6].Write(Colors[ColorIndex][0]);
                        Mux.Pins[7].Write(Colors[ColorIndex][1]);
                        Mux.Pins[8].Write(Colors[ColorIndex][2]);
                    }
                }
                else
                    SelectPressed = false;
            }
        }

    }
}
