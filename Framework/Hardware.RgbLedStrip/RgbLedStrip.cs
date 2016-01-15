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
    /// A chain of RGB LEDs
    /// </summary>
    public class RgbLedStrip
    {
        /// <summary>
        /// Supported types of chips
        /// </summary>
        public enum Chipsets
        {
            /// <summary>LPD8806</summary>
            LPD8806 = 8806,
            /// <summary>WS2801</summary>
            WS2801 = 2801
        }

        /// <summary>
        /// The used chipset
        /// </summary>
        private Chipsets _Chipset;

        /// <summary>
        /// Reference to the SPI connection
        /// </summary>
        private MultiSPI _Conn;

        /// <summary>
        /// State stored for each LED
        /// </summary>
        private byte[] _LedState;

        /// <summary>
        /// Brightness stored for each LED
        /// </summary>
        private byte[] _Brightness;

        /// <summary>
        /// The SPI write buffer
        /// </summary>
        private byte[] _Buffer;

        /// <summary>
        /// Amount of LEDs
        /// </summary>
        public int LedCount { get; protected set; }

        /// <summary>
        /// When false, brightness won't be taken into account with the <see cref="InsertColorAtBack(int, bool)"/> and <see cref="InsertColorAtFront(int, bool)"/> methods
        /// </summary>
        public bool ShiftBrightness { get; set; }

        /// <summary>
        /// These are the possible values for <see cref="Sequence"/>
        /// </summary>
        public enum Sequences
        {
            /// <summary>Red/Green/Blue</summary>
            RGB = 123,
            /// <summary>Red/Blue/Green</summary>
            RBG = 132,
            /// <summary>Green/Red/Blue</summary>
            GRB = 213,
            /// <summary>Green/Blue/Red</summary>
            GBR = 231,
            /// <summary>Blue/Red/Green</summary>
            BRG = 312,
            /// <summary>Blue/Green/Red</summary>
            BGR = 321
        }

        /// <summary>
        /// There are some strips that use a different sequence for the red, green and blue bytes. Change that here.
        /// </summary>
        public Sequences Sequence { get; set; }

        /// <summary>
        /// Defines a chain of RGB LEDs
        /// </summary>
        /// <param name="Chipset">The chipset used to daisychain the LEDs</param>
        /// <param name="LedCount">The amount of LEDs in the chain</param>
        /// <param name="SPI_Device">The SPI bus the chain is connected to</param>
        public RgbLedStrip(Chipsets Chipset, int LedCount, SPI.SPI_module SPI_Device) : this(Chipset, LedCount, SPI_Device, Cpu.Pin.GPIO_NONE, false) { }

        /// <summary>
        /// Defines a chain of RGB LEDs
        /// </summary>
        /// <param name="Chipset">The chipset used to daisychain the LEDs</param>
        /// <param name="LedCount">The amount of LEDs in the chain</param>
        /// <param name="SPI_Device">The SPI bus the chain is connected to</param>
        /// <param name="ChipSelect_Port">If there's a CS circuitry, specify it's pin</param>
        /// <param name="ChipSelect_ActiveState">If there's a CS circuitry, specify it's active state</param>
        public RgbLedStrip(Chipsets Chipset, int LedCount, SPI.SPI_module SPI_Device, Cpu.Pin ChipSelect_Port, bool ChipSelect_ActiveState)
        {
            // The used chipset
            this._Chipset = Chipset;
            
            // Stores the amount of LEDs
            this.LedCount = LedCount;

            // Extends the arrays for the LED states and brightness
            this._LedState = new byte[LedCount * 3];
            this._Brightness = new byte[LedCount];

            // Settings for the LPD8806 chip
            if (Chipset == Chipsets.LPD8806)
            {
                // Creates a new buffer (final 3 bytes should always be 0 and tells the chain we're done for now)
                this._Buffer = new byte[LedCount * 3 + 3];

                // Default sequence of the Adafruit strips
                this.Sequence = Sequences.GRB;
            }

            // Settings for the WS2801 chip
            if (Chipset == Chipsets.WS2801)
            {
                // Creates a new buffer
                this._Buffer = new byte[LedCount * 3];

                // Default sequence of the Adafruit chains
                this.Sequence = Sequences.RGB;
            }

            // Configures the SPI bus
            this._Conn = new MultiSPI(new SPI.Configuration(
                ChipSelect_Port: ChipSelect_Port,
                ChipSelect_ActiveState: ChipSelect_ActiveState,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: true,
                Clock_RateKHz: 1000,
                SPI_mod: SPI_Device
            ));

            // Set brightness only half way, most LED strips are just way too bright imho
            this.SetBrightnessAll(128);
            // Turns off all LEDs
            this.SetColorAll(0);
            // Writes for the first time
            this.Write();
        }

        /// <summary>
        /// Shifts all LEDs to the right and adds a new one at the left
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void InsertColorAtBack(byte Red, byte Green, byte Blue, bool Delayed = true)
        {
            for (int LedNo = this.LedCount - 2; LedNo >= 0; --LedNo)
            {
                //this._Buffer[LedNo * 3 + 3] = this._Buffer[LedNo * 3 + 0];
                //this._Buffer[LedNo * 3 + 4] = this._Buffer[LedNo * 3 + 1];
                //this._Buffer[LedNo * 3 + 5] = this._Buffer[LedNo * 3 + 2];
                this._LedState[LedNo * 3 + 3] = this._LedState[LedNo * 3 + 0];
                this._LedState[LedNo * 3 + 4] = this._LedState[LedNo * 3 + 1];
                this._LedState[LedNo * 3 + 5] = this._LedState[LedNo * 3 + 2];
                this._StateToBuffer(LedNo + 1);
            }
            this.SetColor(0, Red, Green, Blue, Delayed);
        }

        /// <summary>
        /// Shifts all LEDs to the right and adds a new one at the left
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void InsertColorAtBack(int Color, bool Delayed = true)
        {
            int[] Colors = Tools.ColorToRgb(Color);
            this.InsertColorAtBack((byte)Colors[0], (byte)Colors[1], (byte)Colors[2], Delayed);
        }

        /// <summary>
        /// Shifts all LEDs to the left and adds a new one at the right
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void InsertColorAtFront(byte Red, byte Green, byte Blue, bool Delayed = true)
        {
            for (int LedNo = 0; LedNo < this.LedCount - 1; ++LedNo)
            {
                //this._Buffer[LedNo * 3 + 0] = this._Buffer[LedNo * 3 + 3];
                //this._Buffer[LedNo * 3 + 1] = this._Buffer[LedNo * 3 + 4];
                //this._Buffer[LedNo * 3 + 2] = this._Buffer[LedNo * 3 + 5];
                this._LedState[LedNo * 3 + 0] = this._LedState[LedNo * 3 + 3];
                this._LedState[LedNo * 3 + 1] = this._LedState[LedNo * 3 + 1];
                this._LedState[LedNo * 3 + 2] = this._LedState[LedNo * 3 + 2];
                this._StateToBuffer(LedNo);
            }
            this.SetColor(this.LedCount - 1, Red, Green, Blue, Delayed);
        }

        /// <summary>
        /// Shifts all LEDs to the left and adds a new one at the right
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void InsertColorAtFront(int Color, bool Delayed = true)
        {
            int[] Colors = Tools.ColorToRgb(Color);
            this.InsertColorAtFront((byte)Colors[0], (byte)Colors[1], (byte)Colors[2], Delayed);
        }

        /// <summary>
        /// Configures all LEDs to a specific color
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetColorAll(byte Red, byte Green, byte Blue, bool Delayed = true)
        {
            for (int LedNo = 0; LedNo < this.LedCount; ++LedNo)
                this.SetColor(LedNo, Red, Green, Blue, true);

            if (!Delayed) this.Write();
        }

        /// <summary>
        /// Configures all LEDs to a specific color
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetColorAll(int Color, bool Delayed = true)
        {
            int[] Colors = Tools.ColorToRgb(Color);
            this.SetColorAll((byte)Colors[0], (byte)Colors[1], (byte)Colors[2], Delayed);
        }

        /// <summary>
        /// Sets the brightness for all LEDs
        /// </summary>
        /// <param name="Brightness">Brightness from 0 to 255</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetBrightnessAll(byte Brightness, bool Delayed = true)
        {
            for (int LedNo = 0; LedNo < this.LedCount; ++LedNo)
                this.SetBrightness(LedNo, Brightness, true);

            if (!Delayed) this.Write();
        }

        /// <summary>
        /// Gets the brightness from a single LED
        /// </summary>
        /// <param name="LedNo">The LED to read (starts counting at 0)</param>
        /// <returns>Brightness from 0 to 255</returns>
        public byte GetBrightness(int LedNo)
        {
            return this._Brightness[LedNo];
        }

        /// <summary>
        /// Gets the color from a single LED
        /// </summary>
        /// <param name="LedNo">The LED to read (starts counting at 0)</param>
        /// <returns>Color from 0x000000 to 0xffffff</returns>
        public int GetColor(int LedNo)
        {
            int Color = 0;
            byte b1 = this._LedState[LedNo * 3 + 0];
            byte b2 = this._LedState[LedNo * 3 + 1];
            byte b3 = this._LedState[LedNo * 3 + 2];

            // To get red, we shift 16 bits, to get green we shift 8 bits and to get blue, we shift 0 bits
            byte R = 16, G = 8, B = 0;
            if (this.Sequence == Sequences.RGB)      { Color = (b1 << R) + (b2 << G) + (b3 << B); }
            else if (this.Sequence == Sequences.RBG) { Color = (b1 << R) + (b2 << B) + (b3 << G); }
            else if (this.Sequence == Sequences.GRB) { Color = (b1 << G) + (b2 << R) + (b3 << B); }
            else if (this.Sequence == Sequences.GBR) { Color = (b1 << G) + (b2 << B) + (b3 << R); }
            else if (this.Sequence == Sequences.BRG) { Color = (b1 << B) + (b2 << R) + (b3 << G); }
            else if (this.Sequence == Sequences.BGR) { Color = (b1 << B) + (b2 << G) + (b3 << R); }

            return Color;
        }

        /// <summary>
        /// Sets the brightness for a single LED
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        /// <param name="Brightness">Brightness from 0 to 255</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetBrightness(int LedNo, byte Brightness, bool Delayed = true)
        {
            // Stores the brightness
            this._Brightness[LedNo] = Brightness;

            // Actually sets the state
            this._StateToBuffer(LedNo);

            // Do we need to write?
            if (!Delayed) this.Write();
        }

        /// <summary>
        /// Configures a specific LED
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetColor(int LedNo, byte Red, byte Green, byte Blue, bool Delayed = true)
        {
            // Determines the sequence of the 3 bytes
            byte b1 = 0, b2 = 0, b3 = 0;
            if (this.Sequence == Sequences.RGB)      { b1 = Red; b2 = Green; b3 = Blue; }
            else if (this.Sequence == Sequences.RBG) { b1 = Red; b2 = Blue; b3 = Green; }
            else if (this.Sequence == Sequences.GRB) { b1 = Green; b2 = Red; b3 = Blue; }
            else if (this.Sequence == Sequences.GBR) { b1 = Green; b2 = Blue; b3 = Red; }
            else if (this.Sequence == Sequences.BRG) { b1 = Blue; b2 = Red; b3 = Green; }
            else if (this.Sequence == Sequences.BGR) { b1 = Blue; b2 = Green; b3 = Red; }

            // Stores the basic LED state
            this._LedState[LedNo * 3 + 0] = b1;
            this._LedState[LedNo * 3 + 1] = b2;
            this._LedState[LedNo * 3 + 2] = b3;

            // Actually sets the state
            this._StateToBuffer(LedNo);

            // Do we need to write?
            if (!Delayed) this.Write();
        }

        /// <summary>
        /// Translates the LED states towards the SPI buffer
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        private void _StateToBuffer(int LedNo)
        {
            // Adjusts brightness
            byte b1 = (byte)((float)this._LedState[LedNo * 3 + 0] * ((float)this._Brightness[LedNo] / 255f));
            byte b2 = (byte)((float)this._LedState[LedNo * 3 + 1] * ((float)this._Brightness[LedNo] / 255f));
            byte b3 = (byte)((float)this._LedState[LedNo * 3 + 2] * ((float)this._Brightness[LedNo] / 255f));

            // For the LPD8806 we need to set the first bit to 1 and shift all other bits one position
            if (this._Chipset == Chipsets.LPD8806)
            {
                b1 = (byte)(0x80 | (b1 >> 1));
                b2 = (byte)(0x80 | (b2 >> 1));
                b3 = (byte)(0x80 | (b3 >> 1));
            }

            // Adds all bytes to the buffer
            this._Buffer[LedNo * 3 + 0] = b1;
            this._Buffer[LedNo * 3 + 1] = b2;
            this._Buffer[LedNo * 3 + 2] = b3;
        }

        /// <summary>
        /// Configures a specific LED
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Delayed">Do we have to write all LEDs immediately?</param>
        public void SetColor(int LedNo, int Color, bool Delayed = true)
        {
            int[] Colors = Tools.ColorToRgb(Color);
            this.SetColor(LedNo, (byte)Colors[0], (byte)Colors[1], (byte)Colors[2], Delayed);
        }

        /// <summary>
        /// Writes the status of all LEDs
        /// </summary>
        public void Write()
        {
            this._Conn.Write(this._Buffer);
        }
    }
}
