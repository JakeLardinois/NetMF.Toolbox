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

    Sub Main()
        Dim Display As Hd44780Lcd = New Hd44780Lcd(
                                    Data4:=Pins.GPIO_PIN_D4,
                                    Data5:=Pins.GPIO_PIN_D5,
                                    Data6:=Pins.GPIO_PIN_D6,
                                    Data7:=Pins.GPIO_PIN_D7,
                                    ClockEnablePin:=Pins.GPIO_PIN_D8,
                                    RegisterSelectPin:=Pins.GPIO_PIN_D9
                                    )

        Display.ClearDisplay()
        Display.Write("Hello World!")
    End Sub

End Module
