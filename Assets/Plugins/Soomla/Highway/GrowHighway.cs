/*
 * Copyright (C) 2012-2015 Soomla Inc. - All Rights Reserved
 *
 *   Unauthorized copying of this file, via any medium is strictly prohibited
 *   Proprietary and confidential
 *
 *   Written by Refael Dakar <refael@soom.la>
 */
 
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Soomla;

namespace Grow.Highway
{
	public class GrowHighway
	{
#if UNITY_IOS && !UNITY_EDITOR
		[DllImport ("__Internal")]
		private static extern int growHighway_initialize(string gameKey, string envKey);
		[DllImport ("__Internal")]
		private static extern int growHighway_setHighwayUrl(string url);
		[DllImport ("__Internal")]
		private static extern int growHighway_setServicesUrl(string url);
		[DllImport ("__Internal")]
		private static extern int growHighway_start();
#endif

		public static void Initialize() {
			SoomlaUtils.LogDebug (TAG, "SOOMLA/UNITY Initializing Highway");
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniGrowHighwayClass = new AndroidJavaClass("com.soomla.highway.GrowHighway")) {

				AndroidJavaObject jniGrowHighwayInstance = jniGrowHighwayClass.CallStatic<AndroidJavaObject>("getInstance");
				jniGrowHighwayInstance.Call("initialize", HighwaySettings.HighwayGameKey, HighwaySettings.HighwayEnvKey);

				// Uncomment this and change the URL for testing
//				using(AndroidJavaClass jniConfigClass = new AndroidJavaClass("com.soomla.highway.HighwayConfig")) {
//					AndroidJavaObject jniConfigObject = jniConfigClass.CallStatic<AndroidJavaObject>("getInstance");
//					jniConfigObject.Call("setUrls", "http://example.com", "http://example.com");
//				}

				jniGrowHighwayInstance.Call("start");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
#elif UNITY_IOS && !UNITY_EDITOR
			growHighway_initialize(HighwaySettings.HighwayGameKey, HighwaySettings.HighwayEnvKey);

			// Uncomment this and change the URL for testing
//			growHighway_setHighwayUrl("http://example.com");
//			growHighway_setServicesUrl("http://example.com");

			growHighway_start();
#else
			SoomlaUtils.LogError(TAG, "Highway only works on Android or iOS devices !");
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		/// <summary> Class Members </summary>

		private const string TAG = "SOOMLA GrowHighway";

	}
}
