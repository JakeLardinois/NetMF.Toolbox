Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF
Imports Toolbox.NETMF.Hardware

'  Copyright 2011-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
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
        ' Defines an RGB Led connected to three PWM pins, common cathode
        ' Some RGB-leds are common anode, in those cases, set the last boolean on true
        Dim Led As RgbLed = New RgbLed(New Netduino.PWM(Pins.GPIO_PIN_D9), New Netduino.PWM(Pins.GPIO_PIN_D6), New Netduino.PWM(Pins.GPIO_PIN_D5), False)

        Do

            ' Fade to red
            For Red As Integer = 0 To 255
                Led.Write(CByte(Red), 0, 0)
                Thread.Sleep(25)
            Next

            ' Fade to green
            For Green As Integer = 0 To 255
                Led.Write(0, CByte(Green), 0)
                Thread.Sleep(25)
            Next

            ' Fade to blue
            For Blue As Integer = 0 To 255
                Led.Write(0, 0, CByte(Blue))
                Thread.Sleep(25)
            Next

            Led.Write(CInt(Tools.Hex2Dec("0000ff"))) : Thread.Sleep(500) ' Blue
            Led.Write(CInt(Tools.Hex2Dec("00ff00"))) : Thread.Sleep(500) ' Green
            Led.Write(CInt(Tools.Hex2Dec("00ffff"))) : Thread.Sleep(500) ' Turquoise
            Led.Write(CInt(Tools.Hex2Dec("ff0000"))) : Thread.Sleep(500) ' Red
            Led.Write(CInt(Tools.Hex2Dec("ff00ff"))) : Thread.Sleep(500) ' Purple
            Led.Write(CInt(Tools.Hex2Dec("ffff00"))) : Thread.Sleep(500) ' Yellow
            Led.Write(CInt(Tools.Hex2Dec("ffffff"))) : Thread.Sleep(500) ' White
            Led.Write(CInt(Tools.Hex2Dec("000000"))) : Thread.Sleep(500) ' Off
        Loop

    End Sub

End Module
