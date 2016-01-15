using System;
using Microsoft.SPOT.Hardware;
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
namespace Programs
{
    /// <summary>
    /// Netduino Plus demo
    /// </summary>
    public static class NetduinoPlus
    {
        private static Cpu.Pin ONBOARD_SW1;
        private static Cpu.Pin ONBOARD_LED;

        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        /// <param name="ONBOARD_SW1">Reference to the onboard button</param>
        /// <param name="ONBOARD_LED">Reference to the onboard LED</param>
        public static void Bind(ShellCore Shell, Cpu.Pin ONBOARD_SW1, Cpu.Pin ONBOARD_LED)
        {
            NetduinoPlus.ONBOARD_SW1 = ONBOARD_SW1;
            NetduinoPlus.ONBOARD_LED = ONBOARD_LED;
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Unbinds from the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Unbind(ShellCore Shell)
        {
            Shell.OnCommandReceived -= new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        private static void Shell_OnCommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time)
        {
            if (Arguments[0].ToUpper() == "NETDUINOPLUS")
            {
                SuspressError = true;
                NetduinoPlus.Start(Shell.TelnetServer);
            }
            if (Arguments[0].ToUpper() == "HELP")
            {
                if (Arguments.Length == 1 || Arguments[1].ToUpper() == "NETDUINOPLUS")
                {
                    Shell.TelnetServer.Print("NETDUINOPLUS                       An interactive Netduino Plus demo");
                    SuspressError = true;
                }
            }
        }

        /// <summary>
        /// Actually starts the demo
        /// </summary>
        /// <param name="Server">Reference to the Telnet Server</param>
        public static void Start(TelnetServer Server)
        {
            // Configures the onboard button. If this fails, it's probably already in use by another app.
            InputPort Button = null;
            try { Button = new InputPort(ONBOARD_SW1, false, Port.ResistorMode.Disabled); }
            catch (Exception e)
            {
                Server.Color(TelnetServer.Colors.LightRed);
                Server.Print("Exception " + e.Message + " given. Were the onboard Button already configured?");
                Server.Color(TelnetServer.Colors.White);
                return;
            }

            // Configures the onboard LED. If this fails, it's probably already in use by another app.
            OutputPort Led = null;
            try { Led = new OutputPort(ONBOARD_LED, false); }
            catch (Exception e)
            {
                Button.Dispose(); // The button is already defined
                Server.Color(TelnetServer.Colors.LightRed);
                Server.Print("Exception " + e.Message + " given. Were the onboard LED already configured?");
                Server.Color(TelnetServer.Colors.White);
                return;
            }

            // Disables echoing of keypresses
            Server.EchoEnabled = false;
            // Clears the screen
            Server.ClearScreen();
            
            // Draws a Netduino Plus in ANSI/ASCII art
            Server.Print("\xda\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xbf");
            Server.Print("\xb3             \xfe\xfe\xfe\xfe\xfe\xfe\xfe\xfe \xfe\xfe\xfe\xfe\xfe\xfe\xfe\xfe\xb3");
            Server.Print("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb                   \xdc  \xb3");
            Server.Print("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb             NETDUINO \xb3");
            Server.Print("\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb\xdb               PLUS   \xb3");
            Server.Print("\xb3                            ..\xb3");
            Server.Print("\xdb\xdb                       \xdb\xdb  ::\xb3");
            Server.Print("\xb3       \xdb\xdb\xdb\xdb\xdb\xdb                 \xb3");
            Server.Print("\xdb\xdb\xdb\xdb\xdb   \xdb\xdb\xdb\xdb\xdb\xdb                 \xb3");
            Server.Print("\xdb\xdb\xdb\xdb\xdb   \xdb\xdb\xdb\xdb\xdb\xdb    \xfe\xfe\xfe\xfe\xfe\xfe \xfe\xfe\xfe\xfe\xfe\xfe\xb3");
            Server.Print("\xc0\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xdb\xdb\xdb\xdb\xdb\xdb\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xc4\xd9");
            Server.Print("", true);
            Server.Print("Push the Netduino Plus onboard button, or press a key to close this app");

            // We only update the screen if the LastState is different from the current state
            bool LastState = false;
            while (Server.IsConnected && Server.Input(1, false) == "") 
            {
                // We need to update
                if (Button.Read() == LastState)
                {
                    // Lets record the last state
                    LastState = !Button.Read();
                    Led.Write(LastState);

                    // Draws the button
                    if (LastState) Server.Color(TelnetServer.Colors.HighIntensityWhite);
                    else Server.Color(TelnetServer.Colors.Gray);
                    Server.Locate(7, 26, true); Server.Print("\xdb\xdb", true, true);

                    // Draws the LED
                    if (LastState) Server.Color(TelnetServer.Colors.LightBlue);
                    else Server.Color(TelnetServer.Colors.Gray);
                    Server.Locate(3, 29, true); Server.Print("\xdc", true);

                    // Brings back the cursor to the last line
                    Server.Locate(14, 1);
                }
            }

            // Releases the pins
            Button.Dispose();
            Led.Dispose();

            // Enables echo again and clears the screen
            if (Server.IsConnected)
            {
                Server.EchoEnabled = true;
                Server.ClearScreen();
            }
        }
    }
}
