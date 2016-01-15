using System;
using Microsoft.SPOT;
using System.IO.Ports; // Microsoft.SPOT.Hardware.SerialPort

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
    /// Pololu Micro Serial Servo Controller
    /// </summary>
    public class MicroSerialServoController : IDisposable
    {
        /// <summary>
        /// Reference to the serial port
        /// </summary>
        private SerialPort _Controller;

        /// <summary>
        /// Contains the current used mode
        /// </summary>
        private Modes _Mode;

        /// <summary>
        /// Different communication modes of the board
        /// </summary>
        public enum Modes
        {
            /// <summary>MiniSSC2 communication mode (use this when a jumper is on the board)</summary>
            MiniSSC2 = 0,
            /// <summary>Pololu communication mode (Recommended; use this when no jumper is on the board)</summary>
            Pololu = 1
        }

        /// <summary>
        /// Pololu Micro Serial Servo Controller
        /// </summary>
        /// <param name="Port">Serial port the controller is connected to</param>
        /// <param name="Mode">The mode the controller is in (when the Mode-jumper is placed on the board, it's MiniSSC2)</param>
        public MicroSerialServoController(String Port, Modes Mode)
        {
            this._Mode = Mode;
            this._Controller = new SerialPort(Port, 9600);
            this._Controller.Open();
        }

        /// <summary>
        /// Sets specific parameters for a servo (Pololu-mode only)
        /// </summary>
        /// <param name="MotorId">The number of the motor</param>
        /// <param name="On">Specifies whether the motor is on or not</param>
        /// <param name="Reversed">When true, it will move in reverse</param>
        /// <param name="Range">The range through which the servo moves (0 to 31)</param>
        public void SetParameters(byte MotorId, bool On, bool Reversed, byte Range = 15)
        {
            if (this._Mode != Modes.Pololu)
                throw new InvalidOperationException();
            if (Range > 31)
                throw new ArgumentOutOfRangeException();

            // Combines the range and both boolean values
            byte Parameters = (byte)(
                Range +
                (Reversed ? 0x20 : 0) +
                (On ? 0x40 : 0)
            );

            this.SendCommand(0, MotorId, Parameters);
        }

        /// <summary>
        /// Sets the speed at which the servo moves (Pololu-mode only)
        /// </summary>
        /// <param name="MotorId">The number of the motor</param>
        /// <param name="Speed">The speed of movement (1 to 127 or 0 to disable any delay)</param>
        public void SetSpeed(byte MotorId, byte Speed)
        {
            if (this._Mode != Modes.Pololu)
                throw new InvalidOperationException();
            if (Speed > 127)
                throw new ArgumentOutOfRangeException();

            this.SendCommand(1, MotorId, Speed);
        }

        /// <summary>
        /// Changes the position of a motor (works in both modes)
        /// </summary>
        /// <param name="MotorId">The number of the motor</param>
        /// <param name="Position">The position the motor must go to (0 to 254)</param>
        public void SetPosition(byte MotorId, byte Position)
        {
            if (MotorId > 254 || Position > 254)
                throw new ArgumentOutOfRangeException();

            if (this._Mode == Modes.MiniSSC2)
            {
                // MiniSSC2 mode
                byte[] WriteBuffer = new byte[] { 0xff, MotorId, Position };
                this._Controller.Write(WriteBuffer, 0, WriteBuffer.Length);
            }
            else
            {
                // Pololu mode
                this.SendCommand(2, MotorId, (byte)(Position & 0x7F), (byte)(Position >> 7));
            }
        }

        /// <summary>
        /// Sends a customized command (Pololu-mode only, see manual for commands)
        /// </summary>
        /// <param name="Command">Command ID</param>
        /// <param name="MotorId">Motor ID</param>
        /// <param name="Data1">Data byte 1</param>
        public void SendCommand(byte Command, byte MotorId, byte Data1) {
            byte[] WriteBuffer = new byte[] {
                0x80,    // Start Byte
                0x01,    // Device ID
                Command, // Command
                MotorId, // Servo number
                Data1    // Data byte 1
            };
            this.SendCommand(WriteBuffer);
        }

        /// <summary>
        /// Sends a customized command (Pololu-mode only, see manual for commands)
        /// </summary>
        /// <param name="Command">Command ID</param>
        /// <param name="MotorId">Motor ID</param>
        /// <param name="Data1">Data byte 1</param>
        /// <param name="Data2">Data byte 2</param>
        public void SendCommand(byte Command, byte MotorId, byte Data1, byte Data2)
        {
            byte[] WriteBuffer = new byte[] {
                0x80,    // Start Byte
                0x01,    // Device ID
                Command, // Command
                MotorId, // Servo number
                Data1,   // Data byte 1
                Data2    // Data byte 2
            };
            this.SendCommand(WriteBuffer);
        }

        /// <summary>
        /// Sends a customized command (Pololu-mode only, see manual for commands)
        /// </summary>
        /// <param name="WriteBuffer">A byte array with the command and it's parameters</param>
        private void SendCommand(byte[] WriteBuffer)
        {
            // First byte must be 0x80
            if (WriteBuffer[0] != 0x80)
                throw new InvalidOperationException();

            // All other bytes may not contain more then 7 bits
            for (int Pos = 1; Pos < WriteBuffer.Length; ++Pos)
                if (WriteBuffer[Pos] > 127)
                    throw new ArgumentOutOfRangeException();

            this._Controller.Write(WriteBuffer, 0, WriteBuffer.Length);
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            this._Controller.Close();
            this._Controller.Dispose();
        }
    }
}
