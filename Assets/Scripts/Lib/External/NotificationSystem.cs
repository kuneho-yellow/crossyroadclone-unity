/******************************************************************************
*  @file       NotificationSystem.cs
*  @brief      Handles One Signal notifications framework
*  @author     Ron
*  @date       October 9, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;

#endregion // Namespaces

public class NotificationSystem
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public bool Initialize(bool useNotifs)
    {
#if !UNITY_EDITOR
        if (useNotifs)
        {
            // Enable line below to enable logging if you are having issues
            //  setting up OneSignal. (logLevel, visualLogLevel)
            //OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.INFO);

            OneSignal.Init("ed4f54c2-6e47-11e5-9777-bb1a46033f58", "518470690122", HandleNotification);
        }
#endif
        // Set the initialized flag
        m_isInitialized = true;

        return true;
    }

    /// <summary>
    /// Gets called when the player opens the notification.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="additionalData">The additional data.</param>
    /// <param name="isActive">if set to <c>true</c> [is active].</param>
    public static void HandleNotification(string message, Dictionary<string, object> additionalData, bool isActive)
    {
        if (BuildInfo.IsDebugMode)
        {
            Debug.Log("Receive notification: " + message);
        }
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
	{
		get { return m_isInitialized; }
	}
	
#endregion // Public Interface
	
#region Variables
	
	private bool m_isInitialized = false;
	
#endregion // Variables
}
