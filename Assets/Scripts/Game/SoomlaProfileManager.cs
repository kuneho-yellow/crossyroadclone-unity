/******************************************************************************
*  @file       SoomlaProfileManager.cs
*  @brief      Handles Soomla Profile activities
*  @author     Ron
*  @date       October 10, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System;
using Soomla.Profile;

#endregion // Namespaces

public class SoomlaProfileManager
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Initialize only once
        if (m_isInitialized)
        {
            return;
        }

        // Subscribe to Profile events
        ProfileEvents.OnSoomlaProfileInitialized    += OnSoomlaProfileInitialized;
        ProfileEvents.OnSocialActionStarted         += OnSocialActionStarted;
        ProfileEvents.OnSocialActionFinished        += OnSocialActionFinished;
        ProfileEvents.OnSocialActionCancelled       += OnSocialActionCancelled;
        ProfileEvents.OnSocialActionFailed          += OnSocialActionFailed;

        SoomlaProfile.Initialize();
    }

    /// <summary>
    /// Logs in to Twitter.
    /// </summary>
    public void LoginToTwitter()
    {
        SoomlaProfile.Login(Provider.TWITTER);
    }

    /// <summary>
    /// Shares the specified message on one of the available social platforms.
    /// </summary>
    /// <param name="shareMessage">The message to share.</param>
    public void MultiShareText(string shareMessage)
    {
        SoomlaProfile.MultiShare(shareMessage);
    }

    /// <summary>
    /// Shares the provided image on one of the available social platforms.
    /// </summary>
    /// <param name="shareMessage">The message to share along with the image.</param>
    /// <param name="imageFilePath">The path to the image.</param>
    public void MultiShareImage(string shareMessage, string imageFilePath)
    {
        MultiShareScreenshot(-1, -1, null, true, shareMessage, imageFilePath, true, true);
    }

    /// <summary>
    /// Takes a full screenshot of the game screen and shares it on one of the available social platforms.
    /// </summary>
    /// <param name="shareMessage">The message to share along with the screenshot.</param>
    public void MultiShareScreenshot(string shareMessage)
    {
        MultiShareScreenshot(-1, -1, null, false, shareMessage, null, false, true);
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        // Unsubscribe from Profile events
        foreach (var d in ProfileEvents.OnSoomlaProfileInitialized.GetInvocationList())
        {
            ProfileEvents.OnSoomlaProfileInitialized -= d as ProfileEvents.Action;
        }
        foreach (var d in ProfileEvents.OnSocialActionStarted.GetInvocationList())
        {
            ProfileEvents.OnSocialActionStarted -= d as System.Action<Provider, SocialActionType, string>;
        }
        foreach (var d in ProfileEvents.OnSocialActionFinished.GetInvocationList())
        {
            ProfileEvents.OnSocialActionFinished -= d as System.Action<Provider, SocialActionType, string>;
        }
        foreach (var d in ProfileEvents.OnSocialActionCancelled.GetInvocationList())
        {
            ProfileEvents.OnSocialActionCancelled -= d as System.Action<Provider, SocialActionType, string>;
        }
        foreach (var d in ProfileEvents.OnSocialActionFailed.GetInvocationList())
        {
            ProfileEvents.OnSocialActionFailed -= d as System.Action<Provider, SocialActionType, string, string>;
        }

        // Clean up screenshot texture
        UpdateScreenshot(null);
    }

    /// <summary>
    /// Gets whether Soomla Profile is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    #endregion // Public Interface

    #region Variables
    
    private bool m_isInitialized = false;
    
    #endregion // Variables

    #region Multishare

    /// <summary>
    /// Shares a screenshot on one of the available social platforms.
    /// </summary>
    /// <param name="screenshotWidth">Width of the screenshot.</param>
    /// <param name="screenshotHeight">Height of the screenshot.</param>
    /// <param name="screenshotCamera">The camera used to take the screenshot.</param>
    /// <param name="useOneCamera">if set to <c>true</c> take screenshot as seen by one camera.
    ///     Else, take screenshot as seen by player (views of all cameras combined).</param>
    /// <param name="shareMessage">The message to share along with the screenshot.</param>
    /// <param name="screenshotFilePath">The path to the prepared screenshot, if available.
    ///     Else, this is the path to save the screenshot to.</param>
    /// <param name="screenshotProvided">if set to <c>true</c> the screenshot is already provided
    ///     in the specified file path, and needs only to be shared.</param>
    /// <param name="deleteOnShare">if set to <c>true</c> delete the local copy of the screenshot
    ///     after it has been successfully shared.</param>
    private void MultiShareScreenshot(int screenshotWidth, int screenshotHeight,
                                      Camera screenshotCamera, bool useOneCamera,
                                      string shareMessage, string screenshotFilePath,
                                      bool screenshotProvided, bool deleteOnShare)
    {
        // If width is not specified, use screen width
        int width = (screenshotWidth > 0) ? screenshotWidth : Screen.width;
        // If height is not specified, use screen height
        int height = (screenshotHeight > 0) ? screenshotHeight : Screen.height;

        // If camera is not specified, use main camera
        Camera camera = (screenshotCamera != null) ? screenshotCamera : Camera.main;

        // If file path is not specified, save the screenshot to a generic location
        string filePath = !string.IsNullOrEmpty(screenshotFilePath) ? screenshotFilePath :
            Application.persistentDataPath + "/CRCScreen_" + DateTime.Now.GetTimestamp() + ".png";

        // Take screenshot and save it to file
        if (!screenshotProvided || (screenshotProvided && string.IsNullOrEmpty(screenshotFilePath)))
        {
            if (useOneCamera)
            {
                // Screenshot as seen from one camera
                Texture2D screenshot = ScreenshotUtils.TakeScreenshotSave(width, height, camera, filePath);
                UpdateScreenshot(screenshot);
            }
            else
            {
                // Screenshot as seen by player
                Application.CaptureScreenshot(filePath);
            }
        }

        // Open the multishare window and pass in the screenshot path
        SoomlaProfile.MultiShare(shareMessage, filePath);

        // Delete local copy of screenshot
        // TODO: Wait until multishare finishes before deleting
        if (deleteOnShare && ScreenshotUtils.IsScreenshotReady(filePath))
        {
            //System.IO.File.Delete(filePath);
        }
    }

    #endregion // Multishare

    #region Screenshot

    private Texture2D m_screenshotTexture = null;

    /// <summary>
    /// Updates the cached screenshot.
    /// </summary>
    /// <param name="newScreenshot">The new screenshot.</param>
    private void UpdateScreenshot(Texture2D newScreenshot)
    {
        // Delete the previous texture first
        if (m_screenshotTexture != null)
        {
            UnityEngine.Object.Destroy(m_screenshotTexture);
        }
        // Store new texture
        m_screenshotTexture = newScreenshot;
    }

    #endregion // Screenshot

    #region Soomla Profile Delegates

    /// <summary>
    /// Called when Soomla Profile is initialized.
    /// </summary>
    private void OnSoomlaProfileInitialized()
    {
        // Set initialized flag
        m_isInitialized = true;
    }

    private void OnSocialActionStarted(Provider provider, SocialActionType type, string payload)
    {
        Debug.Log("Social action started. Provider: " + provider.ToString() + " Action: " + type.ToString());
    }

    private void OnSocialActionFinished(Provider provider, SocialActionType type, string payload)
    {
        Debug.Log("Social action finished. Provider: " + provider.ToString() + " Action: " + type.ToString());
    }

    private void OnSocialActionCancelled(Provider provider, SocialActionType type, string payload)
    {
        Debug.Log("Social action cancelled");
    }

    private void OnSocialActionFailed(Provider provider, SocialActionType type, string error, string payload)
    {
        Debug.Log("Social action failed with error message: " + error);
    }

    #endregion // Soomla Profile Delegates
}
