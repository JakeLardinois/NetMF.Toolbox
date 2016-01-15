using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

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
namespace Wearable_Keypad
{
    public class Program
    {
        public static void Main()
        {
            WearableKeypad Keypad = new WearableKeypad(Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D6);
            while (true)
            {
                switch (Keypad.Read())
                {
                    case 0: Debug.Print("Up"); break;
                    case 1: Debug.Print("Right"); break;
                    case 2: Debug.Print("Down"); break;
                    case 3: Debug.Print("Left"); break;
                    case 4: Debug.Print("Center"); break;
                }
                Thread.Sleep(100);
            }
        }

    }
}
