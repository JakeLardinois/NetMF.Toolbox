Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino

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

    Sub Main()

        ' The EL Escudo Dos Shield has EL-wire sockets connected to pins D2 to D9
        Dim Channels() As OutputPort = New OutputPort() {
            New OutputPort(Pins.GPIO_PIN_D2, False),
            New OutputPort(Pins.GPIO_PIN_D3, False),
            New OutputPort(Pins.GPIO_PIN_D4, False),
            New OutputPort(Pins.GPIO_PIN_D5, False),
            New OutputPort(Pins.GPIO_PIN_D6, False),
            New OutputPort(Pins.GPIO_PIN_D7, False),
            New OutputPort(Pins.GPIO_PIN_D8, False),
            New OutputPort(Pins.GPIO_PIN_D9, False)
        }
        ' The EL Escudo Dos Shield has a status LED connected to pin D10
        Dim StatusLED As OutputPort = New OutputPort(Pins.GPIO_PIN_D10, True)

        Do
            For Wire As Integer = 0 To Channels.Length - 1
                Channels(Wire).Write(True)
                Thread.Sleep(500)
                Channels(Wire).Write(False)
            Next
        Loop

    End Sub

End Module
