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

    ' Initializes a new rotary encoder object
    Dim WithEvents Knob As RotaryEncoder = New RotaryEncoder(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1)

    Sub Main()
        ' Wait infinitely
        Thread.Sleep(Timeout.Infinite)
    End Sub

    Private Sub Knob_Rotated(Unused As UInteger, Value As UInteger, time As Date) Handles Knob.Rotated
        If Value = 1 Then Debug.Print("Clockwise") Else Debug.Print("Counter clockwise")
    End Sub
End Module
