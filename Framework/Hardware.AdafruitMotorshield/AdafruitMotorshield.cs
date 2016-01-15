using System;
using Microsoft.SPOT.Hardware;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://netmftoolbox.codeplex.com/)
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
    /// Adafruit Motor Control Shield
    /// </summary>
    public class AdafruitMotorshield : IDisposable
    {
        /// <summary>
        /// Contains all motors
        /// </summary>
        public enum Motors
        {
            /// <summary>First motor</summary>
            Motor1 = 1,
            /// <summary>Second motor</summary>
            Motor2 = 2,
            /// <summary>Third motor</summary>
            Motor3 = 3,
            /// <summary>Forth motor</summary>
            Motor4 = 4
        }

        /// <summary>Reference to the Enable Pin</summary>
        private OutputPort _EnablePin;
        /// <summary>Pin A for motor 1</summary>
        private IGPOPort _Motor1aPin;
        /// <summary>Pin B for motor 1</summary>
        private IGPOPort _Motor1bPin;
        /// <summary>PWM Pin for motor 1</summary>
        private IPWMPort _Motor1Pwm = null;
        /// <summary>Pin A for motor 2</summary>
        private IGPOPort _Motor2aPin;
        /// <summary>Pin B for motor 2</summary>
        private IGPOPort _Motor2bPin;
        /// <summary>PWM Pin for motor 2</summary>
        private IPWMPort _Motor2Pwm = null;
        /// <summary>Pin A for motor 3</summary>
        private IGPOPort _Motor3aPin;
        /// <summary>Pin B for motor 3</summary>
        private IGPOPort _Motor3bPin;
        /// <summary>PWM Pin for motor 3</summary>
        private IPWMPort _Motor3Pwm = null;
        /// <summary>Pin A for motor 4</summary>
        private IGPOPort _Motor4aPin;
        /// <summary>Pin B for motor 4</summary>
        private IGPOPort _Motor4bPin;
        /// <summary>PWM Pin for motor 4</summary>
        private IPWMPort _Motor4Pwm = null;

        /// <summary>Reference to the one 74HC595 IC</summary>
        private Ic74hc595 _IcOut;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClockPin">SPI Clock pin</param>
        /// <param name="EnablePin">SPI Enable pin</param>
        /// <param name="DataPin">SPI Data pin</param>
        /// <param name="LatchPin">SPI Latch pin</param>
        /// <param name="Motor1Pwm">Motor 1 PWM pin</param>
        /// <param name="Motor2Pwm">Motor 2 PWM pin</param>
        /// <param name="Motor3Pwm">Motor 3 PWM pin</param>
        /// <param name="Motor4Pwm">Motor 4 PWM pin</param>
        public AdafruitMotorshield(
            Cpu.Pin ClockPin, Cpu.Pin EnablePin, Cpu.Pin DataPin, Cpu.Pin LatchPin,
            IPWMPort Motor1Pwm, IPWMPort Motor2Pwm, IPWMPort Motor3Pwm, IPWMPort Motor4Pwm
        )
        {
            // This one should always be false
            this._EnablePin = new OutputPort(EnablePin, false);

            // Defines the 74HC595 chip by bitbanging
            this._IcOut = new Ic74hc595(ClockPin, DataPin, LatchPin);

            // Defines all 8 pins on the 74HC595
            this._Motor1aPin = this._IcOut.Pins[2]; // M1A
            this._Motor1bPin = this._IcOut.Pins[3]; // M1B
            this._Motor2aPin = this._IcOut.Pins[1]; // M2A
            this._Motor2bPin = this._IcOut.Pins[4]; // M3B
            this._Motor4aPin = this._IcOut.Pins[0]; // M4A
            this._Motor4bPin = this._IcOut.Pins[6]; // M4B
            this._Motor3aPin = this._IcOut.Pins[5]; // M3A
            this._Motor3bPin = this._IcOut.Pins[7]; // M3B

            // Motor PWM pins
            this._Motor1Pwm = Motor1Pwm;              // PWM2A
            this._Motor2Pwm = Motor2Pwm;              // PWM2B
            this._Motor3Pwm = Motor3Pwm;              // PWM0A
            this._Motor4Pwm = Motor4Pwm;              // PWM0B

            if (this._Motor1Pwm != null) { this._Motor1Pwm.SetDutyCycle(0); this._Motor1Pwm.StartPulse(); }
            if (this._Motor2Pwm != null) { this._Motor2Pwm.SetDutyCycle(0); this._Motor2Pwm.StartPulse(); }
            if (this._Motor3Pwm != null) { this._Motor3Pwm.SetDutyCycle(0); this._Motor3Pwm.StartPulse(); }
            if (this._Motor4Pwm != null) { this._Motor4Pwm.SetDutyCycle(0); this._Motor4Pwm.StartPulse(); }
        }

        /// <summary>
        /// Sets the state of a motor
        /// </summary>
        /// <param name="Motor">The motor to change</param>
        /// <param name="Speed">The speed to move with; -100 (full speed backward) to 100 (full speed forward)</param>
        public void SetState(Motors Motor, sbyte Speed)
        {
            // We expect a value between -100 and 100
            if (Speed < -100 || Speed > 100)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            // Gets the direction straight
            bool PinA = false;
            bool PinB = true;
            if (Speed < 0)
            {
                // Makes it positive again
                Speed = (sbyte)(0 - Speed);
                PinA = true;
                PinB = false;
            }
            else if (Speed == 0)
            {
                // No speed at all
                PinA = false;
                PinB = false;
            }

            switch (Motor)
            {
                case Motors.Motor1:
                    if (this._Motor1Pwm == null) throw new InvalidOperationException("Can't drive motor 1 without PWM pin");
                    this._Motor1aPin.Write(PinA);
                    this._Motor1bPin.Write(PinB);
                    this._Motor1Pwm.SetDutyCycle((uint)Speed);
                    break;
                case Motors.Motor2:
                    if (this._Motor2Pwm == null) throw new InvalidOperationException("Can't drive motor 2 without PWM pin");
                    this._Motor2aPin.Write(PinA);
                    this._Motor2bPin.Write(PinB);
                    this._Motor2Pwm.SetDutyCycle((uint)Speed);
                    break;
                case Motors.Motor3:
                    if (this._Motor3Pwm == null) throw new InvalidOperationException("Can't drive motor 3 without PWM pin");
                    this._Motor3aPin.Write(PinA);
                    this._Motor3bPin.Write(PinB);
                    this._Motor3Pwm.SetDutyCycle((uint)Speed);
                    break;
                case Motors.Motor4:
                    if (this._Motor4Pwm == null) throw new InvalidOperationException("Can't drive motor 4 without PWM pin");
                    this._Motor4aPin.Write(PinA);
                    this._Motor4bPin.Write(PinB);
                    this._Motor4Pwm.SetDutyCycle((uint)Speed);
                    break;
                default:
                    // Should never happen!! :-)
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Disposes the AdafruitMotorshield object
        /// </summary>
        public void Dispose()
        {
            this._Motor1aPin.Dispose();
            this._Motor1bPin.Dispose();
            this._Motor2aPin.Dispose();
            this._Motor2bPin.Dispose();
            this._Motor3aPin.Dispose();
            this._Motor3bPin.Dispose();
            this._Motor4aPin.Dispose();
            this._Motor4bPin.Dispose();
            this._IcOut.Dispose();
            this._EnablePin.Dispose();
            if (this._Motor1Pwm != null) this._Motor1Pwm.StopPulse();
            if (this._Motor2Pwm != null) this._Motor2Pwm.StopPulse();
            if (this._Motor3Pwm != null) this._Motor3Pwm.StopPulse();
            if (this._Motor4Pwm != null) this._Motor4Pwm.StopPulse();
        }
    }
}
