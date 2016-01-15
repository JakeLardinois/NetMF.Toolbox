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
        Dim RTC As DS1307 = New DS1307()

        ' Comment this line out to set the time for the first time
        'RTC.SetTime(Year:=2012, Month:=5, Day:=27, Hour:=22, Minute:=52, Second:=0)

        ' Synchronises the Netduino with the DS1307 RTC module
        RTC.Synchronize()

        Do
            Debug.Print(DateTime.Now.ToString())
            Thread.Sleep(1000)
        Loop
    End Sub

End Module
