Imports System
Imports Microsoft.SPOT
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
    Public Module Ntp

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
            Select Case Arguments(0).ToUpper()
                Case "NTPSYNC"
                    If Arguments.Length <> 2 Then
                        Throw New ArgumentException("Need 1 parameter, see HELP NTPSYNC for more info.")
                    Else
                        Ntp.Sync(Shell.Telnetserver, Arguments(1))
                    End If
                    SuspressError = True
                Case "HELP"
                    Dim PageFound As Boolean = False
                    If Arguments.Length = 1 Then
                        PageFound = Ntp.DoHelp(Shell.Telnetserver, "")
                    Else
                        PageFound = Ntp.DoHelp(Shell.Telnetserver, Arguments(1).ToUpper())
                    End If
                    If PageFound Then SuspressError = True
            End Select
        End Sub

        ''' <summary>
        ''' Shows a specific help page
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="Page">The page</param>
        ''' <returns>True when the page exists</returns>
        Private Function DoHelp(Server As TelnetServer, Page As String) As Boolean
            Select Case Page
                Case ""
                    Server.Print("NTPSYNC [HOSTNAME]                 Synchronizes with a timeserver")
                    Return True
                Case "NTPSYNC"
                    Server.Print("NTPSYNC [HOSTNAME]                 Synchronizes with a timeserver")
                    Server.Print("- [HOSTNAME]  The hostname of an NTP-server (example: pool.ntp.org)")
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Synchronizes with an NTP-server
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="NtpServer">Hostname of the NTP server</param>
        Private Sub Sync(Server As TelnetServer, NtpServer As String)
            Dim Client As SNTP_Client = New SNTP_Client(New IntegratedSocket(NtpServer, 123))
            Client.Synchronize()
            Server.Print("Current time: " + DateTime.Now.ToString())
        End Sub

    End Module
End Namespace
