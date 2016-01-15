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
namespace SN754410_Motor_Driver
{
    public class Program
    {
        public static void Main()
        {
            // Defines the H-Bridge IC on the correct pins
            HBridge MotorDriver = new HBridge(new Netduino.PWM(Pins.GPIO_PIN_D6), Pins.GPIO_PIN_D7, new Netduino.PWM(Pins.GPIO_PIN_D5), Pins.GPIO_PIN_D4);

            // Motor 1 half speed backward
            MotorDriver.SetState(HBridge.Motors.Motor1, -50);
            // Motor 2 half speed forward
            MotorDriver.SetState(HBridge.Motors.Motor2, 50);

            // Lets run for 5 seconds
            Thread.Sleep(5000);

            // Motor 1 full speed backward
            MotorDriver.SetState(HBridge.Motors.Motor1, -100);
            // Motor 2 full speed forward
            MotorDriver.SetState(HBridge.Motors.Motor2, 100);

            // Lets run for 5 seconds
            Thread.Sleep(5000);

            // Stops both motors
            MotorDriver.SetState(HBridge.Motors.Motor1, 0);
            MotorDriver.SetState(HBridge.Motors.Motor2, 0);
        }

    }
}
