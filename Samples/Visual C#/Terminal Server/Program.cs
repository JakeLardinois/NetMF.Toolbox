using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
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
namespace Terminal_Server
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G   W A R N I N G   W A R N I N G  !
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // ! Never connect this directly to the web. The author can't guarantee the safety of your device and !
    // ! your network. Lets just say the author can guarantee you and your network won't be safe at all.  !
    // ! Just enjoy this code and use with caution :-)                                                    !
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class Program
    {
        public static void Main()
        {
            // Prints out all IP addresses
            Debug.Print("You could try to telnet to one of these IP addresses:");
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface[] Ips = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < Ips.Length; ++i)
                Debug.Print(Ips[i].IPAddress.ToString());
            
            // Initiates a new shell on port 23
            Programs.ShellCore Shell = new Programs.ShellCore(new IntegratedSocket("", 23));
            // When disconnected, we want to start listening again
            Shell.OnDisconnected += new Programs.ShellCore.ConnectionStateChange(Shell_OnDisconnected);

            // Lets bind to the authorization provider (optional, but useful ;-))
            Programs.Auth LoginModule = new Programs.Auth(Shell);
            LoginModule.OnAuthorize += new Programs.Auth.IsAuthorized(Auth_OnAuthorize);

            // Lets bind some more apps
            Programs.ShellCommands.Bind(Shell); // Some basic shell commands like MOTD, CLS, etc.
            Programs.ColorDemo.Bind(Shell);     // ANSI color demo
            Programs.NetworkInfo.Bind(Shell);   // Network information
            Programs.Ntp.Bind(Shell);           // Time Synchronization client
            Programs.Time.Bind(Shell);          // Access to the DateTime object
            Programs.FileSystem.Bind(Shell);    // Access to the SD-card
            Programs.Serial.Bind(Shell);        // Serial terminal
            Programs.Edlin.Bind(Shell);         // Text editor
            Programs.NetduinoPlus.Bind(Shell, Pins.ONBOARD_SW1, Pins.ONBOARD_LED); // Interactive Netduino Plus sample

            // Binds the commands to access all 22 pins
            Programs.Pins.Bind(Shell, 22);
            Programs.Pins.ConfigurePin(0, Pins.GPIO_PIN_A0, "A0");
            Programs.Pins.ConfigurePin(1, Pins.GPIO_PIN_A1, "A1");
            Programs.Pins.ConfigurePin(2, Pins.GPIO_PIN_A2, "A2");
            Programs.Pins.ConfigurePin(3, Pins.GPIO_PIN_A3, "A3");
            Programs.Pins.ConfigurePin(4, Pins.GPIO_PIN_A4, "A4");
            Programs.Pins.ConfigurePin(5, Pins.GPIO_PIN_A5, "A5");
            Programs.Pins.ConfigurePin(6, Pins.ONBOARD_LED, "LED");
            Programs.Pins.ConfigurePin(7, Pins.ONBOARD_SW1, "SW1");
            Programs.Pins.ConfigurePin(8, Pins.GPIO_PIN_D0, "D0");
            Programs.Pins.ConfigurePin(9, Pins.GPIO_PIN_D1, "D1");
            Programs.Pins.ConfigurePin(10, Pins.GPIO_PIN_D2, "D2");
            Programs.Pins.ConfigurePin(11, Pins.GPIO_PIN_D3, "D3");
            Programs.Pins.ConfigurePin(12, Pins.GPIO_PIN_D4, "D4");
            Programs.Pins.ConfigurePin(13, Pins.GPIO_PIN_D5, "D5");
            Programs.Pins.ConfigurePin(14, Pins.GPIO_PIN_D6, "D6");
            Programs.Pins.ConfigurePin(15, Pins.GPIO_PIN_D7, "D7");
            Programs.Pins.ConfigurePin(16, Pins.GPIO_PIN_D8, "D8");
            Programs.Pins.ConfigurePin(17, Pins.GPIO_PIN_D9, "D9");
            Programs.Pins.ConfigurePin(18, Pins.GPIO_PIN_D10, "D10");
            Programs.Pins.ConfigurePin(19, Pins.GPIO_PIN_D11, "D11");
            Programs.Pins.ConfigurePin(20, Pins.GPIO_PIN_D12, "D12");
            Programs.Pins.ConfigurePin(21, Pins.GPIO_PIN_D13, "D13");

            // We start the shell
            Shell.Start();
        }
        
        /// <summary>
        /// Triggered when the connection has been lost
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="RemoteAddress">Hostname of the user</param>
        /// <param name="Time">Timestamp of the event</param>
        static void Shell_OnDisconnected(Programs.ShellCore Shell, string RemoteAddress, DateTime Time)
        {
            // Starts to listen again
            Shell.Start();
        }

        /// <summary>
        /// Asks if a user is authorized
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Username">The username</param>
        /// <param name="Password">The password</param>
        /// <param name="RemoteAddress">The remote host address</param>
        /// <returns>Return true if authorization is successfull</returns>
        static bool Auth_OnAuthorize(Programs.ShellCore Shell, string Username, string Password, string RemoteAddress)
        {
            if (Username == "test" && Password == "test") return true;
            if (Username == "admin" && Password == "nimda")
            {
                // We could bind or unbind additional programs here
                //Programs.Whatever.Bind(Shell);
                //Programs.Whatever.Unbind(Shell);
                return true;
            }

            Shell.TelnetServer.Print("Hint: admin/nimda or test/test :-)");
            return false;
        }
    }
}
