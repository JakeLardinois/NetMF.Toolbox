using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
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
namespace Web_client
{
    public class Program
    {
        public static void Main()
        {
            // Creates a new web session
            HTTP_Client WebSession = new HTTP_Client(new IntegratedSocket("www.netmftoolbox.com", 80));

            // Requests the latest source
            HTTP_Client.HTTP_Response Response = WebSession.Get("/helloworld/");

            // Did we get the expected response? (a "200 OK")
            if (Response.ResponseCode != 200)
                throw new ApplicationException("Unexpected HTTP response code: " + Response.ResponseCode.ToString());

            // Fetches a response header
            Debug.Print("Current date according to www.netmftoolbox.com: " + Response.ResponseHeader("date"));

            // Gets the response as a string
            Debug.Print(Response.ToString());
        }

    }
}
