using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// SOMO-14D Driver
    /// </summary>
    public class Somo
    {
        /// <summary>Reference to the data pin</summary>
        private OutputPort _DataPin = null;
        /// <summary>Reference to the clock pin</summary>
        private OutputPort _ClockPin = null;
        /// <summary>Reference to the busy pin</summary>
        private InterruptPort _BusyPin = null;

        /// <summary>Event is triggered when a track starts playing</summary>
        public event NativeEventHandler OnStartPlaying;
        /// <summary>Event is triggered when a track stops playing</summary>
        public event NativeEventHandler OnStopPlaying;

        /// <summary>Stores the currently selected track</summary>
        private ushort _CurrentTrack = 0;

        // Used when we're using the Busy pin
        private bool _PlayingStarted = false;
        private bool _PlayingStopped = false;
        private bool _Repeating = false;

        /// <summary>
        /// Returns true when the module is playing. Requires the Busy signal.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (this._BusyPin == null) throw new InvalidOperationException("Can't use IsPlaying when no Busy pin is defined");
                if (this._PlayingStarted && !this._PlayingStopped) return true;
                return false;
            }
        }

        /// <summary>
        /// The currently selected track
        /// </summary>
        private ushort CurrentTrack { get { return this._CurrentTrack; } }

        /// <summary>
        /// New SOMO-14D Module
        /// </summary>
        /// <param name="Clock">The pin for the clock signal</param>
        /// <param name="Data">The pin for the data signal</param>
        /// <param name="Busy">The pin for the busy signal (optional)</param>
        public Somo(Cpu.Pin Clock, Cpu.Pin Data, Cpu.Pin Busy = Cpu.Pin.GPIO_NONE)
        {
            this._DataPin = new OutputPort(Data, false);
            this._ClockPin = new OutputPort(Clock, false);

            if (Busy != Cpu.Pin.GPIO_NONE)
            {
                this._BusyPin = new InterruptPort(Busy, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
                this._BusyPin.OnInterrupt += new NativeEventHandler(_BusyPin_OnInterrupt);
            }
        }

        /// <summary>
        /// The busy pin interrupt has been triggered
        /// </summary>
        /// <param name="PinId">The Id of the pin</param>
        /// <param name="Value">The current value</param>
        /// <param name="Time">Timestamp of the event</param>
        private void _BusyPin_OnInterrupt(uint PinId, uint Value, DateTime Time)
        {
            if (Value == 1)
            {
                this._PlayingStarted = true;
                if (this.OnStartPlaying != null)
                    this.OnStartPlaying(PinId, Value, Time);
            }
            else if (Value == 0)
            {
                this._PlayingStopped = true;
                if (this.OnStopPlaying != null)
                    this.OnStopPlaying(PinId, Value, Time);
                if (this._Repeating)
                {
                    Thread.Sleep(10);
                    this.PlayTrack(this._CurrentTrack, false);
                }
            }
        }

        /// <summary>
        /// Plays a track infinitely in the backround, until we get a Stop command
        /// </summary>
        /// <param name="TrackNo"></param>
        public void PlayRepeat(ushort TrackNo)
        {
            this._Repeating = true;
            this.PlayTrack(TrackNo, false);
        }

        /// <summary>
        /// Plays a specific track
        /// </summary>
        /// <param name="TrackNo">The number of the track (0 to 511)</param>
        /// <param name="Wait">When the Busy pin is defined, and this is true, this method won't return until the track is finished.</param>
        public void PlayTrack(ushort TrackNo, bool Wait = false)
        {
            // Is this a valid track?
            if (TrackNo > 511) throw new ArgumentOutOfRangeException("TrackNo can be 0 to 511");
            // We haven't started, nor stopped yet
            this._CurrentTrack = TrackNo;
            this._PlayingStarted = false;
            this._PlayingStopped = false;
            // Start the track
            this._SendByte(TrackNo);

            if (Wait)
            {
                if (this._BusyPin == null) throw new ArgumentException("Can't use Wait = True when no busy pin is defined");
                // Wait until it's started
                while (!this._PlayingStarted) { Thread.Sleep(1); }
                // Now wait until it's stopped
                while (!this._PlayingStopped) { Thread.Sleep(1); }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Sets the volume
        /// </summary>
        /// <param name="Volume">The volume (0 to 7)</param>
        public void SetVolume(sbyte Volume)
        {
            if (Volume > 7) throw new ArgumentOutOfRangeException("Volume can be 0 to 7");
            this._SendByte((ushort)(0xfff0 + Volume));
        }

        /// <summary>
        /// Plays or pauses the current track
        /// </summary>
        public void PlayPause()
        {
            this._SendByte(0xfffe);
        }

        /// <summary>
        /// Stops the current track
        /// </summary>
        public void Stop()
        {
            this._Repeating = false;
            this._SendByte(0xffff);
        }

        /// <summary>
        /// Sends a byte, bitbanged over the serial protocol
        /// </summary>
        /// <param name="Byte">The 16-bit byte to send</param>
        private void _SendByte(ushort Byte)
        {
            for (ushort Bit = 0x8000; Bit > 0; Bit >>= 1)
            {
                // Sets the pin to the current bit value
                this._DataPin.Write(((Byte & Bit) == Bit));
                // Pulses the clock
                this._ClockPin.Write(true);
                Thread.Sleep(1);
                this._ClockPin.Write(false);
                Thread.Sleep(1);
            }
            // Reset the data pin
            this._DataPin.Write(false);
            // Give it some time before we send in the next command
            Thread.Sleep(10);
        }
    }
}
