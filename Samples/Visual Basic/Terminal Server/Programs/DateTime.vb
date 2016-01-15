Imports System
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
    Public Module Time

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
                Case "TIME"
                    If Arguments.Length = 1 Then
                        Shell.Telnetserver.Print("Current time: " + DisplayTime())
                    Else
                        ChangeTime(Shell.Telnetserver, Arguments(1))
                    End If
                    SuspressError = True
                Case "DATE"
                    If Arguments.Length = 1 Then
                        Shell.Telnetserver.Print("Current date: " + DisplayDate())
                    Else
                        ChangeDate(Shell.Telnetserver, Arguments(1))
                    End If
                    SuspressError = True
                Case "HELP"
                    Dim PageFound As Boolean = False
                    If Arguments.Length = 1 Then
                        PageFound = DoHelp(Shell.Telnetserver, "")
                    Else
                        PageFound = DoHelp(Shell.Telnetserver, Arguments(1).ToUpper())
                    End If
                    If PageFound Then SuspressError = True
            End Select
        End Sub

        ''' <summary>
        ''' Changes the current time
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Time">New time</param>
        Private Sub ChangeTime(Server As TelnetServer, Time As String)
            ' Splits up time parts
            Dim Parts() As String = Time.Split(":".ToCharArray())
            If Parts.Length <> 3 Then Throw New ArgumentException("Invalid TIME format. See HELP TIME for more.")
            ' Validates data
            Dim Hours As Integer = CInt(Parts(0))
            Dim Minutes As Integer = CInt(Parts(1))
            Dim Seconds As Integer = CInt(Parts(2))
            If Hours > 23 Or Hours < 0 Then Throw New ArgumentException("Invalid TIME format. See HELP TIME for more.")
            If Minutes > 59 Or Minutes < 0 Then Throw New ArgumentException("Invalid TIME format. See HELP TIME for more.")
            If Seconds > 59 Or Seconds < 0 Then Throw New ArgumentException("Invalid TIME format. See HELP TIME for more.")

            Dim NewTime As DateTime = New DateTime(
                year:=DateTime.Now.Year,
                month:=DateTime.Now.Month,
                day:=DateTime.Now.Day,
                hour:=Hours,
                minute:=Minutes,
                second:=Seconds
            )
            Utility.SetLocalTime(NewTime)

            Server.Print("New time: " + DisplayTime())
        End Sub

        ''' <summary>
        ''' Changes the current date
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="SDate">New date</param>
        Private Sub ChangeDate(Server As TelnetServer, SDate As String)
            ' Splits up date parts
            Dim Parts() As String = SDate.Split("-".ToCharArray())
            If Parts.Length <> 3 Then Throw New ArgumentException("Invalid TIME format. See HELP DATE for more.")
            ' Validates data
            Dim Years As Integer = CInt(Parts(0))
            Dim Months As Integer = CInt(Parts(1))
            Dim Days As Integer = CInt(Parts(2))
            If Years < 2000 Or Years > 3000 Then Throw New ArgumentException("Invalid DATE format. See HELP DATE for more.")
            If Months > 12 Or Months < 1 Then Throw New ArgumentException("Invalid DATE format. See HELP DATE for more.")
            If Days > 31 Or Days < 1 Then Throw New ArgumentException("Invalid DATE format. See HELP DATE for more.")

            Dim NewTime As DateTime = New DateTime(
                year:=Years,
                month:=Months,
                day:=Days,
                hour:=DateTime.Now.Hour,
                minute:=DateTime.Now.Minute,
                second:=DateTime.Now.Second
            )
            Utility.SetLocalTime(NewTime)

            Server.Print("New date: " + DisplayDate())
        End Sub

        ''' <summary>
        ''' Returns the time
        ''' </summary>
        ''' <returns>Time in hh:mm:ss format</returns>
        Private Function DisplayTime() As String
            Return Tools.ZeroFill(DateTime.Now.Hour, 2) + ":" + Tools.ZeroFill(DateTime.Now.Minute, 2) + ":" + Tools.ZeroFill(DateTime.Now.Second, 2)
        End Function

        ''' <summary>
        ''' Returns the date
        ''' </summary>
        ''' <returns>Date in yyyy:mm:dd format</returns>
        Private Function DisplayDate() As String
            Return DateTime.Now.Year.ToString() + "-" + Tools.ZeroFill(DateTime.Now.Month, 2) + "-" + Tools.ZeroFill(DateTime.Now.Day, 2)
        End Function

        ''' <summary>
        ''' Shows a specific help page
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="Page">The page</param>
        ''' <returns>True when the page exists</returns>
        Private Function DoHelp(Server As TelnetServer, Page As String) As Boolean
            Select Case Page.ToUpper()
                Case ""
                    Server.Print("TIME [TIME]                        Displays or changes the current time")
                    Server.Print("DATE [DATE]                        Displays or changes the current date")
                    Return True
                Case "TIME"
                    Server.Print("TIME [TIME]                        Displays or changes the current time")
                    Server.Print("- [TIME]  If set, will change the time (Format: hh:mm:ss)")
                    Return True
                Case "DATE"
                    Server.Print("DATE [DATE]                        Displays or changes the current date")
                    Server.Print("- [DATE]  If set, will change the date (Format: yyyy-mm-dd)")
                    Return True
                Case Else
                    Return False
            End Select
        End Function
    End Module
End Namespace
