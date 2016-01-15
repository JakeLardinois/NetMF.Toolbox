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
namespace Hd44780LcdSnake
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
            IGPOPort Red = Mux.Pins[6];    // Red backlight
            IGPOPort Green = Mux.Pins[7];  // Green backlight
            IGPOPort Blue = Mux.Pins[8];   // Blue backlight

            // Pins 9 to 15 are connected to the HD44780 LCD
            Hd44780Lcd Display = new Hd44780Lcd(
                Data: Mux.CreateParallelOut(9, 4),
                ClockEnablePin: Mux.Pins[13],
                ReadWritePin: Mux.Pins[14],
                RegisterSelectPin: Mux.Pins[15]
            );

            // Initializes the game
            Games.HD44780_Snake.Init(Display, ButtonSelect, ButtonLeft, ButtonRight, ButtonUp, ButtonDown);

            // Turn on blue backlight
            Blue.Write(false); Red.Write(true); Green.Write(true);

            // Display splash
            Games.HD44780_Snake.Splash();

            // Wait 5 sec.
            Thread.Sleep(5000);

            // Turn on green backlight
            Blue.Write(true); Red.Write(true); Green.Write(false);

            // Starts the game
            try
            {
                Games.HD44780_Snake.Start();
            } catch (Exception e) {
                Display.ClearDisplay();
                Display.Write(e.Message);
            }

            // Turn on red backlight
            Blue.Write(true); Red.Write(false); Green.Write(true);
        }
    }
}
