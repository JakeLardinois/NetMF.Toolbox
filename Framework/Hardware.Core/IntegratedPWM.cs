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
    /// <summary>.NETMF 4.2 PWM wrapper</summary>
    public class IntegratedPWM : IPWMPort
    {
        /// <summary>Reference to the PWM port</summary>
        private PWM _Port;

        /// <summary>Defines the PWM pin</summary>
        /// <param name="PWMChannel">The channel the pin is connected to</param>
        public IntegratedPWM(Cpu.PWMChannel PWMChannel)
        {
            this._Port = new PWM(PWMChannel, 0, 0, PWM.ScaleFactor.Microseconds, false);
        }
        
        /// <summary>Starts the signal</summary>
        public override void StartPulse()
        {
            this._Port.Start();
            this.Active = true;
        }

        /// <summary>Stops the signal</summary>
        public override void StopPulse()
        {
            this._Port.Stop();
            this.Active = false;
        }

        /// <summary>Sets the signal in pulses</summary>
        /// <param name="period_ns">The time for each timeframe in nanoseconds</param>
        /// <param name="duration_ns">The amount of nanoseconds the pulse must be high</param>
        public override void SetPulse(uint period_ns, uint duration_ns)
        {
            switch (this._Port.Scale)
            {
                case PWM.ScaleFactor.Nanoseconds:
                    this._Port.Duration = duration_ns;
                    this._Port.Period = period_ns;
                    break;
                case PWM.ScaleFactor.Microseconds:
                    this._Port.Duration = duration_ns / 1000;
                    this._Port.Period = period_ns / 1000;
                    break;
                case PWM.ScaleFactor.Milliseconds:
                    this._Port.Duration = duration_ns / 1000000;
                    this._Port.Period = period_ns / 1000000;
                    break;
                default:
                    throw new NotImplementedException("Appairently the PWM has a scale factor that is unknown to me!");
            }
        }

        /// <summary>Disposes the PWM object</summary>
        public override void Dispose()
        {
            this._Port.Stop();
            this._Port.Dispose();
        }
    }
}
