Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF
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

    Dim WithEvents Gps As NmeaGps = New NmeaGps()
    Dim Led As OutputPort = New OutputPort(Pins.ONBOARD_LED, False)

    Sub Main()
        ' Starts the GPS device
        Debug.Print("Trying to get a fix...")
        Gps.Start()

        ' Nice blinking LED effect when we have a fix
        Do
            Led.Write(Gps.Fix)
            Thread.Sleep(450)
            Led.Write(Not Gps.Fix)
            Thread.Sleep(50)
        Loop
    End Sub

    Private Sub Gps_GotFix(Unused As UInteger, FixType As UInteger, GPSTime As Date) Handles Gps.GotFix
        Debug.Print("We got a fix, yay!!")
    End Sub

    Private Sub Gps_LostFix(Unused As UInteger, FixType As UInteger, GPSTime As Date) Handles Gps.LostFix
        Debug.Print("We lost our GPS fix :(")
    End Sub

    Private Sub Gps_PositionChanged(Unused As UInteger, FixType As UInteger, GPSTime As Date) Handles Gps.PositionChanged
        Dim Outp As String = ""
        Outp += "3D-Fix: " + Gps.Fix3D.ToString()
        Outp += ", Sattellites: " + Gps.Satellites.ToString()
        Outp += ", Time: " + Gps.GPSTime.ToString()
        Outp += ", Latitude: " + Gps.SLatitude
        Outp += ", Longitude: " + Gps.SLongitude
        Outp += ", Altitude: " + Gps.SAltitude
        Outp += ", Knots: " + Gps.Knots.ToString() + " (" + Gps.Kmh.ToString() + " km/h)"
        Debug.Print(Outp)

        ' If you want to translate this to a Bing Maps URL, try this:
        Debug.Print("http://www.bing.com/maps/?q=" + Tools.RawUrlEncode(Gps.Latitude.ToString() + " " + Gps.Longitude.ToString()))
    End Sub
End Module
