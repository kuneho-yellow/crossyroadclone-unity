/*
 * Copyright (C) 2012-2015 Soomla Inc. - All Rights Reserved
 *
 *   Unauthorized copying of this file, via any medium is strictly prohibited
 *   Proprietary and confidential
 *
 *   Written by Refael Dakar <refael@soom.la>
 */

 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Soomla;
using System;
using Grow.Sync;
using Grow.Gifting;
using Grow.Leaderboards;
using Grow.Insights;
using System.Runtime.InteropServices;

namespace Grow.Highway {

	public class HighwayEvents : MonoBehaviour {

		#if UNITY_IOS 
		//&& !UNITY_EDITOR
		[DllImport ("__Internal")]
		private static extern int unityHighwayEventDispatcher_Init();
		#endif

		private const string TAG = "SOOMLA HighwayEvents";
		
		private static HighwayEvents instance = null;
		
		/// <summary>
		/// Initializes the game state before the game starts.
		/// </summary>
		void Awake(){
			if(instance == null){ 	// making sure we only initialize one instance.
				SoomlaUtils.LogDebug(TAG, "Initializing HighwayEvents (Awake)");
				instance = this;
				GameObject.DontDestroyOnLoad(this.gameObject);
				Initialize();
			} else {				// Destroying unused instances.
				GameObject.Destroy(this.gameObject);
			}
		}

		/// <summary>
		/// Initializes the different native event handlers in Android / iOS
		/// </summary>
		public static void Initialize() {
			SoomlaUtils.LogDebug (TAG, "Initializing StoreEvents ...");
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniEventHandler = new AndroidJavaClass("com.soomla.highway.unity.HighwayEventHandler")) {
				jniEventHandler.CallStatic("initialize");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);

			#elif UNITY_IOS && !UNITY_EDITOR
			unityHighwayEventDispatcher_Init();
			#endif
		}

		public void onGrowSyncInitialized() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGrowSyncInitialized");
			HighwayEvents.OnGrowSyncInitialized();
		}

		public void onModelSyncStarted() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onModelSyncStarted");
			HighwayEvents.OnModelSyncStarted();
		}

		public void onModelSyncFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onModelSyncFinished: " + message);

			JSONObject eventJSON = new JSONObject(message);
			List<JSONObject> changedComponentsJSON = eventJSON["changedComponents"].list;

			List<string> changedComponents = new List<string>();
			foreach (var changedComponentJSON in changedComponentsJSON) {
				changedComponents.Add(changedComponentJSON.str);
			}

			GrowSync.HandleModelSyncFinised();

			HighwayEvents.OnModelSyncFinished(changedComponents);
		}

		public void onModelSyncFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onModelSyncFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			int errorCode = (int)eventJSON["errorCode"].n;
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnModelSyncFailed((ModelSyncErrorCode)errorCode, errorMessage);
		}

		public void onStateSyncStarted() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateSyncStarted");
			HighwayEvents.OnStateSyncStarted();
		}
		
		public void onStateSyncFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateSyncFinished: " + message);
			
			JSONObject eventJSON = new JSONObject(message);

			List<JSONObject> changedComponentsJSON = eventJSON["changedComponents"].list;
			List<string> changedComponents = new List<string>();
			foreach (var changedComponentJSON in changedComponentsJSON) {
				changedComponents.Add(changedComponentJSON.str);
			}

			List<JSONObject> failedComponentsJSON = eventJSON["failedComponents"].list;
			List<string> failedComponents = new List<string>();
			foreach (var failedComponentJSON in failedComponentsJSON) {
				failedComponents.Add(failedComponentJSON.str);
			}
			
			GrowSync.HandleStateSyncFinised();
			
			HighwayEvents.OnStateSyncFinished(changedComponents, failedComponents);
		}
		
		public void onStateSyncFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateSyncFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			int errorCode = (int)eventJSON["errorCode"].n;
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnStateSyncFailed((StateSyncErrorCode)errorCode, errorMessage);
		}

		public void onStateResetStarted() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateResetStarted");
			HighwayEvents.OnStateResetStarted();
		}
		
		public void onStateResetFinished() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateResetFinished");
			
			GrowSync.HandleStateSyncFinised();
			
			HighwayEvents.OnStateResetFinished();
		}
		
		public void onStateResetFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onStateResetFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			int errorCode = (int)eventJSON["errorCode"].n;
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnStateResetFailed((StateSyncErrorCode)errorCode, errorMessage);
		}

		public void onGrowGiftingInitialized() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGrowGiftingInitialized");
			HighwayEvents.OnGrowGiftingInitialized();
		}

		public void onGiftsRetrieveStarted() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftsRetrieveStarted");
			HighwayEvents.OnGiftsRetrieveStarted();
		}

		public void onGiftsRetrieveFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftsRetrieveFinished: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			
			List<JSONObject> givenGiftsJSON = eventJSON["givenGifts"].list;
			List<Gift> givenGifts = new List<Gift>();
			foreach (var givenGiftJSON in givenGiftsJSON) {
				givenGifts.Add(new Gift(givenGiftJSON));
			}
			
			HighwayEvents.OnGiftsRetrieveFinished(givenGifts);
		}

		public void onGiftsRetrieveFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftsRetrieveFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnGiftsRetrieveFailed(errorMessage);
		}

		public void onGiftSendStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftSendStarted: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			Gift gift = new Gift(eventJSON["gift"]);
			
			HighwayEvents.OnGiftSendStarted(gift);
		}

		public void onGiftSendFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftSendFinished: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			Gift gift = new Gift(eventJSON["gift"]);
			
			HighwayEvents.OnGiftSendFinished(gift);
		}

		public void onGiftSendFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftSendFailed: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			Gift gift = new Gift(eventJSON["gift"]);
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnGiftSendFailed(gift, errorMessage);
		}

		public void onGiftHandOutSuccess(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftHandOutSuccess: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			Gift gift = new Gift(eventJSON["gift"]);
			
			HighwayEvents.OnGiftHandOutSuccess(gift);
		}

		public void onGiftHandOutFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGiftHandOutFailed: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			Gift gift = new Gift(eventJSON["gift"]);
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnGiftHandOutFailed(gift, errorMessage);
		}
		
		public void onFetchFriendsStatesStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onFetchFriendsStatesStarted: " + message);

			JSONObject eventJSON = new JSONObject(message);
			int providerId = (int)eventJSON["providerId"].n;

			HighwayEvents.OnFetchFriendsStatesStarted(providerId);
		}
		
		public void onFetchFriendsStatesFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onFetchFriendsStatesFinished: " + message);
			
			JSONObject eventJSON = new JSONObject(message);

			int providerId = (int)eventJSON["providerId"].n;

			List<JSONObject> friendsStatesJSON = eventJSON["friendsStates"].list;
			List<FriendState> friendsStates = new List<FriendState>();
			foreach (var friendStateJSON in friendsStatesJSON) {
				friendsStates.Add(new FriendState(friendStateJSON));
			}
			
			HighwayEvents.OnFetchFriendsStatesFinished(providerId, friendsStates);
		}
		
		public void onFetchFriendsStatesFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onFetchFriendsStatesFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			int providerId = (int)eventJSON["providerId"].n;
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnFetchFriendsStatesFailed(providerId, errorMessage);
		}

		public void onGrowDLCInitialized() {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGrowDLCInitialized");

			HighwayEvents.OnGrowDLCInitialized();
		}

		public void onDLCPackagesStatusUpdate(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onDLCPackagesStatusUpdate: " + message);
			
			JSONObject eventJSON = new JSONObject(message);

			bool hasChanges = eventJSON["hasChanges"].b;

			List<string> packagesToSync = new List<string>();
			List<JSONObject> packagesToSyncJSON = eventJSON["packagesToSync"].list;
			if (packagesToSyncJSON != null) {
				foreach (var packageToSyncJSON in packagesToSyncJSON) {
					packagesToSync.Add(packageToSyncJSON.str);
				}
			}

			List<string> packagesDeleted = new List<string>();
			List<JSONObject> packagesDeletedJSON = eventJSON["packagesDeleted"].list;
			if (packagesDeletedJSON != null) {
				foreach (var packageDeletedJSON in packagesDeletedJSON) {
					packagesDeleted.Add(packageDeletedJSON.str);
				}
			}
			
			HighwayEvents.OnDLCPackagesStatusUpdate(hasChanges, packagesToSync, packagesDeleted);
		}
		
		public void onDLCPackageSyncStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onDLCPackageSyncStarted: " + message);
			
			JSONObject eventJSON = new JSONObject(message);
			string packageId = eventJSON["packageId"].str;
			
			HighwayEvents.OnDLCPackageSyncStarted(packageId);
		}
		
		public void onDLCPackageSyncFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onDLCPackageSyncFinished:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			string packageId = eventJSON["packageId"].str;
			
			HighwayEvents.OnDLCPackageSyncFinished(packageId);
		}

		public void onDLCPackageSyncFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onDLCPackageSyncFailed:" + message);
			
			JSONObject eventJSON = new JSONObject(message);
			string packageId = eventJSON["packageId"].str;
			int errorCode = (int)eventJSON["errorCode"].n;
			string errorMessage = eventJSON["errorMessage"].str;
			
			HighwayEvents.OnDLCPackageSyncFailed(packageId, (DLCSyncErrorCode)errorCode, errorMessage);
		}

		public void onGrowInsightsInitialized(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onGrowInsightsInitialized");

			GrowInsights.I_SyncWithNative ();

			HighwayEvents.OnGrowInsightsInitialized();
		}

		public void onInsightsRefreshFailed(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInsightsRefreshFailed");
			
			HighwayEvents.OnInsightsRefreshFailed();
		}

		public void onInsightsRefreshStarted(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInsightsRefreshStarted");
			
			HighwayEvents.OnInsightsRefreshStarted();
		}

		public void onInsightsRefreshFinished(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onInsightsRefreshFinished");

			GrowInsights.I_SyncWithNative ();

			HighwayEvents.OnInsightsRefreshFinished();
		}

		/// <summary>
		/// Fired when Grow Sync is intialized.
		/// </summary>
		public static Action OnGrowSyncInitialized = delegate {};
		/// <summary>
		/// Fired when the model sync process has began.
		/// </summary>
		public static Action OnModelSyncStarted = delegate {};
		/// <summary>
		/// Fired when the model sync process has finished.
		/// Provides a list of modules which were synced.
		/// </summary>
		public static Action<IList<string>> OnModelSyncFinished = delegate {};
		/// <summary>
		/// Fired when the model sync process has failed.
		/// Provides an error code and reason.
		/// </summary>
		public static Action<ModelSyncErrorCode, string> OnModelSyncFailed = delegate {};
		/// <summary>
		/// Fired when the state sync process has began.
		/// </summary>
		public static Action OnStateSyncStarted = delegate {};
		/// <summary>
		/// Fired when the state sync process has finished.
		/// Provides a list of modules which had their state updated, and a list of 
		/// modules which failed to update.
		/// </summary>
		public static Action<IList<string>, IList<string>> OnStateSyncFinished = delegate {};
		/// <summary>
		/// Fired when the state sync process has failed.
		/// Provides an error code and reason.
		/// </summary>
		public static Action<StateSyncErrorCode, string> OnStateSyncFailed = delegate {};
		/// <summary>
		/// Fired when the state reset process has began.
		/// </summary>
		public static Action OnStateResetStarted = delegate {};
		/// <summary>
		/// Fired when the state reset process has finished.
		/// </summary>
		public static Action OnStateResetFinished = delegate {};
		/// <summary>
		/// Fired when the state reset process has failed.
		/// Provides an error code and reason.
		/// </summary>
		public static Action<StateSyncErrorCode, string> OnStateResetFailed = delegate {};

		/// <summary>
		/// Fired when Grow Gifting is intialized.
		/// </summary>
		public static Action OnGrowGiftingInitialized = delegate {};
		/// <summary>
		/// Fired when gifting has started retrieving a list of gifts for the user.
		/// </summary>
		public static Action OnGiftsRetrieveStarted = delegate {};
		/// <summary>
		/// Fired when the list of gifts for the user has been retrieved.
		/// Provides a list of <c>Gift</c>s which will be handed out.
		/// <remarks>This event is fired just before the gifts are handed out
		/// to the user</remarks>
		/// </summary>
		public static Action<IList<Gift>> OnGiftsRetrieveFinished = delegate {};
		/// <summary>
		/// Fired when gifting failed to retrive a list of gifts for the user.
		/// Provides a reason for the failure.
		/// </summary>
		public static Action<string> OnGiftsRetrieveFailed = delegate {};
		/// <summary>
		/// Fired when a gift has began to be sent to the server.
		/// Provides the gift that is being sent.
		/// </summary>
		public static Action<Gift> OnGiftSendStarted = delegate {};
		/// <summary>
		/// Fired when sending the gift has failed.
		/// Provides a reason why the gift has failed to be sent.
		/// </summary>
		public static Action<Gift, string> OnGiftSendFailed = delegate {};
		/// <summary>
		/// Fired when the gift has been sent to the server.
		/// Provides the gift which was sent.
		/// <remarks>At this point the gift will have an ID</remarks>
		/// </summary>
		public static Action<Gift> OnGiftSendFinished = delegate {};
		/// <summary>
		/// Fired when the gift was handed out to the user.
		/// Provides the gift which was handed out to the user.
		/// </summary>
		public static Action<Gift> OnGiftHandOutSuccess = delegate {};
		/// <summary>
		/// Fired when handing out the gift to the user has failed.
		/// Provides the gift that failed and a reason.
		/// </summary>
		public static Action<Gift, string> OnGiftHandOutFailed = delegate {};

		/// <summary>
		/// Fired when grow leaderboards starts fetching friends' states for a
		/// specific social provider.
		/// Provides the social provider ID for which the fetch operation started
		/// </summary>
		public static Action<int> OnFetchFriendsStatesStarted = delegate {};
		/// <summary>
		/// Fired when grow leaderboards fails to fetch friends' states for a
		/// specific social provider.
		/// Provides the social provider ID for which the fetch operation failed
		/// and an error message which is the reason for the failure
		/// </summary>
		public static Action<int, string> OnFetchFriendsStatesFailed = delegate {};
		/// <summary>
		/// Fired when grow leaderboards finished fetching friends' states for a
		/// specific social provider.
		/// Provides the social provider ID for which the fetch operation finished
		/// Provides a list of <c>FriendState</c>s with the friends' states in the.
		/// </summary>
		public static Action<int, IList<FriendState>> OnFetchFriendsStatesFinished = delegate {};

		/// <summary>
		/// Fired when the DLC client is initialized.
		/// </summary>
		public static Action OnGrowDLCInitialized = delegate {};
		/// <summary>
		/// Fired when a package/s update status check has returned from the server.
		/// Provides a bool to signify if there are changes in any of the requested packages.
		/// Provides all packages which were updated on the server and need to be synced
		/// on the device.
		/// Provides All packages which were deleted from the device as a result of their
		/// deletion from the server
		/// </summary>
		public static Action<bool, IList<string>, IList<string>> OnDLCPackagesStatusUpdate = delegate {};
		/// <summary>
		/// This event is fired when the package starts the syncing process.
		/// Provides the package ID which the DLC sync will start for.
		/// </summary>
		public static Action<string> OnDLCPackageSyncStarted = delegate {};
		/// <summary>
		/// This event is fired when the package finishes the syncing process.
		/// Provides the package ID which the DLC sync has finished for.
		/// </summary>
		public static Action<string> OnDLCPackageSyncFinished = delegate {};
		/// <summary>
		/// This event is fired when the package failed the syncing process.
		/// Provides the package ID which the DLC sync failed for, an error code for reason,
		/// and the error message.
		/// </summary>
		public static Action<string, DLCSyncErrorCode, string> OnDLCPackageSyncFailed = delegate {};

		public static Action OnGrowInsightsInitialized = delegate {};
		public static Action OnInsightsRefreshFailed = delegate {};
		public static Action OnInsightsRefreshFinished = delegate {};
		public static Action OnInsightsRefreshStarted = delegate {};

		/* Internal SOOMLA events ... Not meant for public use */

		public void onConflict(string message) {
			SoomlaUtils.LogDebug(TAG, "SOOMLA/UNITY onConflict:" + message);

			JSONObject eventJSON = new JSONObject(message);
			string remoteStateStr = eventJSON["remoteState"].str;
			string currentStateStr = eventJSON["currentState"].str;
			string stateDiffStr = eventJSON["stateDiff"].str;

			JSONObject remoteState = new JSONObject(remoteStateStr);
			JSONObject currentState = new JSONObject(currentStateStr);
			JSONObject stateDiff = new JSONObject(stateDiffStr);

			GrowSync.HandleStateSyncConflict(remoteState, currentState, stateDiff);
		}
	}

	/// <summary>
	/// Enumeration of model sync failure codes
	/// </summary>
	public enum ModelSyncErrorCode
	{
		/// <summary>
		/// General error has occured
		/// </summary>
		GeneralError = 0,
		/// <summary>
		/// Failed due to server communication failure
		/// </summary>
		ServerError = 1,
		/// <summary>
		/// The model was not able to update
		/// </summary>
		UpdateModelError = 2
	}

	/// <summary>
	/// Enumeration of state sync failure codes
	/// </summary>
	public enum StateSyncErrorCode
	{
		/// <summary>
		/// General error has occured
		/// </summary>
		GeneralError = 0,
		/// <summary>
		/// Failed due to server communication failure
		/// </summary>
		ServerError = 1,
		/// <summary>
		/// The state was not able to update
		/// </summary>
		UpdateStateError = 2
	}

	/// <summary>
	/// Available error codes for DLC Sync operation
	/// </summary>
	public enum DLCSyncErrorCode {
		/// <summary>
		/// General error has occured
		/// </summary>
		GeneralError = 0,
		/// <summary>
		/// There was an error while downloading the DLC
		/// </summary>
		DownloadError = 1,
		/// <summary>
		/// There was an error while deleting local DLC packages
		/// </summary>
		DeleteError = 2,
		/// <summary>
		/// There was an error while communicating with the server
		/// </summary>
		ServerError = 3
	}
}