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
namespace Sound_Module
{
    public class Program
    {
        /*
         * This sample uses 5 files on the SD-card. They can be found in netmftoolbox\Samples\Visual C#\Sound Module\samples
         */
        public static ushort EXTERMINATE = 0;
        public static ushort WAAAAAAAAAAAA = 1;
        public static ushort YOUMAYNOTLEAVE = 2;
        public static ushort TARDIS = 3;
        public static ushort SONIC = 4;

        public static void Main()
        {
            // The module is connected to these pins
            Somo SoundModule = new Somo(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2);

            // Sets the volume fully open
            SoundModule.SetVolume(7);

            // Plays "Exterminate!"
            Debug.Print("EXTERMINATE");
            SoundModule.PlayTrack(EXTERMINATE, true);

            // Plays the Tardis-sound
            Debug.Print("TARDIS");
            SoundModule.PlayTrack(TARDIS, true);

            // Plays "You may not leave my precence!"
            Debug.Print("YOUMAYNOTLEAVE");
            SoundModule.PlayTrack(YOUMAYNOTLEAVE, true);

            // Plays the sonic screwdriver sound
            Debug.Print("SONIC");
            SoundModule.PlayTrack(SONIC, true);

            // Repeatedly play "Waaaaaaaaa!"
            Debug.Print("WAAAAAAAAAAAA (repeated)");
            SoundModule.PlayRepeat(WAAAAAAAAAAAA);

            // Let this go on infinitely
            Thread.Sleep(Timeout.Infinite);
        }
    }
}