using System;
using System.Net;
using System.Net.Sockets;
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
namespace SNTP_Client_Sample
{
    public class Program
    {
        public static void Main()
        {
            // Initializes the time client
            SNTP_Client TimeClient = new SNTP_Client(new IntegratedSocket("time-a.nist.gov", 123));
            // Displays the time in three ways:
            Debug.Print("Amount of seconds since 1 jan. 1900: " + TimeClient.Timestamp.ToString());
            Debug.Print("UTC time: " + TimeClient.UTCDate.ToString());
            Debug.Print("Local time: " + TimeClient.LocalDate.ToString());
            // Synchronizes the internal clock
            TimeClient.Synchronize();
        }

    }
}
