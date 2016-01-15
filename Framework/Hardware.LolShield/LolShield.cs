using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://netmftoolbox.codeplex.com/)
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
    /// Lots of Leds shield
    /// </summary>
    public class LolShield : IDisposable
    {
        #region "Generic stuff"
        /// <summary>Width of the LoL Shield</summary>
        private int _Width = 14;
        /// <summary>Height of the LoL Shield</summary>
        private int _Height = 9;

        /// <summary>Width of the LoL Shield</summary>
        public int Width { get { return this._Width; } }
        /// <summary>Height of the LoL Shield</summary>
        public int Height { get { return this._Height; } }

        /// <summary>Led map; defines the low and high pins for each LED</summary>
        private byte[][] _LedMap = new byte[126][] {
            new byte[2] { 11,  3 }, new byte[2] { 11,  4 }, new byte[2] { 11,  5 }, new byte[2] { 11,  6 }, new byte[2] { 11,  7 }, new byte[2] { 11,  8 },
            new byte[2] { 11,  9 }, new byte[2] { 11, 10 }, new byte[2] { 11,  2 }, new byte[2] {  2, 11 }, new byte[2] { 11,  1 }, new byte[2] {  1, 11 },
            new byte[2] { 11,  0 }, new byte[2] {  0, 11 }, new byte[2] { 10,  3 }, new byte[2] { 10,  4 }, new byte[2] { 10,  5 }, new byte[2] { 10,  6 }, 
            new byte[2] { 10,  7 }, new byte[2] { 10,  8 }, new byte[2] { 10,  9 }, new byte[2] { 10, 11 }, new byte[2] { 10,  2 }, new byte[2] {  2, 10 },
            new byte[2] { 10,  1 }, new byte[2] {  1, 10 }, new byte[2] { 10,  0 }, new byte[2] {  0, 10 }, new byte[2] {  9,  3 }, new byte[2] {  9,  4 },
            new byte[2] {  9,  5 }, new byte[2] {  9,  6 }, new byte[2] {  9,  7 }, new byte[2] {  9,  8 }, new byte[2] {  9, 10 }, new byte[2] {  9, 11 }, 
            new byte[2] {  9,  2 }, new byte[2] {  2,  9 }, new byte[2] {  9,  1 }, new byte[2] {  1,  9 }, new byte[2] {  9,  0 }, new byte[2] {  0,  9 },
            new byte[2] {  8,  3 }, new byte[2] {  8,  4 }, new byte[2] {  8,  5 }, new byte[2] {  8,  6 }, new byte[2] {  8,  7 }, new byte[2] {  8,  9 },
            new byte[2] {  8, 10 }, new byte[2] {  8, 11 }, new byte[2] {  8,  2 }, new byte[2] {  2,  8 }, new byte[2] {  8,  1 }, new byte[2] {  1,  8 }, 
            new byte[2] {  8,  0 }, new byte[2] {  0,  8 }, new byte[2] {  7,  3 }, new byte[2] {  7,  4 }, new byte[2] {  7,  5 }, new byte[2] {  7,  6 },
            new byte[2] {  7,  8 }, new byte[2] {  7,  9 }, new byte[2] {  7, 10 }, new byte[2] {  7, 11 }, new byte[2] {  7,  2 }, new byte[2] {  2,  7 },
            new byte[2] {  7,  1 }, new byte[2] {  1,  7 }, new byte[2] {  7,  0 }, new byte[2] {  0,  7 }, new byte[2] {  6,  3 }, new byte[2] {  6,  4 }, 
            new byte[2] {  6,  5 }, new byte[2] {  6,  7 }, new byte[2] {  6,  8 }, new byte[2] {  6,  9 }, new byte[2] {  6, 10 }, new byte[2] {  6, 11 },
            new byte[2] {  6,  2 }, new byte[2] {  2,  6 }, new byte[2] {  6,  1 }, new byte[2] {  1,  6 }, new byte[2] {  6,  0 }, new byte[2] {  0,  6 },
            new byte[2] {  5,  3 }, new byte[2] {  5,  4 }, new byte[2] {  5,  6 }, new byte[2] {  5,  7 }, new byte[2] {  5,  8 }, new byte[2] {  5,  9 }, 
            new byte[2] {  5, 10 }, new byte[2] {  5, 11 }, new byte[2] {  5,  2 }, new byte[2] {  2,  5 }, new byte[2] {  5,  1 }, new byte[2] {  1,  5 },
            new byte[2] {  5,  0 }, new byte[2] {  0,  5 }, new byte[2] {  4,  3 }, new byte[2] {  4,  5 }, new byte[2] {  4,  6 }, new byte[2] {  4,  7 },
            new byte[2] {  4,  8 }, new byte[2] {  4,  9 }, new byte[2] {  4, 10 }, new byte[2] {  4, 11 }, new byte[2] {  4,  2 }, new byte[2] {  2,  4 }, 
            new byte[2] {  4,  1 }, new byte[2] {  1,  4 }, new byte[2] {  4,  0 }, new byte[2] {  0,  4 }, new byte[2] {  3,  4 }, new byte[2] {  3,  5 },
            new byte[2] {  3,  6 }, new byte[2] {  3,  7 }, new byte[2] {  3,  8 }, new byte[2] {  3,  9 }, new byte[2] {  3, 10 }, new byte[2] {  3, 11 },
            new byte[2] {  3,  2 }, new byte[2] {  2,  3 }, new byte[2] {  3,  1 }, new byte[2] {  1,  3 }, new byte[2] {  3,  0 }, new byte[2] {  0,  3 }
        };

        /// <summary>This array will contain the grid, from a low to high point of view</summary>
        private bool[][] _LowToHigh;

        /// <summary>Reference to all pins</summary>
        private TristatePort[] _Pin;

        /// <summary>Main loop thread</summary>
        private Thread _LoopThread;

        /// <summary>Defines a new LoL Shield</summary>
        /// <param name="CpuPins">An array of all relevant CPU Pins</param>
        public LolShield(Cpu.Pin[] CpuPins)
        {
            // We've got a Led Map. In the future we might make this a second option.
            // The Led Map must have equally or less Leds as the maximum amount that can be charlieplexed with the amount of CpuPins.
            int MaxLeds = CpuPins.Length * CpuPins.Length - CpuPins.Length; // n²-n
            if (MaxLeds < this._LedMap.Length) throw new InvalidOperationException("Can't charlieplex " + this._LedMap.Length.ToString() + " LEDs with only " + CpuPins.Length.ToString() + " CPU pins");

            // Prepaires the LowToHigh array
            this._LowToHigh = new bool[CpuPins.Length][];
            for (int Counter = 0; Counter < this._LowToHigh.Length; ++Counter)
                this._LowToHigh[Counter] = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };

            // Makes tristates of all pins
            this._Pin = new TristatePort[CpuPins.Length];
            for (int Counter = 0; Counter < CpuPins.Length; ++Counter)
                this._Pin[Counter] = new TristatePort(CpuPins[Counter], false, false, Port.ResistorMode.Disabled);

            // Creates a new background thread
            this._LoopThread = new Thread(new ThreadStart(this._Loop));
            this._LoopThread.Start();
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void _Loop()
        {
            while (true)
            {
                for (int LowPins = 0; LowPins < this._LowToHigh.Length; ++LowPins)
                {
                    // Sets the High pin High and enables it
                    this._SetState(LowPins, true, true);
                    // Sets the required Low pins low and enables it
                    for (int HighPins = 0; HighPins < this._LowToHigh[LowPins].Length; ++HighPins)
                    {
                        if (this._LowToHigh[LowPins][HighPins])
                            this._SetState(HighPins, true, false);
                    }
                    // Sets all pins off again
                    for (int HighPins = 0; HighPins < this._LowToHigh[LowPins].Length; ++HighPins)
                        this._SetState(HighPins, false);
                    this._SetState(LowPins, false);
                }
            }
        }

        /// <summary>
        /// Disposes all pins and stops the display cycle
        /// </summary>
        public void Dispose()
        {
            this._LoopThread.Abort();
            for (int Counter = 0; Counter < this._Pin.Length; ++Counter)
                this._Pin[Counter].Dispose();
        }
        #endregion

        #region "Get/set pixels"
        /// <summary>
        /// Sets a pixel to a specific value
        /// </summary>
        /// <param name="Row">The row</param>
        /// <param name="Col">The column</param>
        /// <param name="Value">The new value</param>
        public void Set(int Row, int Col, bool Value)
        {
            this.Set(Row * this._Width + Col, Value);
        }

        /// <summary>
        /// Sets a pixel to a specific value
        /// </summary>
        /// <param name="PixNo">The index of the pixel</param>
        /// <param name="Value">The new value</param>
        public void Set(int PixNo, bool Value)
        {
            byte[] Pix = this._LedMap[PixNo];
            this._LowToHigh[Pix[0]][Pix[1]] = Value;
        }

        /// <summary>
        /// Gets the current value of a specific pixel
        /// </summary>
        /// <param name="Row">The row</param>
        /// <param name="Col">The column</param>
        /// <returns>The current value</returns>
        public bool Get(int Row, int Col)
        {
            return this.Get(Row * this._Width + Col);
        }

        /// <summary>
        /// Gets the current value of a specific pixel
        /// </summary>
        /// <param name="PixNo">The index of the pixel</param>
        /// <returns>The current value</returns>
        public bool Get(int PixNo)
        {
            byte[] Pix = this._LedMap[PixNo];
            return this._LowToHigh[Pix[0]][Pix[1]];
        }

        /// <summary>Changes the settings of a specific pin</summary>
        /// <param name="PinId">The Id of the pin</param>
        /// <param name="Active">Should be active?</param>
        /// <param name="State">Current state</param>
        private void _SetState(int PinId, bool Active, bool State = false)
        {
            if (Active != this._Pin[PinId].Active)
                this._Pin[PinId].Active = Active;

            if (Active)
                this._Pin[PinId].Write(State);
        }
        #endregion

        #region "Display modifiers"
        /// <summary>
        /// Clears the display
        /// </summary>
        /// <param name="Value">When true, it will fill the display instead</param>
        public void Clear(bool Value = false)
        {
            for (int PixNo = 0; PixNo < this._LedMap.Length; ++PixNo)
                this.Set(PixNo, Value);
        }

        /// <summary>
        /// Inverts the display
        /// </summary>
        public void Invert()
        {
            for (int PixNo = 0; PixNo < this._LedMap.Length; ++PixNo)
                this.Set(PixNo, !this.Get(PixNo));
        }

        /// <summary>
        /// Draws a horizontal line
        /// </summary>
        /// <param name="Row">The row to draw the line in</param>
        /// <param name="Value">The value for the line</param>
        public void HorizontalLine(int Row, bool Value = true)
        {
            for (int Col = 0; Col < this._Width; ++Col)
                this.Set(Row, Col, Value);
        }

        /// <summary>
        /// Draws a vertical line
        /// </summary>
        /// <param name="Col">The column to draw the line in</param>
        /// <param name="Value">The value for the line</param>
        public void VerticalLine(int Col, bool Value = true)
        {
            for (int Row = 0; Row < this._Height; ++Row)
                this.Set(Row, Col, Value);
        }

        /// <summary>Loads a monochrome bitmap onto the LoL Shield</summary>
        /// <param name="Bitmap">An array with bits</param>
        public void LoadBitmap(byte[] Bitmap)
        {
            int PixNo = 0;
            int ByteNo = 0;
            while (true)
            {
                for (byte BitMask = 0x80; BitMask > 0; BitMask >>= 1)
                {
                    this.Set(PixNo, (Bitmap[ByteNo] & BitMask) == BitMask);
                    ++PixNo;
                    if (PixNo >= this._LedMap.Length) break;
                }
                ++ByteNo;
                if (ByteNo >= Bitmap.Length) break;
                if (PixNo >= this._LedMap.Length) break;
            }
        }

        /// <summary>
        /// Loads a monochrome bitmap onto the LoL Shield
        /// </summary>
        /// <param name="Bitmap">An array with bits</param>
        /// <param name="BitmapWidth">The width of the bitmap</param>
        /// <param name="StartLeft">The column from where the bitmap must be read</param>
        /// <param name="StartTop">The row from where the bitmap must be read</param>
        /// <param name="DrawLeft">The first column where we must start displaying</param>
        /// <param name="DrawTop">The first row where we must start displaying</param>
        public void LoadBitmap(byte[] Bitmap, int BitmapWidth, int StartLeft = 0, int StartTop = 0, int DrawLeft = 0, int DrawTop = 0)
        {
            this.LoadBitmap(Bitmap, BitmapWidth, StartLeft, StartTop, DrawLeft, DrawTop, this.Width - DrawLeft, this._Height - DrawTop);
        }

        /// <summary>
        /// Loads a monochrome bitmap onto the LoL Shield
        /// </summary>
        /// <param name="Bitmap">An array with bits</param>
        /// <param name="BitmapWidth">The width of the bitmap</param>
        /// <param name="StartLeft">The column from where the bitmap must be read</param>
        /// <param name="StartTop">The row from where the bitmap must be read</param>
        /// <param name="DrawLeft">The first column where we must start displaying</param>
        /// <param name="DrawTop">The first row where we must start displaying</param>
        /// <param name="DrawWidth">The amount of columns we must display</param>
        /// <param name="DrawHeight">The amount of rows we must display</param>
        public void LoadBitmap(byte[] Bitmap, int BitmapWidth, int StartLeft, int StartTop, int DrawLeft, int DrawTop, int DrawWidth, int DrawHeight)
        {
            // Loops through all bytes in the bitmap array
            for (int SrcByteNo = 0; SrcByteNo < Bitmap.Length; ++SrcByteNo)
            {
                // Loops through all bits in the current byte
                byte SrcBitNo = 0;
                for (byte BitMask = 0x80; BitMask > 0; BitMask >>= 1)
                {
                    // Getting some info from the current position
                    int SrcPixNo = SrcByteNo * 8 + SrcBitNo;
                    int SrcPixRow = SrcPixNo / BitmapWidth;
                    int SrcPixCol = SrcPixNo - (SrcPixRow * BitmapWidth);
                    // Where should it be placed in the target display?
                    int TgtPixRow = SrcPixRow - StartTop + DrawTop;
                    int TgtPixCol = SrcPixCol - StartLeft + DrawLeft;
                    int TgtPixNo = TgtPixRow * this._Width + TgtPixCol;
                    // Should it be placed?
                    if (
                        TgtPixCol >= 0  // Can't write at negative space
                        && TgtPixRow >= 0  // Can't write at negative space
                        && TgtPixCol < this._Width // Can't write outside the screen
                        && TgtPixRow < this._Height // Can't write outside the screen
                        && TgtPixCol < DrawLeft + DrawWidth
                        && TgtPixRow < DrawTop + DrawHeight
                    )
                    {
                        this.Set(TgtPixNo, (Bitmap[SrcByteNo] & BitMask) == BitMask);
                    }
                    ++SrcBitNo;
                }
            }
        }

        #endregion
    }
}
