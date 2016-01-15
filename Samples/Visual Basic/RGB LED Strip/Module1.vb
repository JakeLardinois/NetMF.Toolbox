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
        ' There's a 1M strip (32 LEDs) connected to the first SPI bus on the Netduino
        Dim Chain As RgbLedStrip = New RgbLedStrip(RgbLedStrip.Chipsets.LPD8806, 32, SPI_Devices.SPI1)

        ' Repeats all demos infinitely
        Do
            ' Shows a red, green and blue LED
            Chain.SetColor(0, &HFF0000)
            Chain.SetColor(1, &HFF00)
            Chain.SetColor(2, &HFF)
            Chain.Write()
            Thread.Sleep(5000)

            ' Loops R, G and B for one minute
            Chain.SetColorAll(0, False)
            For Seconds As Integer = 0 To 30
                Chain.InsertColorAtBack(&HFF0000, False) : Thread.Sleep(333)
                Chain.InsertColorAtBack(&HFF00, False) : Thread.Sleep(334)
                Chain.InsertColorAtBack(&HFF, False) : Thread.Sleep(333)
            Next
            Chain.SetColorAll(0, False)
            For Seconds As Integer = 0 To 30
                Chain.InsertColorAtFront(&HFF, False) : Thread.Sleep(333)
                Chain.InsertColorAtFront(&HFF00, False) : Thread.Sleep(334)
                Chain.InsertColorAtFront(&HFF0000, False) : Thread.Sleep(333)
            Next

            ' Just changing colors
            Chain.SetColorAll(&HFF0000, False) : Thread.Sleep(1000)
            Chain.SetColorAll(&HFF00, False) : Thread.Sleep(1000)
            Chain.SetColorAll(&HFF, False) : Thread.Sleep(1000)

            ' Loops nicely through all colors
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(CByte(Brightness), 0, 0, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(255, CByte(Brightness), 0, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(CByte(255 - Brightness), 255, 0, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(0, 255, CByte(Brightness), False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(0, CByte(255 - Brightness), 255, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(CByte(Brightness), 0, 255, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(255, CByte(Brightness), 255, False) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.SetColorAll(CByte(255 - Brightness), CByte(255 - Brightness), CByte(255 - Brightness), False) : Thread.Sleep(1)
            Next
        Loop
    End Sub

End Module
