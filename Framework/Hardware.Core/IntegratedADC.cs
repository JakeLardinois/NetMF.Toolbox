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
    /// <summary>.NETMF 4.2 AnalogInput wrapper</summary>
    public class IntegratedADC : IADCPort
    {
        /// <summary>Reference to the ADC port</summary>
        private AnalogInput _port;

        /// <summary>Defines a ADC pin</summary>
        /// <param name="AnalogChannel">The analog channel channel the pin is connected to</param>
        public IntegratedADC(Cpu.AnalogChannel AnalogChannel)
        {
            this._port = new AnalogInput(AnalogChannel);
        }

        /// <summary>Reads out a value between 0 and 1</summary>
        public override double AnalogRead()
        {
            return this._port.Read();
        }

        /// <summary>Disposes the ADC object</summary>
        public override void Dispose()
        {
            this._port.Dispose();
        }
    }
}
