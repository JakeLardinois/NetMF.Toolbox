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

    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G    W A R N I N G    W A R N I N G  !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' ! This may technically draw more current than recommended by the microcontroller specifications, and !
    ' ! may void your warranty. Have fun and keep the fire extinguisher nearby :-)                         !
    ' ! See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.LolShield         !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    Sub Main()
        ' Defines a new LoL-shield connected to pins 2 to 13
        Dim Shield As LolShield = New LolShield(New Cpu.Pin() {
            Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5,
            Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8, Pins.GPIO_PIN_D9,
            Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D11, Pins.GPIO_PIN_D12, Pins.GPIO_PIN_D13
        })

        ' Then, a test for every single LED:
        For LedNo As Integer = 0 To (Shield.Width * Shield.Height) - 1
            Shield.Set(LedNo, True)
            Thread.Sleep(100)
            Shield.Set(LedNo, False)
        Next

        ' Loop over all columns
        For Col As Integer = 0 To Shield.Width - 1
            Shield.VerticalLine(Col, True)
            Thread.Sleep(100)
            Shield.VerticalLine(Col, False)
        Next

        ' Loop over all rows
        For Row As Integer = 0 To Shield.Height - 1
            Shield.HorizontalLine(Row, True)
            Thread.Sleep(100)
            Shield.HorizontalLine(Row, False)
        Next

        ' Now turns on all LEDs for a few seconds
        Shield.Clear(True)
        Thread.Sleep(2000)

        ' Draws a bitmap to the screen
        Shield.LoadBitmap(Bitmaps.lolshield_bmp.Data, Bitmaps.lolshield_bmp.Width)

        ' And inverts that state after a few seconds, for 2 times
        Thread.Sleep(2000) : Shield.Invert()
        Thread.Sleep(2000) : Shield.Invert()

        ' Pixel locations for the NP2 logo
        Dim Positions() As Integer = {
            84, 70, 56, 42, 29, 30, 45, 59, 73, 87, _
            117, 103, 89, 75, 61, 47, 34, 35, 50, 64, 77, 76, _
            38, 25, 26, 41, 55, 68, 81, 94, 95, 96, 97 _
        }

        Do
            ' Draws the NP2 logo
            Shield.Clear()
            For pixno As Integer = 0 To Positions.Length - 1
                Shield.Set(Positions(pixno), True)
                Thread.Sleep(50)
            Next

            For repeat As Integer = 0 To 4
                ' Clears the screen
                Shield.Clear()
                Thread.Sleep(250)

                ' Draws the logo again, but now faster
                For pixno As Integer = 0 To Positions.Length - 1
                    Shield.Set(Positions(pixno), True)
                Next
                Thread.Sleep(250)
            Next
        Loop

    End Sub

End Module
