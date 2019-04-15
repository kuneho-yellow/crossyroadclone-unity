/******************************************************************************
*  @file       GiftUI.cs
*  @brief      Handles the gift UI
*  @author     Ron
*  @date       October 4, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class GiftUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CoinWinAnimator coinWinAnimUI,
                           System.EventHandler<System.EventArgs> giftOpenDelegate,
                           System.EventHandler<System.EventArgs> giftPlayDelegate,
                           System.EventHandler<GiftOpenedEventArgs> giftOpenedDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        m_coinAnim = coinWinAnimUI;

        // Initialize buttons
        m_openGiftBtn.Initialize(giftOpenDelegate, UIButton.TriggerType.ON_RELEASE);
        m_giftPlayBtn.Initialize(giftPlayDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set button sounds
        m_openGiftBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_giftPlayBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize invisible button on gift model
        m_giftOpener.Initialize(giftOpenDelegate, UIButton.TriggerType.ON_PRESS);

        // Initialize text
        m_giftAmountText.Initialize();
        m_nextGiftTimeText.Initialize();

        // Get delegate to call when gift has been opened
        m_giftOpenedDelegate = giftOpenedDelegate;

        // Initialize gift overlay UI animator
        //  State 1: 1 alpha, covering gift and entire screen
        //  State 2: 0 alpha, revealing gift contents
        m_giftOverlayAnimator = new UIAnimator(m_giftOverlay);
        m_giftOverlayAnimator.SetAlphaAnimation(1.0f, 0.0f);
        m_giftOverlayAnimator.SetAnimTime(m_giftOverlayAnimTime);
        m_giftOverlayAnimator.ResetToState1();

        // Note gift animator's initial speed
        m_giftAnimInitialSpeed = m_giftAnim.speed;

        // Reset state (should come after initializing other values)
        Reset();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the gift UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_openGiftBtn.UpdateScreenPosition();
        m_giftPlayBtn.UpdateScreenPosition();

        m_giftRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the gift UI.
    /// </summary>
    public void Hide()
    {
        m_giftRoot.SetActive(false);
        m_coinAnim.Reset();
    }

    /// <summary>
    /// Sets the text display for the time until the next free gift.
    /// </summary>
    /// <param name="nextGiftTime">The time until the next free gift.</param>
    /// <param name="inHours">if set to <c>true</c> time is in hours. Else, in minutes.</param>
    public void SetNextGiftTime(int nextGiftTime, bool inHours)
    {
        m_nextGiftTimeText.SetText(nextGiftTime.ToString() + (inHours ? "H" : "M"));
    }

    /// <summary>
    /// Starts the gift drop animation.
    /// </summary>
    public void StartGiftDropAnimation()
    {
        // Randomly select and enable one gift model
        m_giftModels[Random.Range(0, m_giftModels.Length)].SetActive(true);
        // Enable gift root object
        m_giftModelRoot.SetActive(true);
        // Start animation
        m_giftAnim.Play(DROP_GIFT_ANIM_NAME);
    }

    /// <summary>
    /// Notifies that the gift drop animation has ended.
    /// </summary>
    public void NotifyGiftDropAnimationEnd()
    {
        // Show open gift button
        m_openGiftBtn.gameObject.SetActive(true);
        // Enable the invisible gift button
        m_giftOpener.gameObject.SetActive(true);

        // Start idle gift animation
        m_giftAnim.Play(IDLE_GIFT_ANIM_NAME);
    }

    /// <summary>
    /// Starts the gift open animation.
    /// </summary>
    /// <param name="coinsInGift">The coin amount to award to player.</param>
    /// <param name="minutesUntilNextGift">The minutes until the next free gift.</param>
    public void StartGiftOpenAnimation(int coinsInGift, int minutesUntilNextGift)
    {
        m_coinsInGift = coinsInGift;

        // Set gift amount text (but don't show yet)
        m_giftAmountText.gameObject.SetActive(false);
        m_giftAmountText.SetText(coinsInGift.ToString());

        // Set next gift time text (but don't show yet)
        m_nextGiftTimeText.gameObject.SetActive(false);
        int timeDisplay = minutesUntilNextGift;
        string timeUnitStr = "M";
        // If time is 60 or greater, show hours
        if (minutesUntilNextGift >= 60)
        {
            timeDisplay /= 60;
            timeUnitStr = "H";
        }
        m_nextGiftTimeText.SetText(NEXT_GIFT_STRING + timeDisplay.ToString() + timeUnitStr);

        // Hide the open gift button
        m_openGiftBtn.gameObject.SetActive(false);

        // Disable the invisible gift button
        m_giftOpener.gameObject.SetActive(false);

        // Play gift opening animation
        m_giftAnim.Play(OPEN_GIFT_ANIM_NAME);

        m_isOpeningGift = true;
    }

    /// <summary>
    /// Notifies that the gift open animation has ended.
    /// </summary>
    public void NotifyGiftOpenAnimationEnd()
    {
        // Show overlay to hide the rest of the screen
        m_giftOverlay.gameObject.SetActive(true);
        
        // Hide the gift
        m_giftModelRoot.SetActive(false);

        // Show the gift contents
        m_giftAmountText.gameObject.SetActive(true);
        
        // Show next gift time and play button
        m_nextGiftTimeText.gameObject.SetActive(true);
        m_giftPlayBtn.gameObject.SetActive(true);

        // Start "win coins" animation
        m_coinAnim.StartWinCoinsAnim(m_coinsInGift, m_winCoinsAnimDelay);
        m_waitWinCoins = true;

        // Notify (scene master) that gift opening has finished
        m_giftOpenedDelegate.Invoke(this, new GiftOpenedEventArgs(m_coinsInGift));

        // Start overlay animation
        m_giftOverlayAnimator.AnimateToState2();

        m_isOpeningGift = false;
    }

    /// <summary>
    /// Custom event args class for the gift opened event.
    /// </summary>
    public class GiftOpenedEventArgs : System.EventArgs
    {
        public int GiftAmount { get; set; }
        public GiftOpenedEventArgs(int giftAmount)
        {
            GiftAmount = giftAmount;
        }
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

        m_giftAnimEvents.Pause();
        m_giftOverlayAnimator.Pause();
        m_giftAnimSpeedBeforePause = m_giftAnim.speed;
        m_giftAnim.speed = 0.0f;

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

        m_giftAnimEvents.Unpause();
        m_giftOverlayAnimator.Unpause();
        m_giftAnim.speed = m_giftAnimSpeedBeforePause;

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        // Hide overlay
        m_giftOverlay.gameObject.SetActive(false);
        m_giftOverlayAnimator.ResetToState1();
        // Reset gift animation
        //m_giftAnim.Play(EMPTY_ANIM_NAME);
        m_giftAnim.speed = m_giftAnimInitialSpeed;
        m_isOpeningGift = false;
        m_waitWinCoins = false;
        m_timeSinceGiftOpened = 0.0f;
        // Hide gift
        m_giftModelRoot.gameObject.SetActive(false);
        // Disable all gift models
        foreach (GameObject model in m_giftModels)
        {
            model.SetActive(false);
        }
        // Hide buttons
        m_openGiftBtn.gameObject.SetActive(false);
        m_giftPlayBtn.gameObject.SetActive(false);
        // Disable the invisible gift button
        m_giftOpener.gameObject.SetActive(false);
        // Reset text
        m_nextGiftTimeText.SetText("");
        m_giftAmountText.SetText("");       
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        m_openGiftBtn.Delete();
        m_giftPlayBtn.Delete();
        m_nextGiftTimeText.Delete();
        m_giftAmountText.Delete();
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets whether a gift is being opened.
    /// </summary>
    public bool IsOpeningGift
    {
        get { return m_isOpeningGift; }
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

    [SerializeField] private GameObject     m_giftRoot              = null;
    [SerializeField] private Animator       m_giftAnim              = null;
    [SerializeField] private GiftAnimEvents m_giftAnimEvents        = null;
    [SerializeField] private SpriteRenderer m_giftOverlay           = null;
    [SerializeField] private float          m_giftOverlayAnimTime   = 0.3f;
    [SerializeField] private GameObject     m_giftModelRoot         = null;
    [SerializeField] private GameObject[]   m_giftModels            = null;

    [SerializeField] private UIText         m_giftAmountText        = null;
    [SerializeField] private UIText         m_nextGiftTimeText      = null;
    [SerializeField] private UIButton       m_openGiftBtn           = null;
    [SerializeField] private UIButton       m_giftPlayBtn           = null;
    [SerializeField] private UIButton       m_giftOpener            = null;

    [Tooltip("Time from the end of the open gift animation until the win coins animation starts")]
    [SerializeField] private float          m_winCoinsAnimDelay     = 1.0f;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    private float   m_giftAnimInitialSpeed      = 0.0f;
    private float   m_giftAnimSpeedBeforePause  = 0.0f;

    private bool    m_isOpeningGift     = false;
    private int     m_coinsInGift       = 0;

    private CoinWinAnimator   m_coinAnim  = null;

    private System.EventHandler<GiftOpenedEventArgs> m_giftOpenedDelegate = null;

    #endregion // Variables

    #region Constants

    private const string NEXT_GIFT_STRING = "FREE GIFT IN ";

    #endregion // Constants

    #region Animation

    private UIAnimator      m_giftOverlayAnimator   = null;

    private const string    DROP_GIFT_ANIM_NAME     = "DropGift";
    private const string    IDLE_GIFT_ANIM_NAME     = "IdleGift";
    private const string    OPEN_GIFT_ANIM_NAME     = "OpenGift";
    private const string    EMPTY_ANIM_NAME         = "Empty";

    private bool    m_waitWinCoins          = false;
    private float   m_timeSinceGiftOpened   = 0.0f;

    /// <summary>
    /// Updates the win coins wait time.
    /// </summary>
    private void UpdateWaitWinCoins()
    {
        if (!m_waitWinCoins)
        {
            return;
        }

        m_timeSinceGiftOpened += Time.deltaTime;
        if (m_timeSinceGiftOpened > m_winCoinsAnimDelay)
        {
            m_waitWinCoins = false;
            m_timeSinceGiftOpened = 0.0f;

            // Animate white overlay
            m_giftOverlay.gameObject.SetActive(true);
            m_giftOverlayAnimator.AnimateToState2();
        }
    }

    #endregion // Animation

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

        UpdateWaitWinCoins();

        // Update overlay animator
        m_giftOverlayAnimator.Update(Time.deltaTime);
        // When overlay animates to state 2, disable it upon reaching state 2
        if (m_giftOverlayAnimator.IsInState2)
        {
            m_giftOverlay.gameObject.SetActive(false);
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
