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
    /// Bitbang Buzzer class for if a buzzer isn't conneced to a PWM-pin
    /// </summary>
    /// <remarks>
    /// Sometimes you want to add a buzzer, but all PWM-pins are already taken. This class comes to rescue!
    /// </remarks>
    public class BitBangBuzzer : IDisposable
    {
        /// <summary>
        /// Contains the current state of the buzzer
        /// </summary>
        private bool _State;

        /// <summary>
        /// Contains the pin
        /// </summary>
        private OutputPort _OutputPin;

        /// <summary>
        /// Interval of the speaker in ms
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Reference to the seperate buzzer thread
        /// </summary>
        private Thread _PinSwitcherThread;

        /// <summary>
        /// Bit Bang Buzzer
        /// </summary>
        /// <remarks>
        /// Sometimes you want to add a buzzer, but all PWM-pins are already taken. This class comes to rescue!
        /// </remarks>
        /// <param name="PortId">The pin the buzzer is connected to (on Netduino: Pins.GPIO_PIN_D3)</param>
        /// <param name="InitialState">The buzzers initial state</param>
        public BitBangBuzzer(Cpu.Pin PortId, bool InitialState = false)
        {
            this._State = InitialState;
            this._OutputPin = new OutputPort(PortId, InitialState);
            this.Interval = 10;

            // Creates a background thread for pin switching
            ThreadStart PinSwitcher = new ThreadStart(this._PinSwitcherLoop);
            _PinSwitcherThread = new Thread(PinSwitcher);
            _PinSwitcherThread.Start();

        }

        /// <summary>
        /// Switches the pin values if needed
        /// </summary>
        private void _PinSwitcherLoop()
        {
            while (true)
            {
                this._OutputPin.Write(this._State ? !this._OutputPin.Read() : false);
                Thread.Sleep(this.Interval);
            }
        }

        /// <summary>
        /// Writes the state of the buzzer
        /// </summary>
        /// <param name="Value"></param>
        public void Write(bool Value)
        {
            this._State = Value;
        }

        /// <summary>
        /// Reads the state of the buzzer
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            return this._State;
        }

        /// <summary>
        /// Disposes the buzzer object
        /// </summary>
        public void Dispose()
        {
            // Stops the background thread
            _PinSwitcherThread.Suspend();
            // Frees the pin
            _OutputPin.Dispose();
        }
    }
}