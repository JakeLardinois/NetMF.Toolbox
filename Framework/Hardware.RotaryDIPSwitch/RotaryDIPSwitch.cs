using System;
using System.Threading;
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
    /// A binary Rotary DIP Switch
    /// </summary>
    public class RotaryDIPSwitch
    {
        /// <summary>
        /// Contains references to all pins
        /// </summary>
        private InterruptPort[] _Pins;

        /// <summary>
        /// Contains the last state
        /// </summary>
        private uint _LastState;

        /// <summary>
        /// Event triggered when the state changes
        /// </summary>
        public event NativeEventHandler OnInterrupt;

        /// <summary>
        /// Defines a binary Rotary DIP Switch
        /// </summary>
        /// <param name="Pins">An array with the pins for each bit</param>
        public RotaryDIPSwitch(Cpu.Pin[] Pins)
        {
            // Specifies the right dimentions for the _Pins Array
            this._Pins = new InterruptPort[Pins.Length];
            // Defines all pins as interrupt ports
            for (int i = 0; i < Pins.Length; ++i)
            {
                this._Pins[i] = new InterruptPort(Pins[i], false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                this._Pins[i].OnInterrupt += new NativeEventHandler(RotaryDIPSwitch_OnInterrupt);
                this._Pins[i].EnableInterrupt();
            }
            // Sets the initial value
            this._LastState = this._Read();
        }

        /// <summary>
        /// Interrupt triggered when a pin changes state
        /// </summary>
        /// <param name="Pin">The pin id that's changed</param>
        /// <param name="State">The new state</param>
        /// <param name="Time">Time of the event</param>
        private void RotaryDIPSwitch_OnInterrupt(uint Pin, uint State, DateTime Time)
        {
            // We want 5 times the same reading before we call it stable
            uint LastState = 0;
            uint CurrState = this._Read();
            for (int i = 0; i < 5; ++i)
            {
                LastState = CurrState;
                Thread.Sleep(1);
                CurrState = this._Read();
                // State has been changed. Another interrupt will pick that change up.
                if (LastState != CurrState) return;
            }
            // The state actually has been changed
            if (CurrState != this._LastState)
            {
                this._LastState = CurrState;
                // Triggers the interrupt
                if (this.OnInterrupt != null)
                    this.OnInterrupt(0, CurrState, new DateTime());
            }
        }

        /// <summary>
        /// Returns the current state of the switch
        /// </summary>
        /// <returns>It's current state</returns>
        private uint Read()
        {
            return this._LastState;
        }

        /// <summary>
        /// Reads the value of the switch
        /// </summary>
        /// <returns>The value of the switch</returns>
        private uint _Read()
        {
            uint Value = 0;
            uint Multiplier = 1;
            for (int i = 0; i < this._Pins.Length; ++i)
            {
                if (!this._Pins[i].Read()) Value += Multiplier;
                Multiplier *= 2;
            }
            return Value;
        }
    }
}