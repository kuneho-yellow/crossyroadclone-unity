/******************************************************************************
*  @file       SignInUI.cs
*  @brief      Handles the Google sign-in UI
*  @author     Ron
*  @date       October 15, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SignInUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> signInDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        // Initialize buttons
        m_signInBtn.Initialize(signInDelegate, UIButton.TriggerType.ON_RELEASE);
        m_signInBackBtn.Initialize((object sender, System.EventArgs e) => { Hide(); },
                                   UIButton.TriggerType.ON_RELEASE);
        // Add button sounds
        m_signInBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_signInBackBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the sign-in UI.
    /// </summary>
    public void Show()
    {
        m_signInRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the sign-in UI.
    /// </summary>
    public void Hide()
    {
        m_signInRoot.SetActive(false);
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

    /// <summary>
    /// Gets whether this instance is shown.
    /// </summary>
    public bool IsShown
    {
        get { return m_signInRoot.activeInHierarchy; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject m_signInRoot    = null;
    [SerializeField] private UIButton   m_signInBackBtn = null;
    [SerializeField] private UIButton   m_signInBtn     = null;

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
