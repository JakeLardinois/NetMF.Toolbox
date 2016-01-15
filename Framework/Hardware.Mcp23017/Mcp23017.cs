using System;

using Microsoft.SPOT;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
 * 
 * This code is inspired on the Adafruit MCP23017 library for Arduino
 * available at https://github.com/adafruit/Adafruit-MCP23017-Arduino-Library
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
    /// MCP23017 pin expander
    /// </summary>
    public class Mcp23017
    {
        #region "Contructor"
        /// <summary>
        /// Reference to the I²C bus
        /// </summary>
        private MultiI2C _Device;

        // Registers for port A
        private const byte _IODIRA = 0x00;
        //private const byte _IPOLA = 0x02;
        //private const byte _GPINTENA = 0x04;
        //private const byte _DEFVALA = 0x06;
        //private const byte _INTCONA = 0x08;
        //private const byte _IOCONA = 0x0a;
        private const byte _GPPUA = 0x0c;
        //private const byte _INTFA = 0x0e;
        //private const byte _INTCAPA = 0x10;
        private const byte _GPIOA = 0x12;
        private const byte _OLATA = 0x14;

        // Registers for port B
        private const byte _IODIRB = 0x01;
        //private const byte _IPOLB = 0x03;
        //private const byte _GPINTENB = 0x05;
        //private const byte _DEFVALB = 0x07;
        //private const byte _INTCONB = 0x09;
        //private const byte _IOCONB = 0x0b;
        private const byte _GPPUB = 0x0d;
        //private const byte _INTFB = 0x0f;
        //private const byte _INTCAPB = 0x11;
        private const byte _GPIOB = 0x13;
        private const byte _OLATB = 0x15;

        /// <summary>
        /// Initialises a new MCP23017 pin expander
        /// </summary>
        /// <param name="Address">The I²C address</param>
        /// <param name="ClockRateKhz">The module speed in Khz</param>
        public Mcp23017(ushort Address = 0x20, int ClockRateKhz = 200)
        {
            // Initialises the device
            this._Device = new MultiI2C(Address, ClockRateKhz);
            // Set all pins to input by default
            this._Device.Write(new byte[] { _IODIRA, 0xff }); // All pins on port A
            this._Device.Write(new byte[] { _IODIRB, 0xff }); // All pins on port B

            // Prepares all pins
            this.Pins = new Mcp23017Port[16];
            for (int PinNo = 0; PinNo < this.Pins.Length; ++PinNo)
                this.Pins[PinNo] = new Mcp23017Port(this, PinNo);
        }
        #endregion

        #region "Reading/writing"
        /// <summary>
        /// Reads the state of a pin
        /// </summary>
        /// <param name="Pin">The pin (0 to 15)</param>
        /// <returns>True if it's high, false if it's low</returns>
        protected bool GetPin(int Pin)
        {
            // Pin in range?
            if (Pin < 0 || Pin > 15) throw new IndexOutOfRangeException("There are only 16 pins");

            // By default we work on block A
            byte GpioAddress = _GPIOA;

            // Pins 8 and higher are on block B
            if (Pin > 7)
            {
                Pin -= 8;
                GpioAddress = _GPIOB;
            }

            // Read the current GPIO values
            byte[] ReadBuffer = new byte[1];
            this._Device.WriteRead(new byte[] { GpioAddress }, ReadBuffer);
            byte GpioValues = ReadBuffer[0];

            // Returns the value of this specific pin
            return ((GpioValues >> Pin) & 0x01) == 0x01;
        }

        /// <summary>
        /// Enables pull-ups
        /// </summary>
        /// <param name="Pin">The pin (0 to 15)</param>
        /// <param name="PullHigh">True if the pin must be pulled high, false if it must not be</param>
        public void EnablePullup(int Pin, bool PullHigh)
        {
            // Pin in range?
            if (Pin < 0 || Pin > 15) throw new IndexOutOfRangeException("There are only 16 pins");

            // By default we work on block A
            byte PullAddress = _GPPUA;

            // Pins 8 and higher are on block B
            if (Pin > 7)
            {
                Pin -= 8;
                PullAddress = _GPPUB;
            }

            // Read the current pull-up values
            byte[] ReadBuffer = new byte[1];
            this._Device.WriteRead(new byte[] { PullAddress }, ReadBuffer);
            byte PullValues = ReadBuffer[0];

            // Toggles the pullup value
            if (PullHigh)
                PullValues |= (byte)(1 << Pin);
            else
                PullValues &= (byte)~(1 << Pin);

            // Writes the new pull-up values
            this._Device.Write(new byte[] { PullAddress, PullValues });
        }

        /// <summary>
        /// Sets the state of an output port
        /// </summary>
        /// <param name="Pin">The pin (0 to 15)</param>
        /// <param name="Value">True for high, false for low</param>
        protected void SetPin(int Pin, bool Value)
        {
            // Pin in range?
            if (Pin < 0 || Pin > 15) throw new IndexOutOfRangeException("There are only 16 pins");

            // By default we work on block A
            byte OlatAddress = _OLATA;
            byte GpioAddress = _GPIOA;

            // Pins 8 and higher are on block B
            if (Pin > 7)
            {
                Pin -= 8;
                OlatAddress = _OLATB;
                GpioAddress = _GPIOB;
            }

            // Read the current GPIO values
            byte[] ReadBuffer = new byte[1];
            this._Device.WriteRead(new byte[] { OlatAddress }, ReadBuffer);
            byte GpioValues = ReadBuffer[0];

            // Toggles the pin value
            if (Value)
                GpioValues |= (byte)(1 << Pin);
            else
                GpioValues &= (byte)~(1 << Pin);

            // Writes the new GPIO values
            this._Device.Write(new byte[] { GpioAddress, GpioValues });
        }

        /// <summary>
        /// Sets the state of multiple output ports
        /// </summary>
        /// <param name="StartBit">The first bit to write</param>
        /// <param name="Data">The data to write</param>
        /// <param name="BitCount">The amount of bits to write</param>
        /// <param name="Inverted">When true, bits will be inverted</param>
        protected void WriteByte(uint StartBit, uint Data, int BitCount, bool Inverted = false)
        {
            // Lets read all values
            byte[] ReadBuffer = new byte[1];
            this._Device.WriteRead(new byte[] { _OLATA }, ReadBuffer); byte BlockA = ReadBuffer[0];
            this._Device.WriteRead(new byte[] { _OLATB }, ReadBuffer); byte BlockB = ReadBuffer[0];

            // Changes all bits
            for (int BitNo = 0; BitNo < BitCount; ++BitNo)
            {
                // Gets the new value of the specific bit
                int BitMask = 1 << BitNo;
                bool NewValue = (Data & BitMask) == BitMask;
                // Gets the pin number
                int PinNo;
                if (Inverted)
                    PinNo = (int)(StartBit + BitNo);
                else 
                    PinNo = (int)(StartBit + BitCount - 1 - BitNo);

                // Sets the value to the right bit block
                if (PinNo < 8)
                {
                    if (NewValue)
                        BlockA |= (byte)(1 << PinNo);
                    else
                        BlockA &= (byte)~(1 << PinNo);
                }
                else
                {
                    PinNo -= 8;
                    if (NewValue)
                        BlockB |= (byte)(1 << PinNo);
                    else
                        BlockB &= (byte)~(1 << PinNo);
                }
            }

            // Writes the new GPIO values
            this._Device.Write(new byte[] { _GPIOA, BlockA });
            this._Device.Write(new byte[] { _GPIOB, BlockB });
        }

        /// <summary>
        /// Changes the mode of a pin
        /// </summary>
        /// <param name="Pin">The pin (0 to 15)</param>
        /// <param name="Output">True for output port, false for input port</param>
        protected void PinMode(int Pin, bool Output)
        {
            // Pin in range?
            if (Pin < 0 || Pin > 15) throw new IndexOutOfRangeException("There are only 16 pins");
            
            // By default we work on block A
            byte IODirAddress = _IODIRA;

            // Pins 8 and higher are on block B
            if (Pin > 7)
            {
                Pin -= 8;
                IODirAddress = _IODIRB;
            }

            // Requests the current block
            byte[] ReadBuffer = new byte[1];
            this._Device.WriteRead(new byte[] { IODirAddress }, ReadBuffer);
            byte IODirections = ReadBuffer[0];

            // Toggles the bit for the right pin
            if (Output)
                IODirections &= (byte)~(1 << Pin);
            else
                IODirections |= (byte)(1 << Pin);

            // Writes the new value
            this._Device.Write(new byte[] { IODirAddress, IODirections });
        }
        #endregion

        #region "Tristate pins"
        /// <summary>IRQ Port wrapper for the SPIShifterIn class</summary>
        protected class Mcp23017Port : ITRIPort
        {
            /// <summary>Reference to the main chip</summary>
            private Mcp23017 _Module;
            /// <summary>The number of the pin</summary>
            private int _PinNo;

            /// <summary>True when this is an outputport</summary>
            private bool _IsOutput = false;

            /// <summary>
            /// Defines a Tristate Port
            /// </summary>
            /// <param name="Module">The object of the main chip</param>
            /// <param name="PinNo">The number of the pin</param>
            public Mcp23017Port(Mcp23017 Module, int PinNo)
            {
                this._Module = Module;
                this._PinNo = PinNo;
            }

            /// <summary>Writes the pin value</summary>
            /// <param name="State">True for high, false for low</param>
            public void Write(bool State)
            {
                if (!this._IsOutput)
                {
                    this._IsOutput = true;
                    this._Module.PinMode(this._PinNo, true);
                }
                this._Module.SetPin(this._PinNo, State);
                this.State = State;
            }

            /// <summary>Reads the pin value</summary>
            /// <returns>True when high, false when low</returns>
            public bool Read()
            {
                if (this._IsOutput)
                {
                    this._IsOutput = false;
                    this._Module.PinMode(this._PinNo, false);
                }

                return this.InvertReadings ? !this._Module.GetPin(this._PinNo) : this._Module.GetPin(this._PinNo);
            }

            /// <summary>True when the pin is high, false when low</summary>
            public bool State { get; protected set; }

            /// <summary>Frees the pin</summary>
            public void Dispose() { }

            /// <summary>When true, the read value is inverted (useful when working with pull-up resistors)</summary>
            public bool InvertReadings { get; set; }
        }

        /// <summary>Reference to all pins</summary>
        public ITRIPort[] Pins;
        #endregion

        #region "Parallel Out support"
        /// <summary>
        /// Parallel Out class
        /// </summary>
        protected class Mcp23017ParallelOut : IParallelOut
        {
            /// <summary>Reference to the main chain</summary>
            private Mcp23017 _Module;
            /// <summary>The bit to start at</summary>
            private uint _StartBit;
            /// <summary>The amount of bits in this chain</summary>
            private uint _BitCount;
            /// <summary>The buffer of the data</summary>
            private uint _Buffer = 0;
            /// <summary>When true, bits will be inverted</summary>
            private bool _Inverted;
            /// <summary>Frees the pin for other usage</summary>
            public void Dispose() { }

            /// <summary>Initialises a new parallel output port</summary>
            /// <param name="Module">The object of the main chain</param>
            /// <param name="StartBit">The first bit to write</param>
            /// <param name="BitCount">The amount of bits to write</param>
            /// <param name="Inverted">When true, bits will be inverted</param>
            public Mcp23017ParallelOut(Mcp23017 Module, uint StartBit, uint BitCount, bool Inverted)
            {
                this._Module = Module;
                this._StartBit = StartBit;
                this._BitCount = BitCount;
                this._Inverted = Inverted;
            }

            /// <summary>Returns the last written block of data</summary>
            /// <returns>The last written block of data</returns>
            public uint Read()
            {
                return this._Buffer;
            }

            /// <summary>Amount of bits in the array</summary>
            public uint Size { get { return this._BitCount; } }

            /// <summary>Writes a block of data to the array</summary>
            /// <param name="Value">The block of data to write</param>
            public void Write(uint Value)
            {
                this._Buffer = Value;
                this._Module.WriteByte(this._StartBit, this._Buffer, (int)this._BitCount, this._Inverted);
            }
        }

        /// <summary>
        /// Creates a new parallel output port on this IC chain
        /// </summary>
        /// <param name="StartBit">The first bit to write to</param>
        /// <param name="BitCount">The amount of bits</param>
        /// <param name="Inverted">When true, bits will be inverted</param>
        /// <returns>Parallel output port object</returns>
        public IParallelOut CreateParallelOut(uint StartBit = 0, uint BitCount = 8, bool Inverted = false)
        {
            // Sets all pins as outputs
            for (uint PinNo = StartBit; PinNo < (StartBit + BitCount); ++PinNo)
                this.PinMode((int)PinNo, true);

            // Returns the new port
            return new Mcp23017ParallelOut(this, StartBit, BitCount, Inverted);
        }
        #endregion
    }
}
