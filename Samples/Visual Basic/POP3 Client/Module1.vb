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
Module Module1

    Sub Main()
        Dim Mailbox As POP3_Client = New POP3_Client(New IntegratedSocket("pop.yourisp.com", 110), "yourusername", "yourpassword")
        Mailbox.Connect()
        Debug.Print("Message count: " + Mailbox.MessageCount.ToString())
        Debug.Print("Box size in bytes: " + Mailbox.BoxSize.ToString())

        Dim ReqHeaders(0 To 2) As String
        ReqHeaders(0) = "subject"
        ReqHeaders(1) = "from"
        ReqHeaders(2) = "date"

        Dim Id() As UInteger, Size() As UInteger
        Mailbox.ListMails(Id, Size)
        For Index As Integer = 0 To Id.Length - 1
            Dim Headers() As String = Mailbox.FetchHeaders(Id(Index), ReqHeaders)
            Debug.Print("Mail ID " + Id(Index).ToString() + " is " + Size(Index).ToString() + " bytes")
            Debug.Print("Subject: " + Headers(0))
            Debug.Print("From: " + Headers(1))
            Debug.Print("Date: " + Headers(2))
            Debug.Print("======================================================================")
        Next

        Mailbox.Close()
    End Sub

End Module
