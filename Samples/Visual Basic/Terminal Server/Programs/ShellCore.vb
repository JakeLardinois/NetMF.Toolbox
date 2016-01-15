Imports System
Imports Microsoft.SPOT
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

    ''' <summary>
    ''' Shell core
    ''' </summary>
    Public Class ShellCore

        ''' <summary>
        ''' Reference to the TelnetServer object
        ''' </summary>
        Public Property Telnetserver As TelnetServer

        ''' <summary>
        ''' Command line prompt prefix
        ''' </summary>
        Public Property Prompt As String

        ''' <summary>
        ''' The last enterred command line
        ''' </summary>
        Public Property LastCommandline As String

        ''' <summary>
        ''' When running in the background, this thread is used
        ''' </summary>
        Private _BgThread As Thread

        ''' <summary>
        ''' Initializes a new Shell core
        ''' </summary>
        ''' <param name="Socket">Socket to use</param>
        Public Sub New(Socket As SimpleSocket)
            Me.Telnetserver = New TelnetServer(Socket)
            Me.Prompt = ">"
        End Sub

        ''' <summary>
        ''' Starts listening
        ''' </summary>
        Public Sub Start()
            Me.Start(False)
        End Sub

        ''' <summary>
        ''' Starts listening
        ''' </summary>
        ''' <param name="Background">Set to true to start the shell in the background</param>
        Public Sub Start(Background As Boolean)
            If Background Then
                ' Creates a background thread
                Dim BgThread As ThreadStart = New ThreadStart(AddressOf Me.Start)
                Me._BgThread = New Thread(BgThread)
                Me._BgThread.Start()
                Exit Sub
            End If

            Me.Telnetserver.Listen()
            RaiseEvent OnConnected(Me, Me.Telnetserver.RemoteAddress, DateTime.Now)

            Do While Me.Telnetserver.IsConnected
                Me.Telnetserver.Color(Telnetserver.Colors.Yellow)
                Me.Telnetserver.Print(Me.Prompt + " ", True)
                Me.Telnetserver.Color(Telnetserver.Colors.White)
                Dim Command As String = Me.Telnetserver.Input()
                If Command <> "" Then Me._ExecuteCommand(Command)
            Loop

            RaiseEvent OnDisconnected(Me, Me.Telnetserver.RemoteAddress, DateTime.Now)
        End Sub

        ''' <summary>
        ''' Executes a command
        ''' </summary>
        ''' <param name="CommandLine">The command line</param>
        Private Sub _ExecuteCommand(CommandLine As String)
            Me.LastCommandline = CommandLine

            ' Splits up the command line in arguments
            Dim Arguments() As String = CommandLine.Split(" ".ToCharArray())
            ' By default, we don't suspress the error "Command unknown"
            Dim SuspressError As Boolean = False
            ' Tries to execute the command
            Try
                RaiseEvent OnCommandReceived(Me, Arguments, SuspressError, Date.Now)
            Catch Ex As Exception
                ' If we fail executing the command, an error message will show up
                Me._PrintError("An exception has been triggered: " + Ex.Message)
                SuspressError = True
            End Try
            ' If SuspressError hasn't been set to True, we show the error "Command unknown"
            If Not SuspressError Then Me._PrintError("Command " + Arguments(0).ToUpper() + " unknown")
        End Sub

        ''' <summary>
        ''' // Prints out an error
        ''' </summary>
        ''' <param name="Text">The error to show</param>
        Private Sub _PrintError(Text As String)
            Me.Telnetserver.Color(Telnetserver.Colors.LightRed)
            Me.Telnetserver.Print(Text)
            Me.Telnetserver.Color(Telnetserver.Colors.White)
        End Sub

        ''' <summary>
        ''' Triggered when a command has been given
        ''' </summary>
        Public Event OnCommandReceived As CommandReceived

        ''' <summary>
        ''' Triggered when a command has been given
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="Arguments">Command line arguments</param>
        ''' <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        ''' <param name="Time">Current timestamp</param>
        Public Delegate Sub CommandReceived(Shell As ShellCore, Arguments() As String, ByRef SuspressError As Boolean, Time As DateTime)

        ''' <summary>
        ''' Triggered when the connection has been made
        ''' </summary>
        Public Event OnConnected As ConnectionStateChange
        ''' <summary>
        ''' Triggered when the connection has been lost
        ''' </summary>
        Public Event OnDisconnected As ConnectionStateChange

        ''' <summary>
        ''' Triggered when the connection state changes
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="RemoteAddress">Hostname of the user</param>
        ''' <param name="Time">Timestamp of the event</param>
        Public Delegate Sub ConnectionStateChange(Shell As ShellCore, RemoteAddress As String, Time As DateTime)

    End Class
End Namespace
