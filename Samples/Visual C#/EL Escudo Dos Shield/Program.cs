using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

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
namespace EL_Escudo_Dos_Shield
{
    public class Program
    {
        public static void Main()
        {
            // The EL Escudo Dos Shield has EL-wire sockets connected to pins D2 to D9
            OutputPort[] Channels = {
                new OutputPort(Pins.GPIO_PIN_D2, false),
                new OutputPort(Pins.GPIO_PIN_D3, false),
                new OutputPort(Pins.GPIO_PIN_D4, false),
                new OutputPort(Pins.GPIO_PIN_D5, false),
                new OutputPort(Pins.GPIO_PIN_D6, false),
                new OutputPort(Pins.GPIO_PIN_D7, false),
                new OutputPort(Pins.GPIO_PIN_D8, false),
                new OutputPort(Pins.GPIO_PIN_D9, false)
            };
            // The EL Escudo Dos Shield has a status LED connected to pin D10
            OutputPort StatusLED = new OutputPort(Pins.GPIO_PIN_D10, true);

            while (true)
            {
                for (int Wire = 0; Wire < Channels.Length; ++Wire)
                {
                    Channels[Wire].Write(true);
                    Thread.Sleep(500);
                    Channels[Wire].Write(false);
                }
            }
        }

    }
}
