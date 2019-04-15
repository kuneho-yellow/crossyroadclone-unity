/******************************************************************************
*  @file       TitleUI.cs
*  @brief      Handles the title UI
*  @author     Ron
*  @date       October 10, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class TitleUI : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// Note: Title UI is initialized ahead of the rest of the UI.
    /// </summary>
    /// <param name="startWithBG">if set to <c>true</c> background is shown from the start.</param>
    public void Initialize(bool startWithBG)
    {
        // Create title BG fader animator
        //  State 1: BG visible
        //  State 2: BG invisible
        m_titleBGFader = new UIAnimator(m_background);
        m_titleBGFader.SetAlphaAnimation(m_bgOpaqueAlpha, 0.0f);
        m_titleBGFader.SetAnimTime(m_titleBGFadeTime);
        m_titleBGFader.ResetToState(startWithBG ?
                                    UIAnimator.UIAnimationState.STATE1 :
                                    UIAnimator.UIAnimationState.STATE2);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the title UI.
    /// </summary>
    public void Show()
    {
        m_titleRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the title UI.
    /// </summary>
    public void Hide()
    {
        m_titleRoot.SetActive(false);
    }

    /// <summary>
    /// Starts the TitleEnter animation.
    /// </summary>
    public void StartTitleEnter()
    {
        m_titleAnim.Play(TITLE_ENTER_ANIM_NAME, 0, 0.0f);
        m_isTitleShown = true;
    }

    /// <summary>
    /// Starts the TitleExit animation.
    /// </summary>
    public void StartTitleExit()
    {
        m_titleAnim.Play(TITLE_EXIT_ANIM_NAME, 0, 0.0f);
    }

    /// <summary>
    /// Shows the title.
    /// </summary>
    public void ShowTitle()
    {
        //m_titleAnim.gameObject.SetActive(true);
        m_titleAnim.GetComponentInChildren<SpriteRenderer>().enabled = true;
        m_isTitleShown = true;
    }

    /// <summary>
    /// Hides the title.
    /// </summary>
    public void HideTitle()
    {
        //m_titleAnim.gameObject.SetActive(false);
        m_titleAnim.GetComponentInChildren<SpriteRenderer>().enabled = false;
        m_isTitleShown = false;
    }

    /// <summary>
    /// Shows the background overlay.
    /// </summary>
    /// <param name="titleFade">if set to <c>true</c> fade in the overlay to hide the game screen.
    ///     Else, just show the overlay as a background in UI screens.</param>
    public void ShowBG(bool titleFade)
    {
        m_background.gameObject.SetActive(true);
        m_timeSinceTitleDisplay = 0.0f;
        if (titleFade)
        {
            m_background.sortingOrder = m_bgTitleFadeSortingOrder;
            m_titleBGFader.AnimateToState1();
        }
        else
        {
            m_background.sortingOrder = m_bgNormalSortingOrder;
            m_titleBGFader.ResetToState(UIAnimator.UIAnimationState.STATE1);
        }
    }

    /// <summary>
    /// Hides the background overlay.
    /// </summary>
    /// <param name="titleFade">if set to <c>true</c> fade out the overlay to show the game screen.
    ///     Else, just hide the overlay.</param>
    public void HideBG(bool titleFade)
    {
        if (titleFade) m_titleBGFader.AnimateToState2();
        else m_titleBGFader.ResetToState(UIAnimator.UIAnimationState.STATE2);
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

        // Pause title animation
        m_titleBGFader.Pause();
        m_titleAnimSpeedBeforePause = m_titleAnim.speed;
        m_titleAnim.speed = 0.0f;

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

        // Unpause title animation
        m_titleBGFader.Unpause();
        m_titleAnim.speed = m_titleAnimSpeedBeforePause;

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_titleBGFader.ResetToState1();
        m_timeSinceTitleDisplay = 0.0f;
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

    /// <summary>
    /// Gets whether the title is shown.
    /// </summary>
    public bool IsTitleShown
    {
        get { return m_isTitleShown; }
    }

    /// <summary>
    /// Gets whether the title background is fully shown.
    /// </summary>
    public bool IsBGShown
    {
        get { return m_titleBGFader.IsInState1; }
    }

    /// <summary>
    /// Gets whether the background can be faded out, which is when
    ///  it has been displayed for more than a set duration.
    /// </summary>
    public bool IsReadyToFadeOutBG
    {
        get { return m_timeSinceTitleDisplay > m_titleFadeMinDuration; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject     m_titleRoot     = null;
    [SerializeField] private Animator       m_titleAnim     = null;
    [SerializeField] private SpriteRenderer m_background    = null;

    [Tooltip("Alpha value of the background overlay when \"opaque\"")]
    [SerializeField] private float      m_bgOpaqueAlpha             = 0.99f;
    [Tooltip("Sorting order of the background overlay when in \"background\" mode")]
    [SerializeField] private int        m_bgNormalSortingOrder      = 0;
    [Tooltip("Sorting order of the background overlay during title animation")]
    [SerializeField] private int        m_bgTitleFadeSortingOrder   = 5;
    [Tooltip("Minimum duration that the background overlay remains on screen during title animation before fading out")]
    [SerializeField] private float      m_titleFadeMinDuration      = 1.0f;

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    private float   m_titleAnimSpeedBeforePause = 0.0f;

    #endregion // Variables
    
    #region Animations

    private const string TITLE_ENTER_ANIM_NAME  = "TitleEnter";
    private const string TITLE_EXIT_ANIM_NAME   = "TitleExit";
    private const string TITLE_EMPTY_ANIM_NAME  = "TitleEmpty";

    private bool        m_isTitleShown          = false;
    private float       m_timeSinceTitleDisplay = 0.0f;

    private UIAnimator  m_titleBGFader          = null;
    private float       m_titleBGFadeTime       = 0.5f;

    #endregion // Animations

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

        // Update background animator
        m_titleBGFader.Update(Time.deltaTime);
        // Track the time the background is displayed
        if (m_titleBGFader.IsInState1)
        {
            m_timeSinceTitleDisplay += Time.deltaTime;
        }
        // When background animates to state 2, disable it upon reaching state 2
        if (m_titleBGFader.IsInState2)
        {
            m_background.gameObject.SetActive(false);
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
