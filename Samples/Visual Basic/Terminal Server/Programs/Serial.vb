Imports System
Imports System.IO.Ports ' Microsoft.SPOT.Hardware.SerialPort.dll
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
    Public Module Serial
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
            Select Arguments(0).ToUpper()
                Case "SERIAL"
                    If Arguments.Length = 1 Then
                        Throw New ArgumentException("COM-port required, see HELP SERIAL for more.")
                    ElseIf Arguments.Length = 2 Then
                        Serial.Start(Shell.Telnetserver, Arguments(1))
                    ElseIf Arguments.Length = 3 Then
                        Serial.Start(Shell.Telnetserver, Arguments(1), Arguments(2))
                    ElseIf Arguments.Length = 4 Then
                        Serial.Start(Shell.Telnetserver, Arguments(1), Arguments(2), Arguments(3))
                    ElseIf Arguments.Length = 5 Then
                        Serial.Start(Shell.Telnetserver, Arguments(1), Arguments(2), Arguments(3), Arguments(4))
                    Else
                        Serial.Start(Shell.Telnetserver, Arguments(1), Arguments(2), Arguments(3), Arguments(4), Arguments(5))
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
        ''' Shows a specific help page
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="Page">The page</param>
        ''' <returns>True when the page exists</returns>
        Private Function DoHelp(Server As TelnetServer, Page As String) As Boolean
            Select Case Page.ToUpper()
                Case ""
                    Server.Print("SERIAL [COM]                       Opens a serial connection")
                    Return True
                Case "SERIAL"
                    Server.Print("SERIAL [COM]                       Opens a serial connection")
                    Server.Print("- [COM]       The COM-port to connect to")
                    Server.Print("Additional parameters:")
                    Server.Print("- [BAUD]      Connection speed (default: 9600)")
                    Server.Print("- [PARITY]    Parity (default: none)")
                    Server.Print("- [DATABITS]  The amount of databits (default: 8)")
                    Server.Print("- [STOPBITS]  The amount of stopbits (default: 1)")
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' Starts the serial client
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Com">COM-port</param>
        ''' <param name="Baud">Baudrate</param>
        ''' <param name="SParity">Parity</param>
        ''' <param name="Databits">Databits</param>
        ''' <param name="Stopbits">Stopbits</param>
        Private Sub Start(Server As TelnetServer, Com As String, Optional Baud As String = "9600", Optional SParity As String = "NONE", Optional Databits As String = "8", Optional Stopbits As String = "1")
            ' Parses parity
            Dim PParity As Parity
            Select Case SParity.ToUpper()
                Case "EVEN" : PParity = Parity.Even
                Case "MARK" : PParity = Parity.Mark
                Case "NONE" : PParity = Parity.None
                Case "ODD" : PParity = Parity.Odd
                Case "SPACE" : PParity = Parity.Space
                Case Else : Throw New ArgumentException("Parity " + SParity + " unknown. Known values are EVEN, MARK, NONE, ODD, SPACE")
            End Select

            ' Parses Stopbits
            Dim SStop As StopBits
            Select Case Stopbits.ToUpper()
                Case "0" : SStop = IO.Ports.StopBits.None
                Case "1" : SStop = IO.Ports.StopBits.One
                Case "1.5" : SStop = IO.Ports.StopBits.OnePointFive
                Case "2" : SStop = IO.Ports.StopBits.Two
                Case Else : Throw New ArgumentException("Stopbits " + Stopbits + " unknown. Known values are 0, 1, 1.5, 2")
            End Select

            ' Configures the serial port
            Dim Port As SerialPort = New SerialPort(
                portName:=Com.ToUpper(),
                BaudRate:=CInt(Baud),
                Parity:=PParity,
                Databits:=CInt(Databits),
                Stopbits:=SStop
            )

            Server.Print("Connecting to " + Com + "...")
            Port.DiscardInBuffer()
            Port.Open()
            Server.Print("Connected. Press Ctrl+C to close the connection")

            Server.EchoEnabled = False
            Do
                Dim Data() As Byte = Tools.Chars2Bytes(Server.Input(1, False).ToCharArray())
                If Data.Length > 0 Then
                    If Data(0) = 3 Then Exit Do ' Ctrl+C
                    Port.Write(Data, 0, Data.Length)
                End If
                If Port.BytesToRead > 0 Then
                    Dim Buffer(Port.BytesToRead) As Byte
                    Port.Read(Buffer, 0, Buffer.Length)
                    Server.Print(New String(Tools.Bytes2Chars(Buffer)), True)
                End If
            Loop
            Server.EchoEnabled = True

            Port.Close()
            Server.Print("", False, True)
            Server.Print("Connection closed")
        End Sub

    End Module
End Namespace
