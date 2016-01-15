Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.NetduinoPlus
Imports Toolbox.NETMF.NET

'  Copyright 2011-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
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

    Sub Main()
        ' Creates a new web session
        Dim WebSession As HTTP_Client = New HTTP_Client(New IntegratedSocket("www.netmftoolbox.com", 80))

        ' Requests the latest source
        Dim Response As HTTP_Client.HTTP_Response = WebSession.Get("/helloworld/")

        ' Did we get the expected response? (a "200 OK")
        If Response.ResponseCode <> 200 Then
            Throw New ApplicationException("Unexpected HTTP response code: " + Response.ResponseCode.ToString())
        End If

        ' Fetches a response header
        Debug.Print("Current date according to www.netmftoolbox.com: " + Response.ResponseHeader("date"))

        ' Gets the response as a string
        Debug.Print(Response.ToString())
    End Sub

End Module
