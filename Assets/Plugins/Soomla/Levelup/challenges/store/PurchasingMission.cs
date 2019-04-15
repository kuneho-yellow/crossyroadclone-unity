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
	/// A specific type of <c>Mission</c> that has an associated virtual item.   
	/// The <c>Mission</c> is complete once the item has been purchased.
	/// </summary>
	public class PurchasingMission : Mission
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">ID.</param>
		/// <param name="name">Name.</param>
		/// <param name="associatedItemId">ID of the item that is to be purchased.</param>
		public PurchasingMission(string id, string name, string associatedItemId)
			: base(id, name, typeof(PurchasableGate), new object[] { associatedItemId })
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">ID.</param>
		/// <param name="name">Name.</param>
		/// <param name="rewards"><c>Reward</c>s for this <c>Mission</c>.</param>
		/// <param name="associatedItemId">ID of the item that is to be purchased.</param>
		public PurchasingMission(string id, string name, List<Reward> rewards, string associatedItemId)
			: base(id, name, rewards, typeof(PurchasableGate), new object[] { associatedItemId })
		{
		}

		/// <summary>
		/// Constructor. 
		/// Generates an instance of <c>PurchasingMission</c> from the given JSONObject. 
		/// </summary>
		/// <param name="jsonMission">JSON mission.</param>
		public PurchasingMission(JSONObject jsonMission)
			: base(jsonMission)
		{
			// TODO: implement this when needed. It's irrelevant now.
		}

		/// <summary>
		/// Converts this <c>PurchasingMission</c> to a JSONObject.
		/// </summary>
		/// <returns>The JSON object.</returns>
		public override JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();
			
			// TODO: implement this when needed. It's irrelevant now.
			
			return obj;
		}

	}
}

