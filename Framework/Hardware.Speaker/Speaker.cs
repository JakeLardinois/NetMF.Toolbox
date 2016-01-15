using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

/*
 * Copyright 2011-2014 Steven Don & Stefan Thoolen (http://www.netmftoolbox.com/)
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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// Simple Speaker interface
    /// </summary>
    public class Speaker : IDisposable
    {

        /// <summary>
        /// Stores a reference to the PWM device of the speaker
        /// </summary>
        private IPWMPort _Speaker;

        /// <summary>
        /// Contains 120 periods from A to G# from octave 0 to 9
        /// </summary>
        private uint[] _Tonebar = {
            0, // rest
            //     0         1         2         3         4         5         6         7         8         9        10        11
            //     C        C#         D        D#         E         F        F#         G        G#         A        A#         B
            30578118, 28861591, 27241925, 25713138, 24270004, 22907539, 21621914, 20408424, 19262951, 18181818, 17161490, 16198267,  // Octave 0
            15289059, 14430795, 13620963, 12856569, 12135002, 11453770, 10810957, 10204212,  9631476,  9090909,  8580745,  8099133,  // Octave 1
             7644529,  7215398,  6810481,  6428284,  6067501,  5726885,  5405478,  5102106,  4815738,  4545455,  4290372,  4049567,  // Octave 2
             3822265,  3607699,  3405241,  3214142,  3033750,  2863442,  2702739,  2551053,  2407869,  2272727,  2145186,  2024783,  // Octave 3
             1911132,  1803849,  1702620,  1607071,  1516875,  1431721,  1351370,  1275526,  1203934,  1136364,  1072593,  1012392,  // Octave 4
              955566,   901925,   851310,   803536,   758438,   715861,   675685,   637763,   601967,   568182,   536297,   506196,  // Octave 5
              477783,   450962,   425655,   401768,   379219,   357930,   337842,   318882,   300984,   284091,   268148,   253098,  // Octave 6
              238892,   225481,   212828,   200884,   189609,   178965,   168921,   159441,   150492,   142045,   134074,   126549,  // Octave 7
              119446,   112741,   106414,   100442,    94805,    89483,    84461,    79720,    75246,    71023,    67037,    63274,  // Octave 8
               59723,    56370,    53207,    50221,    47402,    44741,    42230,    39860,    37623,    35511,    33519,    31637   // Octave 9
        };

        /// <summary>
        /// The length of each note (1 - 64). L1 is whole note, L2 is half note, etc.
        /// </summary>
        private byte _PlayLength = 4;

        /// <summary>
        /// The tempo in quarter notes per minute (32 - 255).
        /// </summary>
        private byte _PlayTempo = 120;

        /// <summary>
        /// The octave the Play-method currently is in.
        /// </summary>
        private byte _PlayOctave = 4;

        /// <summary>
        /// All existing play modes
        /// </summary>
        private enum _PlayModes
        {
            Legato,   // tied together
            Normal,
            Staccato  // detached
        }

        /// <summary>
        /// Triggered after each played note. data2 contains the remaining bytes to play
        /// </summary>
        public event NativeEventHandler OnPlay;

        /// <summary>
        /// Holds the play mode the Play-method currently is in.
        /// </summary>
        private _PlayModes _PlayMode = _PlayModes.Normal;

        /// <summary>
        /// Plays musical notes. See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.Speaker
        /// </summary>
        /// <example><![CDATA[
        /// Octave and tone commands:
        ///   Ooctave    Sets the current octave (0 - 9).
        ///   < or >     Moves up or down one octave.
        ///   A - G      Plays the specified note in the current octave.
        ///   Nnote      Plays a specified note (0 - 84) in the seven octave
        ///              range (0 is a rest).
        /// 
        /// Duration and tempo commands:
        ///   Llength    Sets the length of each note (1 - 64). L1 is whole note,
        ///              L2 is half note, etc.
        ///   ML         Sets music legato.
        ///   MN         Sets music normal.
        ///   MS         Sets music staccato.
        ///   Ppause     Specifies a pause (1 - 64). P1 is a whole-note pause,
        ///              P2 is a half-note pause, etc.
        ///   Ttempo     Sets the tempo in quarter notes per minute (32 - 255).
        ///   
        /// Mode commands:
        ///   MF         Plays music in foreground.
        ///   MB         Plays music in background.
        /// 
        /// Suffix commands:
        ///   # or +     Turns preceding note into a sharp.
        ///   -          Turns preceding note into a flat.
        ///   .          Plays the preceding note 3/2 as long as specified.
        /// 
        /// See also: http://netmftoolbox.codeplex.com/wikipage?title=Toolbox.NETMF.Hardware.Speaker
        /// ]]></example>
        /// <param name="CommandString">A string expression that contains one or more of the PLAY commands.</param>
        public void Play(string CommandString)
        {
            // Used at many other points to read three digits
            byte Digit1;
            byte Digit2;
            byte Digit3;

            // We handle commands in upper string
            CommandString = CommandString.ToUpper() + "  "; // The additional spaces makes it more easy to parse the string :-)

            // Loop through all the commands
            for (var CSCounter = 0; CSCounter < CommandString.Length; ++CSCounter)
            {
                switch (CommandString[CSCounter])
                {
                    case 'O': // Need to change the Octave to a specific number
                        ++CSCounter;
                        this._PlayOctave = (byte)(CommandString[CSCounter] - 48); // 48 = 0, 49 = 1, 50 = 2, etc.
                        break;
                    case '>': // Need to change the Octave one up
                        ++this._PlayOctave;
                        break;
                    case '<': // Need to change the Octave one down
                        --this._PlayOctave;
                        break;
                    case 'A': // Needs to play the A note
                    case 'B': // Needs to play the B note
                    case 'C': // Needs to play the C note
                    case 'D': // Needs to play the D note
                    case 'E': // Needs to play the E note
                    case 'F': // Needs to play the F note
                    case 'G': // Needs to play the G note
                    case 'N': // Needs to play a specific note
                        byte BaseNote = 0;
                        byte BaseLength = this._PlayLength;
                        if (CommandString[CSCounter] == 'C') BaseNote = (byte)(this._PlayOctave * 12 + 0 + 1);
                        else if (CommandString[CSCounter] == 'D') BaseNote = (byte)(this._PlayOctave * 12 + 2 + 1);
                        else if (CommandString[CSCounter] == 'E') BaseNote = (byte)(this._PlayOctave * 12 + 4 + 1);
                        else if (CommandString[CSCounter] == 'F') BaseNote = (byte)(this._PlayOctave * 12 + 5 + 1);
                        else if (CommandString[CSCounter] == 'G') BaseNote = (byte)(this._PlayOctave * 12 + 7 + 1);
                        else if (CommandString[CSCounter] == 'A') BaseNote = (byte)(this._PlayOctave * 12 + 9 + 1);
                        else if (CommandString[CSCounter] == 'B') BaseNote = (byte)(this._PlayOctave * 12 + 11 + 1);
                        else if (CommandString[CSCounter] == 'N')
                        {
                            Digit1 = (byte)CommandString[CSCounter + 1];
                            Digit2 = (byte)CommandString[CSCounter + 2];
                            if (Digit2 > 47 && Digit2 < 58)
                            { // 48 = 0, 59 = 9
                                BaseNote = (byte)((Digit2 - 48) + ((Digit1 - 48) * 10));
                                CSCounter = CSCounter + 2;
                            }
                            else
                            {
                                BaseNote = (byte)(Digit1 - 48);
                                CSCounter = CSCounter + 1;
                            }
                        }
                        if (CommandString[CSCounter + 1] == '#' || CommandString[CSCounter + 1] == '+')
                        { // Turns preceding note into a sharp.
                            ++BaseNote;
                            ++CSCounter;
                        }
                        if (CommandString[CSCounter + 1] == '-')
                        { // Turns preceding note into a flat.
                            --BaseNote;
                            ++CSCounter;
                        }
                        if (CommandString[CSCounter + 1] == '.')
                        { // Plays the preceding note 3/2 as long as specified.
                            BaseLength = (byte)(BaseLength / 2 * 3);
                            ++CSCounter;
                        }
                        this._PlayNote(BaseNote, this._PlayTempo, BaseLength);
                        break;
                    case 'L': // Sets the length of each note (1 - 64). L1 is whole note, L2 is half note, etc.
                        Digit1 = (byte)CommandString[CSCounter + 1];
                        Digit2 = (byte)CommandString[CSCounter + 2];
                        if (Digit2 > 47 && Digit2 < 58)
                        { // 48 = 0, 59 = 9
                            this._PlayLength = (byte)((Digit2 - 48) + ((Digit1 - 48) * 10));
                            CSCounter = CSCounter + 2;
                        }
                        else
                        {
                            this._PlayLength = (byte)(Digit1 - 48);
                            CSCounter = CSCounter + 1;
                        }
                        break;
                    case 'M': // Changes mode
                        ++CSCounter;
                        if (CommandString[CSCounter] == 'N') this._PlayMode = _PlayModes.Normal;
                        else if (CommandString[CSCounter] == 'L') this._PlayMode = _PlayModes.Legato;
                        else if (CommandString[CSCounter] == 'S') this._PlayMode = _PlayModes.Staccato;
                        else if (CommandString[CSCounter] == 'B')
                        {
                            this._PlayBackground(CommandString.Substring(CSCounter + 1));
                            return;
                        }
                        break;
                    case 'P': // Specifies a pause (1 - 64). P1 is a whole-note pause, P2 is a half-note pause, etc.
                        Digit1 = (byte)CommandString[CSCounter + 1];
                        Digit2 = (byte)CommandString[CSCounter + 2];
                        if (Digit2 > 47 && Digit2 < 58)
                        { // 48 = 0, 59 = 9
                            this._PlayNote(0, this._PlayTempo, (byte)((Digit2 - 48) + ((Digit1 - 48) * 10)));
                            CSCounter = CSCounter + 2;
                        }
                        else
                        {
                            this._PlayNote(0, this._PlayTempo, (byte)(Digit1 - 48));
                            CSCounter = CSCounter + 1;
                        }
                        break;
                    case 'T': // Sets the tempo in quarter notes per minute (32 - 255).
                        Digit1 = (byte)CommandString[CSCounter + 1];
                        Digit2 = (byte)CommandString[CSCounter + 2];
                        Digit3 = (byte)CommandString[CSCounter + 3];
                        if (Digit2 > 47 && Digit2 < 58 && Digit3 > 47 && Digit3 < 58)
                        { // 48 = 0, 59 = 9
                            this._PlayTempo = (byte)((Digit3 - 48) + ((Digit2 - 48) * 10) + ((Digit1 - 48) * 100));
                            CSCounter = CSCounter + 3;
                        }
                        else if (Digit2 > 47 && Digit2 < 58)
                        {
                            this._PlayTempo = (byte)((Digit2 - 48) + ((Digit1 - 48) * 10));
                            CSCounter = CSCounter + 2;
                        }
                        else
                        {
                            this._PlayTempo = (byte)(Digit1 - 48);
                            CSCounter = CSCounter + 1;
                        }
                        break;
                }

                // Triggers the event
                if (this.OnPlay != null) this.OnPlay(0, (uint)(CommandString.Length - CSCounter - 1), new DateTime());
            }
        }

        /// <summary>
        /// Plays a song in the background
        /// </summary>
        /// <param name="CommandString">The song to play</param>
        public void _PlayBackground(string CommandString)
        {
            // Creates the new thread
            Thread MusicThread = new Thread(new ThreadStart(delegate()
            {
                this.Play(CommandString);
            }));
            // Starts the new thread
            MusicThread.Start();
        }

        /// <summary>
        /// Plays a note
        /// </summary>
        /// <param name="Note">The note (0 to 84)</param>
        /// <param name="Tempo">The tempo in quarter notes per minute (32 - 255).</param>
        /// <param name="Length">The length of each note (1 - 64). L1 is whole note, L2 is half note, etc.</param>
        private void _PlayNote(byte Note, byte Tempo, byte Length)
        {
            // Do we actually need to play a note or is it just a rest?
            if (Note > 0)
            {
                uint Period = _Tonebar[Note];
                this._Speaker.SetPulse(Period, Period / 2);
                this._Speaker.StartPulse();
            }

            // Length of a full note in miliseconds (60 seconds in a minute, 1000ms in a second, 4 quarter notes in a full note)
            uint NoteMs = (uint)(60 * 1000 / Tempo * 4);

            // We define the duration of the tone and of the silence after the tone
            int ToneDuration = (int)(NoteMs / Length);
            int SilenceDuration = 0;

            // Wait for the right amount of time
            switch (this._PlayMode)
            {
                case _PlayModes.Staccato:
                    SilenceDuration = ToneDuration / 4;
                    ToneDuration = ToneDuration - SilenceDuration;
                    break;
                case _PlayModes.Normal:
                    SilenceDuration = ToneDuration / 8;
                    ToneDuration = ToneDuration - SilenceDuration;
                    break;
            }

            Thread.Sleep(ToneDuration);
            this._Speaker.StopPulse();
            Thread.Sleep(SilenceDuration);
        }

        /// <summary>
        /// Defines a speaker
        /// </summary>
        /// <param name="PwmPort">The PWM port the speaker is connected to</param>
        public Speaker(IPWMPort PwmPort)
        {
            this._Speaker = PwmPort;
        }

        /// <summary>
        /// Defines a speaker
        /// </summary>
        /// <param name="PwmPort">The PWM port the speaker is connected to</param>
        public Speaker(Cpu.Pin PwmPort)
        {
            this._Speaker = ProviderCollection.GetPWMByPin(PwmPort);
        }

#if MF_FRAMEWORK_VERSION_V4_2
        /// <summary>
        /// Defines a speaker
        /// </summary>
        /// <param name="PwmPort">The PWM port the speaker is connected to</param>
        public Speaker(Cpu.PWMChannel PwmPort)
        {
            this._Speaker = new IntegratedPWM(PwmPort);
        }
#endif

        /// <summary>
        /// Generates a sound through your speaker.
        /// </summary>
        /// <param name="Frequency">The frequency of the sound in hertz; a value in the range 37 through 32,767</param>
        /// <param name="Duration">The number of ticks the sound should last; a value in the range 0 through 65,535. There are 18.2 ticks per second.</param>
        public void Sound(float Frequency, float Duration)
        {
            if (Frequency < 37 || Frequency > 32767)
            {
                // Value should be between 37 and 32767
                throw new InvalidOperationException();
            }
            if (Duration < 0 || Duration > 65535)
            {
                // Value should be between 0 and 65535
                throw new InvalidOperationException();
            }

            uint PeriodNs = (uint)(1000000000 / Frequency);
            int Timeout = (int)(Duration * (1000 / 18.2f));

            // Sets the tone
            this._Speaker.SetPulse(PeriodNs, PeriodNs / 2);
            this._Speaker.StartPulse();
            // Waits for 'timeout'
            Thread.Sleep(Timeout);
            // Resets the tone
            this._Speaker.StopPulse();
        }

        /// <summary>
        /// Generates a beep sound from your speaker.
        /// </summary>
        public void Beep()
        {
            this.Sound(1000, 5);
        }

        /// <summary>Disposes this object</summary>
        public void Dispose()
        {
            this._Speaker.Dispose();
        }
    }
}
