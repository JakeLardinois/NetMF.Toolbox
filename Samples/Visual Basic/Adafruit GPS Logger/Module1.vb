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
        writer.WriteLine("ticks,datetime,fix,latitude,longitude,altitude,satellites,kmh")
#End If

        ' The GPS unit is connected to COM2
        Dim gps As NmeaGps = New NmeaGps("COM2")

        ' Lets power on the GPS unit (active: LOW)
        Dim gps_enable As OutputPort = New OutputPort(Pins.GPIO_PIN_D4, False)

        ' LEDs are connected to pins 5 and 6
        Dim led1 As OutputPort = New OutputPort(Pins.GPIO_PIN_D5, False)
        Dim led2 As OutputPort = New OutputPort(Pins.GPIO_PIN_D6, False)

        ' Starts the GPS unit
        gps.Start()

        Do
            ' LED1 status ON
            led1.Write(True)

            ' Builds the output
            Dim output As String = ""
            output += DateTime.Now.Ticks.ToString() + ", "
            output += gps.GPSTime.ToString() + ", "
            If Not gps.Fix Then
                output += "0, 0, 0, 0, 0, 0"
            Else

                output += "1, "
                output += gps.Latitude.ToString() + ", "
                output += gps.Longitude.ToString() + ", "
                output += gps.Altitude.ToString() + ", "
                output += gps.Satellites.ToString() + ", "
                output += gps.Kmh.ToString()
            End If

            ' Prints the output to the debugger
            Debug.Print(output)
#If SD_ENABLED = 1 Then
            ' Writes the output to the SD buffer
            writer.WriteLine(output)
#End If

            ' LED1 status OFF, LED1 status ON
            led1.Write(False)
            led2.Write(True)

#If SD_ENABLED = 1 Then
            ' Flushes the buffers to the SD card
            writer.Flush()
            stream.Flush()
#End If

            ' LED2 status OFF
            led2.Write(False)

            ' Sleeps for a second
            Thread.Sleep(1000)
        Loop
    End Sub

End Module
