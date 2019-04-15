/******************************************************************************
*  @file       ScreenshotTaker.cs
*  @brief      Handles taking of screenshots in Crossy Road Clone
*  @author     Ron
*  @date       October 14, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

//#define DEBUG_SCREENSHOT

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public enum ScreenshotDir
{
    Front,
    Side_Left,
    Side_Right,
    Back
}

public class ScreenshotTaker : MonoBehaviour
{
    #region Public Interface
    
    /// <summary>
    /// Takes a screenshot.
    /// </summary>
    /// <param name="focus">The focus of the screenshot.</param>
    /// <param name="screenshotDir">The screenshot direction.</param>
    /// <returns>The screenshot texture</returns>
    public Texture2D TakeScreenshot(Vector3 focus, ScreenshotDir screenshotDir, string scoreText, string topScoreText)
    {
        // Enable screenshot camera to take the screenshot
        m_screenshotCamera.gameObject.SetActive(true);

        // Lazy init UIText
        if (!m_scoreText.IsInitialized)     m_scoreText.Initialize();
        if (!m_topScoreText.IsInitialized)  m_topScoreText.Initialize();

        // Set up score and top score display
        m_scoreText.SetText(scoreText);
        m_topScoreText.SetText(topScoreText);

        // Rotate dir around the x and y axes
        float vertAngle = m_vertAngleOffset + Random.Range(-m_randomDeltaAngle, m_randomDeltaAngle);
        float horAngle = Random.Range(-m_randomDeltaAngle, m_randomDeltaAngle);
        // Screenshot can be taken obliquely from the right or from the left of the subject
        float horAngleOffsetSign = (Random.value > 0.5f) ? 1.0f : -1.0f;
        switch (screenshotDir)
        {
            case ScreenshotDir.Front:
                horAngle += m_horAngleOffset * horAngleOffsetSign;
                break;
            case ScreenshotDir.Side_Left:
                horAngle += -90.0f;
                break;
            case ScreenshotDir.Side_Right:
                horAngle += 90.0f;
                break;
            case ScreenshotDir.Back:
                horAngle += 180.0f + m_horAngleOffset * horAngleOffsetSign;
                break;
        }
        // Get the direction vector from the subject to the screenshot position
        //  (i.e. where camera should take the screenshot from)
        Vector3 focusToScreenshotPosDir = Quaternion.Euler(vertAngle, -horAngle, 0.0f) * Vector3.back;

        // Set camera distance and vertical offset from the subject
        float camDist = m_screenshotCamDist + Random.Range(-m_randomDeltaDist, m_randomDeltaDist);
        focus.y += m_verticalOffset;
        // Move camera to the screenshot position and make it look at the focus point
        m_screenshotCamera.transform.position = focus + focusToScreenshotPosDir * camDist;
        m_screenshotCamera.transform.LookAt(focus);

        // Take screenshot
        m_screenshotResult = ScreenshotUtils.TakeScreenshot(m_screenshotWidth, m_screenshotHeight, m_screenshotCamera);

        // Disable screenshot camera once screenshot has been taken
        m_screenshotCamera.gameObject.SetActive(false);

        return m_screenshotResult;
    }

    /// <summary>
    /// Saves the last screenshot taken.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>Whether screenshot saving was successful</returns>
    public bool SaveLastScreenshot(string filePath)
    {
        return ScreenshotUtils.SaveScreenshot(m_screenshotResult, filePath);
    }
    
    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Camera used for taking the screenshot")]
    [SerializeField] private Camera     m_screenshotCamera  = null;
    [Tooltip("Score text displayed as part of screenshot")]
    [SerializeField] private UIText     m_scoreText         = null;
    [Tooltip("Top score text displayed as part of screenshot")]
    [SerializeField] private UIText     m_topScoreText      = null;

    [Tooltip("Distance of camera from subject when taking the screenshot")]
    [SerializeField] private float      m_screenshotCamDist = 65.0f;
    [Tooltip("Amount by which the camera distance can vary")]
    [SerializeField] private float      m_randomDeltaDist   = 5.0f;
    [Tooltip("Width of the full screenshot")]
    [SerializeField] private int        m_screenshotWidth   = 550;
    [Tooltip("Width of the full screenshot")]
    [SerializeField] private int        m_screenshotHeight  = 300;
    [Tooltip("Vertical tilt of the camera relative to the subject when taking the screenshot")]
    [SerializeField] private float      m_vertAngleOffset   = 45.0f;
    [Tooltip("Horizontal tilt of the camera relative to the subject when taking the screenshot")]
    [SerializeField] private float      m_horAngleOffset    = 50.0f;
    [Tooltip("Amount by which the camera tilt can vary")]
    [SerializeField] private float      m_randomDeltaAngle  = 5.0f;
    [Tooltip("Amount by which the camera's focus is offset from the actual subject's position")]
    [SerializeField] private float      m_verticalOffset    = 15.0f;

#if DEBUG_SCREENSHOT
    [Tooltip("For testing: Subject of screenshot")]
    [SerializeField] private Transform  m_screenshotFocus   = null;
#endif

    #endregion // Serialized Variables

    #region Variables

#if DEBUG_SCREENSHOT
    [SerializeField]
    private ScreenshotDir   m_screenshotDir     = ScreenshotDir.Front;
#endif
    private Texture2D       m_screenshotResult  = null;

#endregion // Variables

#region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
	{

	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	private void Start()
	{
		
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update()
	{
#if DEBUG_SCREENSHOT
        if (m_screenshotFocus == null)
        {
            GameManager gm = Locator.GetGameManager();
            if (gm != null && gm.CharacterInstance != null)
            {
                m_screenshotFocus = gm.CharacterInstance.transform;
            }
        }
#endif
    }

#if DEBUG_SCREENSHOT

    // TOOD: Test
    private void OnGUI()
    {
        float x = 10.0f;
        float y = 10.0f;
        float buttonDim = 50.0f;
        float spacing = 5.0f;

        if (GUI.Button(new Rect(x, y, buttonDim, buttonDim), "Shoot"))
        {
            TakeScreenshot(m_screenshotFocus.position, m_screenshotDir);
        }

        // Display screenshot
        if (m_screenshotResult != null)
        {
            y += buttonDim + spacing;
            GUI.Box(new Rect(x, y, m_screenshotWidth, m_screenshotHeight), m_screenshotResult);
        }
    }
#endif

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
	{

	}

#endregion // MonoBehaviour
}
