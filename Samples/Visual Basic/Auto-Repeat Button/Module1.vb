Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF.Hardware

'  Copyright 2011-2014 Mario Vernari (http://www.netmftoolbox.com/)
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

    ' Initializes the button
    Dim WithEvents Button As AutoRepeatInputPort = New AutoRepeatInputPort(Pins.ONBOARD_SW1, Port.ResistorMode.PullUp, True)

    Sub Main()
        ' Tells you what to do :-)
        Debug.Print("Press the onboard switch for a while")
    End Sub

    ''' <summary>
    ''' Triggered when a button is pressed, still pressed, or released
    ''' </summary>
    ''' <param name="Sender">The AutoRepeatInputPort object</param>
    ''' <param name="EventArgs">Event arguments</param>
    Private Sub Button_StateChanged(Sender As Object, EventArgs As Toolbox.NETMF.Hardware.AutoRepeatEventArgs) Handles Button.StateChanged
        Select Case EventArgs.State
            Case AutoRepeatInputPort.AutoRepeatState.Press
                Debug.Print("Pressed")
            Case AutoRepeatInputPort.AutoRepeatState.Release
                Debug.Print("Released")
            Case AutoRepeatInputPort.AutoRepeatState.Tick
                Debug.Print("Tick-Tock Goes the Clock")
        End Select
    End Sub
End Module
