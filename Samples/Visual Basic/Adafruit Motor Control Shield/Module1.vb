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
        ' This constructor works perfectly for the Netduino Classic and Plus,
        ' but if you're using a Netduino Classic or Plus, you can drive just two motors.
        ' So to keep the code working, I used null for Motor1Pwm and Motor2Pwm.
        Dim Shield As AdafruitMotorshield = New AdafruitMotorshield(
                                            ClockPin:=Pins.GPIO_PIN_D4,
                                            EnablePin:=Pins.GPIO_PIN_D7,
                                            DataPin:=Pins.GPIO_PIN_D8,
                                            LatchPin:=Pins.GPIO_PIN_D13,
                                            Motor1Pwm:=Nothing,
                                            Motor2Pwm:=Nothing,
                                            Motor3Pwm:=New Netduino.PWM(Pins.GPIO_PIN_D6),
                                            Motor4Pwm:=New Netduino.PWM(Pins.GPIO_PIN_D5)
        )

        Do
            For i As SByte = -100 To 100 Step 1
                Shield.SetState(AdafruitMotorshield.Motors.Motor4, i)
                Thread.Sleep(100)
            Next
            For i As SByte = 99 To -99 Step -1
                Shield.SetState(AdafruitMotorshield.Motors.Motor4, i)
                Thread.Sleep(100)
            Next
        Loop
    End Sub

End Module
