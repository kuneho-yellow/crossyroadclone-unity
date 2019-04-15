/******************************************************************************
*  @file       SettingsUI.cs
*  @brief      Handles the settings UI
*  @author     Ron
*  @date       October 17, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SettingsUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(bool isMuted, bool isRotationLocked,
                           System.EventHandler<System.EventArgs> creditsDelegate,
                           System.EventHandler<System.EventArgs> muteDelegate,
                           System.EventHandler<System.EventArgs> rotationLockDelegate,
                           System.EventHandler<System.EventArgs> restorePurchasesDelegate,
                           System.EventHandler<System.EventArgs> signInDelegate,
                           System.EventHandler<System.EventArgs> signOutDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        // Initialize buttons
        m_creditsBtn.Initialize(creditsDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set initial state of toggle buttons
        UIToggleButton.ToggleButtonState muteInitialState = !isMuted ?
                                                            UIToggleButton.ToggleButtonState.STATE1 : 
                                                            UIToggleButton.ToggleButtonState.STATE2;
        m_muteBtn.Initialize(muteDelegate, muteDelegate,
                             UIToggleButton.TriggerType.ON_RELEASE, muteInitialState);
        UIToggleButton.ToggleButtonState rotationLockInitialState = !isRotationLocked ?
                                                                    UIToggleButton.ToggleButtonState.STATE1 :
                                                                    UIToggleButton.ToggleButtonState.STATE2;
        m_rotationLockBtn.Initialize(rotationLockDelegate, rotationLockDelegate,
                                     UIToggleButton.TriggerType.ON_RELEASE, rotationLockInitialState);
        // TODO: Add button! And confirmation UI!
        //m_restorePurchasesBtn.Initialize(restorePurchasesDelegate, UIToggleButton.TriggerType.ON_RELEASE);
        m_signInBtn.Initialize(signInDelegate, UIButton.TriggerType.ON_RELEASE);
        m_signOutBtn.Initialize(signOutDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set button sounds
        m_creditsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_muteBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_rotationLockBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_signInBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_signOutBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        //m_restorePurchasesBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize signed-in/out UI
        SetSignedInState(Locator.GetPlayServicesSystem().IsSignedInToGoogle);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets whether player is signed in to Google
    /// </summary>
    /// <param name="signedIn">if set to <c>true</c> player is signed in.</param>
    public void SetSignedInState(bool signedIn)
    {
        m_signedInRoot.SetActive(signedIn);
        m_signedOutRoot.SetActive(!signedIn);
    }

    /// <summary>
    /// Shows the settings UI.
    /// </summary>
    public void Show()
    {
        m_settingsRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the settings UI.
    /// </summary>
    public void Hide()
    {
        m_settingsRoot.SetActive(false);
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

    [SerializeField] private GameObject     m_settingsRoot      = null;

    // Buttons
    [SerializeField] private UIButton       m_creditsBtn            = null;
    [SerializeField] private UIToggleButton m_muteBtn               = null;
    [SerializeField] private UIToggleButton m_rotationLockBtn       = null;
    //[SerializeField] private UIButton       m_restorePurchasesBtn   = null;
    [SerializeField] private UIButton       m_signInBtn             = null;
    [SerializeField] private UIButton       m_signOutBtn            = null;

    [SerializeField] private GameObject     m_signedOutRoot         = null;
    [SerializeField] private GameObject     m_signedInRoot          = null;

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized     = false;
    private bool    m_isPaused          = false;

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
