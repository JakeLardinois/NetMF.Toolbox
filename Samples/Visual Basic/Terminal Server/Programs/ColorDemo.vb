Imports System
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports Toolbox.NETMF
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
Namespace Programs
    Public Module ColorDemo

        ''' <summary>
        ''' Binds to the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Bind(Shell As ShellCore)
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
            If Arguments(0).ToUpper() = "COLORDEMO" Then
                ColorDemo.Start(Shell.Telnetserver)
                SuspressError = True
            ElseIf Arguments(0).ToUpper() = "HELP" Then
                If Arguments.Length = 1 Then
                    Shell.Telnetserver.Print("COLORDEMO                          Shows an ANSI Color demo")
                    SuspressError = True
                ElseIf Arguments(1).ToUpper() = "COLORDEMO" Then
                    Shell.Telnetserver.Print("COLORDEMO                          Shows an ANSI Color demo")
                    SuspressError = True
                End If
            End If
        End Sub

        ''' <summary>
        ''' Actually starts the demo
        ''' </summary>
        ''' <param name="Server">Reference to the Telnet Server</param>
        Private Sub Start(Server As TelnetServer)
            For f As Integer = 0 To 15
                For b As Integer = 0 To 7
                    Server.Color(CType(f, TelnetServer.Colors), CType(b, TelnetServer.Colors))
                    Server.Print(" " + Tools.ZeroFill(f, 2) + "," + Tools.ZeroFill(b, 2) + " ", True, True)
                Next
                Server.Color(TelnetServer.Colors.White, TelnetServer.Colors.Black)
                Server.Print("")
            Next
        End Sub

    End Module
End Namespace
