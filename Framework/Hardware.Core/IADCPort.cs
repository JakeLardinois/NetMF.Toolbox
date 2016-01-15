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
    /// Generic ADC Port interface
    /// </summary>
    public abstract class IADCPort : IDisposable
    {
        /// <summary>Reads out a value between 0 and 1</summary>
        public abstract double AnalogRead();
        /// <summary>Disposes the ADC port</summary>
        public abstract void Dispose();

        /// <summary>Gets the range minimum</summary>
        public int RangeMin { get; private set; }
        /// <summary>Gets the range maximum</summary>
        public int RangeMax { get; private set; }

        /// <summary>
        /// Reads the value between <see cref="RangeMin"/> and  <see cref="RangeMax"/>
        /// </summary>
        /// <returns>The value within range</returns>
        public int RangeRead()
        {
            return (int)(this.AnalogRead() * (RangeMax - RangeMin) + RangeMin);
        }

        /// <summary>Sets the range for <see cref="RangeRead"/></summary>
        /// <param name="Min">Minimal value</param>
        /// <param name="Max">Maximal value</param>
        public void RangeSet(int Min, int Max)
        {
            this.RangeMin = Min;
            this.RangeMax = Max;
        }
    }
}
