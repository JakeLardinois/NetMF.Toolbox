using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Toolbox.NETMF.Hardware;

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
    /// <summary>The Provider Collection links hardware providers to other classes</summary>
    public static class ProviderCollection
    {
        #region "PWM Pin Mapper"
        /// <summary>Reference to the PWM Pin Mapper method</summary>
        private static PWMPinMapper _PWMPinMapper;

        /// <summary>Gets a IPWMPort-compatible class from a pin</summary>
        /// <param name="pin">The pin</param>
        /// <returns>The IPWMPort-compatible class</returns>
        public static IPWMPort GetPWMByPin(Cpu.Pin pin)
        {
            if (_PWMPinMapper == null) 
                throw new NotImplementedException("There is no .NET Micro Framework Toolbox hardware provider loaded with PWM functionalities.");
            return _PWMPinMapper(pin);
        }

        /// <summary>Registers a PWM Pin Mapper method</summary>
        /// <param name="mapper">The mapper method</param>
        public static void RegisterPWMPinMapper(PWMPinMapper mapper)
        {
            _PWMPinMapper = mapper;
        }
        
        /// <summary>Interface for a PWM Pin Mapper method</summary>
        /// <param name="pin">The pin</param>
        /// <returns>The IPWMPort-compatible class</returns>
        public delegate IPWMPort PWMPinMapper(Cpu.Pin pin);
        #endregion

        #region "ADC Pin Mapper"
        /// <summary>Reference to the ADC Pin Mapper method</summary>
        private static ADCPinMapper _ADCPinMapper;

        /// <summary>Gets a IADCPort-compatible class from a pin</summary>
        /// <param name="pin">The pin</param>
        /// <returns>The IADCPort-compatible class</returns>
        public static IADCPort GetADCByPin(Cpu.Pin pin)
        {
            if (_ADCPinMapper == null)
                throw new NotImplementedException("There is no .NET Micro Framework Toolbox hardware provider loaded with ADC functionalities.");
            return _ADCPinMapper(pin);
        }

        /// <summary>Registers a ADC Pin Mapper method</summary>
        /// <param name="mapper">The mapper method</param>
        public static void RegisterADCPinMapper(ADCPinMapper mapper)
        {
            _ADCPinMapper = mapper;
        }

        /// <summary>Interface for a ADC Pin Mapper method</summary>
        /// <param name="pin">The pin</param>
        /// <returns>The IADCPort-compatible class</returns>
        public delegate IADCPort ADCPinMapper(Cpu.Pin pin);
        #endregion
    }
}
