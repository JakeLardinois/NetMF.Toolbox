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

    ' In the global scope so we can call the leds from the events
    Public Leds(16) As IGPOPort

    Sub Main()
        ' Defining two 74HC165s daisychained on the SPI bus, pin 10 as latchpin
        Dim IcInChain As Ic74hc165 = New Ic74hc165(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 2)

        ' Defining two 74HC595s daisychained on the SPI bus, pin 9 as latchpin
        Dim IcOutChain As Ic74hc595 = New Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D9, 2)

        ' Defines all 16 leds
        Dim Counter As Integer
        For Counter = 0 To 15
            Leds(Counter) = IcOutChain.Pins(Counter)
        Next

        ' Defines all 16 buttons
        ' Because 'WithEvents' variables cannot be typed as arrays, we need to allocate the events a bit different in Visual Basic, so we use the AddHandler method
        Dim Buttons(16) As IIRQPort
        For Counter = 0 To 15
            Buttons(Counter) = IcInChain.Pins(Counter)
            AddHandler Buttons(Counter).OnStateChange, AddressOf Program_OnStateChange
            Buttons(Counter).ID = Counter.ToString()
        Next

        ' Enables interrupts
        IcInChain.EnableInterrupts()

        ' Wait infinite; let the events to their jobs
        Thread.Sleep(Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' Event triggered when a state changes
    ''' </summary>
    ''' <param name="PinObject">The pin that triggered the change</param>
    ''' <param name="State">The current value</param>
    ''' <param name="Time">Time and date of the event</param>
    Private Sub Program_OnStateChange(PinObject As IIRQPort, State As Boolean, Time As Date)
        Debug.Print("State changed of the 74HC165 port " + PinObject.ID.ToString() + " to " + State.ToString())
        Leds(CInt(PinObject.ID)).Write(State)
    End Sub

End Module
