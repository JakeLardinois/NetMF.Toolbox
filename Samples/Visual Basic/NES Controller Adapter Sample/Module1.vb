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

    Dim WithEvents Controller As NESControllerAdapter = New NESControllerAdapter(Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3, Pins.GPIO_PIN_A4, Pins.GPIO_PIN_A5)

    Sub Main()
        Controller.EventsEnabled = True
        Thread.Sleep(Timeout.Infinite)
    End Sub

    Private Sub Controller_OnButtonChanged(This As Toolbox.NETMF.Hardware.NESControllerAdapter, Socket As Toolbox.NETMF.Hardware.NESControllerAdapter.Socket, Button As Toolbox.NETMF.Hardware.NESControllerAdapter.Button, Value As Boolean, Time As Date) Handles Controller.OnButtonChanged
        Debug.Print("Button " + Button.ToString() + " on Socket " + Socket.ToString() + " changed to " + Value.ToString())
    End Sub
End Module
