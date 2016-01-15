using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Toolbox.NETMF.NET;

/*
 * Copyright 2011-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SMTPClient
{
    public class Program
    {
        public static void Main()
        {
            // By defining the CORRECT! date, mail messages could get a lower spam score in spam filters
            //Utility.SetLocalTime(new DateTime(2012, 1, 26, 20, 43, 0, 0));

            // Defines the sender
            SMTP_Client.MailContact From = new SMTP_Client.MailContact("administrator@localhost", "Your name");
            // Defines the receiver
            SMTP_Client.MailContact Receiver = new SMTP_Client.MailContact("someone@else", "Recipients name");
            // Defines the mail message
            SMTP_Client.MailMessage Message = new SMTP_Client.MailMessage("Small test result");
            Message.Body = "This mail is sent by a Netduino :-)\r\n";
            Message.Body += "Good day!";

            // Initializes the mail sender class (When your ISP blocks port 25, try 587. A lot of SMTP servers respond to that as well!)
            SMTP_Client Sender = new SMTP_Client(new IntegratedSocket("smtp.yourisp.com", 25));

            // Sends the mail
            Sender.Send(Message, From, Receiver);
        }

    }
}
