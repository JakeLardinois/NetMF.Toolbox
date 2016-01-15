using System;
using Microsoft.SPOT;
using Toolbox.NETMF.Hardware;

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
namespace Games
{
    public static class HD44780_Snake
    {
        // Constants that compile all characters
        private const byte FRUIT_BOTTOM = 0x01; // b0001
        private const byte FRUIT_TOP    = 0x02; // b0010
        private const byte SNAKE_TOP    = 0x05; // b0101
        private const byte SNAKE_BOTTOM = 0x08; // b1000

        /// <summary>Reference to the display</summary>
        private static Hd44780Lcd _disp;
        /// <summary>Start button</summary>
        private static IGPIPort _btnStart;
        /// <summary>Left button</summary>
        private static IGPIPort _btnLeft;
        /// <summary>Right button</summary>
        private static IGPIPort _btnRight;
        /// <summary>Up button</summary>
        private static IGPIPort _btnUp;
        /// <summary>Down button</summary>
        private static IGPIPort _btnDown;

        /// <summary>Random helper</summary>
        private static Random _rand;

        /// <summary>Max. width of the game</summary>
        public static int Width = 16;
        /// <summary>Max. height of the game</summary>
        public static int Height = 4;

        /// <summary>Storage for the position of the snake (y * Height + x). The integer size is the max. length of the snake</summary>
        private static int[] _Snake = new int[40];

        /// <summary>The position for the food</summary>
        private static int _FoodPosition = -1;

        /// <summary>The direction we're moving into (-1 = left, 1 = right, -Width = up, +Width = down)</summary>
        private static int _Direction = -1;

        /// <summary>Each tick this will be increased</summary>
        private static int _Timer = 0;

        /// <summary>The current game speed</summary>
        private static int _Speed = 40;

        /// <summary>
        /// Shows the splash screen
        /// </summary>
        public static void Splash()
        {
            _disp.ClearDisplay();
            _NetduinoLogo();
            _disp.Write(" HD44780  Snake");
            _disp.ChangePosition(1, 2);
            _disp.Write("for ");
            _disp.Write(new byte[] { 0, 1, 2, 3, 4, 5, 0, 6 }, false);
        }

        /// <summary>
        /// Initializes the hardware
        /// </summary>
        /// <param name="Display">Reference to the LCD</param>
        /// <param name="ButtonStart">Start button</param>
        /// <param name="ButtonLeft">Left button</param>
        /// <param name="ButtonRight">Right button</param>
        /// <param name="ButtonUp">Up button</param>
        /// <param name="ButtonDown">Down button</param>
        public static void Init(Hd44780Lcd Display, IGPIPort ButtonStart, IGPIPort ButtonLeft, IGPIPort ButtonRight, IGPIPort ButtonUp, IGPIPort ButtonDown)
        {
            // Copies the parameters to local values
            _disp = Display;
            _btnStart = ButtonStart;
            _btnLeft = ButtonLeft;
            _btnRight = ButtonRight;
            _btnUp = ButtonUp;
            _btnDown = ButtonDown;
            _rand = new Random();
        }

        /// <summary>
        /// Changes the custom characters to the Netduino-logo
        /// </summary>
        private static void _NetduinoLogo()
        {
            // To show the Netduino-logo on a 2x16 LCD, we need 7 custom characters (0x00 to 0x06):
            _CustomizeCharacter(0x00, new byte[] { 0x00, 0x00, 0x00, 0x0e, 0x11, 0x11, 0x11, 0x11 }); // n
            _CustomizeCharacter(0x01, new byte[] { 0x00, 0x00, 0x00, 0x0e, 0x13, 0x14, 0x19, 0x0e }); // e
            _CustomizeCharacter(0x02, new byte[] { 0x00, 0x10, 0x10, 0x1e, 0x10, 0x10, 0x10, 0x0e }); // t
            _CustomizeCharacter(0x03, new byte[] { 0x01, 0x01, 0x01, 0x0f, 0x13, 0x11, 0x11, 0x0e }); // d
            _CustomizeCharacter(0x04, new byte[] { 0x00, 0x00, 0x00, 0x11, 0x11, 0x11, 0x11, 0x0e }); // u
            _CustomizeCharacter(0x05, new byte[] { 0x00, 0x04, 0x00, 0x04, 0x04, 0x04, 0x04, 0x04 }); // i
            _CustomizeCharacter(0x06, new byte[] { 0x00, 0x00, 0x00, 0x0e, 0x11, 0x11, 0x11, 0x0e }); // o
        }

        /// <summary>
        /// Changes the custom characters to the snake
        /// </summary>
        private static void _SnakeChars()
        {
            // To play snake on a 2x16 LCD, we need 7 custom characters (0x00 to 0x06):
            _CustomizeCharacter(0x00, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x0e, 0x00 }); // b000: ₒ     (fruit at the bottom)
            _CustomizeCharacter(0x01, new byte[] { 0x00, 0x0e, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00 }); // b001: °     (fruit at the top)
            _CustomizeCharacter(0x02, new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x00, 0x00, 0x00, 0x00 }); // b010: ▀     (snake at the top)
            _CustomizeCharacter(0x03, new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x00, 0x0e, 0x0e, 0x00 }); // b011: ▀ + ₒ (snake at the top + fruit at the bottom)
            _CustomizeCharacter(0x04, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f }); // b100: ▄     (snake at the bottom)
            _CustomizeCharacter(0x05, new byte[] { 0x00, 0x0e, 0x0e, 0x00, 0x1f, 0x1f, 0x1f, 0x1f }); // b101: ▄ + ° (snake at the bottom + fruit at the top)
            _CustomizeCharacter(0x06, new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f }); // b110: █     (snake at the bottom + snake at the top)
        }

        /// <summary>Screen line 1 buffer</summary>
        private static byte[] _Line1Buffer = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        /// <summary>Screen line 2 buffer</summary>
        private static byte[] _Line2Buffer = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        
        /// <summary>
        /// Draws the screen
        /// </summary>
        private static void _DrawScreen()
        {
            // Lets start with 2 empty lines
            byte[] Line1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] Line2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Adds the fruit
            if (_FoodPosition < 16)      // line 1
                Line1[_FoodPosition] += FRUIT_TOP;
            else if (_FoodPosition < 32) // line 2
                Line1[_FoodPosition - 16] += FRUIT_BOTTOM;
            else if (_FoodPosition < 48) // line 3
                Line2[_FoodPosition - 32] += FRUIT_TOP;
            else                         // line 4
                Line2[_FoodPosition - 48] += FRUIT_BOTTOM;

            // Adds the snake
            for (int Pos = 0; Pos < _Snake.Length; ++Pos)
            {
                if (_Snake[Pos] == -1) continue;

                if (_Snake[Pos] < 16)        // line 1
                    Line1[_Snake[Pos]] += SNAKE_TOP;
                else if (_Snake[Pos] < 32)   // line 2
                    Line1[_Snake[Pos] - 16] += SNAKE_BOTTOM;
                else if (_Snake[Pos] < 48)   // line 3
                    Line2[_Snake[Pos] - 32] += SNAKE_TOP;
                else                         // line 4
                    Line2[_Snake[Pos] - 48] += SNAKE_BOTTOM;
            }

            for (byte pos = 0; pos < 16; ++pos)
            {
                if (Line1[pos] != _Line1Buffer[pos])
                {
                    _disp.ChangePosition(0, pos);
                    if (Line1[pos] == 0)
                        _disp.Write(" ");
                    else 
                        _disp.Write((byte)(Line1[pos] >> 1));
                }
                if (Line2[pos] != _Line2Buffer[pos])
                {
                    _disp.ChangePosition(1, pos);
                    if (Line2[pos] == 0)
                        _disp.Write(" ");
                    else
                        _disp.Write((byte)(Line2[pos] >> 1));
                }
            }
            _Line1Buffer = Line1;
            _Line2Buffer = Line2;
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public static void Start()
        {
            _disp.ClearDisplay();
            // Enables the snake characters
            _SnakeChars();
            // Sets the snake for the first time
            _ResetSnake();
            _ResetFood();
            // Starts the main loop
            while (true)
                _MainLoop();
        }

        /// <summary>
        /// Main game loop
        /// </summary>
        private static void _MainLoop()
        {
            // Draws out the display
            _DrawScreen();

            // Joystick control changes the direction
            if (_btnLeft.Read() && _Direction != 1)
                _Direction = -1;
            if (_btnRight.Read() && _Direction != -1)
                _Direction = 1;
            if (_btnDown.Read() && _Direction != 0 - Width)
                _Direction = Width;
            if (_btnUp.Read() && _Direction != Width)
                _Direction = 0 - Width;

            // Increases the timer
            _Timer++;

            // Do we need to move?
            if (_Timer >= _Speed)
            {
                _DoMovement();
                _Timer = 0;
            }
        }

        /// <summary>
        /// Resets the snake, placing it at a random position
        /// </summary>
        private static void _ResetSnake()
        {
            // The rest of the snake pixels is empty
            for (int Counter = 0; Counter < _Snake.Length; ++Counter)
                _Snake[Counter] = -1; // No position
            // Starts at the middle
            _Snake[0] = Height * Width / 2 - Width / 2;
        }

        /// <summary>
        /// Will place food on the matrix
        /// </summary>
        private static void _ResetFood()
        {
            int NewPos = -1;
            bool Found;

            // Checks if this position isn't currently occupied by the snake
            do
            {
                NewPos = _rand.Next(Width * Height);
                Found = false;
                for (int Counter = 0; Counter < _Snake.Length; ++Counter)
                    if (_Snake[Counter] == NewPos) Found = true;
            } while (Found);

            _FoodPosition = NewPos;
        }

        /// <summary>
        /// Moves the snake one position
        /// </summary>
        private static void _DoMovement()
        {
            // The next position
            int NextPos = _Snake[0] + _Direction;

            // We went out of the screen
            if (NextPos < 0) NextPos += (Height * Width);                                  // At the top
            if (NextPos >= Height * Width) NextPos -= (Height * Width);                    // At the bottom
            if (NextPos / Width < _Snake[0] / Width && _Direction == -1) NextPos += Width; // At the left
            if (NextPos / Width > _Snake[0] / Width && _Direction == 1) NextPos -= Width;  // At the right

            // Are we eating?
            bool Grow = false;
            if (NextPos == _FoodPosition)
            {
                Grow = true;
                _ResetFood();
            }

            // The next list of positions will be stored in here
            int[] NewSnake = (int[])_Snake.Clone();
            // Some checks
            for (int Counter = 0; Counter < _Snake.Length; ++Counter)
            {
                // We ate ourself!
                if (_Snake[Counter] == NextPos) throw new Exception(" Game over!");
                // Is this our length?
                if (_Snake[Counter] == -1 && !Grow) break;
                // Do we need to grow?
                else if (_Snake[Counter] == -1) Grow = false;
                // Moves up one spot
                if (Counter > 0) NewSnake[Counter] = _Snake[Counter - 1];
            }

            // New position
            NewSnake[0] = NextPos;
            _Snake = NewSnake;

            // Did we win?
            if (_Snake[_Snake.Length - 1] != -1)
            {
                throw new Exception (" Next level!");
                //this._ResetSnake();
                //this._ResetSnake();
                //this._Speed = (int)(this._Speed * .9) - 1;
            }
        }

        /// <summary>
        /// Creates a customized character on a HD44780 LCD
        /// </summary>
        /// <param name="Index">Character index (0 to 7)</param>
        /// <param name="Character">Character bitmap</param>
        private static void _CustomizeCharacter(int Index, byte[] CharacterBitmap)
        {
            if (Index < 0 || Index > 7) throw new IndexOutOfRangeException("Index must be a value from 0 to 7");
            if (CharacterBitmap.Length != 8) throw new ArgumentOutOfRangeException("CharacterBitmap must have 8 byte values");
            _disp.Write((byte)(0x40 + 8 * Index), true); // Set CGRAM Address
            _disp.Write(CharacterBitmap);                // Writes the character
            _disp.Write(0x80, true);                     // Set DDRAM Address
        }

    }
}
