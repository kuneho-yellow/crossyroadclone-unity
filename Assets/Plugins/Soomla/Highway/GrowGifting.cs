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
using System;
using System.Runtime.InteropServices;
using Soomla;

namespace Grow.Gifting
{
	/// <summary>
	/// Represents a manager class which is in charge of sending and receiving gifts
	/// between users playing the same game.
	/// </summary>
	public class GrowGifting {
		#if UNITY_IOS  && !UNITY_EDITOR
		[DllImport ("__Internal")]
		private static extern int growGifting_initialize();
		[DllImport ("__Internal")]
		private static extern int growGifting_sendGift(int toProvider, string toProfileId, string itemId, int amount, bool deductFromUser, out bool outResult);
		#endif

		/// <summary>
		/// Initializes the gifting manager
		/// </summary>
		public static void Initialize() {
			SoomlaUtils.LogDebug (TAG, "SOOMLA/UNITY Initializing Gifting");
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniGrowGiftingClass = new AndroidJavaClass("com.soomla.gifting.GrowGifting")) {

				AndroidJavaObject jniGrowGiftinginstance = jniGrowGiftingClass.CallStatic<AndroidJavaObject>("getInstance");
				jniGrowGiftinginstance.Call("initialize");
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);
			#elif UNITY_IOS && !UNITY_EDITOR
			growGifting_initialize();
			#endif
		}

		/// <summary>
		/// Sends a gift from the currently logged in user (with Profile) to
		/// the provided user.
		/// This method gives the gift from the game without affecting the player.
		/// </summary>
		/// <returns><c>false</c>, if the operation cannot be started, <c>true</c> otherwise.</returns>
		/// <param name="toProvider">The social provider ID of the user to send gift to.</param>
		/// <param name="toProfileId">The social provider user ID to send gift to.</param>
		/// <param name="itemId">The virtual item ID to give as a gift.</param>
		/// <param name="amount">The amount of virtual items to gift.</param>
		public static bool SendGift(int toProvider, string toProfileId, string itemId, int amount) {
			return SendGift(toProvider, toProfileId, itemId, amount, false);
		}

		/// <summary>
		/// Sends a gift from the currently logged in user (with Profile) to
		/// the provided user.
		/// </summary>
		/// <returns><c>false</c>, if the operation cannot be started, <c>true</c> otherwise.</returns>
		/// <param name="toProvider">The social provider ID of the user to send gift to.</param>
		/// <param name="toProfileId">The social provider user ID to send gift to.</param>
		/// <param name="itemId">The virtual item ID to give as a gift.</param>
		/// <param name="amount">The amount of virtual items to gift.</param>
		/// <param name="deductFromUser">Should the virtual items be deducted from the
		/// player upon sending the gift</param>
		public static bool SendGift(int toProvider, string toProfileId, string itemId, int amount, bool deductFromUser) {
			SoomlaUtils.LogDebug (TAG, "SOOMLA/UNITY Sending Gift");
			bool result = false;
			
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJNI.PushLocalFrame(100);
			using(AndroidJavaClass jniGrowGiftingClass = new AndroidJavaClass("com.soomla.gifting.GrowGifting")) {

				AndroidJavaObject jniGrowGiftingInstance = jniGrowGiftingClass.CallStatic<AndroidJavaObject>("getInstance");
				result = jniGrowGiftingInstance.Call<bool>("sendGift", toProvider, toProfileId, itemId, amount, deductFromUser);
			}
			AndroidJNI.PopLocalFrame(IntPtr.Zero);

			#elif UNITY_IOS && !UNITY_EDITOR
			growGifting_sendGift(toProvider, toProfileId, itemId, amount, deductFromUser, out result);
			#endif

			return result;
		}

		private const string TAG = "SOOMLA GrowGifting";
	}
}
