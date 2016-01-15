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
namespace Basic_Speaker
{
    public class Program
    {
        public static void Main()
        {
            // Initializes the speaker
            Speaker PCSpeaker = new Speaker(new Netduino.PWM(Pins.GPIO_PIN_D9));

            // Produces a simple beep
            PCSpeaker.Beep();

            // Wait for 2 seconds
            Thread.Sleep(2000);

            // Produces a 440 hertz tone for 5 ticks
            PCSpeaker.Sound(440, 5);

            // Wait for 2 seconds
            Thread.Sleep(2000);

            // Plays a little song
            PCSpeaker.Play("cdec cdec");     // Are you sleeping? Are you sleeping?
            PCSpeaker.Play("efg efg");       // Brother John, Brother John,
            PCSpeaker.Play("gagfec gagfec"); // Morning bells are ringing! Morning bells are ringing!
            PCSpeaker.Play("c<g>c c<g>c");   // Ding, dang, dong. Ding, dang, dong.

            // ------------------------------------------------------------------------
            //                         Speaker.Play() synax
            // ------------------------------------------------------------------------
            // Octave and tone commands:
            //   Ooctave    Sets the current octave (0 - 6).
            //   < or >     Moves up or down one octave.
            //   A - G      Plays the specified note in the current octave.
            //   Nnote      Plays a specified note (0 - 84) in the seven octave
            //              range (0 is a rest).
            // 
            // Duration and tempo commands:
            //   Llength    Sets the length of each note (1 - 64). L1 is whole note,
            //              L2 is half note, etc.
            //   ML         Sets music legato.
            //   MN         Sets music normal.
            //   MS         Sets music staccato.
            //   Ppause     Specifies a pause (1 - 64). P1 is a whole-note pause,
            //              P2 is a half-note pause, etc.
            //   Ttempo     Sets the tempo in quarter notes per minute (32 - 255).
            //   
            // Mode commands:
            //   MF         Plays music in foreground.
            //   MB         Plays music in background.
            // 
            // Suffix commands:
            //   # or +     Turns preceding note into a sharp.
            //   -          Turns preceding note into a flat.
            //   .          Plays the preceding note 3/2 as long as specified.
            // ------------------------------------------------------------------------
            // See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.Speaker
            // ------------------------------------------------------------------------
        }

    }
}
