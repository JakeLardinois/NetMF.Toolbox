using System;
using System.IO.Ports; // Microsoft.SPOT.Hardware.SerialPort.dll
using Microsoft.SPOT;
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
    /// Rdm630 RFID Reader
    /// </summary>
    /// <remarks><![CDATA[
    /// RDM630 pin layout:
    /// 
    ///   10 9  8           7  6
    ///   │  │  │           │  │
    /// █████████████████████████
    /// █████████████████████████
    /// █████████████████████████
    /// █████████████████████████
    ///   │  │  │  │  │         
    ///   1  2  3  4  5         
    ///
    ///  1 TX (Data out) -> Netduino pin 0 or 2 (COM1 or COM2)
    ///  2 RX (Data in) -> Netduino pin 1 or 3 (COM1 or COM2), but since it's read-only, may be left empty
    ///  3 Unused
    ///  4 GND -> Netduino Gnd
    ///  5 +5V(DC) -> Netduino +5V
    ///  6 ANT1 -> Antenna (polarity doesn't matter)
    ///  7 ANT2 -> Antenna (polarity doesn't matter)
    ///  8 GND -> Netduino Gnd (but if pin 4 is already connected, this may be left empty)
    ///  9 +5V(DC) -> Netduino +5V (but if pin 5 is already connected, this may be left empty)
    /// 10 LED -> A led if you want to have a led signalling when there's a transfer
    /// ]]></remarks>
    public class Rdm630
    {
        /// <summary>
        /// Contains a reference to the serial port the Rdm630 is connected to
        /// </summary>
        private SerialPort _Serial;

        /// <summary>
        /// A read buffer of 14 bytes. Since every block of data has 14 bytes, this should be enough.
        /// </summary>
        private byte[] _ReadBuffer = new byte[14];

        /// <summary>
        /// The current position on the _ReadBuffer
        /// </summary>
        private byte _ReadPosition;

        /// <summary>
        /// Table to convert integers from the serial bus to a hex digit quickly
        /// </summary>
        private string _SerialConversionTable = "------------------------------------------------0123456789-------ABCDEF";

        /// <summary>
        /// Contains the last successfull RFID tag
        /// </summary>
        private string _LastSuccessfullRead;

        /// <summary>
        /// Triggered when data has been received
        /// </summary>
        public event NativeEventHandler DataReceived;

        /// <summary>
        /// The most recent scanned tag
        /// </summary>
        public string Tag
        {
            get { return this._LastSuccessfullRead; }
        }

        /// <summary>
        /// Rdm630 RFID Reader
        /// </summary>
        /// <param name="Port">The serial port the Rdm630 is connected to</param>
        public Rdm630(string Port)
        {
            this._Serial = new SerialPort(Port, 9600, Parity.None, 8, StopBits.One);
            this._Serial.ReadTimeout = 1000;
            this._Serial.DataReceived += new SerialDataReceivedEventHandler(_Serial_DataReceived);
            this._Serial.Open();
        }

        /// <summary>
        /// Triggers when there is new data on the serial port
        /// </summary>
        /// <param name="Sender">The sender of the event, which is the SerialPort object</param>
        /// <param name="EventData">A SerialDataReceivedEventArgs object that contains the event data</param>
        private void _Serial_DataReceived(object Sender, SerialDataReceivedEventArgs EventData)
        {
            // Reads the whole buffer from the serial port
            byte[] ReadBuffer = new byte[this._Serial.BytesToRead];
            this._Serial.Read(ReadBuffer, 0, ReadBuffer.Length);

            // Loops through all bytes
            for (uint Index = 0; Index < ReadBuffer.Length; ++Index)
            {
                // Start byte
                if (ReadBuffer[Index] == 2)
                    this._ReadPosition = 0;
                // Adds the digit to the global read buffer
                this._ReadBuffer[this._ReadPosition] = ReadBuffer[Index];
                // Increases the position of the global read buffer
                ++this._ReadPosition;
                // global read buffer is full, lets validate
                if (this._ReadPosition == this._ReadBuffer.Length)
                {
                    // Resets the read position
                    this._ReadPosition = 0;
                    // Announces we got a full set of bytes
                    this._Rdm630_DataReceived();
                }
            }
        }

        /// <summary>
        /// Triggers when a full RFID tag is scanned
        /// </summary>
        private void _Rdm630_DataReceived()
        {
            // Validates the start and stop byte (should be 2 & 3)
            if (this._ReadBuffer[0] != 2 || this._ReadBuffer[13] != 3) return;

            // Fetches the 10 digits
            string Digits = "";
            for (int Index = 0; Index < 10; ++Index)
            {
                // Index + 1 since the first byte is the start byte
                Digits += this._SerialConversionTable[this._ReadBuffer[Index + 1]];
            }

            // Fetches the checksum from the buffer
            string BufferCheckSum = "";
            BufferCheckSum += this._SerialConversionTable[this._ReadBuffer[11]];
            BufferCheckSum += this._SerialConversionTable[this._ReadBuffer[12]];

            // Calculates the checksum from the digits
            uint CalcCheckSum = 0;
            for (int Index = 0; Index < 10; Index = Index + 2)
            {
                CalcCheckSum = CalcCheckSum ^ Tools.Hex2Dec(Digits.Substring(Index, 2));
            }

            // Do both checksums match?
            if (Tools.Hex2Dec(BufferCheckSum) == CalcCheckSum)
            {
                this._LastSuccessfullRead = Digits;
                if (this.DataReceived != null)
                    this.DataReceived(0, 0, new DateTime());
            }
        }
    }
}