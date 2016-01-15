using System;
using Microsoft.SPOT;
using Toolbox.NETMF.NET;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
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
namespace NetduinoPlusApplication1
{
    public class POP3_Client_Sample
    {
        public static void Main()
        {
            POP3_Client Mailbox = new POP3_Client(new IntegratedSocket("pop.yourisp.com", 110), "yourusername", "yourpassword");
            Mailbox.Connect();
            Debug.Print("Message count: " + Mailbox.MessageCount.ToString());
            Debug.Print("Box size in bytes: " + Mailbox.BoxSize.ToString());

            uint[] Id, Size;
            Mailbox.ListMails(out Id, out Size);
            for (int Index = 0; Index < Id.Length; ++Index)
            {
                string[] Headers = Mailbox.FetchHeaders(Id[Index], new string[] { "subject", "from", "date" });
                Debug.Print("Mail ID " + Id[Index].ToString() + " is " + Size[Index].ToString() + " bytes");
                Debug.Print("Subject: " + Headers[0]);
                Debug.Print("From: " + Headers[1]);
                Debug.Print("Date: " + Headers[2]);
                Debug.Print("======================================================================");
            }

            Mailbox.Close();
        }

    }
}
