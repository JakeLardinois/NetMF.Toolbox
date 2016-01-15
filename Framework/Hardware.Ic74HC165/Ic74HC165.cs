using System;
using System.Threading;
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
    /// A chain of one or multiple parallel to serial ICs over managed SPI
    /// </summary>
    public class Ic74hc165 : IDisposable
    {
        #region "Contructors and destructor"
        /// <summary>A reference to the SPI Interface</summary>
        private MultiSPI _SpiInterface;

        /// <summary>Contains all pin values</summary>
        private byte[] _Data;

        /// <summary>When using bitbang mode, this bool is true. <see cref="_SpiInterface"/> won't be used if this is true.</summary>
        private bool _BitBangMode = false;
        /// <summary>When using bitbang mode, this will contain a reference to the SPCK pin. See also <see cref="_BitBangMode"/></summary>
        private IGPOPort _BBM_SPCK;
        /// <summary>When using bitbang mode, this will contain a reference to the CS pin. See also <see cref="_BitBangMode"/></summary>
        private IGPOPort _BBM_CS;
        /// <summary>When using bitbang mode, this will contain a reference to the MISO pin. See also <see cref="_BitBangMode"/></summary>
        private IGPIPort _BBM_MISO;
        /// <summary>Set to true when bitbang pins are created by the constructor</summary>
        private bool _PinDisposalRequired = false;

        /// <summary>
        /// Initialises a chain of one or multiple parallel to serial ICs over managed SPI
        /// </summary>
        /// <param name="SPI_Module">The SPI interface it's connected to</param>
        /// <param name="LatchPin">The slave select pin connected to the IC(s)</param>
        /// <param name="Bytes">The amount of 8-bit IC(s) connected</param>
        /// <param name="SpeedKHz">The max. SPI speed</param>
        public Ic74hc165(SPI.SPI_module SPI_Module, Cpu.Pin LatchPin, uint Bytes = 1, uint SpeedKHz = 1000)
        {
            // Full SPI configuration
            this._SpiInterface = new MultiSPI(new SPI.Configuration(
                ChipSelect_Port: LatchPin,
                ChipSelect_ActiveState: true,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: true,
                Clock_Edge: false,
                Clock_RateKHz: SpeedKHz,
                SPI_mod: SPI_Module
            ));

            // The amount of ICs
            this._Init(Bytes);
        }

        /// <summary>
        /// Initialises a chain of one or multiple parallel to serial ICs over bitbanged SPI [WHEN POSSIBLE, USE MANAGED MODE!]
        /// </summary>
        /// <remarks>
        /// Use only when the managed SPI-pins can't be used. This method is way slower and locks the pins for any other purpose until disposed.
        /// </remarks>
        /// <param name="ClockPin">The clock pin connected to the IC(s)</param>
        /// <param name="DataPin">The data pin connected to the IC(s)</param>
        /// <param name="LatchPin">The slave select pin connected to the IC(s)</param>
        /// <param name="Bytes">The amount of 8-bit IC(s) connected</param>
        public Ic74hc165(Cpu.Pin ClockPin, Cpu.Pin DataPin, Cpu.Pin LatchPin, uint Bytes = 1)
        {
            // Enables bitbang mode and marks the pins as need-to-be-disposed
            this._BitBangMode = true;
            this._PinDisposalRequired = true;
            // Makes references to the SPI pins
            this._BBM_CS = new IntegratedGPO(LatchPin, false);
            this._BBM_MISO = new IntegratedGPI(DataPin);
            this._BBM_SPCK = new IntegratedGPO(ClockPin, true);

            // The amount of ICs
            this._Init(Bytes);
        }
        /*
        /// <summary>
        /// Initialises a chain of one or multiple parallel to serial ICs over bitbanged SPI [WHEN POSSIBLE, USE MANAGED MODE!]
        /// </summary>
        /// <remarks>
        /// Use only when the managed SPI-pins can't be used. This method is way slower and locks the pins for any other purpose until disposed.
        /// </remarks>
        /// <param name="ClockPin">The clock pin connected to the IC(s)</param>
        /// <param name="DataPin">The data pin connected to the IC(s)</param>
        /// <param name="LatchPin">The slave select pin connected to the IC(s)</param>
        /// <param name="Bytes">The amount of 8-bit IC(s) connected</param>
        public SPIShifterIn(IGPOPort ClockPin, IGPIPort DataPin, IGPOPort LatchPin, uint Bytes = 1)
        {
            // Enables bitbang mode
            this._BitBangMode = true;
            // Makes references to the SPI pins
            this._BBM_CS = LatchPin; this._BBM_CS.Write(false);
            this._BBM_MISO = DataPin;
            this._BBM_SPCK = ClockPin; this._BBM_SPCK.Write(true);

            // The amount of ICs
            this._Init(Bytes);
        }
        */
        /// <summary>
        /// Initialises all bits and bytes
        /// </summary>
        /// <param name="Bytes">The amount of 8-bit IC(s) connected</param>
        private void _Init(uint Bytes)
        {
            // Creates the interrupt thread
            this._IRQStarter = new ThreadStart(this._IRQThread);

            // Prepares the buffer array
            this._Data = new byte[Bytes];

            // Declares all pins
            this.Pins = new Ic74hc165IRQPort[Bytes * 8];
            for (uint PinNo = 0; PinNo < this.Pins.Length; ++PinNo)
                this.Pins[PinNo] = new Ic74hc165IRQPort(this, PinNo);
        }

        /// <summary>
        /// Disposes this object, frees all locked items
        /// </summary>
        public void Dispose() {
            // Stops the interrupt thread
            this.DisableInterrupts();
            // Frees all pins
            if (this._PinDisposalRequired) {
                this._BBM_CS.Dispose();
                this._BBM_MISO.Dispose();
                this._BBM_SPCK.Dispose();
            }
        }
        #endregion

        #region "Interrupt handling"
        /// <summary>
        /// Enables interrupt scanning
        /// </summary>
        public void EnableInterrupts()
        {
            if (this._IRQEnabled) return;
            this._IRQEnabled = true;
            // Starts the background thread
            Thread IRQThread = new Thread(this._IRQStarter);
            IRQThread.Start();
        }

        /// <summary>
        /// Disables interrupt scanning
        /// </summary>
        public void DisableInterrupts()
        {
            this._IRQEnabled = false;
        }

        /// <summary>True when the interrupt loop is active</summary>
        private bool _IRQEnabled = false;

        /// <summary>
        /// Contains a reference to the interrupt thread
        /// </summary>
        private ThreadStart _IRQStarter;

        /// <summary>
        /// The interrupt thread itself
        /// </summary>
        private void _IRQThread()
        {
            while (this._IRQEnabled)
            {
                // Copies the previous state
                byte[] OldState = new byte[this._Data.Length];
                this._Data.CopyTo(OldState, 0);
                // Reads the new state
                this._ReadSPI();
                // Compares old to new
                for (uint ByteNo = 0; ByteNo < this._Data.Length; ++ByteNo)
                {
                    for (byte BitNo = 0; BitNo < 8; ++BitNo)
                    {
                        byte BitMask = (byte)(1 << BitNo);
                        if ((this._Data[ByteNo] & BitMask) != (OldState[ByteNo] & BitMask))
                            this.Pins[ByteNo * 8 + BitNo].Read();
                    }
                }
                // Wait for one millisecond so other threads can jump back in
                Thread.Sleep(1);
            }
        }
        #endregion

        #region "Reading"
        /// <summary>
        /// Reads all data from the SPI interface
        /// </summary>
        private void _ReadSPI()
        {
            // Managed SPI mode
            if (!this._BitBangMode)
            {
                this._SpiInterface.Read(this._Data);
                return;
            }
            // Bitbang mode
            // Enables the device
            this._BBM_CS.Write(true);
            // Loops throug all IC's
            for (uint IcIndex = 0; IcIndex < this._Data.Length; ++IcIndex)
            {
                // Loops through all bits
                byte ReadByte = 0;
                byte Multiplier = 0x80;
                for (byte BitNo = 0; BitNo < 8; ++BitNo)
                {
                    // Enables the clock and wait for a ms. Then read out the bit and disables the clock again
                    this._BBM_SPCK.Write(true);
                    Thread.Sleep(1);
                    if (this._BBM_MISO.Read())
                        ReadByte += Multiplier;
                    Multiplier = (byte)(Multiplier / 2);
                    this._BBM_SPCK.Write(false);
                }
                this._Data[IcIndex] = ReadByte;
            }
            // Disables the device
            this._BBM_CS.Write(false);
            this._BBM_SPCK.Write(true);
        }

        /// <summary>
        /// Reads a single bit
        /// </summary>
        /// <param name="Bit">The bit to read</param>
        /// <returns>The current state of the bit</returns>
        protected bool Read(uint Bit)
        {
            // Fetches the value from the SPI buffer when not in IRQ mode
            if (!this._IRQEnabled)
                this._ReadSPI();
            // Calculates the actual bit and actual byte
            byte BitNo = (byte)(Bit & 0x07);
            uint ByteNo = (Bit - BitNo) / 8;
            byte BitMask = (byte)(1 << BitNo);
            // Gets the value of the bit
            return (this._Data[ByteNo] & BitMask) == BitMask;
        }
        #endregion

        #region "IRQ Pins"
        /// <summary>IRQ Port wrapper for the SPIShifterIn class</summary>
        protected class Ic74hc165IRQPort : IIRQPort
        {
            /// <summary>Reference to the main chain</summary>
            private Ic74hc165 _Chain;
            /// <summary>The number of the bit</summary>
            private uint _BitNo;

            /// <summary>Contains the last value, used for the OnStateChange event</summary>
            private bool _LastValue = false;

            /// <summary>Frees the pin</summary>
            public void Dispose() { }

            /// <summary>When true, the read value is inverted (useful when working with pull-up resistors)</summary>
            public bool InvertReadings { get; set; }
            /// <summary>Use this to give this IRQ port a unique identifier (default: blank)</summary>
            public string ID { get; set; }
            /// <summary>Event triggered when a IRQ port state changes</summary>
            public event StateChange OnStateChange;

            /// <summary>
            /// Defines a GPO Port
            /// </summary>
            /// <param name="MainChain">The object of the main chain</param>
            /// <param name="BitNo">The number of the bit</param>
            public Ic74hc165IRQPort(Ic74hc165 MainChain, uint BitNo)
            {
                // Copies the parameters to local values
                this._Chain = MainChain;
                this._BitNo = BitNo;
                this._LastValue = this.Read();
            }

            /// <summary>Reads the pin value</summary>
            /// <returns>True when high, false when low</returns>
            public bool Read()
            {
                bool Value = this.InvertReadings ? !this._Chain.Read(this._BitNo) : this._Chain.Read(this._BitNo);
                if (Value != this._LastValue && this.OnStateChange != null)
                {
                    this._LastValue = Value;
                    this.OnStateChange(this, Value, DateTime.Now);
                }
                return Value;
            }
        }

        /// <summary>Reference to all pins</summary>
        public IIRQPort[] Pins;
        #endregion
    }
}
