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
using Grow.Highway;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Soomla;

namespace Grow.Sync
{
	/// <summary>
	/// Represents a class which is in charge of syncing model and state
	/// between the client and the server.
	/// </summary>
	public class GrowSync {
#if UNITY_IOS  && !UNITY_EDITOR
		[DllImport ("__Internal")]
		private static extern int growSync_initialize(bool modelSync, bool stateSync);
		[DllImport ("__Internal")]
		private static extern int growSync_resetState();
		[DllImport ("__Internal")]
		private static extern int growSync_registerUnityConflictResolver();
		[DllImport ("__Internal")]
		private static extern int growSync_resolveConflict(int strategy, string jsonState);
#endif

		/// <summary>
		/// a Delegate type for resolving conflicts between client and server states
		/// </summary>
		/// <param name="remoteState">The state on the server</param>
		/// <param name="currentState">The state locally</param>
		/// <param name="stateDiff">The difference between remote and local state</param>
		/// <returns>The resolved state (should be in the same format as remote or local state)</returns>
		public delegate JSONObject StateConflictResolver(JSONObject remoteState, JSONObject currentState, JSONObject stateDiff);
		/// <summary>
		/// The current resolver for state conflicts between client and server
		/// </summary>
		public static StateConflictResolver CurrentStateConflictResolver = (JSONObject remoteState, JSONObject currentState, JSONObject stateDiff) =>
		{
			return remoteState;
		};

		/// <summary>
		/// Initialize Grow Sync module
		/// </summary>
		/// <param name="modelSync">Should Soomla Sync synchronize model for integrated modules</param>
		/// <param name="stateSync">Should Soomla Sync synchronize state for integrated modules</param>
		public static void Initialize(bool modelSync, bool stateSync) {
			SoomlaUtils.LogDebug (TAG, "SOOMLA/UNITY Initializing Sync");
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniGrowSyncClass = new AndroidJavaClass("com.soomla.sync.GrowSync")) {
				
				AndroidJavaObject jniGrowSyncInstance = jniGrowSyncClass.CallStatic<AndroidJavaObject>("getInstance");
				jniGrowSyncInstance.Call("initialize", modelSync, stateSync);
			}
			using(AndroidJavaClass jniUnityGrowSyncClass = new AndroidJavaClass("com.soomla.highway.unity.GrowSync")) 
			{	
				jniUnityGrowSyncClass.CallStatic("registerUnityConflictResolver");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS && !UNITY_EDITOR
			growSync_initialize(modelSync, stateSync);
			growSync_registerUnityConflictResolver();
#endif
		}

		public static void ResetState() {
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniGrowSyncClass = new AndroidJavaClass("com.soomla.sync.GrowSync")) {
				
				AndroidJavaObject jniGrowSyncInstance = jniGrowSyncClass.CallStatic<AndroidJavaObject>("getInstance");
				jniGrowSyncInstance.Call("resetState");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS && !UNITY_EDITOR
			growSync_resetState();
#endif
		}

		internal static void HandleModelSyncFinised() {
			if (IsStoreAvailable()) {
				try {
					// Call SoomlaStore.initializeFromDB() using reflection to prevent coupling
					// Loads the synced model from the storage
					Type storeInfoType = Type.GetType("Soomla.Store.StoreInfo");
					if (storeInfoType != null) {
						MethodInfo initFromDBMethod = storeInfoType.GetMethod("initializeFromDB", BindingFlags.NonPublic | BindingFlags.Static);
						if (initFromDBMethod != null) {
							initFromDBMethod.Invoke(null, null);
							SoomlaUtils.LogDebug(TAG, "Finished syncing model Unity");
						}
					}
				} catch (Exception ex) {
					// This should not happen since if native succeeded in loading, Unity should too
					String message = "Unable to handle model sync for Store on Unity side, the application is now in " +
						"an inconsistant state between Unity and native: " + ex.Message;
					SoomlaUtils.LogError(TAG, message);
					throw new InvalidOperationException(message);
				}
			}
		}

		internal static void HandleStateSyncFinised ()
		{
			if (IsStoreAvailable()) {
				try {
					// Call StoreInventory.RefreshLocalInventory() using reflection to prevent coupling
					// Loads the synced state from the storage
					Type storeInventoryType = Type.GetType("Soomla.Store.StoreInventory");
					if (storeInventoryType != null) {
						MethodInfo refreshLocalInventoryMethod = storeInventoryType.GetMethod("RefreshLocalInventory", BindingFlags.Public | BindingFlags.Static);
						if (refreshLocalInventoryMethod != null) {
							refreshLocalInventoryMethod.Invoke(null, null);
							SoomlaUtils.LogDebug(TAG, "Finished syncing state Unity");
						}
					}
				} catch (Exception ex) {
					// This should not happen since if native succeeded in loading, Unity should too
					String message = "Unable to handle state sync for Store on Unity side, the application is now in " +
						"an inconsistant state between Unity and native: " + ex.Message;
					SoomlaUtils.LogError(TAG, message);
					throw new InvalidOperationException(message);
				}
			}
		}

		internal static void HandleStateSyncConflict(JSONObject remoteState, JSONObject currentState, JSONObject stateDiff) {
#if !UNITY_EDITOR
			JSONObject resolvedState = CurrentStateConflictResolver(remoteState, currentState, stateDiff);
			int conflictResolveStrategy = 2;
			if (resolvedState == remoteState) {
				conflictResolveStrategy = 0;
			} else if (resolvedState == currentState) {
				conflictResolveStrategy = 1;
			}

#if UNITY_ANDROID
			using(AndroidJavaClass jniUnityGrowSyncClass = new AndroidJavaClass("com.soomla.highway.unity.GrowSync")) 
			{	
				jniUnityGrowSyncClass.CallStatic("resolveConflict", conflictResolveStrategy, resolvedState.print());
			}
#elif UNITY_IOS
			growSync_resolveConflict(conflictResolveStrategy, resolvedState.print());
#endif

#endif
		}

		private static bool IsStoreAvailable() {
			try {
				Type result = Type.GetType("Soomla.Store.SoomlaStore");
				return (result != null);
			} catch (Exception) {
				return false;
			}
		}

		private const string TAG = "SOOMLA GrowSync";
	}
}