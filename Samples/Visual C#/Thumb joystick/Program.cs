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
namespace Thumb_joystick
{
    public class Program
    {
        public static void Main()
        {
            // Declares the joystick
            ThumbJoystick Joystick = new ThumbJoystick(new Netduino.ADC(Pins.GPIO_PIN_A0), new Netduino.ADC(Pins.GPIO_PIN_A1), Pins.GPIO_PIN_D2);

            // Prints all values from the joystick every second
            while (true)
            {
                Debug.Print("Horz:" + Joystick.HorizontalValue.ToString() + " - Vert:" + Joystick.VerticalValue.ToString() + " - Sel:" + Joystick.PushValue.ToString());
                Thread.Sleep(1000);
            }

        }

    }
}
