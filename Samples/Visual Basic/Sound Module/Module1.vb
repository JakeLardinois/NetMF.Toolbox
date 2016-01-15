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
    '
    ' This sample uses 5 files on the SD-card. They can be found in netmftoolbox\Samples\Visual C#\Sound Module\samples
    '
    Const EXTERMINATE As UShort = 0
    Const WAAAAAAAAAAAA As UShort = 1
    Const YOUMAYNOTLEAVE As UShort = 2
    Const TARDIS As UShort = 3
    Const SONIC As UShort = 4

    Sub Main()
        ' The module is connected to these pins
        Dim SoundModule As Somo = New Somo(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2)

        ' Sets the volume fully open
        SoundModule.SetVolume(7)

        ' Plays "Exterminate!"
        Debug.Print("EXTERMINATE")
        SoundModule.PlayTrack(EXTERMINATE, True)

        ' Plays the Tardis-sound
        Debug.Print("TARDIS")
        SoundModule.PlayTrack(TARDIS, True)

        ' Plays "You may not leave my precence!"
        Debug.Print("YOUMAYNOTLEAVE")
        SoundModule.PlayTrack(YOUMAYNOTLEAVE, True)

        ' Plays the sonic screwdriver sound
        Debug.Print("SONIC")
        SoundModule.PlayTrack(SONIC, True)

        ' Repeatedly play "Waaaaaaaaa!"
        Debug.Print("WAAAAAAAAAAAA (repeated)")
        SoundModule.PlayRepeat(WAAAAAAAAAAAA)

        ' Let this go on infinitely
        Thread.Sleep(Timeout.Infinite)
    End Sub

    End Module
