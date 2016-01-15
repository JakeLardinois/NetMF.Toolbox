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
Module JoystickShield

    Sub Main()
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ' !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G   W A R N I N G   W A R N I N G  !
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        ' ! The Joystick Shield analog ports are connected to 5V since it's originally designed for Arduino. !
        ' ! The Netduino analog inputs work on 3.3V. Herefor a modification to the shield is required.       !
        ' ! See http://netmftoolbox.codeplex.com/wikipage?title=Joystick%20Shield  for more details.         !
        ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        ' The thumb joystick is connected to analog pins 0 and 1, and digital pin 2
        ' On the Joystick shield, the thumb stick is rotated by 90° so we need to invert the horizontal value
        Dim Joystick As ThumbJoystick = New ThumbJoystick(New Netduino.ADC(Pins.GPIO_PIN_A0), New Netduino.ADC(Pins.GPIO_PIN_A1), Pins.GPIO_PIN_D2, True)

        ' The other buttons are connected to digital pins 3, 4, 5 and 6
        ' The shield doesn't have pull-up resistors so we use the internal ones from the Netduino
        Dim ButtonRight As InputPort = New InputPort(Pins.GPIO_PIN_D3, False, Port.ResistorMode.PullUp)
        Dim ButtonUp As InputPort = New InputPort(Pins.GPIO_PIN_D4, False, Port.ResistorMode.PullUp)
        Dim ButtonDown As InputPort = New InputPort(Pins.GPIO_PIN_D5, False, Port.ResistorMode.PullUp)
        Dim ButtonLeft As InputPort = New InputPort(Pins.GPIO_PIN_D6, False, Port.ResistorMode.PullUp)

        ' Infinite loop; so far Analog ports can't handle interrupts, so the Joystick driver can't as well
        Do
            ' Lets start with an empty string
            Dim OutputText As String = ""
            ' Add the values of the thumb stick
            OutputText = "Horizontal: " + Joystick.HorizontalValue.ToString()
            OutputText += " Vertical: " + Joystick.VerticalValue.ToString()
            If Joystick.PushValue Then OutputText += " Thumbstick pushed"

            ' Add button states to the output text (inverted with the NOT because of the pull-up resistor)
            If Not ButtonDown.Read() Then OutputText += " Button Down"
            If Not ButtonUp.Read() Then OutputText += " Button Up"
            If Not ButtonLeft.Read() Then OutputText += " Button Left"
            If Not ButtonRight.Read() Then OutputText += " Button Right"

            ' Displays the output text to the debug window
            Debug.Print(OutputText)

            ' Wait 100ms to read the pins again
            Thread.Sleep(100)
        Loop

    End Sub

End Module
