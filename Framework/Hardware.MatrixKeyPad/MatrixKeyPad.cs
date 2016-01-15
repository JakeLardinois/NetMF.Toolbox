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
    /// <summary>Generic KeyPad driver</summary>
    public class MatrixKeyPad : IDisposable
    {
        /// <summary>When a button is pushed, this event will be triggered</summary>
        public event NativeEventHandler OnKeyDown;

        /// <summary>When a button is released, this event will be triggered</summary>
        public event NativeEventHandler OnKeyUp;

        /// <summary>A collection of all column pins</summary>
        private InterruptPort[] _ColPins;
        /// <summary>A collection of all row pins</summary>
        private TristatePort[] _RowPins;
        /// <summary>A collection of all column pin IDs</summary>
        private uint[] _ColPinIds;

        /// <summary>We have a few states in which we do different checks</summary>
        private enum CheckStates
        {
            /// <summary>Waiting for a key press</summary>
            WaitingForSignal = 0,
            /// <summary>Searching the row</summary>
            RowCheck = 1,
            /// <summary>Waiting for a key release</summary>
            WaitingForRelease = 2,
            /// <summary>Multiple buttons are pressed, waiting for a release</summary>
            WaitingForMultipleRelease = 3,
        }
        /// <summary>Contains the current state</summary>
        private CheckStates _CheckState;

        /// <summary>Stores the last key press</summary>
        private uint _LastKeyPress;

        /// <summary>Generic KeyPad driver</summary>
        /// <param name="RowPins">The pins bound to rows on the keypad matrix</param>
        /// <param name="ColPins">The pins bound to columns on the keypad matrix</param>
        /// <remarks>See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.MatrixKeyPad </remarks>
        public MatrixKeyPad(Cpu.Pin[] RowPins, Cpu.Pin[] ColPins)
        {
            // Defines all RowPins
            this._RowPins = new TristatePort[RowPins.Length];
            for (var RowPinCount = 0; RowPinCount < RowPins.Length; ++RowPinCount)
            {
                this._RowPins[RowPinCount] = new TristatePort(RowPins[RowPinCount], false, false, Port.ResistorMode.PullUp);
                this._RowPins[RowPinCount].Active = true;
            }
            // Defines all ColPins
            this._ColPinIds = new uint[ColPins.Length];
            this._ColPins = new InterruptPort[ColPins.Length];
            for (var ColPinCount = 0; ColPinCount < ColPins.Length; ++ColPinCount)
            {
                this._ColPinIds[ColPinCount] = (uint)ColPins[ColPinCount];
                this._ColPins[ColPinCount] = new InterruptPort(ColPins[ColPinCount], true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                this._ColPins[ColPinCount].OnInterrupt += new NativeEventHandler(MatrixKeyPad_OnInterrupt);
            }
            // Defines the check state
            this._CheckState = CheckStates.WaitingForSignal;
        }

        /// <summary>
        /// Event triggered when a button is pressed or released
        /// </summary>
        /// <param name="ColPinId">The Column Pin in which a key is pressed</param>
        /// <param name="State">The state of the button (0 = pressed, 1 = released)</param>
        /// <param name="time">Time of the event</param>
        void MatrixKeyPad_OnInterrupt(uint ColPinId, uint State, DateTime time)
        {
            // Translates the ColPinId to the actual column
            uint ColPin;
            for (ColPin = 0; ColPin < this._ColPinIds.Length; ++ColPin)
            {
                if (this._ColPinIds[ColPin] == ColPinId)
                {
                    break;
                }
            }

            if (this._CheckState == CheckStates.WaitingForSignal && State == 0)
            {
                // To avoid interrupts interfear with each other we disable them temporarily
                this.ActivateColInterrupts(false);
                // Button pressed. We need to find out in which row!
                this._CheckState = CheckStates.RowCheck;
                // We set each pin high one by one
                for (uint RowPinCount = 0; RowPinCount < this._RowPins.Length; ++RowPinCount)
                {
                    this.ActivateRowPorts(false);
                    this._RowPins[RowPinCount].Active = true;
                    if (!this._ColPins[ColPin].Read())
                    {
                        // Keypress found, we calculate the key number
                        this._LastKeyPress = unchecked((uint)(RowPinCount * this._ColPins.Length + ColPin));
                    }
                }
                // Activates all row pins again
                this.ActivateRowPorts(true);
                // Now lets wait for the key to be released
                this._CheckState = CheckStates.WaitingForRelease;
                // Re-activates the interrupts
                this.ActivateColInterrupts(true);
                // Sends back the keynumber through the event (if it exists)
                if (this.OnKeyDown != null)
                    this.OnKeyDown(this._LastKeyPress, 0, new DateTime());
            }
            else if (this._CheckState == CheckStates.WaitingForRelease && State == 1)
            {
                // Button released, send back the event (if it exists)
                if (this.OnKeyUp != null)
                    this.OnKeyUp(this._LastKeyPress, 0, new DateTime());
                this._CheckState = CheckStates.WaitingForSignal;
            }
        }

        /// <summary>Switches all Row ports activity</summary>
        /// <param name="Active">True when they must be active, false otherwise</param>
        private void ActivateRowPorts(bool Active)
        {
            for (var RowPinCount = 0; RowPinCount < this._RowPins.Length; ++RowPinCount)
            {
                if (this._RowPins[RowPinCount].Active != Active)
                    this._RowPins[RowPinCount].Active = Active;
            }
        }

        /// <summary>Disables or enables all interrupt events</summary>
        /// <param name="Active">When true, all events will be enabled, oftherwise disabled</param>
        private void ActivateColInterrupts(bool Active)
        {
            // Switching the interrupts
            for (uint ColPinCount = 0; ColPinCount < this._ColPins.Length; ++ColPinCount)
            {
                if (Active)
                    this._ColPins[ColPinCount].EnableInterrupt();
                else
                    this._ColPins[ColPinCount].DisableInterrupt();
            }
        }

        /// <summary>Reads the KeyPad and returns the currently pressed scan code</summary>
        /// <returns>The key code or -1 when nothing is pressed</returns>
        public int Read()
        {
            if (this._CheckState == CheckStates.WaitingForSignal)
                return -1;
            else
                return unchecked((int)this._LastKeyPress);
        }

        /// <summary>
        /// Disposes this object and frees the pins
        /// </summary>
        public void Dispose()
        {
            for (int Counter = 0; Counter < this._ColPins.Length; ++Counter)
                this._ColPins[Counter].Dispose();
            for (int Counter = 0; Counter < this._RowPins.Length; ++Counter)
                this._RowPins[Counter].Dispose();
        }

    }
}
