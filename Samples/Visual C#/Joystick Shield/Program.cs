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
namespace Joystick_Shield
{
    public class Program
    {
        public static void Main()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !  W A R N I N G   W A R N I N G   W A R N I N G    W A R N I N G   W A R N I N G   W A R N I N G  !
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // ! The Joystick Shield analog ports are connected to 5V since it's originally designed for Arduino. !
            // ! The Netduino analog inputs work on 3.3V. Herefor a modification to the shield is required.       !
            // ! See http://netmftoolbox.codeplex.com/wikipage?title=Joystick%20Shield  for more details.         !
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // The thumb joystick is connected to analog pins 0 and 1, and digital pin 2
            // On the Joystick shield, the thumb stick is rotated by 90° so we need to invert the horizontal value
            ThumbJoystick Joystick = new ThumbJoystick(new Netduino.ADC(Pins.GPIO_PIN_A0), new Netduino.ADC(Pins.GPIO_PIN_A1), Pins.GPIO_PIN_D2, true);

            // The other buttons are connected to digital pins 3, 4, 5 and 6
            // The shield doesn't have pull-up resistors so we use the internal ones from the Netduino
            InputPort ButtonRight = new InputPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.PullUp);
            InputPort ButtonUp = new InputPort(Pins.GPIO_PIN_D4, false, Port.ResistorMode.PullUp);
            InputPort ButtonDown = new InputPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.PullUp);
            InputPort ButtonLeft = new InputPort(Pins.GPIO_PIN_D6, false, Port.ResistorMode.PullUp);

            // Infinite loop; so far Analog ports can't handle interrupts, so the Joystick driver can't as well
            while (true)
            {
                // Lets start with an empty string
                string OutputText = "";
                // Add the values of the thumb stick
                OutputText = "Horizontal: " + Joystick.HorizontalValue.ToString();
                OutputText += " Vertical: " + Joystick.VerticalValue.ToString();
                if (Joystick.PushValue) OutputText += " Thumbstick pushed";

                // Add button states to the output text (inverted with the ! because of the pull-up resistor)
                if (!ButtonDown.Read()) OutputText += " Button Down";
                if (!ButtonUp.Read()) OutputText += " Button Up";
                if (!ButtonLeft.Read()) OutputText += " Button Left";
                if (!ButtonRight.Read()) OutputText += " Button Right";

                // Displays the output text to the debug window
                Debug.Print(OutputText);

                // Wait 100ms to read the pins again
                Thread.Sleep(100);
            }
        }

    }
}
