using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SL = SecretLabs.NETMF.Hardware;

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
    /// <summary>Collection of Netduino wrappers</summary>
    /// <remarks>I made this as a class so people actually have to type Netduino.PWM. This, to avoid conflicts with any other PWM class.</remarks>
    public static partial class Netduino
    {
        /// <summary>Netduino PWM Wrapper</summary>
        public class PWM : IPWMPort
        {
            /// <summary>Reference to the PWM port</summary>
            private SL.PWM _port;

            /// <summary>The time for each timeframe in nanoseconds</summary>
            private uint _period_ns = 0;
            /// <summary>The amount of nanoseconds the pulse must be high</summary>
            private uint _duration_ns = 0;

            /// <summary>Defines a Netduino PWM pin</summary>
            /// <param name="Pin">The Netduino pin</param>
            public PWM(Cpu.Pin Pin)
            {
                this._port = new SL.PWM(Pin);
            }

            /// <summary>Disposes the PWM object</summary>
            public override void Dispose()
            {
                this.StopPulse();
                this._port.Dispose();
            }

            /// <summary>Starts the signal</summary>
            public override void StartPulse()
            {
                this.Active = true;
                // Netduino PWM is accurate to µs instead of ns
                this._port.SetPulse(this._period_ns / 1000, this._duration_ns / 1000);
            }

            /// <summary>Stops the signal</summary>
            public override void StopPulse()
            {
                this.Active = false;
                this._port.SetDutyCycle(0);
            }

            /// <summary>Sets the signal in pulses</summary>
            /// <param name="period_ns">The time for each timeframe in nanoseconds</param>
            /// <param name="duration_ns">The amount of nanoseconds the pulse must be high</param>
            public override void SetPulse(uint period_ns, uint duration_ns)
            {
                this._period_ns = period_ns;
                this._duration_ns = duration_ns;
                if (this.Active) this.StartPulse();
            }
        }
    }
}