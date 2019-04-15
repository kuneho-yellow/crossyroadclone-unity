/******************************************************************************
*  @file       TopScoreUI.cs
*  @brief      Handles the top score UI
*  @author     Ron
*  @date       October 13, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class TopScoreUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(ScreenshotUI screenshotUI,
                           System.EventHandler<System.EventArgs> shareDelegate, 
                           System.EventHandler<System.EventArgs> backDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        m_screenshotUI = screenshotUI;

        // Initialize buttons
        m_topScoreShareBtn.Initialize(shareDelegate, UIButton.TriggerType.ON_RELEASE);
        m_topScoreBackBtn.Initialize(backDelegate, UIButton.TriggerType.ON_RELEASE);
        // Add button sounds
        m_topScoreShareBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_topScoreBackBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize text
        m_scoreText.Initialize();
        m_labelText.Initialize();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets the score and label.
    /// </summary>
    /// <param name="score">The score.</param>
    /// <param name="isTopScore">if set to <c>true</c> top score. Else, great score.</param>
    public void SetScore(int score, bool isTopScore)
    {
        m_scoreText.SetText(score.ToString());
        m_labelText.SetText(isTopScore ? NEW_TOP_SCORE_TEXT : GREAT_SCORE_TEXT);
    }

    /// <summary>
    /// Starts the screenshot animation.
    /// </summary>
    /// <param name="startPos">The start position for the screenshot.</param>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void StartScreenshotAnim(Vector3 startPos, Texture2D screenshotTexture)
    {
        m_screenshotUI.StartScreenshotAnim(startPos,
                                           m_topScoreShareBtn.transform.position,
                                           screenshotTexture);
    }

    /// <summary>
    /// Shows the still screenshot.
    /// </summary>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void ShowStillScreenshot(Texture2D screenshotTexture = null)
    {
        m_screenshotUI.ShowStillScreenshot(m_topScoreShareBtn.transform.position,
                                           screenshotTexture);
    }

    /// <summary>
    /// Shows the top score UI.
    /// </summary>
    public void Show()
    {
        m_topScoreRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the top score UI.
    /// </summary>
    public void Hide()
    {
        m_topScoreRoot.SetActive(false);
        m_screenshotUI.Hide();
    }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {

    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    /// <summary>
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject m_topScoreRoot      = null;
    [SerializeField] private UIButton   m_topScoreBackBtn   = null;
    [SerializeField] private UIButton   m_topScoreShareBtn  = null;

    // Text
    [SerializeField] private UIText     m_scoreText         = null;
    [SerializeField] private UIText     m_labelText         = null;

    #endregion // Serialized Variables

    #region Text

    private const string NEW_TOP_SCORE_TEXT = "NEW TOP";
    private const string GREAT_SCORE_TEXT   = "GREAT SCORE";
    
    #endregion // Text

    #region Variables
    
    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    private ScreenshotUI m_screenshotUI = null;

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
		if (!m_isInitialized)
        {
            return;
        }
    }

    /// <summary>
    /// Updates this instance after all Update() methods have finished.
    /// </summary>
    private void LateUpdate()
    {
        if (!m_isInitialized)
        {
            return;
        }

        // If screen orientation changes, update animation values for the screenshot UI
        // Do this in late update to allow share button to update position first
        if (Locator.GetUIManager().HasScreenOrientationChanged)
        {
            m_screenshotUI.UpdateAnimPos(m_topScoreShareBtn.transform.position);
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
