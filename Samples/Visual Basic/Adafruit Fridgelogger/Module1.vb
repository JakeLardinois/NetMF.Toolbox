#Const SD_ENABLED = 1
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF
Imports Toolbox.NETMF.Hardware
#If SD_ENABLED = 1 Then
Imports System.IO
Imports SecretLabs.NETMF.IO
#End If


'  Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
'
'  This sample is based on the Light-and-Temp-logger from Adafruit:
'  https://github.com/adafruit/Light-and-Temp-logger
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
#If SD_ENABLED = 1 Then
        StorageDevice.MountSD("SD", SPI_Devices.SPI1, Pins.GPIO_PIN_D10)

        ' Determines the filename
        Dim filename As String = ""
        Dim index As Integer = 0
        Do
            filename = "\SD\LOGGER" + Tools.ZeroFill(index, 2) + ".CSV"
            index = index + 1
        Loop While File.Exists(filename)

        ' Starts writing to the file
        Dim stream As FileStream = New FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)
        Dim writer As StreamWriter = New StreamWriter(stream)

        ' Writes file headers
        writer.WriteLine("ticks,datetime,light,temp")
#End If

        ' LEDs
        Dim red As OutputPort = New OutputPort(Pins.GPIO_PIN_D2, False)
        Dim green As OutputPort = New OutputPort(Pins.GPIO_PIN_D3, False)

        ' An analog light sensor
        Dim light As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A0)
        light.RangeSet(0, 1024)
        ' An analog temperature sensor
        Dim temperature As Tmp36 = New Tmp36(New Netduino.ADC(Pins.GPIO_PIN_A1))

        ' Time module (comment out SetTime once, to set the clock)
        Dim time As DS1307 = New DS1307()
        'time.SetTime(
        '    Day:=11,
        '    Month:=8,
        '    Year:=2012,
        '    Hour:=12,
        '    Minute:=0,
        '    Second:=0
        ')
        time.Synchronize()

        Do
            ' Green status LED ON
            green.Write(True)

            ' Builds the output
            Dim output As String = ""
            output += DateTime.Now.Ticks.ToString() + ", "
            output += DateTime.Now.ToString() + ", "
            output += light.RangeRead().ToString() + ", "
            output += temperature.Temperature.ToString()

            ' Prints the output to the debugger
            Debug.Print(output)
#If SD_ENABLED = 1 Then
            ' Writes the output to the SD buffer
            writer.WriteLine(output)
#End If

            ' Green status LED OFF, Red status LED ON
            green.Write(False)
            red.Write(True)

#If SD_ENABLED = 1 Then
            ' Flushes the buffers to the SD card
            writer.Flush()
            stream.Flush()
#End If

            ' Red status LED OFF
            red.Write(False)

            ' Sleeps for a second
            Thread.Sleep(1000)
        Loop

    End Sub

End Module
