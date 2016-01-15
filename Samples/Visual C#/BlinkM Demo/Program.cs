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
namespace BlinkM_Demo
{
    public class Program
    {
        public static void Main()
        {
            BlinkM Led = new BlinkM();

            while (true)
            {
                Debug.Print("Red");   Led.SetColor(0xff0000); Thread.Sleep(1000);
                Debug.Print("Green"); Led.SetColor(0x00ff00); Thread.Sleep(1000);
                Debug.Print("Blue");  Led.SetColor(0x0000ff); Thread.Sleep(1000);
            }
        }

    }
}
