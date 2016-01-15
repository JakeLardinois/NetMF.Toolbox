Imports System
Imports Microsoft.SPOT

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
    Public Class Auth
        ''' <summary>
        ''' Stores the last username
        ''' </summary>
        Private _Username As String = ""

        ''' <summary>
        ''' Set to true when we logged in
        ''' </summary>
        Private _Authorized As Boolean = False

        ''' <summary>
        ''' Amount of password retries (default: 3)
        ''' </summary>
        Public Property Retries As Integer

        ''' <summary>
        ''' The shell
        ''' </summary>
        Private _Shell As ShellCore

        ''' <summary>
        ''' Creates a new authorization module
        ''' </summary>
        ''' <param name="Shell">Shell to bind to</param>
        Public Sub New(Shell As ShellCore)
            ' Sets some default values
            Me.Retries = 3
            Me.TimeToLogin = 0
            Me._Shell = Shell

            ' Binds to the shell
            AddHandler Shell.OnConnected, AddressOf Me._Shell_OnConnected
            AddHandler Shell.OnCommandReceived, AddressOf Me._Shell_OnCommandReceived
        End Sub

        ''' <summary>
        ''' Unbinds the shell
        ''' </summary>
        Public Sub Dispose()
            ' Unbinds from the shell
            RemoveHandler Me._Shell.OnCommandReceived, AddressOf Me._Shell_OnCommandReceived
            RemoveHandler Me._Shell.OnConnected, AddressOf Me._Shell_OnConnected
        End Sub

        ''' <summary>
        ''' Time to login in seconds (default: 0)
        ''' </summary>
        Public Property TimeToLogin As Integer

        ''' <summary>
        ''' Triggered when the connection has been made
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="RemoteAddress">Hostname of the user</param>
        ''' <param name="Time">Timestamp of the event</param>
        Private Sub _Shell_OnConnected(Shell As Programs.ShellCore, RemoteAddress As String, Time As Date)
            ' Resets the current login
            Me._Authorized = False
            Me._Username = ""

            ' Adds the timer to check for a login timeout
            If Me.TimeToLogin > 0 Then
                Dim t = New Timer(AddressOf Me._LoginTimedout, Nothing, Me.TimeToLogin * 1000, 0)
            End If

            For i As Integer = 0 To Me.Retries - 1
                ' Asks for the username
                Shell.Telnetserver.Print("Username: ", True)
                Dim Username As String = Shell.Telnetserver.Input().Trim()
                If Username = "" Then Continue For

                ' Asks for the password (with echoing disabled!)
                Shell.Telnetserver.Print("Password: ", True)
                Shell.Telnetserver.EchoEnabled = False
                Dim Password As String = Shell.Telnetserver.Input().Trim()
                Shell.Telnetserver.EchoEnabled = True
                Shell.Telnetserver.Print("")

                ' Verifies the details
                Dim IsAuthorized As Boolean = False
                RaiseEvent OnAuthorize(Me._Shell, Username, Password, RemoteAddress, IsAuthorized)
                If IsAuthorized Then
                    Me._Username = Username
                    Me._Authorized = True
                    Exit Sub
                End If
                Shell.Telnetserver.Print("Username and/or password incorrect", False, True)
                Shell.Telnetserver.Print("")
            Next

            ' No valid login has been given
            Shell.Telnetserver.Print("Closing the connection")
            Thread.Sleep(100)
            Shell.Telnetserver.Close()
        End Sub

        ''' <summary>
        ''' Triggered when a command has been given
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="Arguments">Command line arguments</param>
        ''' <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        ''' <param name="Time">Current timestamp</param>
        Private Sub _Shell_OnCommandReceived(Shell As ShellCore, Arguments() As String, ByRef SuspressError As Boolean, Time As DateTime)
            If Arguments(0).ToUpper() = "WHOAMI" Then
                Shell.Telnetserver.Print("Logged in as " + Me._Username)
                SuspressError = True
            ElseIf Arguments(0).ToUpper() = "HELP" Then
                If Arguments.Length = 1 Then
                    Shell.Telnetserver.Print("WHOAMI                             Returns the current username")
                    SuspressError = True
                ElseIf Arguments(1) = "WHOAMI" Then
                    Shell.Telnetserver.Print("WHOAMI                             Returns the current username")
                    SuspressError = True
                End If
            End If

        End Sub

        ''' <summary>
        ''' Triggered after this.TimeToLogin seconds to optionally close a connection
        ''' </summary>
        ''' <param name="Param"></param>
        Private Sub _LoginTimedout(Param As Object)
            If Me._Authorized Then Exit Sub
            Me._Shell.Telnetserver.Print("", False, True)
            Me._Shell.Telnetserver.Print("Login timeout")
            Thread.Sleep(100)
            Me._Shell.Telnetserver.Close()
        End Sub

        ''' <summary>
        ''' Asks if a user is authorized
        ''' </summary>
        Public Event OnAuthorize As IsAuthorized

        ''' <summary>
        ''' Asks if a user is authorized
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="Username">The username</param>
        ''' <param name="Password">The password</param>
        ''' <param name="RemoteAddress">The remote host address</param>
        ''' <param name="IsAuthorized">Return true if authorization is successfull</param>
        Public Delegate Sub IsAuthorized(Shell As ShellCore, Username As String, Password As String, RemoteAddress As String, ByRef IsAuthorized As Boolean)
    End Class
End Namespace
