/******************************************************************************
*  @file       CreditsUI.cs
*  @brief      Handles the credits UI
*  @author     Ron
*  @date       September 6, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class CreditsUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> kunehoSiteDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        // Initialize kuneho site button
        m_kunehoSiteBtn.Initialize(kunehoSiteDelegate, UIButton.TriggerType.ON_RELEASE);
        m_kunehoSiteBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the credits UI.
    /// </summary>
    public void Show()
    {
        m_creditsRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the credits UI.
    /// </summary>
    public void Hide()
    {
        m_creditsRoot.SetActive(false);
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
    
    [SerializeField] private GameObject m_creditsRoot       = null;
    
    [SerializeField] private UIButton   m_kunehoSiteBtn     = null;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isInitialized     = false;
    private bool    m_isPaused          = false;

    #endregion // Variables

    #region Button Delegates

    #endregion // Button Delegates

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
