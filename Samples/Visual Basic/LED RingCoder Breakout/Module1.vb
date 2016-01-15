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

    ' Defines the push button and LEDs in the knob
    Dim WithEvents Button As AutoRepeatInputPort = New AutoRepeatInputPort(Pins.GPIO_PIN_D2, Port.ResistorMode.PullUp, True)
    Dim Red As OutputPort = New OutputPort(Pins.GPIO_PIN_D3, False)
    Dim Green As OutputPort = New OutputPort(Pins.GPIO_PIN_D4, True)

    'There are 16 LEDs in the circle around the knob
    Dim Led(0 To 15) As IGPOPort

    ' We'll store an integer value of the knob
    Dim KnobValue As Integer = 0

    ' Defines the rotary encoder
    Dim WithEvents Knob As RotaryEncoder = New RotaryEncoder(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1)

    Sub Main()
        ' Defines all 16 LEDs linked to two 74HC595 ICs in a chain
        Dim IcChain As Ic74hc595 = New Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 2)
        Led = IcChain.Pins

        ' Wait infinitely
        Thread.Sleep(Timeout.Infinite)
    End Sub

    Private Sub Button_StateChanged(sender As Object, e As Toolbox.NETMF.Hardware.AutoRepeatEventArgs) Handles Button.StateChanged
        ' We will only change when the button gets pressed
        If Button.Read() Then Exit Sub
        ' Toggles Red
        Red.Write(Not Red.Read())
        ' Makes green invert Red
        Green.Write(Not Red.Read())
    End Sub

    Private Sub Knob_Rotated(Unused As UInteger, Value As UInteger, time As Date) Handles Knob.Rotated
        ' Increase or decrease?
        If Value = 1 Then KnobValue = KnobValue + 1 Else KnobValue = KnobValue - 1

        ' Makes it actually go round ;-)
        If Value < 0 Then Value = 15
        If Value > 15 Then Value = 0

        ' Now we'll fill up led 0 to KnobValue to display the actual value
        For LedNo As Integer = 0 To Led.Length - 1
            If KnobValue >= LedNo Then Led(LedNo).Write(True) Else Led(LedNo).Write(False)
        Next
    End Sub
End Module
