using System;
using Microsoft.SPOT;
using GHI = GHIElectronics.NETMF.Hardware;

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
    /// <summary>Collection of Fez wrappers</summary>
    /// <remarks>I made this as a class so people actually have to type Fez.PWM. This, to avoid conflicts with any other PWM class.</remarks>
    public static partial class Fez
    {
        /// <summary>
        /// Fez PWM Wrapper
        /// </summary>
        public class PWM : IPWMPort
        {
            /// <summary>Reference to the PWM port</summary>
            private GHI.PWM _Port;

            /// <summary>The time for each timeframe in nanoseconds</summary>
            private uint _period_ns = 0;
            /// <summary>The amount of nanoseconds the pulse must be high</summary>
            private uint _duration_ns = 0;

            /// <summary>Starts the signal</summary>
            public override void StartPulse()
            {
                this.Active = true;
                this._Port.SetPulse(this._period_ns, this._duration_ns);
            }

            /// <summary>Disposes the PWM object</summary>
            public override void Dispose()
            {
                this.StopPulse();
                this._Port.Dispose();
            }

            /// <summary>Stops the signal</summary>
            public override void StopPulse()
            {
                this.Active = false;
                this._Port.Set(false);
            }

            /// <summary>Defines a Fez PWM pin</summary>
            /// <param name="Pin">The Fez pin</param>
            public PWM(GHI.PWM.Pin Pin)
            {
                this._Port = new GHI.PWM(Pin);
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
