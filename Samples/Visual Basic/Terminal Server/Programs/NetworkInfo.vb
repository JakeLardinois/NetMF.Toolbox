Imports System
Imports Microsoft.SPOT.Net.NetworkInformation
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
    Public Module NetworkInfo

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
            If Arguments(0).ToUpper() = "NETWORKINFO" Then
                NetworkInfo.Start(Shell.Telnetserver)
                SuspressError = True
            ElseIf Arguments(0).ToUpper() = "HELP" Then
                If Arguments.Length = 1 Then
                    Shell.Telnetserver.Print("NETWORKINFO                        Shows details about all network interfaces")
                    SuspressError = True
                ElseIf Arguments(1).ToUpper() = "NETWORKINFO" Then
                    Shell.Telnetserver.Print("NETWORKINFO                        Shows details about all network interfaces")
                    SuspressError = True
                End If
            End If
        End Sub

        ''' <summary>
        ''' Actually starts the demo
        ''' </summary>
        ''' <param name="Server">Reference to the Telnet Server</param>
        Private Sub Start(Server As TelnetServer)
            Dim Ips() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
            For IpCnt = 0 To Ips.Length - 1
                Server.Print("Network interface " + IpCnt.ToString() + ":")
                Server.Print("MAC Address: " + NetworkInfo.MacToString(Ips(IpCnt).PhysicalAddress))
                Server.Print("- IP Address: " + Ips(IpCnt).IPAddress.ToString() + " (" + Ips(IpCnt).SubnetMask.ToString() + ")")
                Server.Print("- Gateway: " + Ips(IpCnt).GatewayAddress.ToString())
                For DnsCnt As Integer = 0 To Ips(IpCnt).DnsAddresses.Length - 1
                    Server.Print("- DNS-server " + DnsCnt.ToString() + ": " + Ips(IpCnt).DnsAddresses(DnsCnt).ToString())
                Next
            Next
            Server.Print("Connected to: " + Server.RemoteAddress)
        End Sub

        ''' <summary>
        ''' Converts a PhysicalAddress array to a MAC address
        ''' </summary>
        ''' <param name="PhysicalAddress">The PhysicalAddress</param>
        ''' <returns>The MAC Address as string</returns>
        Private Function MacToString(PhysicalAddress() As Byte) As String
            If PhysicalAddress.Length = 0 Then Return "00:00:00:00:00:00"
            Dim RetVal As String = ""
            For i As Integer = 0 To PhysicalAddress.Length - 1
                RetVal += ":" + Tools.Dec2Hex(PhysicalAddress(i), 2)
            Next
            Return RetVal.Substring(1)
        End Function

    End Module
End Namespace
