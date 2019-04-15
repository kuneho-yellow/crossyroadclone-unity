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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Soomla.Levelup {
	
	/// <summary>
	/// A game can have multiple <c>World</c>s or a single one, and <c>World</c>s can also contain other 
	/// <c>World</c>s in them. A <c>World</c> can contain a set of <c>Level</c>s, or multiple sets of  
	/// <c>Level</c>s. A <c>World</c> can also have a <c>Gate</c> that defines the criteria to enter it. 
	/// Games that don’t have the concept of <c>World</c>s can be modeled as single <c>World</c> games. 
	/// 
	/// Real Game Example: "Candy Crush" uses <c>World</c>s.
	/// </summary>
	public class World : SoomlaEntity<World> {

		private static string TAG = "SOOMLA World";

		/// <summary>
		/// The <c>World</c> which contains this world directly
		/// </summary>
		public World ParentWorld {
			get;
			private set;
		}

		/// <summary>
		/// <c>Gate</c> that defines the criteria to enter this <c>World</c>.
		/// </summary>
		private Gate gate;
		public Gate Gate {
			get { return gate; }
			set {
				if (value != gate) {
					if (gate != null) {
						gate.OnDetached();
					}
					gate = value;
					if (gate != null) {
						gate.OnAttached();
					}
				}
			}
		}

		/// <summary>
		/// The <c>World</c>s included in this <c>World</c>.
		/// </summary>
		public Dictionary<string, World> InnerWorldsMap = new Dictionary<string, World>();

		/// <summary>
		/// Gets the inner worlds list.
		/// </summary>
		/// <value>The inner worlds list.</value>
		public IEnumerable<World> InnerWorldsList {
			get { return InnerWorldsMap.Values; }
		}

		/// <summary>
		/// The <c>Score</c>s associated with this <c>World</c>.
		/// </summary>
		public Dictionary<string, Score> Scores = new Dictionary<string, Score>();

		/// <summary>
		/// <c>Missions</c> in this <c>World</c>.
		/// </summary>
		public List<Mission> Missions = new List<Mission>();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">ID.</param>
		public World(String id)
			: base(id)
		{
			ParentWorld = null;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id">ID.</param>
		/// <param name="gate">A <c>Gate</c> that needs to be opened in order to enter this 
		/// <c>World</c>.</param>
		/// <param name="innerWorlds">A list of <c>World</c>s contained within this one.</param>
		/// <param name="scores">Scores of this <c>World</c>.</param>
		/// <param name="missions"><c>Missions</c>s that are available in this <c>World</c>.</param>
		public World(string id, Gate gate, Dictionary<string, World> innerWorlds, Dictionary<string, Score> scores, List<Mission> missions)
			: base(id)
		{
			this.InnerWorldsMap = (innerWorlds != null) ? innerWorlds : new Dictionary<string, World>();
			ApplyParentToInnerWorlds();

			this.Scores = (scores != null) ? scores : new Dictionary<string, Score>();
			this.Gate = gate;
			this.Missions = (missions != null) ? missions : new List<Mission>();
		}

		/// <summary>
		/// Constructs a <c>World</c> from a JSON object. 
		/// </summary>
		/// <param name="jsonWorld">Json World.</param>
		public World(JSONObject jsonWorld)
			: base(jsonWorld)
		{
			InnerWorldsMap = new Dictionary<string, World>();
			List<JSONObject> worldsJSON = jsonWorld[LUJSONConsts.LU_WORLDS].list;

			// Iterates over all inner worlds in the JSON array and for each one creates
			// an instance according to the world type.
			foreach (JSONObject worldJSON in worldsJSON) {
				World innerWorld = World.fromJSONObject(worldJSON);
				if (innerWorld != null) {
					InnerWorldsMap.Add(innerWorld._id, innerWorld);
					innerWorld.ParentWorld = this;
				}
			}

			Scores = new Dictionary<String, Score>();
			List<JSONObject> scoresJSON = jsonWorld[LUJSONConsts.LU_SCORES].list;

			// Iterates over all scores in the JSON array and for each one creates
			// an instance according to the score type.
			foreach (JSONObject scoreJSON in scoresJSON) {
				Score score = Score.fromJSONObject(scoreJSON);
				if (score != null) {
					Scores.Add(score.ID, score);
				}
			}

			Missions = new List<Mission>();
			List<JSONObject> missionsJSON = jsonWorld[LUJSONConsts.LU_MISSIONS].list;
			
			// Iterates over all challenges in the JSON array and creates an instance for each one.
			foreach (JSONObject missionJSON in missionsJSON) {
				Missions.Add(Mission.fromJSONObject(missionJSON));
			}

			JSONObject gateJSON = jsonWorld[LUJSONConsts.LU_GATE];
			if (gateJSON != null && gateJSON.keys != null && gateJSON.keys.Count > 0) {
				Gate = Gate.fromJSONObject (gateJSON);
			}
		}

		/// <summary>
		/// Converts this <c>World</c> into a JSON object. 
		/// </summary>
		/// <returns>The JSON object.</returns>
		public override JSONObject toJSONObject() {
			JSONObject obj = base.toJSONObject();

			obj.AddField(LUJSONConsts.LU_GATE, (Gate==null ? new JSONObject(JSONObject.Type.OBJECT) : Gate.toJSONObject()));

			JSONObject worldsArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (World world in InnerWorldsMap.Values) {
				worldsArr.Add(world.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_WORLDS, worldsArr);

			JSONObject scoresArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (Score score in Scores.Values) {
				scoresArr.Add(score.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_SCORES, scoresArr);

			JSONObject missionsArr = new JSONObject(JSONObject.Type.ARRAY);
			foreach (Mission mission in Missions) {
				missionsArr.Add(mission.toJSONObject());
			}
			obj.AddField(LUJSONConsts.LU_MISSIONS, missionsArr);

			return obj;
		}

		/// <summary>
		/// Converts the given JSON object into a <c>World</c>.
		/// </summary>
		/// <returns>The JSON object to be converted.</returns>
		/// <param name="worldObj">World object.</param>
		public static World fromJSONObject(JSONObject worldObj) {
			string className = worldObj[JSONConsts.SOOM_CLASSNAME].str;

			World world = (World) Activator.CreateInstance(Type.GetType("Soomla.Levelup." + className), new object[] { worldObj });

			return world;
		}


		/** Add elements to world. **/

		/// <summary>
		/// Adds the given inner <c>World</c> to this <c>World</c>.
		/// </summary>
		/// <param name="world">World to be added.</param>
		public void AddInnerWorld(World world) {
			InnerWorldsMap.Add(world._id, world);
			world.ParentWorld = this;
		}

		/// <summary>
		/// Adds the given <c>Mission</c> to this <c>World</c>.
		/// </summary>
		/// <param name="mission">Mission to be added.</param>
		public void AddMission(Mission mission) {
			Missions.Add(mission);
		}

		/// <summary>
		/// Adds the given <c>Score</c> to this <c>World</c>.
		/// </summary>
		/// <param name="score">Score to be added.</param>
		public void AddScore(Score score) {
			Scores.Add(score.ID, score);
		}

		/** Get elements to world. **/
		public World GetInnerWorldAt(int index) {
			if ((index < 0) || (index >= InnerWorldsMap.Count)) {
				return null;
			}

			return InnerWorldsMap.Values.ElementAt(index);
		}

		/// <summary>
		/// Creates a batch of <c>Level</c>s and adds them to this <c>World</c>. This function will save you 
		/// a lot of time - instead of creating many levels one by one, you can create them all at once.
		/// </summary>
		/// <param name="numLevels">The number of <c>Level</c>s to be added to this <c>World</c>.</param>
		/// <param name="gateTemplate">The <c>Gate</c> for the levels.</param>
		/// <param name="scoreTemplate"><c>Score</c> template for the <c>Level</c>s.</param>
		/// <param name="missionTemplate"><c>Mission</c> template for the <c>Level</c>s.</param>
		public void BatchAddLevelsWithTemplates(int numLevels, Gate gateTemplate, Score scoreTemplate, Mission missionTemplate) {
			List<Score> scoreTemplates = new List<Score>();
			if (scoreTemplate != null) {
				scoreTemplates.Add(scoreTemplate);
			}
			List<Mission> missionTemplates = new List<Mission>();
			if (missionTemplate != null) {
				missionTemplates.Add(missionTemplate);
			}

			BatchAddLevelsWithTemplates(numLevels, gateTemplate, scoreTemplates, missionTemplates);
		}
		public void BatchAddLevelsWithTemplates(int numLevels, Gate gateTemplate, List<Score> scoreTemplates, List<Mission>missionTemplates) {
			for (int i=0; i<numLevels; i++) {
				string lvlId = IdForAutoGeneratedLevel(_id, i);
				Level aLvl = new Level(lvlId);

				Gate targetGate = null;
				if (gateTemplate != null) {
					targetGate = gateTemplate.Clone(IdForAutoGeneratedGate(lvlId));
				}

				createAddAutoLevel(lvlId, aLvl, targetGate, scoreTemplates, missionTemplates);
			}
		}

		/// <summary>
		/// Creates a batch of <c>Level</c>s and adds them to this <c>World</c>. This function will save you 
		/// a lot of time - instead of creating many levels one by one, you can create them all at once.
		/// </summary>
		/// <param name="numLevels">The number of <c>Level</c>s to be added to this <c>World</c>.</param>
		/// <param name="scoreTemplate"><c>Score</c> template for the <c>Level</c>s.</param>
		/// <param name="missionTemplate"><c>Mission</c> template for the <c>Level</c>s.</param>
		public void BatchAddDependentLevelsWithTemplates(int numLevels, Score scoreTemplate, Mission missionTemplate) {
			List<Score> scoreTemplates = new List<Score>();
			if (scoreTemplate != null) {
				scoreTemplates.Add(scoreTemplate);
			}
			List<Mission> missionTemplates = new List<Mission>();
			if (missionTemplate != null) {
				missionTemplates.Add(missionTemplate);
			}
			
			BatchAddDependentLevelsWithTemplates(numLevels, scoreTemplates, missionTemplates);
		}
		public void BatchAddDependentLevelsWithTemplates(int numLevels, List<Score> scoreTemplates, List<Mission>missionTemplates) {
			string previousLvlId = null;

			for (int i=0; i<numLevels; i++) {
				string lvlId = IdForAutoGeneratedLevel(_id, i);
				Level aLvl = new Level(lvlId);

				Gate completeGate = null;
				if (previousLvlId != null) {
					completeGate = new WorldCompletionGate(IdForAutoGeneratedCompleteGate(lvlId, previousLvlId), previousLvlId);
				}

				createAddAutoLevel(lvlId, aLvl, completeGate, scoreTemplates, missionTemplates);
				previousLvlId = lvlId;
			}
		}


		/** For Single Score **/

		/// <summary>
		/// Sets the single <c>Score</c> value to the given amount.
		/// </summary>
		/// <param name="amount">Amount to set.</param>
		public void SetSingleScoreValue(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			SetScoreValue(Scores.First().Value.ID, amount);
		}

		/// <summary>
		/// Decreases the single <c>Score</c> value by the given amount.
		/// </summary>
		/// <param name="amount">Amount to decrease by.</param>
		public void DecSingleScore(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			DecScore(Scores.First().Value.ID, amount);
		}

		/// <summary>
		/// Increases the single <c>Score</c> value by the given amount.
		/// </summary>
		/// <param name="amount">Amount to increase by.</param>
		public void IncSingleScore(double amount) {
			if (Scores.Count() == 0) {
				return;
			}
			IncScore(Scores.First().Value.ID, amount);
		}

		/// <summary>
		/// Retrieves the single <c>Score</c> value.
		/// </summary>
		/// <returns>The single score.</returns>
		public Score GetSingleScore() {
			if (Scores.Count() == 0) {
				return null;
			}
			return Scores.First().Value;
		}

        /// <summary>
        /// Sums up this world's total <c>Score</c> value.
        /// </summary>
        /// <returns>The total world score.</returns>
        public double SumWorldScoreRecords() {
			return Scores.Select( s => s.Value.Record < 0 ? 0 : s.Value.Record ).Sum( s => s );
        }

        /// <summary>
        /// Sums the inner <c>World</c> records.
        /// </summary>
        /// <returns>The sum of inner <c>World</c> records.</returns>
        [Obsolete( "This method is obsolete, use SumInnerWorldSingleRecords() instead." )]
        public double SumInnerWorldsRecords() {
            return SumInnerWorldSingleRecords();
        }

        /// <summary>
        /// Sums the inner <c>World</c> single score records, non-recursive.
        /// </summary>
        /// <returns>The sum of inner <c>World</c> records.</returns>
		public double SumInnerWorldSingleRecords() {
			double ret = 0;
			foreach( World world in InnerWorldsList ) {
				Score singleScore = world.GetSingleScore();
				if (singleScore != null) {
					double record = singleScore.Record;
					if (record > -1) {
						ret += record;
					}
				}
			}
			return ret;
		}

        /// <summary>
        /// Sums up all the inner <c>World</c> records, recursively.
        /// </summary>
        /// <returns>The sum of inner <c>World</c> records.</returns>
        public double SumAllInnerWorldsRecords() {
            double ret = 0;
            foreach( World world in InnerWorldsList ) {
                ret += world.SumWorldScoreRecords();
                ret += world.SumAllInnerWorldsRecords();
            }
            return ret;
        }


		/** For more than one Score **/

		/// <summary>
		/// Resets the <c>Score</c>s for this <c>World</c>.
		/// </summary>
		/// <param name="save">If set to <c>true</c>, will also calculate and save the record and latest scores.</param>
		public void ResetScores(bool save) {
			if (Scores == null || Scores.Count == 0) {
				SoomlaUtils.LogError(TAG, "(ResetScores) You don't have any scores defined in this world. World id: " + _id);
				return;
			}

			foreach (Score score in Scores.Values) {
				score.Reset(save);
			}
		}

		/// <summary>
		/// Decreases the <c>Score</c> with the given ID by the given amount.
		/// </summary>
		/// <param name="scoreId">ID of the <c>Score</c> to be decreased.</param>
		/// <param name="amount">Amount to decrease by.</param>
		public void DecScore(string scoreId, double amount) {
			Scores[scoreId].Dec(amount);
		}

		/// <summary>
		/// Increases the <c>Score</c> with the given ID by the given amount.
		/// </summary>
		/// <param name="scoreId">ID of the <c>Score</c> to be increased.</param>
		/// <param name="amount">Amount.</param>
		public void IncScore(string scoreId, double amount) {
			Scores[scoreId].Inc(amount);
		}

		/// <summary>
		/// Retrieves the record <c>Score</c>s.
		/// </summary>
		/// <returns>The record <c>Score</c>s - each <c>Score</c> ID with its record.</returns>
		public Dictionary<string, double> GetRecordScores() {
			Dictionary<string, double> records = new Dictionary<string, double>();
			foreach(Score score in Scores.Values) {
				records.Add(score.ID, score.Record);
			}

			return records;
		}

		/// <summary>
		/// Retrieves the latest <c>Score</c>s.
		/// </summary>
		/// <returns>The latest <c>Score</c>s - each <c>Score</c> ID with its record.</returns>
		public Dictionary<string, double> GetLatestScores() {
			Dictionary<string, double> latest = new Dictionary<string, double>();
			foreach (Score score in Scores.Values) {
				latest.Add(score.ID, score.Latest);
			}

			return latest;
		}

		/// <summary>
		/// Sets the <c>Score</c> with the given ID to have the given value.
		/// </summary>
		/// <param name="id"><c>Score</c> whose value is to be set.</param>
		/// <param name="scoreVal">Value to set.</param>
		public void SetScoreValue(string id, double scoreVal) {
			SetScoreValue(id, scoreVal, false);
		}
		public void SetScoreValue(string id, double scoreVal, bool onlyIfBetter) {
			Score score = Scores[id];
			if (score == null) {
				SoomlaUtils.LogError(TAG, "(setScore) Can't find score id: " + id + "  world id: " + this._id);
				return;
			}
			score.SetTempScore(scoreVal, onlyIfBetter);
		}


		/** Completion **/

		/// <summary>
		/// Determines whether this <c>World</c> is completed.
		/// </summary>
		/// <returns><c>true</c> if this instance is completed; otherwise, <c>false</c>.</returns>
		public bool IsCompleted() {
			return WorldStorage.IsCompleted(this);
		}

		/// <summary>
		/// Sets this <c>World</c> as completed.
		/// </summary>
		/// <param name="completed">If set to <c>true</c> completed.</param>
		public virtual void SetCompleted(bool completed) {
			SetCompleted(completed, false);
		}
		public void SetCompleted(bool completed, bool recursive) {
			if (recursive) {
				foreach (World world in InnerWorldsMap.Values) {
					world.SetCompleted(completed, true);
				}
			}

			// keep current completed state
			bool alreadyCompleted = IsCompleted();

			WorldStorage.SetCompleted(this, completed);

			if (!alreadyCompleted && completed && (ParentWorld != null)) {
				ParentWorld.NotifyInnerWorldFirstCompleted(this);
			}
		}


		/** Reward Association **/

		/// <summary>
		/// Assigns the given reward to this <c>World</c>.
		/// </summary>
		/// <param name="reward">Reward to assign.</param>
		public void AssignReward(Reward reward) {
			String olderReward = GetAssignedRewardId();
			if (!string.IsNullOrEmpty(olderReward)) {
				Reward oldReward = SoomlaLevelUp.GetReward(olderReward);
				if (oldReward != null) {
					oldReward.Take();
				}
			}

			// We have to make sure the assigned reward can be assigned unlimited times.
			// There's no real reason why it won't be.
			if (reward.Schedule.ActivationLimit > 0) {
				reward.Schedule.ActivationLimit = 0;
			}

			reward.Give();
			WorldStorage.SetReward(this, reward.ID);
		}

		/// <summary>
		/// Retrieves the assigned reward ID.
		/// </summary>
		/// <returns>The assigned reward ID.</returns>
		public String GetAssignedRewardId() {
			return WorldStorage.GetAssignedReward(this);
		}

		/// <summary>
		/// Gets the last completed inner world ID.
		/// </summary>
		/// <returns>The last completed inner world ID.</returns>
		public string GetLastCompletedInnerWorld() {
			return WorldStorage.GetLastCompletedInnerWorld(this);
		}

		/// <summary>
		/// Determines if this world is available for starting, based on either if there 
		/// is no <c>Gate</c> for this <c>World</c>, or if the <c>Gate</c> is open.
		/// </summary>
		/// <returns><c>true</c> if this instance can start; otherwise, <c>false</c>.</returns>
		public bool CanStart() {
			return Gate == null || Gate.IsOpen();
		}


		/** PRIVATE FUNCTIONS **/

		private void createAddAutoLevel(string id, Level target, Gate targetGate, List<Score> scoreTemplates, List<Mission>missionTemplates) {
			if (targetGate != null) {
				target.Gate = targetGate;
			}
			
			if (scoreTemplates != null) {
				for(int k=0; k<scoreTemplates.Count(); k++) {
					target.AddScore(scoreTemplates[k].Clone(IdForAutoGeneratedScore(id, k)));
				}
			}
			
			if (missionTemplates != null) {
				for(int k=0; k<missionTemplates.Count(); k++) {
					target.AddMission(missionTemplates[k].Clone(IdForAutoGeneratedMission(id, k)));
				}
			}

			this.InnerWorldsMap.Add(id, target);

			target.ParentWorld = this;
		}

		private void ApplyParentToInnerWorlds()
		{
			foreach (var innerWorldEntry in InnerWorldsMap) {
				innerWorldEntry.Value.ParentWorld = this;
			}
		}

		private void NotifyInnerWorldFirstCompleted(World innerWorld) {
			if (innerWorld != null) {
				WorldStorage.SetLastCompletedInnerWorld(this, innerWorld.ID);
			}
		}

		/** Automatic generation of levels. **/
		private string IdForAutoGeneratedLevel(string id, int idx) {
			return id + "_level" + idx;
		}
		private string IdForAutoGeneratedScore(string id, int idx) {
			return id + "_score" + idx;
		}
		private string IdForAutoGeneratedGate(string id) {
			return id + "_gate";
		}
		private string IdForAutoGeneratedCompleteGate(string id, string previousId) {
			return IdForAutoGeneratedGate(id + "_complete_" + previousId);
		}
		private string IdForAutoGeneratedMission(string id, int idx) {
			return id + "_mission" + idx;
		}

	}
}
