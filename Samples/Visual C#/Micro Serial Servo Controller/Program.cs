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
namespace Micro_Serial_Servo_Controller
{
    public class Program
    {
        public static void Main()
        {
            /* Connect the board:
             *   Pololu GND -> Netduino Gnd
             *   Pololu VIN -> Netduino +5v
             *   Pololu SIN -> Netduino GPIO 1 (COM1)
             *   Pololu OUT -> Netduino GPIO 0 (COM1)
             * To enable Pololu-mode (required for this sample) add a jumper on MODE on the Pololu-board
             * Also, you should power the servos. For this you need to connect the two pins in the top left
             * to GND and a power source, for example the Vin-pin on Netduino.
             */
            MicroSerialServoController Servos = new MicroSerialServoController("COM1", MicroSerialServoController.Modes.Pololu);

            // (Re)sets the speed of the first two servos to 0, see the pololu-manual for details about this
            Servos.SetSpeed(0, 0);
            Servos.SetSpeed(1, 0);

            // Wait for 4 seconds
            Thread.Sleep(4000);

            while (true)
            {
                // Set the position of the first two servos
                Servos.SetPosition(0, 0);
                Servos.SetPosition(1, 254);

                // Wait for 2 seconds
                Thread.Sleep(2000);

                // Set the position of the first two servos again to something else
                Servos.SetPosition(0, 254);
                Servos.SetPosition(1, 0);

                // Wait for 2 seconds
                Thread.Sleep(2000);
            }
        }

    }
}
