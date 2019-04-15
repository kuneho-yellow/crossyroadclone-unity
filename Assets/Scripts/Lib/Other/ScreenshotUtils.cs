/******************************************************************************
*  @file       ScreenshotUtils.cs
*  @brief      Holds utility functions for taking in-game screenshots
*  @author     Ron
*  @date       October 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.IO;

#endregion // Namespaces

public class ScreenshotUtils
{
    #region Public Interface

    /// <summary>
    /// Takes a screenshot.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="screenshotCamera">The camera to use to take the screenshot.</param>
    /// <returns>The screenshot</returns>
    public static Texture2D TakeScreenshot(int width, int height, Camera screenshotCamera)
    {
        if (width <= 0 || height <= 0)
        {
            return null;
        }
        // If camera is not specified, fall back to main camera
        if (screenshotCamera == null)
        {
            screenshotCamera = Camera.main;
        }
        RenderTexture rt = new RenderTexture(width, height, 24);
        screenshotCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotCamera.Render();
        RenderTexture.active = rt;
        // Read pixels from screen to the Texture2D
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        // Dispose of the camera's render texture
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Object.Destroy(rt);
        
        return screenShot;
    }

    /// <summary>
    /// Saves the screenshot.
    /// </summary>
    /// <param name="screenshot">The screenshot.</param>
    /// <param name="saveToFilePath">Path and name of the save file.</param>
    /// <returns>Whether screenshot was successfully saved</returns>
    public static bool SaveScreenshot(Texture2D screenshot, string saveToFilePath)
    {
        if (screenshot == null || string.IsNullOrEmpty(saveToFilePath))
        {
            return false;
        }

        byte[] bytes;
        if (saveToFilePath.ToLower().EndsWith(".jpg"))
        {
            bytes = screenshot.EncodeToJPG();
        }
        else
        {
            bytes = screenshot.EncodeToPNG();
        }
        FileStream fs = new FileStream(saveToFilePath, FileMode.OpenOrCreate);
        BinaryWriter w = new BinaryWriter(fs);
        w.Write(bytes);
        w.Close();
        fs.Close();
        return true;
    }

    /// <summary>
    /// Takes a screenshot and saves it to file.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="screenshotCamera">The camera to use to take the screenshot.</param>
    /// <param name="saveToFilePath">Path and name of the save file.</param>
    /// <returns>The screenshot</returns>
    public static Texture2D TakeScreenshotSave(int width, int height,
                                               Camera screenshotCamera, string saveToFilePath)
    {
        Texture2D screenshot = TakeScreenshot(width, height, screenshotCamera);
        SaveScreenshot(screenshot, saveToFilePath);
        return screenshot;
    }

    /// <summary>
    /// Determines whether screenshot in the specified path is ready.
    /// </summary>
    /// <param name="screenshotFilePath">The screenshot file path.</param>
    /// <returns>Whether screenshot is ready</returns>
    public static bool IsScreenshotReady(string screenshotFilePath)
    {
        return File.Exists(screenshotFilePath);
    }

    #endregion // Public Interface
}
