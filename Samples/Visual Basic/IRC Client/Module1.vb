Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.NetduinoPlus
Imports Toolbox.NETMF.NET

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

    ' Channel for our nice IRC-bot
    Public Const Channel As String = "#netduino"

    ' The IRC Client
    Public WithEvents Client As IRC_Client

    ' The onboard LED
    Public Led As OutputPort = New OutputPort(Pins.ONBOARD_LED, False)

    ' The onboard Switch
    Public WithEvents Button As InterruptPort = New InterruptPort(Pins.ONBOARD_SW1, True, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth)

    ' Main method, makes the connection
    Sub Main()
        Client = New IRC_Client(New IntegratedSocket("irc.netmftoolbox.com", 6667), "Guest")
        Client.Connect()
    End Sub

    ' Button state changed
    Private Sub Button_OnInterrupt(PinId As UInteger, Value As UInteger, Time As Date) Handles Button.OnInterrupt
        If Not Client.Authenticated Then Exit Sub
        If Value = 1 Then
            Client.Message(Channel, "Oh thank goodness, the button is released.")
        Else
            Client.Message(Channel, "Oh dear, someone pressed my button!")
        End If
    End Sub

    ' We're connected, lets initialize our client further
    Private Sub Client_OnAuthenticated(Sender As String, Target As String, Data As String) Handles Client.OnAuthenticated
        Debug.Print("Successfully connected to " + Sender + " as " + Target)
        Debug.Print(Data)
        Client.Join(Channel)
        Client.Message(Channel, "Hello, it's me, running " + Client.ClientVersion)
        Client.Message(Channel, "Type ""LED [ON/OFF/STATUS]"" to play with my led :-)")
    End Sub

    ' We got a message. If it's a channel message, we're going to respond to it
    Private Sub Client_OnMessage(Sender As String, Target As String, Data As String) Handles Client.OnMessage
        If Target.ToLower() = Channel Then
            Debug.Print(IRC_Client.SplitName(Sender)(0) + ": " + Data)

            Dim Commandline() As String = Data.ToUpper().Split(" ".ToCharArray(), 2)
            If Commandline(0) = "LED" And Commandline(1) = "ON" Then
                Led.Write(True)
                Client.Message(Channel, "LED just turned ON")
            ElseIf Commandline(0) = "LED" And Commandline(1) = "OFF" Then
                Led.Write(False)
                Client.Message(Channel, "LED just turned OFF")
            ElseIf Commandline(0) = "LED" And Commandline(1) = "STATUS" Then
                If Led.Read() Then
                    Client.Message(Channel, "The LED is currently ON")
                Else
                    Client.Message(Channel, "The LED is currently OFF")
                End If
            End If

        End If
    End Sub
End Module
