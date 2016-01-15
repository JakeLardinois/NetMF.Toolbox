using System;
using Microsoft.SPOT.Hardware;

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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>.NETMF InterruptPort wrapper</summary>
    public class IntegratedIRQ : IIRQPort
    {
        /// <summary>Reference to the IRQ port</summary>
        private InterruptPort _Port;

        /// <summary>When true, the read value is inverted (useful when working with pull-up resistors)</summary>
        public bool InvertReadings { get; set; }
        /// <summary>Use this to give this IRQ port a unique identifier (default: blank)</summary>
        public string ID { get; set; }

        /// <summary>
        /// Creates a new IRQ Port
        /// </summary>
        /// <param name="Pin">The pin number</param>
        /// <param name="GlitchFilter">Turns on or off the glitchfilter</param>
        /// <param name="ResistorMode">Selects the resistor mode</param>
        public IntegratedIRQ(Cpu.Pin Pin, bool GlitchFilter = false, Port.ResistorMode ResistorMode = Port.ResistorMode.Disabled)
        {
            this._Port = new InterruptPort(Pin, GlitchFilter, ResistorMode, Port.InterruptMode.InterruptEdgeBoth);
            this._Port.OnInterrupt += new NativeEventHandler(_Port_OnInterrupt);
        }

        /// <summary>
        /// Triggered when the pin state changes
        /// </summary>
        /// <param name="PinId">The hardware pin #</param>
        /// <param name="Value">The new value</param>
        /// <param name="Time">Time of the event</param>
        private void _Port_OnInterrupt(uint PinId, uint Value, DateTime Time)
        {
            if (this.OnStateChange != null)
                this.OnStateChange(this, this.Read(), Time);
        }

        /// <summary>Reads the pin value</summary>
        /// <returns>True when high, false when low</returns>
        public bool Read()
        {
            return this.InvertReadings ? !this._Port.Read() : this._Port.Read();
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            this._Port.Dispose();
        }

        /// <summary>Event triggered when a IRQ port state changes</summary>
        public event StateChange OnStateChange;
    }
}
