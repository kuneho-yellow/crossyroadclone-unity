/******************************************************************************
*  @file       SwipeUI.cs
*  @brief      Handles the swipe UI
*  @author     Ron
*  @date       September 15, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SwipeUI : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Assume same speed for left and right swipe animations
        m_swipeAnimInitialSpeed = m_leftSwipeAnim.speed;

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the swipe UI.
    /// </summary>
    public void Show(bool showLeftSwipe)
    {
        m_swipeRoot.SetActive(true);
        m_leftSwipeAnim.gameObject.SetActive(showLeftSwipe);
        m_rightSwipeAnim.gameObject.SetActive(!showLeftSwipe);
    }
    
    /// <summary>
    /// Hides the swipe UI.
    /// </summary>
    public void Hide()
    {
        m_swipeRoot.SetActive(false);
        m_leftSwipeAnim.gameObject.SetActive(false);
        m_rightSwipeAnim.gameObject.SetActive(false);
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

        m_swipeAnimSpeedBeforePause = m_leftSwipeAnim.speed;
        m_leftSwipeAnim.speed = 0.0f;
        m_rightSwipeAnim.speed = 0.0f;

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

        m_leftSwipeAnim.speed = m_swipeAnimSpeedBeforePause;
        m_rightSwipeAnim.speed = m_swipeAnimSpeedBeforePause;

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_leftSwipeAnim.speed = m_swipeAnimInitialSpeed;
        m_rightSwipeAnim.speed = m_swipeAnimInitialSpeed;
        m_swipeRoot.SetActive(false);
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

    [SerializeField] private GameObject     m_swipeRoot         = null;
    [SerializeField] private Animator       m_leftSwipeAnim     = null;
    [SerializeField] private Animator       m_rightSwipeAnim    = null;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    private float   m_swipeAnimInitialSpeed       = 0.0f;
    private float   m_swipeAnimSpeedBeforePause   = 0.0f;

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
