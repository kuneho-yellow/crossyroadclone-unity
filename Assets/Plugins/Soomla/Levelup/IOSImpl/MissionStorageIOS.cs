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
/// limitations under the License.

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Soomla;

namespace Soomla.Levelup
{
	/// <summary>
	/// <c>MissionStorage</c> for iOS.
	/// A utility class for persisting and querying the state of <c>Mission</c>s.
	/// Use this class to check if a certain <c>Mission</c> is complete, or to
	/// set its completion status.
	/// </summary>
	public class MissionStorageIOS : MissionStorage {
#if UNITY_IOS && !UNITY_EDITOR
	
	[DllImport ("__Internal")]
	private static extern void missionStorage_SetCompleted(string missionId,
	                                               [MarshalAs(UnmanagedType.Bool)] bool up,
	                                               [MarshalAs(UnmanagedType.Bool)] bool notify);
	[DllImport ("__Internal")]
	private static extern int missionStorage_GetTimesCompleted(string missionId);

	
	override protected void _setCompleted(Mission mission, bool up, bool notify) {
		missionStorage_SetCompleted(mission.ID, up, notify);
	}
	
	override protected int _getTimesCompleted(Mission mission) {
		int times = missionStorage_GetTimesCompleted(mission.ID);
		SoomlaUtils.LogDebug("SOOMLA/UNITY MissionStorageIOS", string.Format("mission {0} completed={1}", mission.ID, times));
		return times;
	}
	
#endif
	}
}

