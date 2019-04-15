/// Copyright (C) 2012-2014 Soomla Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///      http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.using System;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Soomla.Levelup
{
	/// <summary>
	/// A specific type of <c>Gate</c> that has a schedule that defines when the <c>Gate</c>
	/// can be opened. The <c>Gate</c> opens once the player tries to open it in the time 
	/// frame of the defined schedule.
	/// </summary>
	public class ScheduleGate : Gate
	{

		private const string TAG = "SOOMLA ScheduleGate";

		/// <summary>
		/// The <c>Schedule</c> that defines when this <c>Gate</c> can be opened. 
		/// </summary>
		public Schedule Schedule;


		/// <summary>
		/// Constructor. 
		/// </summary>
		/// <param name="id">ID.</param>
		/// <param name="schedule">Schedule.</param>
		public ScheduleGate(string id, Schedule schedule)
			: base(id)
		{
			Schedule = schedule;
		}
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="jsonGate">JSON gate.</param>
		public ScheduleGate(JSONObject jsonGate)
			: base(jsonGate)
		{
			this.Schedule = new Schedule(jsonGate[JSONConsts.SOOM_SCHEDULE]);
		}
		
		/// <summary>
		/// Converts this <c>Gate</c> to a JSON object.
		/// </summary>
		/// <returns>The JSON object.</returns>
		public override JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();
			obj.AddField(JSONConsts.SOOM_SCHEDULE, Schedule.toJSONObject());

			return obj;
		}

		/// <summary>
		/// Registers relevant events: In this case there are none.
		/// </summary>
		protected override void registerEvents() {
			// Not listening to any events
		}

		/// <summary>
		/// Unregisters relevant events: In this case there are none.
		/// </summary>
		protected override void unregisterEvents() {
			// Not listening to any events
		}

		/// <summary>
		/// Checks if this <c>Gate</c> meets its criteria for opening.
		/// </summary>
		/// <returns>If this <c>Gate</c> can be opened returns <c>true</c>; otherwise <c>false</c>.</returns>
		protected override bool canOpenInner() {
			// Gates don't have activation times, they can only be activated once. 
			// We are kind of ignoring the activation limit of Schedule here.
			return Schedule.Approve(GateStorage.IsOpen(this) ? 1 : 0);
		}

		/// <summary>
		/// Opens this <c>Gate</c> if it can be opened (its criteria has been met).
		/// </summary>
		/// <returns>Upon success of opening returns <c>true</c>; otherwise <c>false</c>.</returns>
		protected override bool openInner() {
			if (CanOpen()) {
				// There's nothing to do here... If the criteria was met then the gate is just open.
				ForceOpen(true);
				return true;
			}
			return false;
		}
	}
}

