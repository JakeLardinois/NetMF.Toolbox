Imports System
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports Toolbox.NETMF
Imports Toolbox.NETMF.NET
Imports Microsoft.VisualBasic.Strings

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
Namespace Programs
    Public Module NetduinoPlus
        Private ONBOARD_SW1 As Cpu.Pin
        Private ONBOARD_LED As Cpu.Pin

        ''' <summary>
        ''' Binds to the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        ''' <param name="ONBOARD_SW1">Reference to the onboard button</param>
        ''' <param name="ONBOARD_LED">Reference to the onboard LED</param>
        Public Sub Bind(Shell As ShellCore, ONBOARD_SW1 As Cpu.Pin, ONBOARD_LED As Cpu.Pin)
            NetduinoPlus.ONBOARD_SW1 = ONBOARD_SW1
            NetduinoPlus.ONBOARD_LED = ONBOARD_LED
            AddHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
        End Sub

        ''' <summary>
        ''' Unbinds from the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Unbind(Shell As ShellCore)
            RemoveHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
        End Sub

        ''' <summary>
        ''' Triggered when a command has been given
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="Arguments">Command line arguments</param>
        ''' <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        ''' <param name="Time">Current timestamp</param>
        Private Sub Shell_OnCommandReceived(Shell As ShellCore, Arguments() As String, ByRef SuspressError As Boolean, Time As DateTime)
            If Arguments(0).ToUpper() = "NETDUINOPLUS" Then
                NetduinoPlus.Start(Shell.Telnetserver)
                SuspressError = True
            ElseIf Arguments(0).ToUpper() = "HELP" Then
                If Arguments.Length = 1 Then
                    Shell.Telnetserver.Print("NETDUINOPLUS                       An interactive Netduino Plus demo")
                    SuspressError = True
                ElseIf Arguments(1).ToUpper() = "COLORDEMO" Then
                    Shell.Telnetserver.Print("NETDUINOPLUS                       An interactive Netduino Plus demo")
                    SuspressError = True
                End If
            End If
        End Sub

        ''' <summary>
        ''' Actually starts the demo
        ''' </summary>
        ''' <param name="Server">Reference to the Telnet Server</param>
        Private Sub Start(Server As TelnetServer)
            ' Configures the onboard button. If this fails, it's probably already in use by another app.
            Dim Button As InputPort = Nothing
            Try
                Button = New InputPort(ONBOARD_SW1, False, Port.ResistorMode.Disabled)
            Catch e As Exception
                Server.Color(TelnetServer.Colors.LightRed)
                Server.Print("Exception " + e.Message + " given. Were the onboard Button already configured?")
                Server.Color(TelnetServer.Colors.White)
                Exit Sub
            End Try

            ' Configures the onboard LED. If this fails, it's probably already in use by another app.
            Dim Led As OutputPort = Nothing
            Try
                Led = New OutputPort(ONBOARD_LED, False)
            Catch e As Exception
                Button.Dispose() ' The button is already defined
                Server.Color(TelnetServer.Colors.LightRed)
                Server.Print("Exception " + e.Message + " given. Were the onboard LED already configured?")
                Server.Color(TelnetServer.Colors.White)
                Exit Sub
            End Try

            ' Disables echoing of keypresses
            Server.EchoEnabled = False
            ' Clears the screen
            Server.ClearScreen()

            ' Draws a Netduino Plus in ANSI/ASCII art
            Server.Print(VbUnEscape("\xda\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xbf"))
            Server.Print(VbUnEscape("\xb3             \xfe\xfe\xfe\xfe\xfe\xfe\xfe\xfe \xfe\xfe\xfe\xfe\xfe\xfe\xfe\xfe\xb3"))
            Server.Print(VbUnEscape("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb                   \xdc  \xb3"))
            Server.Print(VbUnEscape("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb             NETDUINO \xb3"))
            Server.Print(VbUnEscape("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb               PLUS   \xb3"))
            Server.Print(VbUnEscape("\xb3                            ..\xb3"))
            Server.Print(VbUnEscape("\xdb\xdb                       \xdb\xdb  ::\xb3"))
            Server.Print(VbUnEscape("\xb3       \xdb\xdb\xdb\xdb\xdb\xdb                 \xb3"))
            Server.Print(VbUnEscape("\xdb\xdb\xdb\xdb\xdb   \xdb\xdb\xdb\xdb\xdb\xdb                 \xb3"))
            Server.Print(VbUnEscape("\xdb\xdb\xdb\xdb\xdb   \xdb\xdb\xdb\xdb\xdb\xdb    \xfe\xfe\xfe\xfe\xfe\xfe \xfe\xfe\xfe\xfe\xfe\xfe\xb3"))
            Server.Print(VbUnEscape("\xc0\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xdb\xdb\xdb\xdb\xdb\xdb\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xd9"))
            Server.Print("", True)
            Server.Print("Push the Netduino Plus onboard button, or press a key to close this app")

            ' We only update the screen if the LastState is different from the current state
            Dim LastState As Boolean = True
            Do While Server.IsConnected And Server.Input(1, False) = ""
                ' We need to update
                If Button.Read() <> LastState Then
                    ' Lets record the last state
                    LastState = Button.Read()
                    Led.Write(LastState)

                    ' Draws the button
                    If LastState Then
                        Server.Color(TelnetServer.Colors.HighIntensityWhite)
                    Else
                        Server.Color(TelnetServer.Colors.Gray)
                    End If
                    Server.Locate(7, 26, True) : Server.Print(VbUnEscape("\xdb\xdb"), True, True)

                    ' Draws the LED
                    If LastState Then
                        Server.Color(TelnetServer.Colors.LightBlue)
                    Else
                        Server.Color(TelnetServer.Colors.Gray)
                    End If
                    Server.Locate(3, 29, True) : Server.Print(VbUnEscape("\xdc"), True)

                    ' Brings back the cursor to the last line
                    Server.Locate(14, 1)
                End If
            Loop

            ' Releases the pins
            Button.Dispose()
            Led.Dispose()

            ' Enables echo again and clears the screen
            If Server.IsConnected Then
                Server.EchoEnabled = True
                Server.ClearScreen()
            End If
        End Sub

        Private Function VbUnEscape(Text As String) As String
            Do
                Dim Pos As Integer = Text.IndexOf("\x")
                If Pos < 0 Then Exit Do
                Dim Value As String = Text.Substring(Pos, 4)
                Text = Text.Substring(0, Pos) + ChrW(CInt(Tools.Hex2Dec(Value.Substring(2)))) + Text.Substring(Pos + 4)
            Loop
            VbUnEscape = Text
        End Function

    End Module
End Namespace
