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
namespace TMP36_Temperature_sensor
{
    public class Program
    {
        public static void Main()
        {
            // The Tmp36-sensor is connected to analog pin 4
            Tmp36 TemperatureSensor = new Tmp36(new Netduino.ADC(Pins.GPIO_PIN_A4));

            while (true)
            {
                float celcius = TemperatureSensor.Temperature;
                float kelvin = (float)(celcius + 273.15);
                float fahrenheit = (float)((celcius * 1.8) + 32.0);
                Debug.Print("Current temperature: " + celcius.ToString() + " celcius / " + kelvin.ToString() + " kelvin / " + fahrenheit.ToString() + " fahrenheit");
                Thread.Sleep(1000);
            }
        }

    }
}
