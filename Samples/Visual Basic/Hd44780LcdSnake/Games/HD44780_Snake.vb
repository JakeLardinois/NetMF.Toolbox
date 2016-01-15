Imports System
Imports Microsoft.SPOT
Imports Toolbox.NETMF.Hardware

'  Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
'
'  Licensed under the Apache License, Version 2.0 (the "License");
'  you may not use this file except in compliance with the License.
'  You may obtain a copy of the License at
'
'      http://www.apache.org/licenses/LICENSE-2.0
'
'  Unless required by applicable law or agreed to in writing, software
'  distributed under the License is distributed on an "AS IS" BASIS,
'  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'  See the License for the specific language governing permissions and
'  limitations under the License.
Namespace Games

    Public Module HD44780_Snake
        ' Constants that compile all characters
        Private Const FRUIT_BOTTOM As Byte = &H1 ' b0001
        Private Const FRUIT_TOP As Byte = &H2    ' b0010
        Private Const SNAKE_TOP As Byte = &H5    ' b0101
        Private Const SNAKE_BOTTOM As Byte = &H8 ' b1000

        ''' <summary>Reference to the display</summary>
        Private _disp As Hd44780Lcd
        ''' <summary>Start button</summary>
        Private _btnStart As IGPIPort
        ''' <summary>Left button</summary>
        Private _btnLeft As IGPIPort
        ''' <summary>Right button</summary>
        Private _btnRight As IGPIPort
        ''' <summary>Up button</summary>
        Private _btnUp As IGPIPort
        ''' <summary>Down button</summary>
        Private _btnDown As IGPIPort

        ''' <summary>Random helper</summary>
        Private _rand As Random

        ''' <summary>Max. width of the game</summary>
        Public Width As Integer = 16
        ''' <summary>Max. height of the game</summary>
        Public Height As Integer = 4

        ''' <summary>Storage for the position of the snake (y * Height + x). The integer size is the max. length of the snake</summary>
        Private _snake(40) As Integer

        ''' <summary>The position for the food</summary>
        Private _FoodPosition As Integer = -1

        ''' <summary>The direction we're moving into (-1 = left, 1 = right, -Width = up, +Width = down)</summary>
        Private _Direction As Integer = -1

        ''' <summary>Each tick this will be increased</summary>
        Private _Timer As Integer = 0

        ''' <summary>The current game speed</summary>
        Private _Speed As Integer = 40

        ''' <summary>
        ''' Shows the splash screen
        ''' </summary>
        Public Sub Splash()
            _disp.ClearDisplay()
            _NetduinoLogo()
            _disp.Write(" HD44780  Snake")
            _disp.ChangePosition(1, 2)
            _disp.Write("for ")
            _disp.Write(New Byte() {0, 1, 2, 3, 4, 5, 0, 6}, False)
        End Sub

        ''' <summary>
        ''' Initializes the hardware
        ''' </summary>
        ''' <param name="Display">Reference to the LCD</param>
        ''' <param name="ButtonStart">Start button</param>
        ''' <param name="ButtonLeft">Left button</param>
        ''' <param name="ButtonRight">Right button</param>
        ''' <param name="ButtonUp">Up button</param>
        ''' <param name="ButtonDown">Down button</param>
        Public Sub Init(Display As Hd44780Lcd, ButtonStart As IGPIPort, ButtonLeft As IGPIPort, ButtonRight As IGPIPort, ButtonUp As IGPIPort, ButtonDown As IGPIPort)
            ' Copies the parameters to local values
            _disp = Display
            _btnStart = ButtonStart
            _btnLeft = ButtonLeft
            _btnRight = ButtonRight
            _btnUp = ButtonUp
            _btnDown = ButtonDown
            _rand = New Random()
        End Sub

        ''' <summary>
        ''' Changes the custom characters to the Netduino-logo
        ''' </summary>
        Private Sub _NetduinoLogo()
            ' To show the Netduino-logo on a 2x16 LCD, we need 7 custom characters (0x00 to 0x06):
            _CustomizeCharacter(&H0, New Byte() {&H0, &H0, &H0, &HE, &H11, &H11, &H11, &H11})   ' n
            _CustomizeCharacter(&H1, New Byte() {&H0, &H0, &H0, &HE, &H13, &H14, &H19, &HE})    ' e
            _CustomizeCharacter(&H2, New Byte() {&H0, &H10, &H10, &H1E, &H10, &H10, &H10, &HE}) ' t
            _CustomizeCharacter(&H3, New Byte() {&H1, &H1, &H1, &HF, &H13, &H11, &H11, &HE})    ' d
            _CustomizeCharacter(&H4, New Byte() {&H0, &H0, &H0, &H11, &H11, &H11, &H11, &HE})   ' u
            _CustomizeCharacter(&H5, New Byte() {&H0, &H4, &H0, &H4, &H4, &H4, &H4, &H4})       ' i
            _CustomizeCharacter(&H6, New Byte() {&H0, &H0, &H0, &HE, &H11, &H11, &H11, &HE})    ' o
        End Sub

        ''' <summary>
        ''' Changes the custom characters to the snake
        ''' </summary>
        Private Sub _SnakeChars()
            ' To play snake on a 2x16 LCD, we need 7 custom characters (0x00 to 0x06):
            _CustomizeCharacter(&H0, New Byte() {&H0, &H0, &H0, &H0, &H0, &HE, &HE, &H0})         ' b000: ₒ     (fruit at the bottom)
            _CustomizeCharacter(&H1, New Byte() {&H0, &HE, &HE, &H0, &H0, &H0, &H0, &H0})         ' b001: °     (fruit at the top)
            _CustomizeCharacter(&H2, New Byte() {&H1F, &H1F, &H1F, &H1F, &H0, &H0, &H0, &H0})     ' b010: ▀     (snake at the top)
            _CustomizeCharacter(&H3, New Byte() {&H1F, &H1F, &H1F, &H1F, &H0, &HE, &HE, &H0})     ' b011: ▀ + ₒ (snake at the top + fruit at the bottom)
            _CustomizeCharacter(&H4, New Byte() {&H0, &H0, &H0, &H0, &H1F, &H1F, &H1F, &H1F})     ' b100: ▄     (snake at the bottom)
            _CustomizeCharacter(&H5, New Byte() {&H0, &HE, &HE, &H0, &H1F, &H1F, &H1F, &H1F})     ' b101: ▄ + ° (snake at the bottom + fruit at the top)
            _CustomizeCharacter(&H6, New Byte() {&H1F, &H1F, &H1F, &H1F, &H1F, &H1F, &H1F, &H1F}) ' b110: █     (snake at the bottom + snake at the top)
        End Sub


        ''' <summary>Screen line 1 buffer</summary>
        Private _Line1Buffer() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        ''' <summary>Screen line 2 buffer</summary>
        Private _Line2Buffer() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        ''' <summary>
        ''' Draws the screen
        ''' </summary>
        Private Sub _DrawScreen()
            ' Lets start with 2 empty lines
            Dim Line1() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            Dim Line2() As Byte = New Byte() {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

            ' Adds the fruit
            If _FoodPosition < 16 Then        ' line 1
                Line1(_FoodPosition) += FRUIT_TOP
            ElseIf _FoodPosition < 32 Then    ' line 2
                Line1(_FoodPosition - 16) += FRUIT_BOTTOM
            ElseIf _FoodPosition < 48 Then    ' line 3
                Line2(_FoodPosition - 32) += FRUIT_TOP
            Else                              ' line 4
                Line2(_FoodPosition - 48) += FRUIT_BOTTOM
            End If

            ' Adds the snake
            For Pos As Integer = 0 To _snake.Length - 1
                If _snake(Pos) = -1 Then Continue For

                If _snake(Pos) < 16 Then      ' line 1
                    Line1(_snake(Pos)) += SNAKE_TOP
                ElseIf _snake(Pos) < 32 Then  ' line 2
                    Line1(_snake(Pos) - 16) += SNAKE_BOTTOM
                ElseIf _snake(Pos) < 48 Then  ' line 3
                    Line2(_snake(Pos) - 32) += SNAKE_TOP
                Else                          ' line 4
                    Line2(_snake(Pos) - 48) += SNAKE_BOTTOM
                End If
            Next

            For Pos As Byte = 0 To 15
                If Line1(Pos) <> _Line1Buffer(Pos) Then
                    _disp.ChangePosition(0, Pos)
                    If Line1(Pos) = 0 Then
                        _disp.Write(" ")
                    Else
                        _disp.Write(CByte(Line1(Pos) >> 1))
                    End If
                End If
                If Line2(Pos) <> _Line2Buffer(Pos) Then

                    _disp.ChangePosition(1, Pos)
                    If Line2(Pos) = 0 Then
                        _disp.Write(" ")
                    Else
                        _disp.Write(CByte(Line2(Pos) >> 1))
                    End If
                End If
            Next

            _Line1Buffer = Line1
            _Line2Buffer = Line2
        End Sub

        ''' <summary>
        ''' Starts the game
        ''' </summary>
        Public Sub Start()
            _disp.ClearDisplay()
            ' Enables the snake characters
            _SnakeChars()
            ' Sets the snake for the first time
            _ResetSnake()
            _ResetFood()
            ' Starts the main loop
            Do
                _MainLoop()
            Loop
        End Sub

        ''' <summary>
        ''' Main game loop
        ''' </summary>
        Private Sub _MainLoop()
            ' Draws out the display
            _DrawScreen()

            ' Joystick control changes the direction
            If _btnLeft.Read() And _Direction <> 1 Then _Direction = -1
            If _btnRight.Read() And _Direction <> -1 Then _Direction = 1
            If _btnDown.Read() And _Direction <> 0 - Width Then _Direction = Width
            If _btnUp.Read() And _Direction <> Width Then _Direction = 0 - Width

            ' Increases the timer
            _Timer = _Timer + 1

            ' Do we need to move?
            If _Timer >= _Speed Then
                _DoMovement()
                _Timer = 0
            End If
        End Sub

        ''' <summary>
        ''' Resets the snake, placing it at a random position
        ''' </summary>
        Private Sub _ResetSnake()
            ' The rest of the snake pixels is empty
            For Counter As Integer = 0 To _snake.Length - 1
                _snake(Counter) = -1 ' No position
            Next

            ' Starts at the middle
            _snake(0) = CInt(Height * Width / 2 - Width / 2)
        End Sub

        ''' <summary>
        ''' Will place food on the matrix
        ''' </summary>
        Private Sub _ResetFood()
            Dim NewPos As Integer = -1
            Dim Found As Boolean

            ' Checks if this position isn't currently occupied by the snake
            Do
                NewPos = _rand.Next(Width * Height)
                Found = False
                For Counter As Integer = 0 To _snake.Length - 1
                    If _snake(Counter) = NewPos Then Found = True
                Next
            Loop While Found

            _FoodPosition = NewPos
        End Sub

        ''' <summary>
        ''' Moves the snake one position
        ''' </summary>
        Private Sub _DoMovement()
            ' The next position
            Dim NextPos As Integer = _snake(0) + _Direction

            ' We went out of the screen
            If NextPos < 0 Then NextPos += (Height * Width) '                                    At the top
            If NextPos >= (Height * Width) Then NextPos -= (Height * Width) '                      At the bottom
            If Floor(NextPos / Width) < Floor(_snake(0) / Width) And (_Direction = -1) Then NextPos += Width ' At the left
            If Floor(NextPos / Width) > Floor(_snake(0) / Width) And (_Direction = 1) Then NextPos -= Width '  At the right

            ' Are we eating?
            Dim Grow As Boolean = False
            If NextPos = _FoodPosition Then
                Grow = True
                _ResetFood()
            End If

            ' The next list of positions will be stored in here
            Dim NewSnake() As Integer = CType(_snake.Clone(), Integer())

            ' Some checks
            For Counter As Integer = 0 To _snake.Length - 1

                ' We ate ourself!
                If _snake(Counter) = NextPos Then Throw New Exception(" Game over!")
                ' Is this our length?
                If _snake(Counter) = -1 And Not Grow Then
                    Exit For
                    ' Do we need to grow?
                ElseIf _snake(Counter) = -1 Then
                    Grow = False
                End If
                ' Moves up one spot
                If Counter > 0 Then NewSnake(Counter) = _snake(Counter - 1)
            Next

            ' New position
            NewSnake(0) = NextPos
            _snake = NewSnake

            ' Did we win?
            If _snake(_snake.Length - 1) <> -1 Then
                Throw New Exception(" Next level!")
                'this._ResetSnake();
                'this._ResetSnake();
                'this._Speed = (int)(this._Speed * .9) - 1;
            End If
        End Sub

        ''' <summary>
        ''' Creates a customized character on a HD44780 LCD
        ''' </summary>
        ''' <param name="Index">Character index (0 to 7)</param>
        ''' <param name="Character">Character bitmap</param>
        Private Sub _CustomizeCharacter(Index As Integer, CharacterBitmap() As Byte)
            If Index < 0 Or Index > 7 Then Throw New IndexOutOfRangeException("Index must be a value from 0 to 7")
            If CharacterBitmap.Length <> 8 Then Throw New ArgumentOutOfRangeException("CharacterBitmap must have 8 byte values")
            _disp.Write(CByte(&H40 + 8 * Index), True)   ' Set CGRAM Address
            _disp.Write(CharacterBitmap)                 ' Writes the character
            _disp.Write(CByte(&H80), True)               ' Set DDRAM Address
        End Sub

        ''' <summary>
        ''' Since VB.NETMF lacks Math.Floor, I made this method
        ''' </summary>
        ''' <param name="Value">Full value</param>
        ''' <return>Value rounded down</return>
        Private Function Floor(ByVal Value As Double) As Integer
            Dim RetValue = CInt(Value)
            If RetValue > Value Then RetValue = RetValue - 1
            Return RetValue
        End Function

    End Module
End Namespace