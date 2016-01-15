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
    /// Common RGB-led
    /// </summary>
    public class RgbLed
    {
        /// <summary>Reference to the red pin</summary>
        private IPWMPort _Red;
        /// <summary>Reference to the green pin</summary>
        private IPWMPort _Green;
        /// <summary>Reference to the blue pin</summary>
        private IPWMPort _Blue;
        /// <summary>True when it's common anode, false if it's common cathode</summary>
        private bool _CommonAnode;

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
        /// <summary>
        /// Common RGB-led
        /// </summary>
        /// <param name="RedPin">The PWM-pin connected to Red</param>
        /// <param name="GreenPin">The PWM-pin connected to Green</param>
        /// <param name="BluePin">The PWM-pin connected to Blue</param>
        /// <param name="CommonAnode">Specifies if the led is common anode</param>
        public RgbLed(Cpu.PWMChannel RedPin, Cpu.PWMChannel GreenPin, Cpu.PWMChannel BluePin, bool CommonAnode = true)
        {
            this._Red = new IntegratedPWM(RedPin);
            this._Green = new IntegratedPWM(GreenPin);
            this._Blue = new IntegratedPWM(BluePin);
            this._Red.StartPulse();
            this._Green.StartPulse();
            this._Blue.StartPulse();
            this._CommonAnode = CommonAnode;
        }
#endif

        /// <summary>
        /// Common RGB-led
        /// </summary>
        /// <param name="RedPin">The PWM-pin connected to Red</param>
        /// <param name="GreenPin">The PWM-pin connected to Green</param>
        /// <param name="BluePin">The PWM-pin connected to Blue</param>
        /// <param name="CommonAnode">Specifies if the led is common anode</param>
        public RgbLed(IPWMPort RedPin, IPWMPort GreenPin, IPWMPort BluePin, bool CommonAnode = true)
        {
            this._Red = RedPin;
            this._Green = GreenPin;
            this._Blue = BluePin;
            this._Red.StartPulse();
            this._Green.StartPulse();
            this._Blue.StartPulse();
            this._CommonAnode = CommonAnode;
        }

        /// <summary>
        /// Sets the value of the RGB led
        /// </summary>
        /// <param name="Value">The RGB value (0x000000 to 0xffffff)</param>
        public void Write(int Value)
        {
            byte Red = (byte)(Value >> 16);
            byte Green = (byte)(Value >> 8 & 0xff);
            byte Blue = (byte)(Value & 0xff);
            this.Write(Red, Green, Blue);
        }

        /// <summary>
        /// Sets the value of the RGB led
        /// </summary>
        /// <param name="Red">Red strength (0 to 255)</param>
        /// <param name="Green">Green strength (0 to 255)</param>
        /// <param name="Blue">Blue strength (0 to 255)</param>
        public void Write(byte Red, byte Green, byte Blue)
        {
            // Values are sent from 0 to 255, but we actually want 0 to 100.
            uint uRed = (uint)(Red * 100 / 255);
            uint uGreen = (uint)(Green * 100 / 255);
            uint uBlue = (uint)(Blue * 100 / 255);
            // Common anode?
            if (this._CommonAnode)
            {
                uRed = 100 - uRed;
                uGreen = 100 - uGreen;
                uBlue = 100 - uBlue;
            }
            // Sets the values
            this._Red.SetDutyCycle(uRed);
            this._Green.SetDutyCycle(uGreen);
            this._Blue.SetDutyCycle(uBlue);
        }
    }
}