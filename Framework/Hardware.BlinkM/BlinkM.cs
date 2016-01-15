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
    /// BlinkM RGB LED
    /// </summary>
    public class BlinkM
    {
        /// <summary>
        /// Reference to the I²C bus
        /// </summary>
        private MultiI2C _Device;

        /// <summary>
        /// Initialises a new BlinkM RGB LED
        /// </summary>
        /// <param name="Address">The I²C address</param>
        /// <param name="ClockRateKhz">The module speed in Khz</param>
        public BlinkM(ushort Address = 0x09, int ClockRateKhz = 100)
        {
            this._Device = new MultiI2C(Address, ClockRateKhz);
            this.SendCommand('o'); // Stop Script
        }

        /// <summary>
        /// Go to RGB Color Now
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public void SetColor(byte r, byte g, byte b)
        {
            this.SendCommand('n', new byte[] { r, g, b });
        }

        /// <summary>
        /// Go to RGB Color Now
        /// </summary>
        /// <param name="Color">The color (0xff0000 is Red)</param>
        public void SetColor(int Color)
        {
            this.SendCommand('n', this._SplitColor(Color));
        }

        /// <summary>
        /// Fade to RGB Color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public void FadeColor(byte r, byte g, byte b)
        {
            this.SendCommand('c', new byte[] { r, g, b });
        }

        /// <summary>
        /// Fade to RGB Color
        /// </summary>
        /// <param name="Color">The color (0xff0000 is Red)</param>
        public void FadeColor(int Color)
        {
            this.SendCommand('c', this._SplitColor(Color));
        }

        /// <summary>
        /// Splits a color as integer to three bytes
        /// </summary>
        /// <param name="Color">The color (0xff0000 is Red)</param>
        /// <returns>A byte with array 3 values; red, green and blue</returns>
        private byte[] _SplitColor(int Color)
        {
            byte r = (byte)((Color & 0xff0000) >> 16);
            byte g = (byte)((Color & 0x00ff00) >> 8);
            byte b = (byte)(Color & 0x0000ff);
            return new byte[] { r, g, b };
        }

        /// <summary>
        /// Sends a raw command
        /// </summary>
        /// <param name="Command">The command to send</param>
        /// <param name="Arguments">Arguments belonging to the command</param>
        public void SendCommand(char Command, byte[] Arguments)
        {
            // Creates the writebuffer
            byte[] WriteBuffer = new byte[Arguments.Length + 1];
            // Starts with the command
            WriteBuffer[0] = (byte)Command;
            // Adds all arguments
            Arguments.CopyTo(WriteBuffer, 1);
            // Writes to the device
            int BytesTransferred = this._Device.Write(WriteBuffer);
            // Validates the transmission
            if (BytesTransferred != WriteBuffer.Length) throw new ApplicationException("Something went wrong executing the command. Did you use proper pull-up resistors?");
        }

        /// <summary>
        /// Sends a raw command
        /// </summary>
        /// <param name="Command">The command to send</param>
        public void SendCommand(char Command)
        {
            byte[] Arguments = new byte[0];
            this.SendCommand(Command, Arguments);
        }
    }
}
