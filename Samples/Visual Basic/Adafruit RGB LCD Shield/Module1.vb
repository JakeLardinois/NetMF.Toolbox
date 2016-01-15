Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
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
Module Module1

    Sub Main()
        ' The Adafruit LCD Shield uses a MCP23017 IC as multiplex chip
        Dim Mux As Mcp23017 = New Mcp23017()

        ' Pins 0 to 4 on the Mux-chip are connected to the buttons
        Dim ButtonSelect As IGPIPort = Mux.Pins(0)
        Dim ButtonRight As IGPIPort = Mux.Pins(1)
        Dim ButtonDown As IGPIPort = Mux.Pins(2)
        Dim ButtonUp As IGPIPort = Mux.Pins(3)
        Dim ButtonLeft As IGPIPort = Mux.Pins(4)
        ' Enables pull-ups for all the buttons
        For i As Integer = 0 To 4
            Mux.EnablePullup(i, True)
            Mux.Pins(i).InvertReadings = True
        Next

        ' Pins 6 to 8 on the Mux-chip are for the backlight
        Mux.Pins(6).Write(False)  ' Red backlight
        Mux.Pins(7).Write(True)   ' Green backlight
        Mux.Pins(8).Write(True)   ' Blue backlight

        ' Pins 9 to 15 are connected to the HD44780 LCD
        Dim Display As Hd44780Lcd = New Hd44780Lcd(
                                    Data:=Mux.CreateParallelOut(9, 4),
                                    ClockEnablePin:=Mux.Pins(13),
                                    ReadWritePin:=Mux.Pins(14),
                                    RegisterSelectPin:=Mux.Pins(15)
        )

        ' Pressing the Select-button will shift through these colors
        Dim Colors(5)() As Boolean
        ReDim Colors(0)(2) : Colors(0)(0) = False : Colors(0)(1) = True : Colors(0)(2) = True
        ReDim Colors(1)(2) : Colors(1)(0) = True : Colors(1)(1) = False : Colors(1)(2) = True
        ReDim Colors(2)(2) : Colors(2)(0) = True : Colors(2)(1) = True : Colors(2)(2) = False
        ReDim Colors(3)(2) : Colors(3)(0) = False : Colors(3)(1) = False : Colors(3)(2) = True
        ReDim Colors(4)(2) : Colors(4)(0) = False : Colors(4)(1) = True : Colors(4)(2) = False
        ReDim Colors(5)(2) : Colors(5)(0) = True : Colors(5)(1) = False : Colors(5)(2) = False
        Dim ColorIndex As Integer = 0

        ' Fills up the display
        Display.ClearDisplay()
        Display.Write("Left:  ? Down: ?")
        Display.ChangePosition(1, 0)
        Display.Write("Right: ? Up:   ?")

        ' Loops infinitely
        Dim SelectPressed As Boolean = False
        Do
            Display.ChangePosition(0, 7) : If ButtonLeft.Read Then Display.Write("1") Else Display.Write("0")
            Display.ChangePosition(1, 7) : If ButtonRight.Read Then Display.Write("1") Else Display.Write("0")
            Display.ChangePosition(0, 15) : If ButtonDown.Read Then Display.Write("1") Else Display.Write("0")
            Display.ChangePosition(1, 15) : If ButtonUp.Read Then Display.Write("1") Else Display.Write("0")

            ' Handles the Select button
            If ButtonSelect.Read Then
                If Not SelectPressed Then
                    SelectPressed = True
                    ColorIndex = ColorIndex + 1
                    If ColorIndex = Colors.Length Then ColorIndex = 0
                    Mux.Pins(6).Write(Colors(ColorIndex)(0))
                    Mux.Pins(7).Write(Colors(ColorIndex)(1))
                    Mux.Pins(8).Write(Colors(ColorIndex)(2))
                End If
            Else
                SelectPressed = False
            End If
        Loop
    End Sub

End Module
