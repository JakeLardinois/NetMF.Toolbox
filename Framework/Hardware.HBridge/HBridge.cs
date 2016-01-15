using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// H-Bridge Motor Driver
    /// </summary>
    /// <remarks><![CDATA[
    /// SN754410/L293D DIP16 pin layout:
    /// 
    ///   16 15 14 13 12 11 10 9
    ///   │  │  │  │  │  │  │  │
    /// █████████████████████████
    /// ▀████████████████████████
    ///   ███████████████████████
    /// ▄████████████████████████
    /// █████████████████████████
    ///   │  │  │  │  │  │  │  │
    ///   1  2  3  4  5  6  7  8
    /// 
    /// NOTE: The pins below aren't mentioned in pin sequence but grouped by connection
    ///
    ///  3 1Y -> Motor 1 negative wire
    ///  6 2Y -> Motor 1 positive wire
    /// 11 1Y -> Motor 2 positive wire
    /// 14 2Y -> Motor 3 negative wire
    ///
    ///  2 1A (Motor 1 direction pin) -> Any GPIO on the Netduino, ex. pin 7
    ///  7 2A (Motor 1 speed pin) -> Any PWM pin on the Netduino, ex. pin 6
    /// 10 3A (Motor 2 direction pin) -> Any GPIO on the Netduino, ex. pin 4
    /// 15 4A (Motor 2 speed pin) -> Any PWM pin on the Netduino, ex. pin 5
    ///
    ///  8 Vcc2 -> Power source for the motors, ex. Vin on the Netduino
    ///
    ///  4 Heatsink and ground -> Ground
    ///  5 Heatsink and ground -> Ground
    /// 12 Heatsink and ground -> Ground
    /// 13 Heatsink and ground -> Ground
    ///
    ///  1 1,2EN -> +5V
    ///  9 3,4EN -> +5V
    /// 16 Vcc1 -> +5V
    /// ]]></remarks>
    public class HBridge : IDisposable
    {
        /// <summary>Reference to the speed-pin of motor 2</summary>
        private IPWMPort _Motor2Speed;
        /// <summary>Reference to the speed-pin of motor 1</summary>
        private IPWMPort _Motor1Speed;
        /// <summary>Reference to the direction-pin of motor 2</summary>
        private OutputPort _Motor2Direction;
        /// <summary>Reference to the direction-pin of motor 1</summary>
        private OutputPort _Motor1Direction;

        /// <summary>
        /// Contains all motors
        /// </summary>
        public enum Motors
        {
            /// <summary>First motor</summary>
            Motor1 = 1,
            /// <summary>Second motor</summary>
            Motor2 = 2
        }

#if MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3
        /// <summary>
        /// H-Bridge-compatible Motor Driver (as used on the DFRobot Motorshield)
        /// </summary>
        /// <param name="Speed1">Motor 1 PWM control</param>
        /// <param name="Direction1">Motor 1 Direction control</param>
        /// <param name="Speed2">Motor 2 PWM control</param>
        /// <param name="Direction2">Motor 2 Direction control</param>
        public HBridge(Cpu.PWMChannel Speed1, Cpu.Pin Direction1, Cpu.PWMChannel Speed2, Cpu.Pin Direction2)
        {
            _Motor1Speed = new IntegratedPWM(Speed1);
            _Motor2Speed = new IntegratedPWM(Speed2);
            _Motor1Direction = new OutputPort(Direction1, false);
            _Motor2Direction = new OutputPort(Direction2, false);
        }
#endif

        /// <summary>
        /// H-Bridge-compatible Motor Driver (as used on the DFRobot Motorshield)
        /// </summary>
        /// <param name="Speed1">Motor 1 PWM control</param>
        /// <param name="Direction1">Motor 1 Direction control</param>
        /// <param name="Speed2">Motor 2 PWM control</param>
        /// <param name="Direction2">Motor 2 Direction control</param>
        public HBridge(IPWMPort Speed1, Cpu.Pin Direction1, IPWMPort Speed2, Cpu.Pin Direction2)
        {
            _Motor1Speed = Speed1;
            _Motor2Speed = Speed2;
            _Motor1Direction = new OutputPort(Direction1, false);
            _Motor2Direction = new OutputPort(Direction2, false);
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

            switch (Motor)
            {
                case Motors.Motor1:
                    // When Speed is below zero, the direction pin is true
                    this._Motor1Direction.Write(Speed < 0);
                    // When Speed is below zero, it will be set to positive (double negative; 0 - -100 = 100)
                    this._Motor1Speed.StopPulse();
                    this._Motor1Speed.SetDutyCycle((uint)(Speed < 0 ? 100 - (0 - Speed) : Speed));
                    this._Motor1Speed.StartPulse();
                    break;
                case Motors.Motor2:
                    // Same as for Motor1
                    this._Motor2Direction.Write(Speed < 0);
                    this._Motor2Speed.StopPulse();
                    this._Motor2Speed.SetDutyCycle((uint)(Speed < 0 ? 100 - (0 - Speed) : Speed));
                    this._Motor2Speed.StartPulse();
                    break;
                default:
                    // Should never happen!! :-)
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Stops both motors and disposes this class
        /// </summary>
        public void Dispose()
        {
            this._Motor1Speed.StopPulse();
            this._Motor2Speed.StopPulse();
            this.Dispose();
        }
    }
}
