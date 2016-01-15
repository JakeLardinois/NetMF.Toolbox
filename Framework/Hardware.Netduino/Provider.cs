using System;
using Microsoft.SPOT.Hardware;

/*
 * Copyright 2013-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
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
        /// <summary>Class that installs the Hardware provider</summary>
        internal static class Provider
        {
            /// <summary>Main method</summary>
            static Provider()
            {
                ProviderCollection.RegisterPWMPinMapper(_PWMMapper);
                ProviderCollection.RegisterADCPinMapper(_ADCMapper);
            }

            /// <summary>PWM Pin Mapper</summary>
            /// <param name="pin">Pin</param>
            /// <returns>IPWMPort-compatible class</returns>
            static IPWMPort _PWMMapper(Cpu.Pin pin)
            {
                return new Netduino.PWM(pin);
            }

            /// <summary>ADC Pin Mapper</summary>
            /// <param name="pin">Pin</param>
            /// <returns>IADCPort-compatible class</returns>
            static IADCPort _ADCMapper(Cpu.Pin pin)
            {
                return new Netduino.ADC(pin);
            }
        }
    }
}
