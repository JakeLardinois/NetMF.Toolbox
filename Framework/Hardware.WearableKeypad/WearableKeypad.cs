using System;
using Microsoft.SPOT.Hardware;

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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// Driver for Sparkfun's Wearable Keypad
    /// </summary>
    public class WearableKeypad : IDisposable
    {
        /// <summary>Reference to pin P5.1</summary>
        private TristatePort _Pin1;
        /// <summary>Reference to pin P5.2</summary>
        private TristatePort _Pin2;
        /// <summary>Reference to pin P5.3</summary>
        private TristatePort _Pin3;

        /// <summary>
        /// Initializes the driver for Sparkfun's Wearable Keypad
        /// </summary>
        /// <param name="Pin1">Pin P5.1 from the keypad</param>
        /// <param name="Pin2">Pin P5.2 from the keypad</param>
        /// <param name="Pin3">Pin P5.3 from the keypad</param>
        public WearableKeypad(Cpu.Pin Pin1, Cpu.Pin Pin2, Cpu.Pin Pin3)
        {
            this._Pin1 = new TristatePort(Pin1, false, false, Port.ResistorMode.PullUp);
            this._Pin2 = new TristatePort(Pin2, false, false, Port.ResistorMode.PullUp);
            this._Pin3 = new TristatePort(Pin3, false, false, Port.ResistorMode.PullUp);
        }

        /// <summary>
        /// Reads the current value
        /// </summary>
        /// <returns>The value of the keypad (0=up, 1=right, 2=down, 3=left, 4=center) or -1 when nothing is pressed</returns>
        public int Read()
        {
            // Initially set all buttons as inputs (passive)
            if (this._Pin2.Active) this._Pin2.Active = false;
            if (this._Pin1.Active) this._Pin1.Active = false;
            if (this._Pin3.Active) this._Pin3.Active = false;

            // Checks if up, down or the logo is pressed
            if (!this._Pin3.Read()) return 2; // down
            if (!this._Pin2.Read()) return 0; // up
            if (!this._Pin1.Read()) return 4; // center

            // Sets P52 to output
            this._Pin2.Active = true;
            // Checks if the right button is pressed
            if (!this._Pin3.Read()) return 1; // right

            // Sets P52 back as input and P51 as output
            this._Pin2.Active = false; this._Pin1.Active = true;
            // Checks if the left button is pressed
            if (!this._Pin3.Read()) return 3; // left

            // Sets P51 back as input
            this._Pin1.Active = false;

            return -1;
        }

        /// <summary>
        /// Disposes this object and frees the pins
        /// </summary>
        public void Dispose()
        {
            this._Pin1.Dispose();
            this._Pin2.Dispose();
            this._Pin3.Dispose();
        }
    }
}