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
    /// Wii Nunchuk
    /// </summary>
    public class WiiNunchuk
    {
        /// <summary>
        /// Reference to the I²C bus
        /// </summary>
        private MultiI2C _Device;

        /// <summary>Analog stick X axis value</summary>
        public byte AnalogStickX { get; protected set; }
        /// <summary>Analog stick Y axis value</summary>
        public byte AnalogStickY { get; protected set; }

        /// <summary>Accelerometer X axis value</summary>
        public ushort AcceleroMeterX { get; protected set; }
        /// <summary>Accelerometer Y axis value</summary>
        public ushort AcceleroMeterY { get; protected set; }
        /// <summary>Accelerometer Z axis value</summary>
        public ushort AcceleroMeterZ { get; protected set; }

        /// <summary>C-Button value</summary>
        public bool ButtonC { get; protected set; }
        /// <summary>Z-Button value</summary>
        public bool ButtonZ { get; protected set; }

        /// <summary>
        /// Initialises a new Wii Nunchuk
        /// </summary>
        /// <param name="Address">The I²C address</param>
        /// <param name="ClockRateKhz">The speed in Khz</param>
        public WiiNunchuk(ushort Address = 0x52, int ClockRateKhz = 100)
        {
            // This initialisation routine seems to work. I got it at http://wiibrew.org/wiki/Wiimote/Extension_Controllers#The_New_Way
            this._Device = new MultiI2C(Address, ClockRateKhz);
            this._Device.Write(new byte[] { 0xf0, 0x55 });
            this._Device.Write(new byte[] { 0xfb, 0x00 });
        }

        /// <summary>
        /// Reads all data from the nunchuk
        /// </summary>
        public void Read()
        {
            // Requests data to read
            this._Device.Write(new byte[] { 0 });

            // Reads 6 bytes
            byte[] ReadBuffer = new byte[6];
            int BytesTransfered = this._Device.Read(ReadBuffer);
            if (BytesTransfered != 6) throw new ApplicationException("Something went wrong reading the Nunchuk. Did you use proper pull-up resistors?");

            // Parses data according to http://wiibrew.org/wiki/Wiimote/Extension_Controllers/Nunchuck#Data_Format
            
            // Analog stick
            this.AnalogStickX = ReadBuffer[0];
            this.AnalogStickY = ReadBuffer[1];
            // Accellerometer
            ushort AX = (ushort)(ReadBuffer[2] << 2);
            ushort AY = (ushort)(ReadBuffer[3] << 2);
            ushort AZ = (ushort)(ReadBuffer[4] << 2);
            AZ += (ushort)((ReadBuffer[5] & 0xc0) >> 6); // 0xc0 = 11000000
            AY += (ushort)((ReadBuffer[5] & 0x30) >> 4); // 0x30 = 00110000
            AX += (ushort)((ReadBuffer[5] & 0x0c) >> 2); // 0x0c = 00001100
            this.AcceleroMeterX = AX;
            this.AcceleroMeterY = AY;
            this.AcceleroMeterZ = AZ;
            // Buttons
            ButtonC = (ReadBuffer[5] & 0x02) != 0x02;    // 0x02 = 00000010
            ButtonZ = (ReadBuffer[5] & 0x01) != 0x01;    // 0x01 = 00000001
        }
    }
}
