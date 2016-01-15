using System;
using System.IO.Ports; // Microsoft.SPOT.Hardware.SerialPort.dll
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
    /// Thermal Printer driver
    /// </summary>
    public class ThermalPrinter : IDisposable
    {
        /// <summary>
        /// Reference to the serial port
        /// </summary>
        private SerialPort _Uart;

        /// <summary>
        /// Triggered when the status changes
        /// </summary>
        public event NativeEventHandler OnStatusChange;

        /// <summary>When true, text will be printed smaller</summary>
        public bool SmallText { get { return this._SmallText; } set { this._SmallText = value; this._WriteModes(); } }
        private bool _SmallText = false;
        /// <summary>When true, text will be printed white on black</summary>
        public bool Inverted { get { return this._Inverted; } set { this._Inverted = value; this._WriteModes(); } }
        private bool _Inverted = false;
        /// <summary>When true, text will be printed upside down</summary>
        public bool UpsideDown { get { return this._UpsideDown; } set { this._UpsideDown = value; this._WriteModes(); } }
        private bool _UpsideDown = false;
        /// <summary>When true, text will be printed with it's doubled height</summary>
        public bool DoubleHeight { get { return this._DoubleHeight; } set { this._DoubleHeight = value; this._WriteModes(); } }
        private bool _DoubleHeight = false;
        /// <summary>When true, text will be printed with it's doubled width</summary>
        public bool DoubleWidth { get { return this._DoubleWidth; } set { this._DoubleWidth = value; this._WriteModes(); } }
        private bool _DoubleWidth = false;
        /// <summary>When true, text will be striked through</summary>
        public bool StrikeThrough { get { return this._StrikeThrough; } set { this._StrikeThrough = value; this._WriteModes(); } }
        private bool _StrikeThrough = false;
        /// <summary>When true, text will be underlined</summary>
        public bool Underlined { get { return this._Underline; } set { this._Underline = value; this._WriteModes(); } }
        private bool _Underline = false;

        /// <summary>Max printing dots，Unit(8dots)，Default:7(64 dots)</summary>
        public byte MaxPrintingDots { get { return this._MaxPrintingDots; } set { this._MaxPrintingDots = value; this._WriteControlParameters(); } }
        private byte _MaxPrintingDots = 7;
        /// <summary>Heating time，Unit(10µs),Default:80(800µs)</summary>
        public byte HeatingTime { get { return this._HeatingTime; } set { this._HeatingTime = value; this._WriteControlParameters(); } }
        private byte _HeatingTime = 80;
        /// <summary>Heating interval,Unit(10µs)，Default:2(20µs)</summary>
        public byte HeatingInterval { get { return this._HeatingInterval; } set { this._HeatingInterval = value; this._WriteControlParameters(); } }
        private byte _HeatingInterval = 20;

        /// <summary>
        /// Writes the control parameters
        /// </summary>
        private void _WriteControlParameters()
        {
            this.Print(new byte[] { 0x1b, 0x37, this._MaxPrintingDots, this._HeatingTime, this._HeatingInterval });
        }

        /// <summary>
        /// Updates the font status
        /// </summary>
        private void _WriteModes()
        {
            byte Modes = 0;
            if (this._SmallText) Modes += 0x01;
            if (this._Inverted) Modes += 0x02;
            if (this._UpsideDown) Modes += 0x04;
            if (this._DoubleHeight) Modes += 0x10;
            if (this._DoubleWidth) Modes += 0x20;
            if (this._StrikeThrough) Modes += 0x40;
            if (this._Underline) Modes += 0x80;
            this.Print(new byte[] { 0x1b, 0x21, Modes });
        }

        /// <summary>The print mode for bitmaps</summary>
        public enum PrintMode
        {
            /// <summary>8dots single density，102dpi</summary>
            SingleDensity8Dots = 0,
            /// <summary>8dots double density，203dpi</summary>
            DoubleDensity8Dots = 1,
            /// <summary>24 dots single density,102dpi</summary>
            SingleDensity24Dots = 31,
            /// <summary>24 dots double density,203dpi</summary>
            DoubleDensity24Dots = 32
        }

        /// <summary>Text alignment values</summary>
        public enum Alignment
        {
            /// <summary>Align to the left</summary>
            AlignLeft = 0,
            /// <summary>Align to the middle</summary>
            AlignCenter = 1,
            /// <summary>Align to the right</summary>
            AlignRight = 2
        }

        /// <summary>Bar code systems</summary>
        public enum BarCodeSystem
        {
            /// <summary>Universal Product Code type A</summary>
            UPC_A = 0,
            /// <summary>Universal Product Code type E</summary>
            UPC_E = 1,
            /// <summary>European Article Number (12 bytes)</summary>
            EAN13 = 2,
            /// <summary>European Article Number (8 bytes)</summary>
            EAN8 = 3,
            /// <summary>Code 39</summary>
            CODE39 = 4,
            /// <summary>Interleaved 2 of 5</summary>
            I25 = 5,
            /// <summary>Codabar</summary>
            CODEBAR = 6,
            /// <summary>Code 93</summary>
            CODE93 = 7,
            /// <summary>Code 128</summary>
            CODE128 = 8,
            /// <summary>Code 11</summary>
            CODE11 = 9,
            /// <summary>Modified Plessey</summary>
            MSI = 10
        }

        /// <summary>
        /// Initializes a Thermal Printer
        /// </summary>
        /// <param name="SerialPort">The serial port the printer is connected to</param>
        /// <param name="BaudRate">The speed of the printer</param>
        public ThermalPrinter(string SerialPort = "COM1", int BaudRate = 19200)
        {
            this._Uart = new SerialPort(SerialPort, BaudRate);
            this._Uart.DataReceived += new SerialDataReceivedEventHandler(_Uart_DataReceived);
            this._Uart.Open();
            // Initializes the printer
            this.Print(new byte[] { 0x1b, 0x40 });
            // Enables Automatic Status Back (ASB)
            this.Print(new byte[] { 0x1d, 0x61, 0x04 });
        }

        /// <summary>
        /// The serial port sent data back
        /// </summary>
        /// <param name="sender">SerialPort object</param>
        /// <param name="e">Event details</param>
        private void _Uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReadBuffer = new byte[this._Uart.BytesToRead];
            this._Uart.Read(ReadBuffer, 0, ReadBuffer.Length);

            // The last byte will probably contain the last status
            if (this.OnStatusChange != null)
                this.OnStatusChange(0, ReadBuffer[ReadBuffer.Length - 1], DateTime.Now);
        }

        /// <summary>Sends data to the printer</summary>
        /// <param name="Data">Data to process</param>
        public void Print(string Data)
        {
            byte[] WriteBuffer = Tools.Chars2Bytes(Data.ToCharArray());
            this.Print(WriteBuffer);
        }

        /// <summary>Sends data to the printer</summary>
        /// <param name="Data">Data to process</param>
        public void Print(byte[] Data)
        {
            this._Uart.Write(Data, 0, Data.Length);
        }

        /// <summary>
        /// Prints a line of text
        /// </summary>
        /// <param name="Text">Line of text to print</param>
        public void PrintLine(string Text)
        {
            this.Print(Text + "\n");
        }

        /// <summary>Executes a line feed</summary>
        public void LineFeed()
        {
            this.LineFeed(1);
        }

        /// <summary>Executes a specific amount of line feeds</summary>
        /// <param name="Amount">The amount of linefeeds</param>
        public void LineFeed(int Amount)
        {
            byte[] buffer = new byte[Amount];
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = 0x0a;
            this.Print(buffer);
        }

        /// <summary>Changes the text alignment</summary>
        /// <param name="Value">Alignment</param>
        public void SetAlignment(Alignment Value)
        {
            this.Print(new byte[] { 0x1b, 0x61, (byte)Value });
        }

        /// <summary>Set the amount of spaces to be added in front of each line of text</summary>
        /// <param name="Value">Amount of spaces</param>
        public void SetLeftSpacing(byte Value)
        {
            if (Value > 47) throw new ArgumentOutOfRangeException("A value from 0 to 47 is required");
            this.Print(new byte[] { 0x1b, 0x42, Value });
        }

        /// <summary>Changes the line spacing</summary>
        /// <param name="Value">New line spacing (default value is 30)</param>
        public void SetLineSpacing(byte Value = 30)
        {
            this.Print(new byte[] { 0x1b, 0x33, Value });
        }

        /// <summary>
        /// Prints a bitmap
        /// </summary>
        /// <param name="Width">Bitmap width (should be a power of 8)</param>
        /// <param name="Height">Bitmap height (should be a power of 8)</param>
        /// <param name="Bitmap">Bitmap data</param>
        /// <param name="Mode">Print mode</param>
        public void PrintBitmap(int Width, int Height, byte[] Bitmap, PrintMode Mode = PrintMode.SingleDensity24Dots)
        {
            if ((Width / 8) * 8 != Width) { throw new ArgumentException("Width should be a power of 8"); }
            if ((Height / 8) * 8 != Height) { throw new ArgumentException("Height should be a power of 8"); }
            if ((Width * Height / 8) != Bitmap.Length) { throw new ArgumentException("Bitmap should have the same amount of bits as Width x Height"); }
            if (Width < 1 || Width > 384) throw new ArgumentOutOfRangeException("Width should be between 0 and 385");
            if (Height < 1 || Height > 2040) throw new ArgumentOutOfRangeException("Height should be between 0 and 2041");
            if ((Width * Height) > 9600) throw new ArgumentOutOfRangeException("The bitmap may only contain 9600 pixels");

            // Defines the bitmap size
            this.Print(new byte[] { 0x1d, 0x2a, (byte)(Width / 8), (byte)(Height / 8) });
            // Sends the bitmap
            this.Print(Bitmap);
            // Actually prints out
            this.Print(new byte[] { 0x1d, 0x2f, (byte)Mode });
        }

        /// <summary>
        /// Prints a bar code
        /// </summary>
        /// <param name="Value">The bar code</param>
        /// <param name="Format">The format to print the code in</param>
        /// <param name="BarWidth">The width of each single bar (so not of the full bar code)</param>
        /// <param name="BarHeight">The height of each single bar (also the height of the full bar code)</param>
        /// <param name="PrintValueAbove">When true, the value will also be printed in text, above the bar code</param>
        /// <param name="PrintValueBelow">When true, the value will also be printed in text, below the bar code</param>
        /// <remarks>
        /// The printer also validates the barcode. If it's not valid it won't print.
        /// If you send the checkbyte yourself (in most barcodes the last digit) and it does not compute, discard it.
        /// Also, if the barcode won't fit the paper, it won't print either. Try reducing the BarWidth in that case.
        /// </remarks>
        public void PrintBarcode(string Value, BarCodeSystem Format, byte BarWidth = 3, byte BarHeight = 50, bool PrintValueAbove = false, bool PrintValueBelow = true)
        {
            // Dimension checks
            if (BarHeight < 1 || BarHeight > 255) throw new ArgumentOutOfRangeException("Height can only be between 1 and 255");
            if (BarWidth < 2 || BarWidth > 3) throw new ArgumentOutOfRangeException("Width can only be between 2 and 3");

            // When true, the barcode is sent with a length parameter too
            bool DynamicLength = false;
            
            // Barcode system checks
            switch (Format)
            {
                case BarCodeSystem.UPC_A:
                    //if (Value.Length != 12) throw new ArgumentOutOfRangeException("UPC_A only support a length of 12 characters");
                    if (Value.Length < 11 || Value.Length > 12) throw new ArgumentOutOfRangeException("UPC_A only support a length of 11 to 12 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("UPC_A only support numbers");
                    break;
                case BarCodeSystem.UPC_E:
                    if (Value.Length < 11 || Value.Length > 12) throw new ArgumentOutOfRangeException("UPC_E only support a length of 11 to 12 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("UPC_E only support numbers");
                    if (Value.Substring(0, 1) != "0") throw new ArgumentException("UPC_E requires the first digit to be 0");
                    // Some other rules do apply, haven't checked all rules
                    break;
                case BarCodeSystem.EAN13:
                    if (Value.Length < 12 || Value.Length > 13) throw new ArgumentOutOfRangeException("EAN13 only support a length of 12 to 13 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("EAN13 only support numbers");
                    break;
                case BarCodeSystem.EAN8:
                    if (Value.Length != 8) throw new ArgumentOutOfRangeException("EAN8 only support a length of 8 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("EAN8 only support numbers");
                    break;
                case BarCodeSystem.CODE39:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("CODE39 requires at least 2 characters and at most 255 characters");
                    if (!this._HasOnlyCharacters(Value, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -$%./+")) throw new ArgumentException("CODE39 only supports the following characters: ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -$%./+");
                    break;
                case BarCodeSystem.I25:
                    if (Value.Length == 0 || (Value.Length % 2) != 0) throw new ArgumentException("I25 only supports an even number of characters, above 0");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("I25 only support numbers");
                    break;
                case BarCodeSystem.CODEBAR:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("CODEBAR requires at least 2 characters and at most 255 characters");
                    if (!this._HasOnlyCharacters(Value, "$+-./0123456789:ABCD")) throw new ArgumentException("CODEBAR only supports the following characters: $+-./0123456789:ABCD");
                    // Some other rules do apply, haven't checked all rules
                    break;
                case BarCodeSystem.CODE93:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("CODE93 requires at least 2 characters and at most 255 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%")) throw new ArgumentException("CODE93 only supports the following characters: 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%");
                    break;
                case BarCodeSystem.CODE128:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("CODE128 requires at least 2 characters and at most 255 characters");
                    break;
                case BarCodeSystem.CODE11:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("CODE11 requires at least 2 characters and at most 255 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789-")) throw new ArgumentException("CODE11 only support numbers and a dash (-)");
                    break;
                case BarCodeSystem.MSI:
                    if (Value.Length < 2 || Value.Length > 255) throw new ArgumentOutOfRangeException("MSI requires at least 2 characters and at most 255 characters");
                    if (!this._HasOnlyCharacters(Value, "0123456789")) throw new ArgumentException("MSI only support numbers");
                    break;
            }

            // Turns the PrintValueAbove & PrintValueBelow to a single byte
            byte PrintValue = (byte)((PrintValueAbove ? 0x01 : 0) + (PrintValueBelow ? 0x02 : 0));
            // Select printing position of human readable characters
            this.Print(new byte[] { 0x1d, 0x48, PrintValue });

            // Set bar code width
            this.Print(new byte[] { 0x1d, 0x77, BarWidth });
            // Set bar code height
            this.Print(new byte[] { 0x1d, 0x68, BarHeight });

            // Actually prints the barcode
            this.Print(new byte[] { 0x1d, 0x6b, (byte)Format });
            if (DynamicLength)
                this.Print(new byte[] { (byte)Value.Length });
            this.Print(Value);
            this.Print(new byte[] { 0x00 });
        }

        /// <summary>Disposes this object</summary>
        public void Dispose()
        {
            this._Uart.Close();
        }

        /// <summary>
        /// Checks if a value contains only legal characters
        /// </summary>
        /// <param name="Value">The value</param>
        /// <param name="LegalChars">All legal characters</param>
        /// <returns>True when it's a valid value</returns>
        private bool _HasOnlyCharacters(string Value, string LegalChars)
        {
            for (int ReadPos = 0; ReadPos  < Value.Length; ++ReadPos)
                if (LegalChars.IndexOf(Value[ReadPos]) < 0) return false;
            return true;
        }
    }
}
