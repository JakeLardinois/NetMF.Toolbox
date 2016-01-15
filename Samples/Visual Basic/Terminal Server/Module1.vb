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

    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G   W A R N I N G   W A R N I N G  !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    ' ! Never connect this directly to the web. The author can't guarantee the safety of your device and !
    ' ! your network. Lets just say the author can guarantee you and your network won't be safe at all.  !
    ' ! Just enjoy this code and use with caution :-)                                                    !
    ' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    Sub Main()
        ' Prints out all IP addresses
        Debug.Print("You could try to telnet to one of these IP addresses:")
        Dim Ips() As Microsoft.SPOT.Net.NetworkInformation.NetworkInterface = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        For i As Integer = 0 To Ips.Length - 1
            Debug.Print(Ips(i).IPAddress.ToString())
        Next

        ' Initiates a new shell on port 23
        Dim Shell As Programs.ShellCore = New Programs.ShellCore(New IntegratedSocket("", 23))
        ' When disconnected, we want to start listening again
        AddHandler Shell.OnDisconnected, AddressOf Shell_OnDisconnected

        ' Lets bind to the authorization provider (optional, but useful ;-))
        Dim LoginModule As Programs.Auth = New Programs.Auth(Shell)
        AddHandler LoginModule.OnAuthorize, AddressOf Auth_OnAuthorize

        ' This sample is a port from the C# sample code.
        ' Some portions were to big for the code memory when the Microsoft.VisualBasic reference was added, so I haven't implemented all programs.
        ' Some of the source files are ported, but not all are loaded into the project for this reason.

        ' Lets bind some more apps
        Programs.ShellCommands.Bind(Shell) ' Some basic shell commands like MOTD, CLS, etc.
        Programs.ColorDemo.Bind(Shell)     ' ANSI color demo
        Programs.NetworkInfo.Bind(Shell)   ' Network information
        Programs.Ntp.Bind(Shell)           ' Time Synchronization client
        Programs.Time.Bind(Shell)          ' Access to the DateTime object
        Programs.FileSystem.Bind(Shell)    ' Access to the SD-card
        Programs.Serial.Bind(Shell)        ' Serial terminal
        Programs.NetduinoPlus.Bind(Shell, Pins.ONBOARD_SW1, Pins.ONBOARD_LED) ' Interactive Netduino Plus sample

        ' We start the shell
        Shell.Start()
    End Sub

    ''' <summary>
    ''' Triggered when the connection has been lost
    ''' </summary>
    ''' <param name="Shell">Reference to the shell</param>
    ''' <param name="RemoteAddress">Hostname of the user</param>
    ''' <param name="Time">Timestamp of the event</param>
    Private Sub Shell_OnDisconnected(Shell As Programs.ShellCore, RemoteAddress As String, Time As Date)
        ' Starts to listen again
        Shell.Start()
    End Sub

    ''' <summary>
    ''' Asks if a user is authorized
    ''' </summary>
    ''' <param name="Shell">Reference to the shell</param>
    ''' <param name="Username">The username</param>
    ''' <param name="Password">The password</param>
    ''' <param name="RemoteAddress">The remote host address</param>
    ''' <param name="IsAuthorized">Return true if authorization is successfull</param>
    Private Sub Auth_OnAuthorize(Shell As Programs.ShellCore, Username As String, Password As String, RemoteAddress As String, ByRef IsAuthorized As Boolean)
        If Username = "test" And Password = "test" Then IsAuthorized = True : Exit Sub
        If Username = "admin" And Password = "nimda" Then
            ' We could bind or unbind additional programs here
            'Programs.Whatever.Bind(Shell);
            'Programs.Whatever.Unbind(Shell);
            IsAuthorized = True : Exit Sub
        End If

        Shell.Telnetserver.Print("Hint: admin/nimda or test/test :-)")
        IsAuthorized = False
    End Sub

End Module
