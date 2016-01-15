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
        Dim Keypad As WearableKeypad = New WearableKeypad(Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D6)

        Do
            Select Case Keypad.Read()
                Case 0 : Debug.Print("Up")
                Case 1 : Debug.Print("Right")
                Case 2 : Debug.Print("Down")
                Case 3 : Debug.Print("Left")
                Case 4 : Debug.Print("Center")
            End Select

            Thread.Sleep(100)
        Loop
    End Sub

End Module
