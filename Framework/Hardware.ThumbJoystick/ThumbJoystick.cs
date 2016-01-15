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
    /// Analog thumb joystick
    /// </summary>
    public class ThumbJoystick : IDisposable
    {
        /// <summary>Reference to the horizontal pin</summary>
        private IADCPort _Horizontal;
        /// <summary>Reference to the vertical pin</summary>
        private IADCPort _Vertical;
        /// <summary>Reference to the push pin</summary>
        private InputPort _Push;

        /// <summary>When true, the horizontal value will be inverted</summary>
        public bool InvertHorizontal { get; set; }
        /// <summary>When true, the vertical value will be inverted</summary>
        public bool InvertVertical { get; set; }


        /// <summary>
        /// Returns the horizontal state (-50 to 50)
        /// </summary>
        public sbyte HorizontalValue
        {
            get
            {
                // Actually the value is inverted by default ;-)
                if (this.InvertHorizontal)
                {
                    return (sbyte)this._Horizontal.RangeRead();
                }
                else
                {
                    return (sbyte)(0 - this._Horizontal.RangeRead());
                }
            }
        }

        /// <summary>
        /// Returns the vertical state (-50 to 50)
        /// </summary>
        public sbyte VerticalValue
        {
            get
            {
                // Actually the value is inverted by default ;-)
                if (this.InvertVertical)
                {
                    return (sbyte)this._Vertical.RangeRead();
                }
                else
                {
                    return (sbyte)(0 - this._Vertical.RangeRead());
                }
            }
        }

        /// <summary>
        /// Returns the pushed state (false or true)
        /// </summary>
        public bool PushValue
        {
            get
            {
                return !this._Push.Read();
            }
        }

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
        /// <summary>
        /// Analog thumb joystick
        /// </summary>
        /// <param name="HorizontalPin">Analog pin for the horizontal bar</param>
        /// <param name="VerticalPin">Analog pin for the vertical bar</param>
        /// <param name="PushPin">Pin for the push button (optionally this class handles the push button)</param>
        /// <param name="InvertHorizontal">When true, the horizontal value will be inverted</param>
        /// <param name="InvertVertical">When true, the vertical value will be inverted</param>
        public ThumbJoystick(Cpu.AnalogChannel HorizontalPin, Cpu.AnalogChannel VerticalPin, Cpu.Pin PushPin = Cpu.Pin.GPIO_NONE, bool InvertHorizontal = false, bool InvertVertical = false)
        {
            // Configures the analog inputs
            this._Horizontal = new IntegratedADC(HorizontalPin);
            this._Horizontal.RangeSet(-50, 50);
            this._Vertical = new IntegratedADC(VerticalPin);
            this._Vertical.RangeSet(-50, 50);
            // Invertion will be copied
            this.InvertHorizontal = InvertHorizontal;
            this.InvertVertical = InvertVertical;
            // If needed, configures the digital input
            if (PushPin != Cpu.Pin.GPIO_NONE)
                this._Push = new InputPort(PushPin, false, Port.ResistorMode.PullUp);
        }
#endif

        /// <summary>
        /// Analog thumb joystick
        /// </summary>
        /// <param name="HorizontalPin">Analog pin for the horizontal bar</param>
        /// <param name="VerticalPin">Analog pin for the vertical bar</param>
        /// <param name="PushPin">Pin for the push button (optionally this class handles the push button)</param>
        /// <param name="InvertHorizontal">When true, the horizontal value will be inverted</param>
        /// <param name="InvertVertical">When true, the vertical value will be inverted</param>
        public ThumbJoystick(IADCPort HorizontalPin, IADCPort VerticalPin, Cpu.Pin PushPin = Cpu.Pin.GPIO_NONE, bool InvertHorizontal = false, bool InvertVertical = false)
        {
            // Configures the analog inputs
            this._Horizontal = HorizontalPin;
            this._Horizontal.RangeSet(-50, 50);
            this._Vertical = VerticalPin;
            this._Vertical.RangeSet(-50, 50);
            // Invertion will be copied
            this.InvertHorizontal = InvertHorizontal;
            this.InvertVertical = InvertVertical;
            // If needed, configures the digital input
            if (PushPin != Cpu.Pin.GPIO_NONE)
                this._Push = new InputPort(PushPin, false, Port.ResistorMode.PullUp);
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            this._Horizontal.Dispose();
            this._Vertical.Dispose();
            this._Push.Dispose();
        }
    }
}
