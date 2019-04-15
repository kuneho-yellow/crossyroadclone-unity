/******************************************************************************
*  @file       StartUI.cs
*  @brief      Handles the start UI
*  @author     Ron
*  @date       October 6, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class StartUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> charSelectDelegate,
                           System.EventHandler<System.EventArgs> achievementsDelegate,
                           System.EventHandler<System.EventArgs> leaderboardsDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        // Initialize buttons
        m_charSelectBtn.Initialize(charSelectDelegate, UIButton.TriggerType.ON_RELEASE);
        m_leaderboardsBtn.Initialize(leaderboardsDelegate, UIButton.TriggerType.ON_RELEASE);
        m_achievementsBtn.Initialize(achievementsDelegate, UIButton.TriggerType.ON_RELEASE);
        // Initialize button positions
        m_charSelectBtn.UpdateScreenPosition();
        m_leaderboardsBtn.UpdateScreenPosition();
        m_achievementsBtn.UpdateScreenPosition();
        // Set button sounds
        m_charSelectBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_leaderboardsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_achievementsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Enables/disables the character select button animation.
    /// </summary>
    /// <param name="enable">if set to <c>true</c> enable button animation.</param>
    public void EnableCharSelectBtnAnim(bool enable = true)
    {
        if (enable) m_charSelectBtn.EnableAnimation();
        else        m_charSelectBtn.DisableAnimation();
    }

    /// <summary>
    /// Shows the start UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_charSelectBtn.UpdateScreenPosition();
        m_leaderboardsBtn.UpdateScreenPosition();
        m_achievementsBtn.UpdateScreenPosition();

        m_startRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the start UI.
    /// </summary>
    public void Hide()
    {
        m_startRoot.SetActive(false);
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

    [SerializeField] private GameObject         m_startRoot         = null;

    [SerializeField] private UIAnimatedButton   m_charSelectBtn     = null;
    [SerializeField] private UIButton           m_achievementsBtn   = null;
    [SerializeField] private UIButton           m_leaderboardsBtn   = null;

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

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
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
