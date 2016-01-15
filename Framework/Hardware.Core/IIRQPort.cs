using System;
using Microsoft.SPOT;

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
    /// <summary>Interrupt ReQuest Port interface</summary>
    public interface IIRQPort : IGPIPort
    {
        /// <summary>Use this to give this IRQ port a unique identifier (default: blank)</summary>
        string ID { get; set; }
        /// <summary>Event triggered when a IRQ port state changes</summary>
        event StateChange OnStateChange;
    }

    /// <summary>Triggered when a IRQ port state changes</summary>
    /// <param name="Object">The IRQ port (use IIRQPort.ID to make it unique)</param>
    /// <param name="State">The new state</param>
    /// <param name="Time">Timestamp of the event</param>
    public delegate void StateChange(IIRQPort Object, bool State, DateTime Time);
}
