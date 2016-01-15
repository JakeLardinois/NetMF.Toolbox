using System;
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
namespace Adafruit_Motor_Control_Shield
{
    public class Program
    {
        public static void Main()
        {
            /*
             * This constructor works perfectly for the Netduino Go! Shield Base.
             * If you're using a Netduino Classic or Plus, you can drive just two motors.
             * To keep the code working, use null for Motor1Pwm and Motor2Pwm.
             */
            AdafruitMotorshield Shield = new AdafruitMotorshield(
                // The bitbanged SPI bus for the DC Motor A/B pins
                ClockPin: Pins.GPIO_PIN_D4,
                EnablePin: Pins.GPIO_PIN_D7,
                DataPin: Pins.GPIO_PIN_D8,
                LatchPin: Pins.GPIO_PIN_D12,
                // DC Motor PWM pins
                //Motor1Pwm: new Netduino.PWM(Pins.GPIO_PIN_D11), // Use null if you're using a Netduino Classic or Plus
                //Motor2Pwm: new Netduino.PWM(Pins.GPIO_PIN_D3),  // Use null if you're using a Netduino Classic or Plus
                Motor1Pwm: null,
                Motor2Pwm: null,
                Motor3Pwm: new Netduino.PWM(Pins.GPIO_PIN_D6),
                Motor4Pwm: new Netduino.PWM(Pins.GPIO_PIN_D5)
            );

            while (true)
            {
                for (sbyte i = -100; i < 100; ++i)
                {
                    Shield.SetState(AdafruitMotorshield.Motors.Motor4, i);
                    Thread.Sleep(100);
                }
                for (sbyte i = 99; i > -100; --i)
                {
                    Shield.SetState(AdafruitMotorshield.Motors.Motor4, i);
                    Thread.Sleep(100);
                }
            }
        }

    }
}
