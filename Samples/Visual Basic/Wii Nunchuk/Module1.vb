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
        Dim Nunchuk As WiiNunchuk = New WiiNunchuk()

        Do
            ' Reads all values
            Nunchuk.Read()

            ' Prints out all values
            Dim Output As String = "Analog stick: " + Nunchuk.AnalogStickX.ToString() + "x" + Nunchuk.AnalogStickY.ToString()
            Output += " / Accellerometer: " + Nunchuk.AcceleroMeterX.ToString() + "x" + Nunchuk.AcceleroMeterY.ToString() + "x" + Nunchuk.AcceleroMeterZ.ToString()
            Output += " / C-Button: " + Nunchuk.ButtonC.ToString()
            Output += " / Z-Button: " + Nunchuk.ButtonZ.ToString()
            Debug.Print(Output)

            Thread.Sleep(1000)
        Loop
    End Sub

End Module
