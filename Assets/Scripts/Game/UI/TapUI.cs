/******************************************************************************
*  @file       TapUI.cs
*  @brief      Handles the tap UI
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class TapUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        m_tapAnimInitialSpeed = m_tapAnim.speed;

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the tap UI.
    /// </summary>
    public void Show()
    {
        m_tapRoot.SetActive(true);
        UpdatePosition();
    }

    /// <summary>
    /// Hides the tap UI.
    /// </summary>
    public void Hide()
    {
        m_tapRoot.SetActive(false);
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

        m_tapAnimSpeedBeforePause = m_tapAnim.speed;
        m_tapAnim.speed = 0.0f;

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

        m_tapAnim.speed = m_tapAnimSpeedBeforePause;

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_tapAnim.speed = m_tapAnimInitialSpeed;
        m_tapRoot.SetActive(false);
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        GameObject.Destroy(this.gameObject);
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

    [SerializeField] private GameObject m_tapRoot   = null;
    [SerializeField] private Animator   m_tapAnim   = null;

    [SerializeField] private Vector2    m_offsetFromChar    = new Vector2(0.4f, -2.0f);

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    private float   m_tapAnimInitialSpeed       = 0.0f;
    private float   m_tapAnimSpeedBeforePause   = 0.0f;

    #endregion // Variables

    #region Position

    /// <summary>
    /// Updates the tap UI position to point to the active character.
    /// </summary>
    private void UpdatePosition()
    {
        if (!m_tapRoot.activeInHierarchy)
        {
            return;
        }
        // Get active character from GameManager
        GameManager gm = Locator.GetGameManager();
        if (gm != null)
        {
            Character character = gm.CharacterInstance;
            if (character != null)
            {
                // Position tap UI to point to active character
                Vector3 worldPos = character.transform.position;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                Vector3 uiPos = Locator.GetUIManager().UICamera.Camera.ScreenToWorldPoint(screenPos);
                m_tapAnim.transform.SetPosXY((Vector2)uiPos + m_offsetFromChar);
            }
        }
    }

    #endregion // Position

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

        if (Locator.GetUIManager().HasScreenOrientationChanged)
        {
            UpdatePosition();
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
