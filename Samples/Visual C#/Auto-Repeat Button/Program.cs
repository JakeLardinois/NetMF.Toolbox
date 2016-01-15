using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

/*
 * Copyright 2011-2014 Mario Vernari (http://www.netmftoolbox.com/)
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
namespace Auto_Repeat_Button
{
    public class Program
    {
        public static void Main()
        {
            // Initializes the button
            AutoRepeatInputPort Button = new AutoRepeatInputPort(Pins.ONBOARD_SW1, Port.ResistorMode.PullUp, true);
            // Attaches the event
            Button.StateChanged += new AutoRepeatEventHandler(Button_StateChanged);
            // Tells you what to do :-)
            Debug.Print("Press the onboard switch for a while");
        }

        /// <summary>
        /// Triggered when a button is pressed, still pressed, or released
        /// </summary>
        /// <param name="Sender">The AutoRepeatInputPort object</param>
        /// <param name="EventArgs">Event arguments</param>
        static void Button_StateChanged(object Sender, AutoRepeatEventArgs EventArgs)
        {
            switch (EventArgs.State)
            {
                case AutoRepeatInputPort.AutoRepeatState.Press:
                    Debug.Print("Pressed");
                    break;
                case AutoRepeatInputPort.AutoRepeatState.Release:
                    Debug.Print("Released");
                    break;
                case AutoRepeatInputPort.AutoRepeatState.Tick:
                    Debug.Print("Tick-Tock Goes the Clock");
                    break;
            }
        }

    }
}
