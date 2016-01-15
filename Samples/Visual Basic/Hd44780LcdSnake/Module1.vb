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
        Dim Mux As Mcp23017 = New Mcp23017(32, 100)

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
        Dim Red As IGPOPort = Mux.Pins(6)    ' Red backlight
        Dim Green As IGPOPort = Mux.Pins(7)  ' Green backlight
        Dim Blue As IGPOPort = Mux.Pins(8)   ' Blue backlight

        ' Pins 9 to 15 are connected to the HD44780 LCD
        Dim Display As Hd44780Lcd = New Hd44780Lcd(
            Data:=Mux.CreateParallelOut(9, 4),
            ClockEnablePin:=Mux.Pins(13),
            ReadWritePin:=Mux.Pins(14),
            RegisterSelectPin:=Mux.Pins(15)
        )

        ' Initializes the game
        Games.HD44780_Snake.Init(Display, ButtonSelect, ButtonLeft, ButtonRight, ButtonUp, ButtonDown)

        ' Turn on blue backlight
        Blue.Write(False) : Red.Write(True) : Green.Write(True)

        ' Display splash
        Games.HD44780_Snake.Splash()

        ' Wait 5 sec.
        Thread.Sleep(5000)

        ' Turn on green backlight
        Blue.Write(True) : Red.Write(True) : Green.Write(False)

        ' Starts the game
        Try
            Games.HD44780_Snake.Start()
        Catch ex As Exception
            Display.ClearDisplay()
            Display.Write(ex.Message)
        End Try

        ' Turn on red backlight
        Blue.Write(True) : Red.Write(False) : Green.Write(True)
    End Sub

End Module
