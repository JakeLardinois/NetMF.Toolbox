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
Module MatrixKeypad_Sample

    ' Defines the keypad with events
    Dim WithEvents kb As MatrixKeyPad

    Sub Main()
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ' !   W A R N I N G    W A R N I N G     W A R N I N G     W A R N I N G    W A R N I N G   !
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ' ! There are a lot of different keypads with different pin sets.                           !
        ' ! See http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.MatrixKeyPad !
        ' ! for more about these keypads.                                                           !
		' ! The pins used below probably won't work on your specific keypad.                        !
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        ' Row pins. The keypad exists out of 4 rows.
        Dim RowPins(0 To 3) As Cpu.Pin
        RowPins(0) = Pins.GPIO_PIN_D4
        RowPins(1) = Pins.GPIO_PIN_D5
        RowPins(2) = Pins.GPIO_PIN_D6
        RowPins(3) = Pins.GPIO_PIN_D7
        ' Col pins. The keypad exists out of 3 columns.
        Dim ColPins(0 To 2) As Cpu.Pin
        ColPins(0) = Pins.GPIO_PIN_D1
        ColPins(1) = Pins.GPIO_PIN_D2
        ColPins(2) = Pins.GPIO_PIN_D3
        ' Initializes the new keypad
        kb = New MatrixKeyPad(RowPins, ColPins)

        ' Lets wait forever for events to occure
        Thread.Sleep(Timeout.Infinite)
    End Sub

    ''' <summary>
    ''' Triggered when a key is pressed
    ''' </summary>
    ''' <param name="KeyCode">The key code</param>
    ''' <param name="Unused">Not used</param>
    ''' <param name="time">Date and time of the event</param>
    Private Sub kb_OnKeyDown(ByVal KeyCode As UInteger, ByVal Unused As UInteger, ByVal time As Date) Handles kb.OnKeyDown
        Debug.Print("Key pressed: " + KeyCode.ToString())
    End Sub

    ''' <summary>
    ''' Triggered when a key is released
    ''' </summary>
    ''' <param name="KeyCode">The key code</param>
    ''' <param name="Unused">Not used</param>
    ''' <param name="time">Date and time of the event</param>
    Private Sub kb_OnKeyUp(ByVal KeyCode As UInteger, ByVal Unused As UInteger, ByVal time As Date) Handles kb.OnKeyUp
        Debug.Print("Key released: " + KeyCode.ToString())
    End Sub
End Module
