using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

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
namespace NES_Controller_Adapter_Sample
{
    public class Program
    {
        public static void Main()
        {
            NESControllerAdapter Controller = new NESControllerAdapter(Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3, Pins.GPIO_PIN_A4, Pins.GPIO_PIN_A5);
            Controller.OnButtonChanged += new NESControllerAdapter.ButtonChanged(Controller_OnButtonChanged);
            Controller.EventsEnabled = true;

            Thread.Sleep(Timeout.Infinite);
        }

        static void Controller_OnButtonChanged(NESControllerAdapter This, NESControllerAdapter.Socket Socket, NESControllerAdapter.Button Button, bool Value, DateTime Time)
        {
            Debug.Print("Button " + Button.ToString()+" on Socket " + Socket.ToString() + " changed to " + Value.ToString());
        }

    }
}
