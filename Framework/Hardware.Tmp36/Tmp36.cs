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
    /// TMP36 Temperature Sensor
    /// </summary>
    public class Tmp36
    {
        /// <summary>
        /// Reference to the analog input
        /// </summary>
        private IADCPort _AnalogPort;

        /// <summary>
        /// Returns the current temperature in Celcius
        /// </summary>
        /// <example>
        /// To read others then Celcius, try this:
        /// <code><![CDATA[
        /// float celcius = [this].Temperature;
        /// float kelvin = (float)(celcius + 273.15);
        /// float fahrenheit = (float)((celcius * 1.8) + 32.0);
        /// ]]></code>
        /// </example>
        public float Temperature
        {
            get
            {
                // Reads the sensor's value
                int CurrentValue = this._AnalogPort.RangeRead();
                // Converts the value to a voltage
                float CurrentVoltage = (float)(CurrentValue * 3.3 / 1024);
                // Converts the value to celsius
                float Temperature = (float)(CurrentVoltage - .5) * 100;
                // Returns the value
                return Temperature;
            }
        }

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
        /// <summary>
        /// TMP 36GZ Temperature Sensor
        /// </summary>
        /// <param name="AnalogPort">The port the sensor is connected to</param>
        public Tmp36(Cpu.AnalogChannel AnalogPort)
        {
            this._AnalogPort = new IntegratedADC(AnalogPort);
            this._AnalogPort.RangeSet(0, 1024);
        }
#endif

        /// <summary>
        /// TMP 36GZ Temperature Sensor
        /// </summary>
        /// <param name="AnalogPort">The port the sensor is connected to</param>
        public Tmp36(IADCPort AnalogPort)
        {
            this._AnalogPort = AnalogPort;
            this._AnalogPort.RangeSet(0, 1024);
        }
    }
}
