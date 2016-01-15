using System;
using System.Threading;
using Microsoft.SPOT;
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
    /// Parallax NES Controller Adapter driver
    /// </summary>
    public class NESControllerAdapter : IDisposable
    {
        /// <summary>Reference to the clock pin</summary>
        private OutputPort _Clk;
        /// <summary>Reference to the latch pin</summary>
        private OutputPort _Latch;
        /// <summary>Reference to the data pin for socket 1</summary>
        private InputPort _Data1;
        /// <summary>Reference to the data pin for socket 2</summary>
        private InputPort _Data2;

        /// <summary>Triggered when a button state changes</summary>
        public event ButtonChanged OnButtonChanged;
        /// <summary>Triggered when a socket state changes</summary>
        public event SocketChanged OnSocketChanged;

        /// <summary>Last state for socket 1</summary>
        private bool[] _LastSocket1 = new bool[8];
        /// <summary>Last state for socket 2</summary>
        private bool[] _LastSocket2 = new bool[8];
        /// <summary>Last scan value</summary>
        private ushort _LastScanValue = 0;

        /// <summary>Contains a reference to the interrupt thread</summary>
        private ThreadStart _InterruptStarter;

        /// <summary>When true, events are enabled</summary>
        private bool _EventsEnabled = false;
        /// <summary>When true, events are enabled</summary>
        public bool EventsEnabled
        {
            get { return this._EventsEnabled; }
            set
            {
                if (value && !this._EventsEnabled)
                {
                    this._EventsEnabled = true;
                    Thread InterruptThread = new Thread(this._InterruptStarter);
                    InterruptThread.Start();
                }
                else
                    this._EventsEnabled = value;
            }
        }

        /// <summary>Checks if a socket is active</summary>
        /// <param name="Socket"></param>
        /// <returns></returns>
        public bool SocketConnected(Socket Socket)
        {
            // Reads the sockets
            bool[] Socket1;
            bool[] Socket2;
            this.Read(out Socket1, out Socket2);
            
            // Checks the scan values
            if (Socket == NESControllerAdapter.Socket.Socket1) return ((this._LastScanValue & 0x00ff) != 0x00ff);
            else if (Socket == NESControllerAdapter.Socket.Socket2) return ((this._LastScanValue & 0xff00) != 0xff00);

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>List of the game sockets</summary>
        public enum Socket
        {
            /// <summary>Socket 1</summary>
            Socket1 = 1,
            /// <summary>Socket 2</summary>
            Socket2 = 2
        }

        /// <summary>List of the buttons on a keypad</summary>
        public enum Button
        {
            /// <summary>Button A</summary>
            Button_A = 0,
            /// <summary>Button B</summary>
            Button_B = 1,
            /// <summary>Select Button</summary>
            Button_Select = 2,
            /// <summary>Start Button</summary>
            Button_Start = 3,
            /// <summary>Up Button</summary>
            Button_Up = 4,
            /// <summary>Down Button</summary>
            Button_Down = 5,
            /// <summary>Left Button</summary>
            Button_Left = 6,
            /// <summary>Right Button</summary>
            Button_Right = 7
        }

        /// <summary>
        /// Initialises the Parallax NES Controller Adapter
        /// </summary>
        /// <param name="Clk">Clock pin</param>
        /// <param name="Latch">Latch pin</param>
        /// <param name="Data1">Data pin for socket 1</param>
        /// <param name="Data2">Data pin for socket 2 (optional)</param>
        public NESControllerAdapter(Cpu.Pin Clk, Cpu.Pin Latch, Cpu.Pin Data1, Cpu.Pin Data2 = Cpu.Pin.GPIO_NONE)
        {
            // Binds to all pins
            this._Clk = new OutputPort(Clk, false);
            this._Latch = new OutputPort(Latch, false);
            this._Data1 = new InputPort(Data1, false, Port.ResistorMode.Disabled);
            if (Data2 != Cpu.Pin.GPIO_NONE) this._Data2 = new InputPort(Data2, false, Port.ResistorMode.Disabled);

            // Creates a background thread for interrupt checking
            this._InterruptStarter = new ThreadStart(this._ScanThread);
        }

        /// <summary>Frees all pins</summary>
        public void Dispose()
        {
            this._EventsEnabled = false;
            this._Clk.Dispose();
            this._Latch.Dispose();
            this._Data1.Dispose();
            if (this._Data2 != null) this._Data2.Dispose();
        }

        /// <summary>Scans both sockets while events are enabled</summary>
        private void _ScanThread()
        {
            bool[] Socket1 = new bool[8];
            bool[] Socket2 = new bool[8];
            ushort Changes = 0;
            while (this._EventsEnabled)
            {
                // Reads out all values
                Changes = this._Read(out Socket1, out Socket2);
                // No changes, lets continue reading
                if (Changes == 0) continue;

                bool Socket1Changed = false;
                bool Socket2Changed = false;
                for (int ButtonIndex = 0; ButtonIndex < 8; ++ButtonIndex)
                {
                    // Is this button changed on socket 1?
                    if (this.SocketConnected(Socket.Socket1))
                    {
                        if ((Changes & (1 << ButtonIndex)) != 0)
                        {
                            if (this.OnButtonChanged != null) this.OnButtonChanged(this, Socket.Socket1, (Button)ButtonIndex, Socket1[ButtonIndex], DateTime.Now);
                            Socket1Changed = true;
                        }
                    }
                    // Is this button changed on socket 2?
                    if (this.SocketConnected(Socket.Socket2))
                    {
                        if ((Changes & (1 << (8 + ButtonIndex))) != 0)
                        {
                            if (this.OnButtonChanged != null) this.OnButtonChanged(this, Socket.Socket2, (Button)ButtonIndex, Socket2[ButtonIndex], DateTime.Now);
                            Socket2Changed = true;
                        }
                    }
                }
                // Is one of the sockets changed?
                if (Socket1Changed && this.OnSocketChanged != null) this.OnSocketChanged(this, Socket.Socket1, Socket1, DateTime.Now);
                if (Socket2Changed && this.OnSocketChanged != null) this.OnSocketChanged(this, Socket.Socket2, Socket2, DateTime.Now);
            }
        }

        /// <summary>Puts a pin high for a short amount of time</summary>
        /// <param name="Pin">The pin to put high</param>
        private void _PinTick(OutputPort Pin)
        {
            Pin.Write(true);
            Pin.Write(false);
        }

        /// <summary>Reads the boolean values of all buttons</summary>
        /// <param name="Socket1">8 boolean values for socket 1: [A,B,Select,Start,Up,Down,Left,Right]</param>
        public void Read(out bool[] Socket1)
        {
            bool[] Socket2;
            this.Read(out Socket1, out Socket2);
        }

        /// <summary>Reads the boolean values of all buttons</summary>
        /// <param name="Socket1">8 boolean values for socket 1: [A,B,Select,Start,Up,Down,Left,Right]</param>
        /// <param name="Socket2">8 boolean values for socket 2: [A,B,Select,Start,Up,Down,Left,Right]</param>
        public void Read(out bool[] Socket1, out bool[] Socket2)
        {
            if (this._EventsEnabled)
            {
                Socket1 = this._LastSocket1;
                Socket2 = this._LastSocket2;
            }
            else
                this._Read(out Socket1, out Socket2);
        }

        /// <summary>Reads the state of a single button</summary>
        /// <param name="Socket">The socket</param>
        /// <param name="Button">The button</param>
        /// <returns>True when pressed</returns>
        public bool ButtonState(Socket Socket, Button Button)
        {
            bool[] Socket1;
            bool[] Socket2;
            this.Read(out Socket1, out Socket2);

            if (Socket == NESControllerAdapter.Socket.Socket1)
                return Socket1[(int)Button];
            else
                return Socket2[(int)Button];
        }

        /// <summary>Reads the boolean values of all buttons</summary>
        /// <param name="Socket1">8 boolean values for socket 1: [A,B,Select,Start,Up,Down,Left,Right]</param>
        /// <param name="Socket2">8 boolean values for socket 2: [A,B,Select,Start,Up,Down,Left,Right]</param>
        /// <returns>0 when there are no changes since the last Read() call, otherwise the bit of the button is set</returns>
        private ushort _Read(out bool[] Socket1, out bool[] Socket2)
        {
            Socket1 = new bool[8];
            Socket2 = new bool[8];

            ushort Changes = 0;
            ushort ScanValue = 0;

            // Locks all values
            this._PinTick(this._Latch);

            // Reads both sockets
            for (int i = 0; i < 8; ++i)
            {
                // Reads out the value for both sockets
                Socket1[i] = !this._Data1.Read();
                if (this._Data2 != null) Socket2[i] = !this._Data2.Read();

                // Selects the next value
                this._PinTick(this._Clk);

                // Stores the value
                if (Socket1[i]) ScanValue += (ushort)(1 << i);
                if (Socket2[i]) ScanValue += (ushort)(1 << (i + 8));

                // Looks for changes
                if (Socket1[i] != this._LastSocket1[i]) Changes += (ushort)(1 << i);
                if (Socket2[i] != this._LastSocket2[i]) Changes += (ushort)(1 << (i + 8));
                this._LastSocket1[i] = Socket1[i];
                this._LastSocket2[i] = Socket2[i];
            }

            this._LastScanValue = ScanValue;
            return Changes;
        }

        /// <summary>Button changed event</summary>
        /// <param name="This">The current object</param>
        /// <param name="Socket">The changed socket</param>
        /// <param name="Button">The changed button</param>
        /// <param name="Value">The new value</param>
        /// <param name="Time">Time of the event</param>
        public delegate void ButtonChanged(NESControllerAdapter This, NESControllerAdapter.Socket Socket, NESControllerAdapter.Button Button, bool Value, DateTime Time);

        /// <summary>Socket changed event</summary>
        /// <param name="This">The current object</param>
        /// <param name="Socket">The changed socket</param>
        /// <param name="ButtonStates">8 boolean values for the socket: [A,B,Select,Start,Up,Down,Left,Right]</param>
        /// <param name="Time">Time of the event</param>
        public delegate void SocketChanged(NESControllerAdapter This, NESControllerAdapter.Socket Socket, bool[] ButtonStates, DateTime Time);
    }
}
