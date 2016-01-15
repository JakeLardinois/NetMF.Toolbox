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
namespace DS1307_RTC_Module
{
    public class Program
    {
        public static void Main()
        {
            DS1307 RTC = new DS1307();

            // Comment this line out to set the time for the first time
            //RTC.SetTime(Year: 2012, Month: 5, Day: 27, Hour: 22, Minute: 52, Second: 0);

            // Synchronises the Netduino with the DS1307 RTC module
            RTC.Synchronize();

            while (true)
            {
                Debug.Print(DateTime.Now.ToString());
                Thread.Sleep(1000);
            }
        }

    }
}
