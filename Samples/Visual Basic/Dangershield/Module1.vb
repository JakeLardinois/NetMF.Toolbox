Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF
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

    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G   W A R N I N G   W A R N I N G  !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' ! The Dangershield analog ports are connected to 5V since it's originally designed for Arduino.    !
    ' ! The Netduino analog inputs work on 3.3V. Herefor a modification to the shield is required.       !
    ' ! See http://netmftoolbox.codeplex.com/wikipage?title=Danger%20Shield for more details.            !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' ! This sample has two states:                                                                      !
    ' !                                                                                                  !
    ' ! State 1 (press button 1 to activate): The three potentiometers are linked to the two leds and    !
    ' !                                       7-segment display                                          !
    ' ! State 2 (press button 2 to activate): The photocell, knock sensor and temperature sensor are     !
    ' !                                       linked to the two leds and 7-segment display               !
    ' ! If you hold button 3, the buzzer will annoy you                                                  !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    Sub Main()
        ' Outputs:
        ' - Buzzer (GPIO D3)
        ' - Segment7 (7-Segment display/74HC595 IC) (SPI D4, D7, D8)
        ' - Led1 (PWM D5)
        ' - Led2 (PWM D6)

        ' The buzzer is connected directly to GPIO pin D3
        Dim Buzzer As BitBangBuzzer = New BitBangBuzzer(Pins.GPIO_PIN_D3)

        ' The 7-segment display is connected with a 74HC595 bitshift IC over GPIO pins D4 (MOSI), D7 (CS) and D8 (SCLK)
        Dim Mux As Ic74hc595 = New Ic74hc595(Pins.GPIO_PIN_D8, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D7)
        Dim Segment7 As SevenSegment = New SevenSegment(Mux.CreateParallelOut())

        ' The DangerShield has the digits defined differently; these bits are used:
        '    Top = 1
        '    UpperRight = 2
        '    LowerRight = 3
        '    Bottom = 4
        '    LowerLeft = 5
        '    UpperLeft = 6
        '    Middle = 7
        '    Dot = 8                                                                       (87654321)
        Segment7.ChangeSignal(0, CByte(Tools.Hex2Dec("3f"))) ' 0 brights up: 0 1 2 3 4 5   (00111111)
        Segment7.ChangeSignal(1, CByte(Tools.Hex2Dec("06"))) ' 1 brights up: 1 2           (00000110)
        Segment7.ChangeSignal(2, CByte(Tools.Hex2Dec("5b"))) ' 2 brights up: 0 1 3 4 6     (01011011)
        Segment7.ChangeSignal(3, CByte(Tools.Hex2Dec("4f"))) ' 3 brights up: 0 1 2 3 6     (01001111)
        Segment7.ChangeSignal(4, CByte(Tools.Hex2Dec("66"))) ' 4 brights up: 1 2 5 6       (01100110)
        Segment7.ChangeSignal(5, CByte(Tools.Hex2Dec("6d"))) ' 5 brights up: 0 2 3 5 6     (01101101)
        Segment7.ChangeSignal(6, CByte(Tools.Hex2Dec("7d"))) ' 6 brights up: 0 2 3 4 5 6   (01111101)
        Segment7.ChangeSignal(7, CByte(Tools.Hex2Dec("07"))) ' 7 brights up: 0 1 2         (00000111)
        Segment7.ChangeSignal(8, CByte(Tools.Hex2Dec("7f"))) ' 8 brights up: 0 1 2 3 4 5 6 (01111111)
        Segment7.ChangeSignal(9, CByte(Tools.Hex2Dec("6f"))) ' 9 brights up: 0 1 2 3 5 6   (01101111)
        Segment7.ChangeDotSignal(8)

        ' Both leds
        Dim Led1 As IPWMPort = New Netduino.PWM(Pins.GPIO_PIN_D5)
        Led1.StartPulse()
        Dim Led2 As IPWMPort = New Netduino.PWM(Pins.GPIO_PIN_D6)
        Led2.StartPulse()

        ' Inputs:
        ' - PotentioMeter1 (ADC A0)
        ' - PotentioMeter2 (ADC A1)
        ' - PotentioMeter3 (ADC A2)
        ' - Photocell (ADC A3)
        ' - TemperatureSensor (ADC A4)
        ' - KnockSensor (ADC A5)
        ' - PushButton1 (GPIO D10)
        ' - PushButton2 (GPIO D11)
        ' - PushButton3 (GPIO D12)

        ' Potentio meters
        Dim PotentioMeter1 As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A0)
        PotentioMeter1.RangeSet(0, 100) ' Same range as Led1.SetDutyCycle()
        Dim PotentioMeter2 As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A1)
        PotentioMeter2.RangeSet(0, 100) ' Same range as Led2.SetDutyCycle()
        Dim PotentioMeter3 As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A2)
        PotentioMeter3.RangeSet(0, 9)   ' Same range as Segment7.SetDigit()
        ' Photocell
        Dim Photocell As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A3)
        Photocell.RangeSet(0, 100) ' Same range as Led2.SetDutyCycle()
        ' Temperature Sensor
        Dim TemperatureSensor As Tmp36 = New Tmp36(New Netduino.ADC(Pins.GPIO_PIN_A4))
        ' Knock Sensor
        Dim KnockSensor As IADCPort = New Netduino.ADC(Pins.GPIO_PIN_A5)
        KnockSensor.RangeSet(0, 200) ' Bigger range as Led1.SetDutyCycle() but you really need to smash hard to reach this value
        ' Push buttons
        Dim PushButton1 As InputPort = New InputPort(Pins.GPIO_PIN_D10, False, Port.ResistorMode.Disabled)
        Dim PushButton2 As InputPort = New InputPort(Pins.GPIO_PIN_D11, False, Port.ResistorMode.Disabled)
        Dim PushButton3 As InputPort = New InputPort(Pins.GPIO_PIN_D12, False, Port.ResistorMode.Disabled)

        ' This value contains which demo is currently active
        Dim Demo As Integer = 1

        ' Contains the last second, so we can switch between two numbers on the 7-segment display (to display the temperature)
        Dim LastSecond As Integer = Utility.GetMachineTime().Seconds

        ' This digit should be currently shown
        Dim ShowSecondDigit As Boolean = False

        ' Infinite loop
        Do
            ' Switches the demo, when required (NOT statement because of the pullup resistors)
            If Not PushButton1.Read() Then Demo = 1
            If Not PushButton2.Read() Then Demo = 2

            If Demo = 1 Then
                ' First demo is currently active
                Led1.SetDutyCycle(CUInt(PotentioMeter1.RangeRead()))
                Led2.SetDutyCycle(CUInt(PotentioMeter2.RangeRead()))
                Segment7.SetDigit(CByte(PotentioMeter3.RangeRead()))
            Else
                ' Second demo is currently active
                Dim Knocking As UInteger = CUInt(KnockSensor.RangeRead())
                If Knocking > 100 Then Led1.SetDutyCycle(100) Else Led1.SetDutyCycle(Knocking)
                Led2.SetDutyCycle(CUInt(Photocell.RangeRead()))

                ' Okay, we want two temperature digits seprated, for display purposes
                Dim Temp As Double = TemperatureSensor.Temperature
                Dim Digit1 As Byte = CByteDown(Temp / 10)
                Dim Digit0 As Byte = CByteDown(Temp - (10 * Digit1))

                ' Switch the digit to be displayed
                If LastSecond <> Utility.GetMachineTime().Seconds Then
                    LastSecond = Utility.GetMachineTime().Seconds
                    If ShowSecondDigit Then Segment7.SetDigit(Digit1) Else Segment7.SetDigit(Digit0)
                    Segment7.SetDot(Not ShowSecondDigit)
                    ShowSecondDigit = Not ShowSecondDigit
                End If
            End If

            ' Links the buzzer to the 3rd pushbutton's value
            Buzzer.Write(Not PushButton3.Read())
        Loop

    End Sub

    ' Because CByte rounds the value both up and down, things can go wrong
    ' This function only rounds down
    Public Function CByteDown(ByRef Value As Double) As Byte
        Dim NewValue As Byte = CByte(Value)
        If NewValue > Value Then NewValue = CByte(NewValue - 1)
        CByteDown = NewValue
    End Function

End Module
