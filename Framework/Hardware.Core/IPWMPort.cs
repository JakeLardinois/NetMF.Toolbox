using System;
using Microsoft.SPOT;

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
    /// <summary>
    /// Generic PWM Port interface
    /// </summary>
    public abstract class IPWMPort : IDisposable
    {
        /// <summary>Starts the signal</summary>
        public abstract void StartPulse();
        /// <summary>Stops the signal</summary>
        public abstract void StopPulse();
        /// <summary>Checks if the signal is active</summary>
        public bool Active { get; protected set; }
        /// <summary>Disposes the PWM port</summary>
        public abstract void Dispose();
        /// <summary>Sets the signal in pulses</summary>
        /// <param name="period_ns">The time for each timeframe in nanoseconds</param>
        /// <param name="duration_ns">The amount of nanoseconds the pulse must be high</param>
        public abstract void SetPulse(uint period_ns, uint duration_ns);

        /// <summary>Sets the signal as dutycycle</summary>
        /// <param name="dutyCycle">The amount of changes per second</param>
        /// <param name="frequency">The PWM frequency</param>
        public void SetDutyCycle(uint dutyCycle, uint frequency = 1000)
        {
            float period_ns = (1 / (float)frequency) * 1000000000; // multiply with 1000000000 to get nano seconds
            float duration_ns = (float)dutyCycle / 100 * period_ns;
            //this._port.SetDutyCycle(dutyCycle);
            this.SetPulse((uint)period_ns, (uint)duration_ns);
        }
    }
}
