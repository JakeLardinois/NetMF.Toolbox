using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using Toolbox.NETMF;
using Toolbox.NETMF.NET;

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
namespace Programs
{
    /// <summary>
    /// Some generic shell commands
    /// </summary>
    public static class Pins
    {

        /// <summary>Reference to all pins as MultiPort classes</summary>
        private static MultiPort[] _Pins;
        /// <summary>Names for all pins</summary>
        private static string[] _PinLabels;

        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        /// <param name="PinCount">The amount of broken out pins</param>
        public static void Bind(ShellCore Shell, int PinCount)
        {
            _PinLabels = new string[PinCount];
            _Pins = new MultiPort[PinCount];
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Unbinds from the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Unbind(ShellCore Shell)
        {
            Shell.OnCommandReceived -= new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Configures a pin
        /// </summary>
        /// <param name="Index">The index</param>
        /// <param name="Pin">The Cpu.Pin</param>
        /// <param name="Label">Name for the pin</param>
        public static void ConfigurePin(int Index, Cpu.Pin Pin, string Label)
        {
            if (_Pins[Index] != null) throw new InvalidOperationException("That index is already defined");
            _PinLabels[Index] = Label;
            _Pins[Index] = new MultiPort(Pin);
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        private static void Shell_OnCommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time)
        {
            switch (Arguments[0].ToUpper())
            {
                case "LISTPINS":
                    string RetVal = "";
                    for (int i = 0; i < _PinLabels.Length; ++i)
                        RetVal += _PinLabels[i] + " ";
                    Shell.TelnetServer.Print(RetVal);
                    SuspressError = true;
                    break;
                case "CONFIGPIN":
                    if (Arguments.Length != 3)
                        throw new ArgumentException("Need 2 parameters, see HELP CONFIGPIN for more info.");
                    else
                        _ConfigPin(Shell.TelnetServer, Arguments[1].ToUpper(), Arguments[2].ToUpper());
                    SuspressError = true;
                    break;
                case "READPIN":
                    if (Arguments.Length != 2)
                        throw new ArgumentException("Need 1 parameter, see HELP READPIN for more info.");
                    else
                        _ReadPin(Shell.TelnetServer, Arguments[1].ToUpper());
                    SuspressError = true;
                    break;
                case "GETPINTYPE":
                    if (Arguments.Length != 2)
                        throw new ArgumentException("Need 1 parameter, see HELP GETPINTYPE for more info.");
                    else
                        _Type(Shell.TelnetServer, Arguments[1].ToUpper());
                    SuspressError = true;
                    break;
                case "WRITEPIN":
                    if (Arguments.Length != 3)
                        throw new ArgumentException("Need 2 parameters, see HELP WRITEPIN for more info.");
                    else
                        _WritePin(Shell.TelnetServer, Arguments[1].ToUpper(), Arguments[2].ToUpper());
                    SuspressError = true;
                    break;
                case "DUTYCYCLE":
                    if (Arguments.Length != 3)
                        throw new ArgumentException("Need 2 parameters, see HELP DUTYCYCLE for more info.");
                    else
                        _DutyCycle(Shell.TelnetServer, Arguments[1].ToUpper(), Arguments[2].ToUpper());
                    SuspressError = true;
                    break;
                case "SETPULSE":
                    if (Arguments.Length < 4)
                        throw new ArgumentException("Need 3 parameters, see HELP SETPULSE for more info.");
                    else
                        _SetPulse(Shell.TelnetServer, Arguments[1].ToUpper(), Arguments[2].ToUpper(), Arguments[3].ToUpper());
                    SuspressError = true;
                    break;
                case "HELP":
                    bool PageFound = false;
                    if (Arguments.Length == 1) PageFound = DoHelp(Shell.TelnetServer, "");
                    else PageFound = DoHelp(Shell.TelnetServer, Arguments[1].ToUpper());
                    if (PageFound) SuspressError = true;
                    break;
            }
        }

        /// <summary>
        /// Shows a specific help page
        /// </summary>
        /// <param name="Server">The telnet server object</param>
        /// <param name="Page">The page</param>
        /// <returns>True when the page exists</returns>
        private static bool DoHelp(TelnetServer Server, string Page)
        {
            switch (Page)
            {
                case "":
                    Server.Print("LISTPINS                           Shows a list of all pins");
                    Server.Print("CONFIG [PIN] [TYPE]                Configures a pin");
                    Server.Print("READPIN [PIN]                      Reads a pin value");
                    Server.Print("GETPINTYPE [PIN]                   Reads a pin type");
                    Server.Print("WRITEPIN [PIN] [VALUE]             Writes a pin value");
                    Server.Print("DUTYCYCLE [PIN] [DUTYCYCLE]        Sets the PWM dutycycle");
                    Server.Print("SETPULSE [PIN] [PULSE] [DURATION]  Sets the PWM pulse");
                    return true;
                case "LISTPINS":
                    Server.Print("LISTPINS                           Shows a list of all pins");
                    return true;
                case "CONFIG":
                    Server.Print("CONFIG [PIN] [TYPE]                Configures a pin");
                    Server.Print("- [PIN]       The name of the pin");
                    Server.Print("- [TYPE]      Can be: GPI, GPO, PWM, ADC or None");
                    return true;
                case "READPIN":
                    Server.Print("READPIN [PIN]                      Reads a pin value");
                    Server.Print("- [PIN]       The name of the pin");
                    return true;
                case "GETPINTYPE":
                    Server.Print("GETPINTYPE [PIN]                   Reads a pin type");
                    Server.Print("- [PIN]       The name of the pin");
                    return true;
                case "WRITEPIN":
                    Server.Print("WRITEPIN [PIN] [VALUE]             Writes a pin value");
                    Server.Print("- [PIN]       The name of the pin");
                    Server.Print("- [VALUE]     1 for high, 0 for low");
                    return true;
                case "DUTYCYCLE":
                    Server.Print("DUTYCYCLE [PIN] [DUTYCYCLE]        Sets the PWM dutycycle");
                    Server.Print("- [PIN]       The name of the pin");
                    Server.Print("- [DUTYCYCLE] The PWM dutycycle (0 to 100)");
                    return true;
                case "SETPULSE":
                    Server.Print("SETPULSE [PIN] [PULSE] [DURATION]  Sets the PWM pulse");
                    Server.Print("- [PIN]       The name of the pin");
                    Server.Print("- [PULSE]     PWM pulse");
                    Server.Print("- [DURATION]  PWM duration");
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Converts a pin name to its index</summary>
        /// <param name="Pin">The pin name</param>
        /// <returns>The index</returns>
        private static int _PinToIndex(string Pin)
        {
            for (int i = 0; i < _PinLabels.Length; ++i)
            {
                if (_PinLabels[i] == Pin) return i;
            }
            return -1;
        }

        /// <summary>
        /// Changes the dutycycle
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        /// <param name="Dutycycle">Dutycycle</param>
        private static void _DutyCycle(TelnetServer Server, string Pin, string Dutycycle)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }
            int Value = int.Parse(Dutycycle);

            _Pins[PinIndex].Write((uint)Value);
            Server.Print("Dutycycle for pin " + Pin + " set to " + Dutycycle);
        }

        /// <summary>
        /// Changes the pwm pulse
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        /// <param name="Pulse">Pulse</param>
        /// <param name="Duration">Duration</param>
        private static void _SetPulse(TelnetServer Server, string Pin, string Pulse, string Duration)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }
            int iPulse = int.Parse(Pulse);
            int iDuration = int.Parse(Duration);

            _Pins[PinIndex].Write((uint)iPulse, (uint)iDuration);
            Server.Print("Pulse for pin " + Pin + " set to " + Pulse + "," + Duration);
        }

        /// <summary>
        /// Writes a digital pin
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        /// <param name="Value">Value</param>
        private static void _WritePin(TelnetServer Server, string Pin, string Value)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }
            // Parses the value
            int iValue = 0;
            if (Value == "TRUE") iValue = 1;
            else if (Value == "FALSE") iValue = 0;
            else if (Value == "1") iValue = 1;
            else if (Value == "0") iValue = 0;
            else throw new ArgumentException("Invalid value: " + Value);
            // Sets the value
            _Pins[PinIndex].Write(iValue == 1 ? true : false);
            // Responds with the new value
            _ReadPin(Server, Pin);
        }

        /// <summary>
        /// Reads a pin type
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        private static void _Type(TelnetServer Server, string Pin)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }
            Server.Print("Pin " + Pin + " has type: " + _Pins[PinIndex].ToString());
        }

        /// <summary>
        /// Reads a pin
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        private static void _ReadPin(TelnetServer Server, string Pin)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }
            Server.Print("Pin " + Pin + " has value: " + _Pins[PinIndex].Read().ToString());
        }

        /// <summary>
        /// Configures a pin
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pin">Pin</param>
        /// <param name="Type">Type</param>
        private static void _ConfigPin(TelnetServer Server, string Pin, string Type)
        {
            int PinIndex = _PinToIndex(Pin);
            if (PinIndex == -1) { throw new ArgumentException("Invalid pin name: " + Pin); }

            switch (Type)
            {
                case "GPI":
                    _Pins[PinIndex].ConfigureInput();
                    Server.Print("Pin " + Pin + " is now a digital input port");
                    break;
                case "GPO":
                    _Pins[PinIndex].ConfigureOutput();
                    Server.Print("Pin " + Pin + " is now a digital output port");
                    break;
                case "PWM":
                    _Pins[PinIndex].ConfigurePWM();
                    Server.Print("Pin " + Pin + " is now a PWM port");
                    break;
                case "ADC":
                    _Pins[PinIndex].ConfigureAnalogIn();
                    Server.Print("Pin " + Pin + " is now an analog input port");
                    break;
                case "NONE":
                    _Pins[PinIndex].ReleasePin();
                    Server.Print("Pin " + Pin + " is now released");
                    break;
                default:
                    throw new ArgumentException("Type " + Type + " unknown");
            }
        }

        #region "MultiPort class"
        private class MultiPort
        {
            /// <summary>
            /// Available port types
            /// </summary>
            public enum PortType
            {
                /// <summary>General Purpose Input port</summary>
                Input,
                /// <summary>General Purpose Output port</summary>
                Output,
                /// <summary>Pulse-Width Modulated port</summary>
                PWM,
                /// <summary>Analog Input port</summary>
                AnalogIn,
                /// <summary>Port currently not in use</summary>
                None
            }

            public override string ToString()
            {
                if (this.Type == PortType.Input) return this._In.ToString();
                if (this.Type == PortType.Output) return this._Out.ToString();
                if (this.Type == PortType.PWM) return this._Pwm.ToString();
                if (this.Type == PortType.AnalogIn) return this._Adc.ToString();
                return base.ToString();
            }

            /// <summary>The pin</summary>
            public Cpu.Pin Pin { get; protected set; }

            /// <summary>Reference to the InputPort class</summary>
            private InputPort _In = null;
            /// <summary>Reference to the OutputPort class</summary>
            private OutputPort _Out = null;
            /// <summary>Reference to the PWM class</summary>
            private PWM _Pwm = null;
            /// <summary>Reference to the AnalogInput class</summary>
            private SecretLabs.NETMF.Hardware.AnalogInput _Adc = null;

            /// <summary>
            /// Returns the current port type
            /// </summary>
            public PortType Type { get; protected set; }

            /// <summary>
            /// Creates a multiport
            /// </summary>
            /// <param name="Pin">The pin</param>
            public MultiPort(Cpu.Pin Pin)
            {
                // Copies the pin value locally
                this.Pin = Pin;
                // Configures the pin as no pin type
                this.Type = PortType.None;
            }

            /// <summary>
            /// Releases the pin
            /// </summary>
            public void ReleasePin()
            {
                if (this.Type == PortType.AnalogIn) { this._Adc.Dispose(); this._Adc = null; }
                if (this.Type == PortType.Input) { this._In.Dispose(); this._In = null; }
                if (this.Type == PortType.Output) { this._Out.Dispose(); this._Out = null; }
                if (this.Type == PortType.PWM) { this._Pwm.Dispose(); this._Pwm = null; }
                this.Type = PortType.None;
            }

            /// <summary>
            /// Configures the pin as digital input
            /// </summary>
            /// <param name="GlitchFilter">Enables the glitch filter</param>
            /// <param name="Resistor">Enables resistor modes</param>
            public void ConfigureInput(bool GlitchFilter = false, Port.ResistorMode Resistor = Port.ResistorMode.Disabled)
            {
                this.ReleasePin();
                this._In = new InputPort(this.Pin, GlitchFilter, Resistor);
                this.Type = PortType.Input;
            }

            /// <summary>
            /// Configures the pin as digital output
            /// </summary>
            /// <param name="initialState">Initial state</param>
            public void ConfigureOutput(bool initialState = false)
            {
                this.ReleasePin();
                this._Out = new OutputPort(this.Pin, initialState);
                this.Type = PortType.Output;
            }

            /// <summary>
            /// Configures the pin as PWM output
            /// </summary>
            public void ConfigurePWM()
            {
                this.ReleasePin();
                this._Pwm = new PWM(this.Pin);
                this.Type = PortType.PWM;
            }

            /// <summary>
            /// Configures the pin as analog input
            /// </summary>
            public void ConfigureAnalogIn()
            {
                this.ReleasePin();
                this._Adc = new SecretLabs.NETMF.Hardware.AnalogInput(this.Pin);
                this.Type = PortType.AnalogIn;
            }

            /// <summary>
            /// Reads the pin value
            /// </summary>
            /// <returns>1 or 0 for boolean values, or an analog value</returns>
            public int Read()
            {
                if (this.Type == PortType.Input) return this._In.Read() ? 1 : 0;
                else if (this.Type == PortType.Output) return this._Out.Read() ? 1 : 0;
                else if (this.Type == PortType.AnalogIn) return this._Adc.Read();
                else throw new NotSupportedException("The current port type does not support this action");
            }

            /// <summary>
            /// Writes a value to the pin
            /// </summary>
            /// <param name="Value">Boolean, high or low</param>
            public void Write(bool Value)
            {
                if (this.Type == PortType.Output) this._Out.Write(Value);
                else throw new NotSupportedException("The current port type does not support this action");
            }

            /// <summary>
            /// Writes a value to the pin
            /// </summary>
            /// <param name="DutyCycle">PWM Dutycycle</param>
            public void Write(uint DutyCycle)
            {
                if (this.Type == PortType.PWM) this._Pwm.SetDutyCycle(DutyCycle);
                else throw new NotSupportedException("The current port type does not support this action");
            }

            /// <summary>
            /// Writes a value to the pin
            /// </summary>
            /// <param name="Period">PWM Period in µs</param>
            /// <param name="Duration">PWM Duration in µs</param>
            public void Write(uint Period, uint Duration)
            {
                if (this.Type == PortType.PWM) this._Pwm.SetPulse(Period, Duration);
                else throw new NotSupportedException("The current port type does not support this action");
            }
        }
        #endregion
    }
}
