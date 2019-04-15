/******************************************************************************
*  @file       PauseUI.cs
*  @brief      Handles the pause UI
*  @author     Ron
*  @date       September 17, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class PauseUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> pauseDelegate,
                           System.EventHandler<System.EventArgs> unpauseScreenDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        // Initialize buttons
        m_pauseBtn.Initialize(pauseDelegate, UIButton.TriggerType.ON_RELEASE);
        m_unpauseScreen.Initialize(unpauseScreenDelegate, UIButton.TriggerType.ON_PRESS);
        m_pauseBtn.UpdateScreenPosition();
        // Set button sounds
        m_pauseBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize text
        m_countdownText.Initialize();
        
        m_countdownTimer = COUNTDOWN_DURATION;

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the pause button.
    /// </summary>
    public void ShowPauseBtn()
    {
        // Update position
        m_pauseBtn.UpdateScreenPosition();

        m_pauseBtn.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the pause button.
    /// </summary>
    public void HidePauseBtn()
    {
        m_pauseBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the pause UI.
    /// </summary>
    public void Show()
    {
        m_pauseBtn.gameObject.SetActive(true);
        m_pauseRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the pause UI.
    /// </summary>
    public void Hide()
    {
        m_pauseBtn.gameObject.SetActive(false);
        m_pauseRoot.SetActive(false);
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_pauseBtn.gameObject.SetActive(true);
        m_pauseRoot.SetActive(false);
        m_pauseIcon.gameObject.SetActive(true);
        m_countdownText.SetText("");
        m_countdownTimer = COUNTDOWN_DURATION;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        m_pauseBtn.Delete();
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Sets up pause UI for game entering pause state.
    /// </summary>
    public void StartPause()
    {
        m_pauseIcon.gameObject.SetActive(true);
        m_countdownText.SetText("");
        m_pauseRoot.SetActive(true);
    }

    /// <summary>
    /// Starts the unpause countdown.
    /// </summary>
    public void StartUnpauseCountdown()
    {
        if (m_isCountingDown)
        {
            return;
        }
        // Replace the pause icon with a countdown display
        m_pauseIcon.gameObject.SetActive(false);
        // Initialize timer
        m_countdownTimer = COUNTDOWN_DURATION;
        m_countdownText.SetText(COUNTDOWN_DURATION.ToString());
        // Start countdown
        m_isCountingDown = true;
    }

    /// <summary>
    /// Cancels the unpause countdown.
    /// </summary>
    public void CancelUnpauseCountdown()
    {
        if (!m_isCountingDown)
        {
            return;
        }
        // Replace the countdown text with the pause icon
        m_pauseIcon.gameObject.SetActive(true);
        m_countdownText.SetText("");
        // Stop countdown
        m_isCountingDown = false;
    }

    /// <summary>
    /// Gets whether the unpause countdown is running.
    /// </summary>
    public bool IsCountingDown
    {
        get { return m_isCountingDown; }
    }

    /// <summary>
    /// Gets whether countdown has finished.
    /// </summary>
    public bool IsCountdownFinished
    {
        get { return m_countdownTimer == 0.0f; }
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject     m_pauseRoot     = null;

    [SerializeField] private UIButton       m_pauseBtn      = null;
    [SerializeField] private UIButton       m_unpauseScreen = null;
    [SerializeField] private SpriteRenderer m_pauseIcon     = null;
    [SerializeField] private UIText         m_countdownText = null;

    #endregion // Serialized Variables

    #region Variables
    
    private bool        m_isInitialized     = false;
    private bool        m_isCountingDown    = false;
    private float       m_countdownTimer    = 0.0f;
    private const int   COUNTDOWN_DURATION  = 3;

    #endregion // Variables

    #region Unpause Countdown

    /// <summary>
    /// Updates the unpause countdown.
    /// </summary>
    private void UpdateUnpauseCountdown()
    {
        if (!m_isCountingDown)
        {
            return;
        }

        // Update countdown
        float prevCountdown = m_countdownTimer;
        m_countdownTimer = Mathf.Clamp(m_countdownTimer - Time.deltaTime, 0.0f, Mathf.Infinity);
        int countdownDisplay = Mathf.CeilToInt(m_countdownTimer);
        if (countdownDisplay > 0)
        {
            // Update countdown display
            m_countdownText.SetText(countdownDisplay.ToString());
            // If timer display changes, play countdown sound
            if (countdownDisplay != Mathf.CeilToInt(prevCountdown) || prevCountdown == COUNTDOWN_DURATION)
            {
                Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.PauseCountdown);
            }
        }
        else
        {
            // Stop countdown
            m_isCountingDown = false;
            m_countdownText.SetText("");
        }
    }

    #endregion // Unpause Countdown

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

        UpdateUnpauseCountdown();
	}

    /// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
