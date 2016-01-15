Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
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
        ' Defines the buzzer connected to pin 3 and ground
        Dim Buzzer As BitBangBuzzer = New BitBangBuzzer(Pins.GPIO_PIN_D3)

        ' Infinite loop
        Do
            ' Lets make some noise for 5 seconds!
            Debug.Print("Cover your ears!")
            Buzzer.Write(True)
            Thread.Sleep(5000)
            ' Lets be silent for a sec
            Debug.Print("Finally, some silence!")
            Buzzer.Write(False)
            Thread.Sleep(1000)
        Loop
    End Sub

End Module
