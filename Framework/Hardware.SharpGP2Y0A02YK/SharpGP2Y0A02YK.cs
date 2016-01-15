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
    /// Sharp Long Distance Measuring Sensor
    /// </summary>
    public class SharpGP2Y0A02YK
    {
        /// <summary>
        /// Contains a reference to the analog input port
        /// </summary>
        private IADCPort _Input;

        /// <summary>
        /// Sends back the distance in centimeters (IR sensors aren't as accurate as sonic though!)
        /// </summary>
        public int Distance
        {
            get
            {
                float Voltage = (float)(3.3 / 1024 * this._Input.RangeRead());
                return (int)(73 / System.Math.Pow(Voltage, 1.45));
            }
        }

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
        /// <summary>
        /// Sharp Long Distance Measuring Sensor
        /// </summary>
        /// <param name="InputPort">Port the sensor is connected to</param>
        public SharpGP2Y0A02YK(Cpu.AnalogChannel InputPort)
        {
            // Defines the input port which reads 0 to 3300 millivolt
            this._Input = new IntegratedADC(InputPort);
            this._Input.RangeSet(0, 1024);
        }
#endif

        /// <summary>
        /// Sharp Long Distance Measuring Sensor
        /// </summary>
        /// <param name="InputPort">Port the sensor is connected to</param>
        public SharpGP2Y0A02YK(IADCPort InputPort)
        {
            // Defines the input port which reads 0 to 3300 millivolt
            this._Input = InputPort;
            this._Input.RangeSet(0, 1024);
        }
    }
}
