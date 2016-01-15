using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
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
namespace IRC
{
    public class Program
    {
        // Channel for our nice IRC-bot
        public const string Channel = "#netduino";

        // The IRC Client
        public static IRC_Client Client;

        // The onboard LED
        public static OutputPort Led = new OutputPort(Pins.ONBOARD_LED, false);

        // The onboard Switch
        public static InterruptPort Button = new InterruptPort(Pins.ONBOARD_SW1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

        // Main method, initiates all events and makes the connection
        public static void Main()
        {
            Button.OnInterrupt += new NativeEventHandler(Button_OnInterrupt);
            Client = new IRC_Client(new IntegratedSocket("irc.netmftoolbox.com", 6667), "Guest");
            Client.OnAuthenticated += new IRC_Client.OnStringReceived(Client_OnAuthenticated);
            Client.OnMessage += new IRC_Client.OnStringReceived(Client_OnMessage);
            Client.Connect();
        }

        // Button state changed
        static void Button_OnInterrupt(uint PinId, uint Value, DateTime Time)
        {
            if (!Client.Authenticated) return;
            if (Value == 1)
                Client.Message(Channel, "Oh thank goodness, the button is released.");
            else
                Client.Message(Channel, "Oh dear, someone pressed my button!");
        }

        // We're connected, lets initialize our client further
        static void Client_OnAuthenticated(string Sender, string Target, string Data)
        {
            Debug.Print("Successfully connected to " + Sender + " as " + Target);
            Debug.Print(Data);
            Client.Join(Channel);
            Client.Message(Channel, "Hello, it's me, running " + Client.ClientVersion);
            Client.Message(Channel, "Type \"LED [ON/OFF/STATUS]\" to play with my led :-)");
        }

        // We got a message. If it's a channel message, we're going to respond to it
        static void Client_OnMessage(string Sender, string Target, string Data)
        {
            if (Target.ToLower() == Channel)
            {
                Debug.Print(IRC_Client.SplitName(Sender)[0] + ": " + Data);

                string[] Commandline = Data.ToUpper().Split(new char[] { ' ' }, 2);
                if (Commandline[0] == "LED" && Commandline[1] == "ON")
                {
                    Led.Write(true);
                    Client.Message(Channel, "LED just turned ON");
                }
                else if (Commandline[0] == "LED" && Commandline[1] == "OFF")
                {
                    Led.Write(false);
                    Client.Message(Channel, "LED just turned OFF");
                }
                else if (Commandline[0] == "LED" && Commandline[1] == "STATUS")
                {
                    Client.Message(Channel, "The LED is currently " + (Led.Read() ? "ON" : "OFF"));
                }
            }
        }

    }
}