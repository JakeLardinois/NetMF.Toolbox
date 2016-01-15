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
    /// <summary>
    /// DS1307 RTC Module
    /// </summary>
    public class DS1307
    {
        /// <summary>
        /// Reference to the I²C bus
        /// </summary>
        private MultiI2C _Device;

        /// <summary>
        /// Initialises a new DS1307 RTC Module
        /// </summary>
        /// <param name="Address">The I²C address</param>
        /// <param name="ClockRateKhz">The module speed in Khz</param>
        public DS1307(byte Address = 0x68, int ClockRateKhz = 100)
        {
            this._Device = new MultiI2C(Address, ClockRateKhz);
        }

        /// <summary>
        /// Sets the time in the RTC module
        /// </summary>
        /// <param name="Time">The current time</param>
        public void SetTime(DateTime Time)
        {
            // Writing 7 bytes to the buffer, starting at address 0
            int BytesTransferred = this._Device.Write(new byte[] {
                0x00, 
                (byte)Tools.Hex2Dec(Time.Second.ToString()),
                (byte)Tools.Hex2Dec(Time.Minute.ToString()),
                (byte)Tools.Hex2Dec(Time.Hour.ToString()),
                (byte)Tools.Hex2Dec(((int)Time.DayOfWeek).ToString()),
                (byte)Tools.Hex2Dec(Time.Day.ToString()),
                (byte)Tools.Hex2Dec(Time.Month.ToString()),
                (byte)Tools.Hex2Dec((Time.Year - 2000).ToString())
            });
            if (BytesTransferred != 8) throw new ApplicationException("Something went wrong setting the time. Did you use proper pull-up resistors and is there a 3V battery connected?");
        }

        /// <summary>
        /// Sets the time in the RTC module
        /// </summary>
        /// <param name="Year">Current year</param>
        /// <param name="Month">Current month</param>
        /// <param name="Day">Current day</param>
        /// <param name="Hour">Current hour</param>
        /// <param name="Minute">Current minute</param>
        /// <param name="Second">Current second</param>
        public void SetTime(int Year, int Month, int Day, int Hour, int Minute, int Second)
        {
            this.SetTime(new DateTime(Year, Month, Day, Hour, Minute, Second));
        }

        /// <summary>
        /// Gets the time from the RTC module
        /// </summary>
        /// <returns>The current time</returns>
        public DateTime GetTime()
        {
            // Reads 7 bytes from the buffer, starting at address 0
            byte[] ReadBuffer = new byte[7];
            int BytesTransferred = this._Device.WriteRead(new byte[] { 0x00 }, ReadBuffer);
            if (BytesTransferred != 8) throw new ApplicationException("Something went wrong reading the time. Did you use proper pull-up resistors and is there a 3V battery connected?");

            return new DateTime(
                second: int.Parse(Tools.Dec2Hex(ReadBuffer[0], 1)),
                minute: int.Parse(Tools.Dec2Hex(ReadBuffer[1], 1)),
                hour: int.Parse(Tools.Dec2Hex(ReadBuffer[2], 1)),
                day: int.Parse(Tools.Dec2Hex(ReadBuffer[4], 1)),
                month: int.Parse(Tools.Dec2Hex(ReadBuffer[5], 1)),
                year: int.Parse(Tools.Dec2Hex(ReadBuffer[6], 1)) + 2000
            );
        }

        /// <summary>
        /// Synchronizes the MCU with the RTC module
        /// </summary>
        public void Synchronize()
        {
            // Sets the time on the MCU
            Utility.SetLocalTime(this.GetTime());
        }
    }
}
