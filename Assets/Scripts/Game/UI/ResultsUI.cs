/******************************************************************************
*  @file       ResultsUI.cs
*  @brief      Handles the results UI
*  @author     Ron
*  @date       October 13, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class ResultsUI : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CoinWinAnimator coinWinAnimator, 
                           ScreenshotUI screenshotUI,
                           System.EventHandler<System.EventArgs> videoAdsDelegate, 
                           System.EventHandler<System.EventArgs> giftDelegate,
                           System.EventHandler<System.EventArgs> gachaDelegate,
                           System.EventHandler<System.EventArgs> settingsDelegate,
                           System.EventHandler<System.EventArgs> shareDelegate,
                           System.EventHandler<System.EventArgs> playAgainDelegate,
                           System.EventHandler<System.EventArgs> leaderboardsDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        m_coinAnim = coinWinAnimator;
        m_screenshotUI = screenshotUI;

        // Initialize buttons
        m_videoAdsBtn.Initialize(videoAdsDelegate, UIButton.TriggerType.ON_RELEASE);
        m_giftBtn.Initialize(giftDelegate, UIButton.TriggerType.ON_RELEASE);
        m_gachaBtn.Initialize(gachaDelegate, UIButton.TriggerType.ON_RELEASE);
        m_settingsBtn.Initialize(settingsDelegate, UIButton.TriggerType.ON_RELEASE);
        m_shareBtn.Initialize(shareDelegate, UIButton.TriggerType.ON_RELEASE);
        m_playAgainBtn.Initialize(playAgainDelegate, UIButton.TriggerType.ON_RELEASE);
        m_leaderboardsBtn.Initialize(leaderboardsDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set button ounds
        m_videoAdsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_giftBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_gachaBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_settingsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_shareBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_playAgainBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_leaderboardsBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize text
        m_videoAdsText.Initialize();
        m_giftText.Initialize();
        m_gachaText.Initialize();

        // Create animators for the UI strips
        //  State 1: Hidden at the left edge of the screen
        //  State 2: Visible at the center of the screen
        InitializeStripAnimator(ref m_videoAdsStripAnimator, m_videoAdsStrip, m_videoAdsStripStartPos);
        InitializeStripAnimator(ref m_giftStripAnimator, m_giftStrip, m_giftStripStartPos);
        InitializeStripAnimator(ref m_gachaStripAnimator, m_gachaStrip, m_gachaStripStartPos);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts the video ads strip animation.
    /// </summary>
    public void StartVideoAdsStripAnim()
    {
        // Start animation
        m_videoAdsStripAnimator.AnimateToState2();
        // Play video ads strip sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.VideoAdsStrip);
    }

    /// <summary>
    /// Starts the gift strip animation.
    /// </summary>
    public void StartGiftStripAnim()
    {
        // Start animation
        m_giftStripAnimator.AnimateToState2();
        // Play gift strip sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.GiftStrip);
    }

    /// <summary>
    /// Starts the gacha strip animation.
    /// </summary>
    public void StartGachaStripAnim()
    {
        // Start animation
        m_gachaStripAnimator.AnimateToState2();
        // Play gacha strip sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.GachaStrip);
    }

    /// <summary>
    /// Sets up values for the video ads strip.
    /// </summary>
    /// <param name="withAds">if set to <c>true</c> a video ad is available.</param>
    /// <param name="watchedAd">if set to <c>true</c> the video ad was watched.</param>
    /// <param name="coinsEarned">Amount of coins earned for watching the videdo ad.</param>
    public void SetUpVideoAdsStrip(bool withAds, bool watchedAd = false, int coinsEarned = 0)
    {
        m_videoAdsBtn.gameObject.SetActive(withAds);
        m_videoAdsText.transform.SetLocalPosX(withAds ? m_textPosXWithButton : 0.0f);
        string text = "";
        if (watchedAd)      text = coinsEarned.ToString() + VIDEO_ADS_TEXT_AD_WATCHED;
        else if (withAds)   text = VIDEO_ADS_TEXT_WITH_ADS;
        else                text = VIDEO_ADS_TEXT_NO_ADS;
        m_videoAdsText.SetText(text);
    }

    /// <summary>
    /// Sets up values for the gift strip.
    /// </summary>
    /// <param name="withGift">if set to <c>true</c> gift is available.</param>
    /// <param name="minutesUntilNextGift">The time until the next gift is available (if currently unavailable).</param>
    public void SetUpGiftStrip(bool withGift, int minutesUntilNextGift = 0)
    {
        // Setup strip first
        m_giftBtn.gameObject.SetActive(withGift);
        m_giftText.transform.SetLocalPosX(withGift ? m_textPosXWithButton : 0.0f);
        m_giftText.SetText(withGift ? GIFT_TEXT_ACTIVE : GetGiftTextInactive(minutesUntilNextGift));
    }

    /// <summary>
    /// Sets up values for the gacha strip.
    /// </summary>
    /// <param name="withGacha">if set to <c>true</c> gacha is available.</param>
    /// <param name="coinsNeeded">The coins needed for the next gacha (if gacha is unavailable).</param>
    public void SetUpGachaStrip(bool withGacha, int coinsNeeded = 0)
    {
        m_gachaBtn.gameObject.SetActive(withGacha);
        m_gachaText.transform.SetLocalPosX(withGacha ? m_textPosXWithButton : 0.0f);
        m_gachaText.SetText(withGacha ? GACHA_TEXT_ACTIVE : GetGachaTextInactive(coinsNeeded));
    }

    /// <summary>
    /// Starts the win coins animation.
    /// </summary>
    /// <param name="coinAmount">The amount of coins to award the player.</param>
    public void StartWinCoinsAnim(int coinAmount)
    {
        m_coinAnim.StartWinCoinsAnim(coinAmount, m_timeFromRewardToWinCoins);
    }

    /// <summary>
    /// Starts the screenshot animation.
    /// </summary>
    /// <param name="startPos">The start position for the screenshot.</param>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void StartScreenshotAnim(Vector3 startPos, Texture2D screenshotTexture)
    {
        m_screenshotUI.StartScreenshotAnim(startPos,
                                           m_shareBtn.transform.position,
                                           screenshotTexture);
    }

    /// <summary>
    /// Shows the still screenshot.
    /// </summary>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void ShowStillScreenshot(Texture2D screenshotTexture = null)
    {
        m_screenshotUI.ShowStillScreenshot(m_shareBtn.transform.position,
                                           screenshotTexture);
    }

    /// <summary>
    /// Shows the results UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_settingsBtn.UpdateScreenPosition();
        m_shareBtn.UpdateScreenPosition();
        m_playAgainBtn.UpdateScreenPosition();
        m_leaderboardsBtn.UpdateScreenPosition();

        m_resultsRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the results UI.
    /// </summary>
    public void Hide()
    {
        m_resultsRoot.SetActive(false);
        m_coinAnim.Reset();
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

        m_videoAdsStripAnimator.Pause();
        m_giftStripAnimator.Pause();
        m_gachaStripAnimator.Pause();

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

        m_videoAdsStripAnimator.Unpause();
        m_giftStripAnimator.Unpause();
        m_gachaStripAnimator.Unpause();

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        // Reset animators
        m_videoAdsStripAnimator.ResetToState1();
        m_giftStripAnimator.ResetToState1();
        m_gachaStripAnimator.ResetToState1();
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

    [SerializeField] private GameObject m_resultsRoot       = null;

    [Header("Video Ads")]
    [SerializeField] private Transform  m_videoAdsStrip     = null;
    [SerializeField] private UIText     m_videoAdsText      = null;
    [SerializeField] private UIButton   m_videoAdsBtn       = null;

    [Header("Gift")]
    [SerializeField] private Transform  m_giftStrip         = null;
    [SerializeField] private UIText     m_giftText          = null;
    [SerializeField] private UIButton   m_giftBtn           = null;

    [Header("Gacha")]
    [SerializeField] private Transform  m_gachaStrip        = null;
    [SerializeField] private UIText     m_gachaText         = null;
    [SerializeField] private UIButton   m_gachaBtn          = null;

    [Header("Other buttons")]
    [SerializeField] private UIButton   m_settingsBtn       = null;
    [SerializeField] private UIButton   m_shareBtn          = null;
    [SerializeField] private UIButton   m_playAgainBtn      = null;
    [SerializeField] private UIButton   m_leaderboardsBtn   = null;

    [SerializeField] private float      m_textPosXWithButton    = -2.5f;

    [Header("Strip animation")]
    [SerializeField] private float      m_videoAdsStripStartPos = -38.0f;
    [SerializeField] private float      m_giftStripStartPos     = -48.0f;
    [SerializeField] private float      m_gachaStripStartPos    = -58.0f;
    [SerializeField] private float      m_stripAnimSpeed        = 80.0f;
    [Tooltip("Time from finishing a video ad until the win coins animation starts")]
    [SerializeField] private float      m_timeFromRewardToWinCoins  = 0.7f;

    #endregion // Serialized Variables

    #region UI Strip Text

    private const string VIDEO_ADS_TEXT_WITH_ADS    = "EARN ©";
    private const string VIDEO_ADS_TEXT_NO_ADS      = "";
    private const string VIDEO_ADS_TEXT_AD_WATCHED  = "© EARNED";
    private const string GIFT_TEXT_ACTIVE           = "FREE GIFT";
    private const string GIFT_TEXT_INACTIVE         = "NEXT GIFT IN ";
    private const string GACHA_TEXT_ACTIVE          = "WIN A PRIZE";
    private const string GACHA_TEXT_INACTIVE        = "© TO GO";

    /// <summary>
    /// Gets the text for the gift strip when gift is currently unavailable.
    /// </summary>
    /// <param name="nextGiftTime">The next gift time.</param>
    /// <returns></returns>
    private string GetGiftTextInactive(int minutesUntilNextGift)
    {
        int timeDisplay = minutesUntilNextGift;
        string timeUnitStr = "M";
        // If time is 60 or greater, show hours
        if (minutesUntilNextGift >= 60)
        {
            timeDisplay /= 60;
            timeUnitStr = "H";
        }
        return GIFT_TEXT_INACTIVE + timeDisplay.ToString() + timeUnitStr;
    }

    /// <summary>
    /// Gets the text for the gacha strip when gacha is currently unavailable.
    /// </summary>
    /// <param name="coinsNeeded">The number of coins needed for the next gacha.</param>
    /// <returns></returns>
    private string GetGachaTextInactive(int coinsNeeded)
    {
        return coinsNeeded.ToString() + GACHA_TEXT_INACTIVE;
    }

    #endregion // UI Strip Text

    #region Variables

    private bool m_isInitialized    = false;
    private bool m_isPaused         = false;

    private CoinWinAnimator   m_coinAnim      = null;
    private ScreenshotUI    m_screenshotUI  = null;

    #endregion // Variables

    #region Animation

    private UIAnimator m_videoAdsStripAnimator  = null;
    private UIAnimator m_giftStripAnimator      = null;
    private UIAnimator m_gachaStripAnimator     = null;

    /// <summary>
    /// Initializes a UI strip animator.
    //  State 1: Hidden at the left edge of the screen
    //  State 2: Visible at the center of the screen
    /// </summary>
    /// <param name="animator">The animator to create and initialize.</param>
    /// <param name="strip">The strip to animate.</param>
    /// <param name="startPos">The starting horizontal position of the strip animation.</param>
    private void InitializeStripAnimator(ref UIAnimator animator, Transform strip, float startPos)
    {
        animator = new UIAnimator(strip);
        Vector3 stripEndPos = strip.position;
        Vector3 stripStartPos = new Vector3(startPos, stripEndPos.y, stripEndPos.z);
        animator.SetPositionAnimation(stripStartPos, stripEndPos);
        animator.SetAnimSpeed(m_stripAnimSpeed);
        animator.ResetToState1();
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

        // Update results strip animators
        m_videoAdsStripAnimator.Update(Time.deltaTime);
        m_giftStripAnimator.Update(Time.deltaTime);
        m_gachaStripAnimator.Update(Time.deltaTime);
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
            m_screenshotUI.UpdateAnimPos(m_shareBtn.transform.position);
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
