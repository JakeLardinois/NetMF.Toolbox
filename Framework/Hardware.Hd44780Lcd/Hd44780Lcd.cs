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
    /// HD44780 Compatible dot matrix LCD
    /// </summary>
    /// <remarks>
    /// I found reading the datasheet a bit complex. It's a big document with a lot of info.
    /// For this reason, I added comments referring to pages of the datasheet.
    /// I used the datasheet from http://www.netmftoolbox.com/documents/Hardware.Hd44780Lcd.pdf
    /// </remarks>
    public class Hd44780Lcd : IDisposable
    {
        #region "Constructors and destructor"
        /// <summary>True when we use a Cpu.Pin constructor</summary>
        private bool _PinDisposalRequired;
        /// <summary>When true, we use pin mode</summary>
        private bool _PinMode;
        /// <summary>Reference to the data block</summary>
        private IParallelOut _Data;
        /// <summary>Reference to the 4th data pin</summary>
        private IGPOPort _Db4Pin;
        /// <summary>Reference to the 5th data pin</summary>
        private IGPOPort _Db5Pin;
        /// <summary>Reference to the 6th data pin</summary>
        private IGPOPort _Db6Pin;
        /// <summary>Reference to the 7th data pin</summary>
        private IGPOPort _Db7Pin;
        /// <summary>Reference to the clock enable pin</summary>
        private IGPOPort _CePin;
        /// <summary>Reference to the register select pin</summary>
        private IGPOPort _RsPin;
        /// <summary>Reference to the read/write pin</summary>
        private IGPOPort _RwPin;
        /// <summary>The amount of columns on the display</summary>
        public int Columns { get; protected set; }
        /// <summary>The amount of rows on the display</summary>
        public int Rows { get; protected set; }
        /// <summary>The amount of characters on the display</summary>
        public int Characters { get { return Rows * Columns; } }

        /* 
         * Datasheet p.29 t.8: "Function Set"
         * N F  Display Lines  Char.font
         * 0 0  1              5x8 dots
         * 0 1  1              5x10 dots
         * 1 *  2              5x8 dots   (Cannot display two lines for 5x10 dot character font) 
         */
        /// <summary>Display lines (false = 1, true = 2)</summary>
        private bool _FunctionSetN = false;
        /// <summary>Character font (false = 5x8, true = 5x10)</summary>
        private bool _FunctionSetF = false;

        /// <summary>
        /// Initializes a HD44780 compatible LCD with a parallel output port
        /// </summary>
        /// <param name="Data">Data port</param>
        /// <param name="ClockEnablePin">Clock enable pin</param>
        /// <param name="RegisterSelectPin">Register select pin</param>
        /// <param name="ReadWritePin">Read/write pin (optional; this driver is always in 'write' mode)</param>
        /// <param name="Columns">The amount of columns (default: 16)</param>
        /// <param name="Rows">The amount of rows (default: 2)</param>
        public Hd44780Lcd(IParallelOut Data, Cpu.Pin ClockEnablePin, Cpu.Pin RegisterSelectPin, Cpu.Pin ReadWritePin = Cpu.Pin.GPIO_NONE, int Columns = 16, int Rows = 2)
        {
            // Validates parameters
            if (Data.Size != 4) throw new ArgumentOutOfRangeException("Can only use 4-bit data blocks right now");

            // Copies all references locally
            this._Data = Data;
            this._CePin = new IntegratedGPO(ClockEnablePin);
            this._RsPin = new IntegratedGPO(RegisterSelectPin);
            this._RwPin = ReadWritePin == Cpu.Pin.GPIO_NONE ? null : new IntegratedGPO(ReadWritePin);
            this.Rows = Rows;
            this.Columns = Columns;

            // Since we are referring to a data block, we're not in pin mode
            this._PinMode = false;

            // Since we use real pins, disposal is required
            this._PinDisposalRequired = true;

            // Starts the LCD initialization
            this._Initialization();
        }

        /// <summary>
        /// Initializes a HD44780 compatible LCD with a parallel output port
        /// </summary>
        /// <param name="Data">Data port</param>
        /// <param name="ClockEnablePin">Clock enable pin</param>
        /// <param name="RegisterSelectPin">Register select pin</param>
        /// <param name="ReadWritePin">Read/write pin (optional; this driver is always in 'write' mode)</param>
        /// <param name="Columns">The amount of columns (default: 16)</param>
        /// <param name="Rows">The amount of rows (default: 2)</param>
        public Hd44780Lcd(IParallelOut Data, IGPOPort ClockEnablePin, IGPOPort RegisterSelectPin, IGPOPort ReadWritePin = null, int Columns = 16, int Rows = 2)
        {
            // Validates parameters
            if (Data.Size != 4) throw new ArgumentOutOfRangeException("Can only use 4-bit data blocks right now");

            // Copies all references locally
            this._Data = Data;
            this._CePin = ClockEnablePin;
            this._RsPin = RegisterSelectPin;
            this._RwPin = ReadWritePin;
            this.Rows = Rows;
            this.Columns = Columns;

            // Since we are referring to a data block, we're not in pin mode
            this._PinMode = false;

            // Since we use virtual pins, disposal isn't required
            this._PinDisposalRequired = false;

            // Starts the LCD initialization
            this._Initialization();
        }

        /// <summary>
        /// Initializes a HD44780 compatible LCD by bitbanging
        /// </summary>
        /// <param name="Data4">Data pin 4</param>
        /// <param name="Data5">Data pin 5</param>
        /// <param name="Data6">Data pin 6</param>
        /// <param name="Data7">Data pin 7</param>
        /// <param name="ClockEnablePin">Clock enable pin</param>
        /// <param name="RegisterSelectPin">Register select pin</param>
        /// <param name="ReadWritePin">Read/write pin (optional; this driver is always in 'write' mode)</param>
        /// <param name="Columns">The amount of columns (default: 16)</param>
        /// <param name="Rows">The amount of rows (default: 2)</param>
        public Hd44780Lcd(Cpu.Pin Data4, Cpu.Pin Data5, Cpu.Pin Data6, Cpu.Pin Data7, Cpu.Pin ClockEnablePin, Cpu.Pin RegisterSelectPin, Cpu.Pin ReadWritePin = Cpu.Pin.GPIO_NONE, int Columns = 16, int Rows = 2)
        {
            // Copies all references locally
            this._Db4Pin = new IntegratedGPO(Data4);
            this._Db5Pin = new IntegratedGPO(Data5);
            this._Db6Pin = new IntegratedGPO(Data6);
            this._Db7Pin = new IntegratedGPO(Data7);
            this._CePin = new IntegratedGPO(ClockEnablePin);
            this._RsPin = new IntegratedGPO(RegisterSelectPin);
            this._RwPin = ReadWritePin == Cpu.Pin.GPIO_NONE ? null : new IntegratedGPO(ReadWritePin);
            this.Rows = Rows;
            this.Columns = Columns;

            // Since we are referring to data pins, we're in pin mode
            this._PinMode = true;

            // Since we use real pins, disposal is required
            this._PinDisposalRequired = true;

            // Starts the LCD initialization
            this._Initialization();
        }

        /// <summary>
        /// Initializes a HD44780 compatible LCD by bitbanging
        /// </summary>
        /// <param name="Data4">Data pin 4</param>
        /// <param name="Data5">Data pin 5</param>
        /// <param name="Data6">Data pin 6</param>
        /// <param name="Data7">Data pin 7</param>
        /// <param name="ClockEnablePin">Clock enable pin</param>
        /// <param name="RegisterSelectPin">Register select pin</param>
        /// <param name="ReadWritePin">Read/write pin (optional; this driver is always in 'write' mode)</param>
        /// <param name="Columns">The amount of columns (default: 16)</param>
        /// <param name="Rows">The amount of rows (default: 2)</param>
        public Hd44780Lcd(IGPOPort Data4, IGPOPort Data5, IGPOPort Data6, IGPOPort Data7, IGPOPort ClockEnablePin, IGPOPort RegisterSelectPin, IGPOPort ReadWritePin = null, int Columns = 16, int Rows = 2)
        {
            // Copies all references locally
            this._Db4Pin = Data4;
            this._Db5Pin = Data5;
            this._Db6Pin = Data6;
            this._Db7Pin = Data7;
            this._CePin = ClockEnablePin;
            this._RsPin = RegisterSelectPin;
            this._RwPin = ReadWritePin;
            this.Rows = Rows;
            this.Columns = Columns;

            // Since we are referring to data pins, we're in pin mode
            this._PinMode = true;

            // Since we use virtual pins, disposal isn't required
            this._PinDisposalRequired = false;

            // Starts the LCD initialization
            this._Initialization();
        }

        /// <summary>
        /// Disposes this object, freeing all pins
        /// </summary>
        public void Dispose()
        {
            if (!this._PinDisposalRequired) return;
            if (this._PinMode)
            {
                this._Db4Pin.Dispose();
                this._Db5Pin.Dispose();
                this._Db6Pin.Dispose();
                this._Db7Pin.Dispose();
            }
            this._CePin.Dispose();
            this._RsPin.Dispose();
            if (this._RwPin != null) this._RwPin.Dispose();
        }
        #endregion

        #region "4-bit LCD Interface initialization"
        /// <summary>
        /// Initializes the display
        /// </summary>
        private void _Initialization()
        {
            // Default state for all pins
            this._CePin.Write(false);
            this._RsPin.Write(false);                          // Low = Command, High = Data
            if (this._RwPin != null) this._RwPin.Write(false); // Low = Write,   High = Read

            // Some configuration
            this._FunctionSetN = (this.Rows > 1); // True when more then one row
            this._FunctionSetF = false;           // Use the small font (when using multiple lines, the big font is unavailable anyways)
            this._DisplayControlB = false;        // By default no blinking
            this._DisplayControlC = false;        // By default no cursor
            this._DisplayControlD = true;         // Display must be on

            /*
             * Datasheet p.46 f.24 "Initializing by Instruction: 4-Bit Interface"
             * 
             * RS  R/W DB7 DB6 DB5 DB4
             * -   -   -   -   -   -   Wait for more than 40 ms after Vcc rises to 2.7 V
             * 0   0   0   0   1   1
             * -   -   -   -   -   -   Wait for more than 4.1 ms
             * 0   0   0   0   1   1
             * -   -   -   -   -   -   Wait for more than 100 µs (=0.1ms)
             * 0   0   0   0   1   1
             * -   -   -   -   -   -
             * 0   0   0   0   1   0   Function set (Set interface to be 4 bits long.)
             *                         Interface is 8 bits in length.
             * -   -   -   -   -   -
             * 0   0   0   0   1   0   Function Set (Interface is 4 bits long. Specify the
             * 0   0   N   F   *   *   number of display lines and character font.)
             *                         The number of display lines and character font
             *                         cannot be changed after this point.
             * -   -   -   -   -   -
             * 0   0   0   0   0   0   Display Off
             * 0   0   1   0   0   0
             * -   -   -   -   -   -
             * 0   0   0   0   0   0   Display Clear
             * 0   0   0   0   0   1
             * -   -   -   -   -   -
             * 0   0   0   0   0   0   Entry mode set
             * 0   0   0   1   I/D S
             * 
             * 
             * Datasheet p.26: "Instruction Description"
             * 
             * Entry Mode Set
             * I/D: Increments (I/D = 1) or decrements (I/D = 0) the DDRAM address by 1 when a character code is written into or read from DDRAM.
             * S: Shifts the entire display either to the right (I/D = 0) or to the left (I/D = 1) when S is 1. The display does not shift if S is 0.
             * 
             * Datasheet p.29 t.8: "Function Set"
             * N F  Display Lines  Char.font
             * 0 0  1              5x8 dots
             * 0 1  1              5x10 dots
             * 1 *  2              5x8 dots   (Cannot display two lines for 5x10 dot character font) 
             */

            // Initializing 4-Bit Interface, we are already in command mode since we've put the register select pin down earlier
            Thread.Sleep(40);
            this._Write4Bits(0x3); // b0011
            Thread.Sleep(5);
            this._Write4Bits(0x3); // b0011
            Thread.Sleep(1);
            this._Write4Bits(0x3); // b0011
            this._Write4Bits(0x2); // b0010

            this.Write((byte)(
                    0x20                               // b00100000
                    + (this._FunctionSetN ? 0x8 : 0x0) // b00001000
                    + (this._FunctionSetF ? 0x4 : 0x0) // b00000100
                    ), true);                          // b0010,N,F,*,*
            this.Write(0x08, true); // b00001000
            this.Write(0x01, true); // b00000001
            this.Write(0x06, true); // b00000110 (b000001,I/D,S)
            Thread.Sleep(1);

            // After initialization, we can start executing other instructions
            this._UpdateDisplayControl();
        }
        #endregion

        #region "Data transfers"
        /// <summary>Sends data to the display</summary>
        /// <param name="Data">The data to send</param>
        /// <param name="CommandMode">When true, the data will be interpreted as a command</param>
        public void Write(byte Data, bool CommandMode = false)
        {
            this.Write(new byte[] { Data }, CommandMode);
        }

        /// <summary>Sends data to the display</summary>
        /// <param name="Data">The data to send</param>
        /// <param name="CommandMode">When true, the data will be interpreted as a command</param>
        public void Write(string Data, bool CommandMode = false)
        {
            byte[] Buffer = Tools.Chars2Bytes(Data.ToCharArray());
            this.Write(Buffer, CommandMode);
        }

        /// <summary>Sends data to the display</summary>
        /// <param name="Data">The data to send</param>
        /// <param name="CommandMode">When true, the data will be interpreted as a command</param>
        public void Write(byte[] Data, bool CommandMode = false)
        {
            // Selects mode; False = Command, True = Data
            this._RsPin.Write(!CommandMode);

            for (int ByteNo = 0; ByteNo < Data.Length; ++ByteNo)
            {
                // Splits up the data in two blocks of 4 bits
                byte FirstFour = (byte)(Data[ByteNo] >> 4);
                byte SecondFour = (byte)(Data[ByteNo] & 0x0f);

                // Sends both blocks of 4 bits
                this._Write4Bits(FirstFour);
                this._Write4Bits(SecondFour);
            }
        }

        /// <summary>
        /// Writes four bits to the LCD interface
        /// </summary>
        /// <param name="Byte">The byte to send (only the last 4 bits will be used)</param>
        private void _Write4Bits(byte Byte)
        {
            // Writes all 4 bits
            if (this._PinMode)
            {
                this._Db7Pin.Write((Byte & 0x8) == 0x8);
                this._Db6Pin.Write((Byte & 0x4) == 0x4);
                this._Db5Pin.Write((Byte & 0x2) == 0x2);
                this._Db4Pin.Write((Byte & 0x1) == 0x1);
            }
            else
            {
                this._Data.Write(Byte);
            }

            // Enables the pin for a moment
            this._CePin.Write(true);
            this._CePin.Write(false);
        }
        #endregion

        #region "Simple LCD commands"
        /*
         * Datasheet p.24 t.6: "Instructions"
         * 0 0 0 0 1 D C B   Sets entire display (D) on/off, cursor on/off (C), and
         *                   blinking of cursor position character (B).
         */
        /// <summary>Entire display on/off</summary>
        private bool _DisplayControlD;
        /// <summary>Cursor on/off</summary>
        private bool _DisplayControlC;
        /// <summary>Blinking of cursor position character on/off</summary>
        private bool _DisplayControlB;

        /// <summary>When true, a cursor will be shown on the display</summary>
        public bool ShowCursor
        {
            get { return this._DisplayControlC; }
            set { this._DisplayControlC = value; this._UpdateDisplayControl(); }
        }

        /// <summary>When true, the cursor position character will blink</summary>
        public bool BlinkPosition
        {
            get { return this._DisplayControlB; }
            set { this._DisplayControlB = value; this._UpdateDisplayControl(); }
        }
        /// <summary>
        /// Updates the display control bits
        /// </summary>
        private void _UpdateDisplayControl()
        {
            /*
             * Datasheet p.24 t.6: "Instructions"
             * 0 0 0 0 1 D C B   Sets entire display (D) on/off, cursor on/off (C), and
             *                   blinking of cursor position character (B).
             */
            byte Data = 0x08;                        // b00001000 (b00001,D,C,B)
            if (this._DisplayControlD) Data += 0x04; // b00000100
            if (this._DisplayControlC) Data += 0x02; // b00000010
            if (this._DisplayControlB) Data += 0x01; // b00000001
            this.Write(Data, true);
        }

        /// <summary>
        /// Clears the entire display
        /// </summary>
        public void ClearDisplay()
        {
            /*
             * Datasheet p.24 t.6: "Instructions"
             * 0 0 0 0 0 0 0 1   Clears entire display and sets DDRAM address 0 in address counter.
             * 
             * Datasheet p.26 "Clear Display"
             * Clear display writes space code 20H (character pattern for character code 20H must be a blank pattern) into
             * all DDRAM addresses. It then sets DDRAM address 0 into the address counter, and returns the display to
             * its original status if it was shifted. In other words, the display disappears and the cursor or blinking goes to
             * the left edge of the display (in the first line if 2 lines are displayed). It also sets I/D to 1 (increment mode)
             * in entry mode. S of entry mode does not change.
             */
            this.Write(0x01, true); // b00000001
        }

        /// <summary>
        /// Changes the cursor location
        /// </summary>
        /// <param name="Row">Row</param>
        /// <param name="Column">Column</param>
        public void ChangePosition(byte Row, byte Column)
        {
            /*
             * Datasheet p.10: "Display Data RAM (DDRAM)"
             * Display data RAM (DDRAM) stores display data represented in 8-bit character codes. Its extended
             * capacity is 80 x 8 bits, or 80 characters. The area in display data RAM (DDRAM) that is not used for
             * display can be used as general data RAM. See Figure 1 for the relationships between DDRAM addresses
             * and positions on the liquid crystal display.
             *
             * Datasheet p.24 t.6: "Instructions"
             * 1 A A A A A A A   Sets DDRAM address. DDRAM data is sent and received after this setting.
             * 
             * Datasheet p.29 "Set DDRAM Address"
             * Set DDRAM address sets the DDRAM address binary AAAAAAA into the address counter.
             * Data is then written to or read from the MPU for DDRAM.
             * However, when N is 0 (1-line display), AAAAAAA can be 00H to 4FH. When N is 1 (2-line display),
             * AAAAAAA can be 00H to 27H for the first line, and 40H to 67H for the second line.
             * 
             * Conclusion:
             * It seems the HD44780 instructionset isn't made for more then 2 lines.
             * The first line starts at 0x00, the second at 0x40, like from the datasheet.
             * With 4-line displays, the 3rd and 4th line start after 20 (0x14) characters on the 1st and 2nd line.
             */
            byte[] RowStart = { 0x00, 0x40, 0x14, 0x54 };

            byte Data = 0x80;                       // b10000000 (=b1,A,A,A,A,A,A,A)
            Data += (byte)(RowStart[Row] + Column); // Actual position
            this.Write(Data, true);
        }
        #endregion
    }
}
