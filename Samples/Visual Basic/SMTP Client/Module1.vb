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
        ' By defining the CORRECT! date, mail messages could get a lower spam score in spam filters
        'Utility.SetLocalTime(New DateTime(2011, 10, 16, 20, 43, 0, 0))

        ' Defines the sender
        Dim From As SMTP_Client.MailContact = New SMTP_Client.MailContact("administrator@localhost", "Your name")
        ' Defines the receiver
        Dim Receiver As SMTP_Client.MailContact = New SMTP_Client.MailContact("someone@else", "Recipients name")
        ' Defines the mail message
        Dim Message As SMTP_Client.MailMessage = New SMTP_Client.MailMessage("Small test result")
        Message.Body = "This mail is sent by a Netduino :-)" + Constants.vbCrLf
        Message.Body = Message.Body + "Good day!"

        ' Initializes the mail sender class (When your ISP blocks port 25, try 587. A lot of SMTP servers respond to that as well!)
        Dim Sender As SMTP_Client = New SMTP_Client(New IntegratedSocket("smtp.yourisp.com", 25))

        ' Sends the mail
        Sender.Send(Message, From, Receiver)
    End Sub

End Module
