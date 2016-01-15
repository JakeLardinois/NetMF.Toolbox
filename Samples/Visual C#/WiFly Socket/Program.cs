using System;
using Microsoft.SPOT;
using Toolbox.NETMF.Hardware;
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
namespace WiFly_Socket
{
    public class Program
    {
        public static void Main()
        {
            // Declares the WiFly module, configures the IP address and joins a wireless network
            WiFlyGSX WifiModule = new WiFlyGSX();
            WifiModule.EnableDHCP();
            WifiModule.JoinNetwork("Netduino");

            // Showing some interesting output
            Debug.Print("Local IP: " + WifiModule.LocalIP);
            Debug.Print("MAC address: " + WifiModule.MacAddress);

            // Creates a socket
            SimpleSocket Socket = new WiFlySocket("www.netmftoolbox.com", 80, WifiModule);

            // Connects to the socket
            Socket.Connect();

            // Does a plain HTTP request
            Socket.Send("GET /helloworld/ HTTP/1.1\r\n");
            Socket.Send("Host: " + Socket.Hostname + "\r\n");
            Socket.Send("Connection: Close\r\n");
            Socket.Send("\r\n");

            // Prints all received data to the debug window, until the connection is terminated and there's no data left anymore
            while (Socket.IsConnected || Socket.BytesAvailable > 0)
            {
                string Text = Socket.Receive();
                if (Text != "")
                    Debug.Print(Text);
            }

            // Closes down the socket
            Socket.Close();
        }

    }
}
