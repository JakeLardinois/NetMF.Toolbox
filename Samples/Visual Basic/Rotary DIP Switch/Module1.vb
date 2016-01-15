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
    Public WithEvents Switch As RotaryDIPSwitch

    Sub Main()
        ' The switch is connected to pins 1, 2, 4 and 8, so a 4-bit switch (define them in the correct sequence)
        Dim RotaryPins(0 To 3) As Cpu.Pin
        RotaryPins(0) = Pins.GPIO_PIN_D1
        RotaryPins(1) = Pins.GPIO_PIN_D2
        RotaryPins(2) = Pins.GPIO_PIN_D4
        RotaryPins(3) = Pins.GPIO_PIN_D8
        Switch = New RotaryDIPSwitch(RotaryPins)

        ' Let the events do the rest
        Thread.Sleep(Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' The switch' state is changed
    ''' </summary>
    ''' <param name="Unused">Unused</param>
    ''' <param name="State">New state</param>
    ''' <param name="Time">Time of the event</param>
    Private Sub Switch_OnInterrupt(Unused As UInteger, State As UInteger, Time As Date) Handles Switch.OnInterrupt
        Debug.Print(State.ToString())
    End Sub
End Module
