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
    Public Module ShellCommands

        ''' <summary>
        ''' Binds to the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Bind(Shell As ShellCore)
            AddHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
            AddHandler Shell.OnConnected, AddressOf Shell_OnConnected
        End Sub

        ''' <summary>
        ''' Unbinds from the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Unbind(Shell As ShellCore)
            RemoveHandler Shell.OnConnected, AddressOf Shell_OnConnected
            RemoveHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
        End Sub

        ''' <summary>
        ''' Triggered when the connection has been made
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="RemoteAddress">Hostname of the user</param>
        ''' <param name="Time">Timestamp of the event</param>
        Private Sub Shell_OnConnected(Shell As ShellCore, RemoteAddress As String, Time As DateTime)
            Shell.Telnetserver.Color(TelnetServer.Colors.White, TelnetServer.Colors.Black)
            Shell.Telnetserver.ClearScreen()
            _SendMotd(Shell.Telnetserver)
        End Sub

        ''' <summary>
        ''' Sends the MOTD
        ''' </summary>
        Private Sub _SendMotd(Server As TelnetServer)
            Server.Color(TelnetServer.Colors.HighIntensityWhite)
            Server.Print("Welcome to the Netduino Telnet Server, " + Server.RemoteAddress)
            Server.Print("Copyright 2012-2014 by Stefan Thoolen (http://www.netmftoolbox.com/)")
            Server.Print("Type HELP to see a list of all supported commands")
            Server.Print("")
            Server.Color(TelnetServer.Colors.White)
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
                Case "CLS"
                    Shell.Telnetserver.ClearScreen()
                    SuspressError = True
                Case "MOTD"
                    _SendMotd(Shell.Telnetserver)
                    SuspressError = True
                Case "ECHO"
                    Shell.Telnetserver.Print(Shell.LastCommandline.Substring(5))
                    SuspressError = True
                Case "REBOOT"
                    Shell.Telnetserver.Print("Rebooting...")
                    Thread.Sleep(100)
                    Shell.Telnetserver.Close()
                    PowerState.RebootDevice(False)
                    SuspressError = True
                Case "QUIT"
                    Shell.Telnetserver.Print("Bye!")
                    Thread.Sleep(100)
                    Shell.Telnetserver.Close()
                    SuspressError = True
                Case "INFO"
                    Shell.Telnetserver.Print("Manufacturer: " + SystemInfo.OEMString)
                    Shell.Telnetserver.Print("Firmware version: " + SystemInfo.Version.ToString())
                    Shell.Telnetserver.Print("Memory available: " + Tools.MetricPrefix(Debug.GC(False), True) + "B")
                    If PowerState.Uptime.Days = 0 Then
                        Shell.Telnetserver.Print("Uptime: " + PowerState.Uptime.ToString())
                    Else
                        Shell.Telnetserver.Print("Uptime: " + PowerState.Uptime.Days.ToString() + " days, " + PowerState.Uptime.ToString())
                    End If
                    Shell.Telnetserver.Print("Hardware provider: " + Tools.HardwareProvider)
                    Shell.Telnetserver.Print("System clock: " + Tools.MetricPrefix(Cpu.SystemClock) + "hz")
                    If SystemInfo.IsBigEndian Then
                        Shell.Telnetserver.Print("Endianness: Big Endian")
                    Else
                        Shell.Telnetserver.Print("Endianness: Little Endian")
                    End If
                    If System.Diagnostics.Debugger.IsAttached Then
                        Shell.Telnetserver.Print("Debugger: attached")
                    Else
                        Shell.Telnetserver.Print("Debugger: not attached")
                    End If
                    SuspressError = True
                Case "VER"
                    Dim Assemblies() As System.Reflection.Assembly = AppDomain.CurrentDomain.GetAssemblies()
                    For i As Integer = 0 To Assemblies.Length - 1
                        Shell.Telnetserver.Print(Assemblies(i).FullName)
                    Next
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
        ''' Shows a specific help page
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="Page">The page</param>
        ''' <returns>True when the page exists</returns>
        Private Function DoHelp(Server As TelnetServer, Page As String) As Boolean
            Select Case Page.ToUpper()
                Case ""
                    Server.Print("CLS                                Clears the screen")
                    Server.Print("ECHO [TEXT]                        Prints out the text")
                    Server.Print("MOTD                               Shows the message of the day")
                    Server.Print("QUIT                               Closes the connection")
                    Server.Print("VER                                Shows the version of all loaded assemblies")
                    Server.Print("INFO                               Shows some system info")
                    Server.Print("REBOOT                             Restarts the device")
                    Return True
                Case "VER"
                    Server.Print("VER                                Shows the version of all loaded assemblies")
                    Return True
                Case "REBOOT"
                    Server.Print("REBOOT                             Restarts the device")
                    Return True
                Case "MOTD"
                    Server.Print("MOTD                               Shows the message of the day")
                    Return True
                Case "INFO"
                    Server.Print("INFO                               Shows some system info")
                    Return True
                Case "CLS"
                    Server.Print("CLS                                Clears the screen")
                    Return True
                Case "ECHO"
                    Server.Print("ECHO [TEXT]                        Prints out the text")
                    Server.Print("- [TEXT]  Text to print out")
                    Return True
                Case "QUIT"
                    Server.Print("QUIT                               Closes the connection")
                    Return True
                Case Else
                    Return False
            End Select
        End Function
    End Module
End Namespace
