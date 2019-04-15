/******************************************************************************
*  @file       GameSceneMaster.cs
*  @brief      Handles the lifetime of a single scene
*  @author     Ron
*  @date       October 19, 2015
*      
*  @par [explanation]
*		> Loads and unloads resources for one scene
******************************************************************************/

#define ENABLE_TEST_PURCHASES

#region Namespaces

using UnityEngine;
using UnityEngine.Advertisements;

#endregion // Namespaces

public class GameSceneMaster : SceneMasterBase 
{
    #region Public Interface

	public override bool Load()
	{
        m_characterResource.InitializeData(m_characterDataJSONPath);

        // Initialize data system using character resource
        m_dataSystem = (SoomlaDataSystem)Locator.GetDataSystem();
        m_dataSystem.LateInitialize(m_characterResource);

        // The following systems may need time to initialize
        // GameSceneMaster will be considered "initialized" once all systems have finished initializing
        // Note: This should be done after character resource has been initialized
        m_storeManager.Initialize(m_characterResource, OnCharPurchaseSucceeded, OnCharPurchaseCancelled,
                                   OnRestorePurchasesCompleted, OnRestorePurchasesFailed,
                                   OnCoinBalanceChanged, OnGachaBalanceChanged);
        m_profileManager.Initialize();
        m_gameManager.Initialize();

        return true;
	}

    public override bool Unload()
	{
        //m_storeManager.Delete();
        //m_profileManager.Delete();
        
        // Clear initialized flag
        m_isInitialized = false;

        return true;
	}

	public override void StartScene()
	{
        // Initialize gift manager
        // Note: This should be done after SoomlaDataSystem has been initialized
        m_giftManager.Initialize(m_dataSystem);

        // If rotation-locked, switch to preferred orientation
        bool isRotationLocked = m_dataSystem.IsRotationLocked;
        if (isRotationLocked)
        {
            LockScreenRotation(m_dataSystem.GetOrientationPref());
        }
        // If no preference saved yet, default to auto-rotation
        else
        {
            m_dataSystem.SetOrientationPref(ScreenOrientation.AutoRotation);
        }
        
        // Apply mute state
        bool isMuted = m_dataSystem.GetMuteState();
        if (isMuted)
        {
            Locator.GetSoundManager().MuteAllSounds();
        }
        
        // Get button sound delegates from UIManager
        UIManager uiManager = (UIManager)Locator.GetUIManager();
        System.EventHandler<System.EventArgs> UIButtonPressHandler = uiManager.UIButtonPressHandler;
        System.EventHandler<System.EventArgs> UIButtonReleaseHandler = uiManager.UIButtonReleaseHandler;

        // Initialize UI classes
        // Initialize the "master" GameUI first
        m_gameUI.Initialize(OnScreenshotScreenTouch, OnBackBtnTap, UIButtonPressHandler, UIButtonReleaseHandler);
        // Start (Character Select, Achievements, Leaderboards)
        m_gameUI.StartUI.Initialize(OnCharSelectBtnTap, OnAchievementsBtnTap, OnLeaderboardsBtnTap,
                                    UIButtonPressHandler, UIButtonReleaseHandler);
        // Sign-in
        m_gameUI.SignInUI.Initialize(OnSignInBtnTap, UIButtonPressHandler, UIButtonReleaseHandler);
        // Pause
        m_gameUI.PauseUI.Initialize(OnPauseBtnTap, OnUnpauseScreenTouch, UIButtonPressHandler, UIButtonReleaseHandler);
        // Settings
        m_gameUI.SettingsUI.Initialize(isMuted, isRotationLocked, OnCreditsBtnTap, OnMuteBtnTap, OnRotationLockBtnTap,
                                       OnRestorePurchasesBtnTap, OnSettingsSignInBtnTap, OnSignOutBtnTap,
                                       UIButtonPressHandler, UIButtonReleaseHandler);
        // Credits
        m_gameUI.CreditsUI.Initialize(OnKunehoSiteBtnTap, UIButtonPressHandler, UIButtonReleaseHandler);
        // Character Select
        m_gameUI.CharSelectUI.Initialize(this, m_gameUI.NewCharWinAnimator, m_defaultCharRotation, m_characterResource,
                                         OnPlayCharacterBtnTap, OnBuyCharacterBtnTap, OnCharSelectShareBtnTap,
                                         OnOwnedCharCountChanged, UIButtonPressHandler, UIButtonReleaseHandler);
        // Top Score
        m_gameUI.TopScoreUI.Initialize(m_gameUI.ScreenshotUI, OnTopScoreShareBtnTap, OnBackBtnTap,
                                       UIButtonPressHandler, UIButtonReleaseHandler);
        // Results
        m_gameUI.ResultsUI.Initialize(m_gameUI.CoinWinAnimator, m_gameUI.ScreenshotUI, OnVideoAdsBtnTap,
                                      OnGiftBtnTap, OnGachaBtnTap, OnSettingsBtnTap,
                                      OnResultsShareBtnTap, OnPlayAgainBtnTap, OnLeaderboardsBtnTap,
                                      UIButtonPressHandler, UIButtonReleaseHandler);
        // Gift
        m_gameUI.GiftUI.Initialize(m_gameUI.CoinWinAnimator, OnGiftOpenBtnTap, OnGiftPlayBtnTap, OnGiftOpened,
                                   UIButtonPressHandler, UIButtonReleaseHandler);
        // Gacha
        m_gameUI.GachaUI.Initialize(m_characterResource, m_gameUI.NewCharWinAnimator, m_defaultCharRotation,
                                    OnGachaOpenBtnTap, OnGachaPlayBtnTap, OnGachaShareBtnTap, OnRepeatGachaBtnTap,
                                    OnGachaOpened, UIButtonPressHandler, UIButtonReleaseHandler);
        // Score (Score and Top Score HUD)
        m_gameUI.ScoreUI.Initialize();
        // Coins (HUD)
        m_gameUI.CoinsUI.Initialize();

        // If game started from Game scene, scroll in Title UI
        // If game started from Title scene, Title UI would already be scrolled in
        // Note: This should be done after Game UI has been initialized
        if (Locator.GetMain().GetPrevSceneEnum != SceneInfo.SceneEnum.TITLE)
        {
            // Scroll in title and show title BG
            m_gameUI.TitleUI.ShowTitle();
            m_gameUI.TitleUI.StartTitleEnter();
            m_gameUI.TitleUI.ShowBG(false);
        }

        // Switch to LOADING state
        ChangeState(MetaGameState.LOADING);
    }
    
    /// <summary>
    /// Notifies that the game has ended.
    /// </summary>
    /// <param name="endScore">The score at the end of the game.</param>
    /// <param name="screenshot">The screenshot.</param>
    public void NotifyGameOver(int endScore, Texture2D screenshot)
    {
        if (m_state != MetaGameState.GAME)
        {
            return;
        }
        // Get current top score
        int topScore = m_dataSystem.GetTopScore();
        // Check if score is a new top score
        bool isNewTopScore = endScore > topScore;
        // Check if score is a great score, i.e. higher than a certain fraction of the current top score
        bool isGreatScore = !isNewTopScore && (endScore > m_greatScoreFractionOfTop * topScore);

        // If new top score...
        if (isNewTopScore)
        {
            // Save
            m_dataSystem.SetTopScore(endScore);

            // Post to top score leaderboard
            Locator.GetPlayServicesSystem().PostScoreToLeaderboard(endScore, LeaderboardType.TopScore, OnReportScoreFinished);
        }

        // Set top score text
        m_gameUI.ScoreUI.SetTopScore(isNewTopScore ? endScore : topScore, isNewTopScore);

        // Check if unlock requirements for special characters are met
        //  Ron - Score >= 6 using Black & White Cow
        if (endScore >= 6 && EquippedCharacter == CharacterType.Cow_BlackWhite)
        {
            m_storeManager.GiveCharacter(CharacterType.Ron);
            // Update character list in character select
            m_gameUI.CharSelectUI.OnGetNewGachaCharacter(CharacterType.Ron);
        }
        //  Lori - Score >= 11 using White Rabbit
        if (endScore >= 11 && EquippedCharacter == CharacterType.Rabbit_White)
        {
            m_storeManager.GiveCharacter(CharacterType.Lori);
            // Update character list in character select
            m_gameUI.CharSelectUI.OnGetNewGachaCharacter(CharacterType.Lori);
        }
        // TODO: Add special character unlock UI

        // If top or great score...
        if (isNewTopScore || isGreatScore)
        {
            // Set values in top score window
            m_gameUI.TopScoreUI.SetScore(endScore, isNewTopScore);
            // Set game over screenshot
            if (endScore >= MIN_SCORE_FOR_SCREENSHOT)
            {
                // If score is above a certain value, show a special screenshot animation
                m_gameUI.TopScoreUI.StartScreenshotAnim(m_screenshotAnimStartPos, screenshot);
            }
            else
            {
                m_gameUI.ScreenshotUI.SetScreenshotTexture(screenshot);
            }
            // Switch to TOP_SCORE state
            ChangeState(MetaGameState.TOP_SCORE);
        }
        else
        {
            // Set game over screenshot
            m_gameUI.ScreenshotUI.SetScreenshotTexture(screenshot);
            // Switch to RESULTS state
            ChangeState(MetaGameState.RESULTS);
        }
    }

    /// <summary>
    /// Notifies that a score of 50 (or a multiple of it) was reached.
    /// </summary>
    public void NotifyReach50Score()
    {
        m_gameUI.OnReach50Score();
    }

    /// <summary>
    /// Notifies that a coin was collected.
    /// </summary>
    /// <param name="amount">The amount of coins to add to the balance.</param>
    public void NotifyCollectCoin(int amount = 1)
    {
        // Play coin pickup sound
        SoundManager soundManager = (SoundManager)Locator.GetSoundManager();
        soundManager.PlayCoinPickupSound();
        
        // Update coin balance
        int prevBal = m_storeManager.GetCoinBalance();
        m_storeManager.AddToCoinBalance(amount);
        int curBal = m_storeManager.GetCoinBalance();
        
        // Check if balance reaches the amount needed for gacha
        int gachaPrice = m_storeManager.GetGachaPrice();
        if (prevBal < gachaPrice && prevBal + amount >= gachaPrice)
        {
            m_gameUI.OnReach100Coins();
        }

        // Update coins display
        m_gameUI.CoinsUI.SetCoins(curBal);
    }

    /// <summary>
    /// Notifies of a new unlocked achievement.
    /// </summary>
    /// <param name="achievement">The achievement.</param>
    public void NotifyAchievementUnlocked(AchievementType achievement)
    {
        // Unlock the achievement
        Locator.GetPlayServicesSystem().UnlockAchievement(achievement, OnUnlockAchievementFinished);
        // TODO: For now, assume unlocking always succeeds
        switch (achievement)
        {
            case AchievementType.CollectCharacters00:
                m_dataSystem.SetAchievLockState_CollectChars00(true);
                break;
            case AchievementType.CollectCharacters01:
                m_dataSystem.SetAchievLockState_CollectChars01(true);
                break;
            case AchievementType.CollectCharacters02:
                m_dataSystem.SetAchievLockState_CollectChars02(true);
                break;
            case AchievementType.PlayGacha00:
                m_dataSystem.SetAchievLockState_PlayGacha00(true);
                break;
            case AchievementType.StayOnTrainTracks00:
                m_dataSystem.SetAchievLockState_TrainTracks00(true);
                break;
            case AchievementType.MoveInStraightLine00:
                m_dataSystem.SetAchievLockState_StraightLine00(true);
                break;
        }
    }

    /// <summary>
    /// Determines whether the specified character is owned by the player.
    /// </summary>
    public bool IsOwned(CharacterType character)
    {
        return m_storeManager.IsOwned(character);
    }

    /// <summary>
    /// Determines whether the specified character has been used by the player at least once.
    /// </summary>
    public bool IsUsed(CharacterType character) 
    {
        return m_dataSystem.GetCharacterUsed(character);
    }

    /// <summary>
    /// Gets the character resource.
    /// </summary>
    public CharacterResource CharacterResource
    {
        get { return m_characterResource; }
    }

    /// <summary>
    /// Gets the currently equipped character.
    /// </summary>
    public CharacterType EquippedCharacter
    {
        get { return m_storeManager.GetEquippedCharacter(); }
    }

    /// <summary>
    /// Gets the top score.
    /// </summary>
    public int TopScore
    {
        get { return m_dataSystem.GetTopScore(); }
    }

    /// <summary>
    /// Gets the current metagame state.
    /// </summary>
    public MetaGameState State
    {
        get { return m_state; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Reference to the GameManager instance in the game scene")]
    [SerializeField] private    GameManager     m_gameManager               = null;
    [Tooltip("Reference to the GameUI instance in the game scene")]
    [SerializeField] private    GameUI          m_gameUI                    = null;
    [Tooltip("Site to load when the Kuneho button in credits is tapped")]
    [SerializeField] private    string          m_kunehoSiteAddress         = "https://ronlori.wordpress.com/";
    [Tooltip("Path to the character data .json file in the Resources folder")]
    [SerializeField] private    string          m_characterDataJSONPath     = "CharacterData";
    [Tooltip("Optional zone specifier used in Unity Ads")]
    [SerializeField] private    string          m_unityAdsZoneID            = "rewardedVideo";
    [Tooltip("The fraction of the current top score at or above which current score " +
             "will be considered a \"great score\"")]
    [SerializeField] private    float           m_greatScoreFractionOfTop   = 0.8f;
    [Tooltip("Default rotation of characters when shown in UI (in euler angles)")]
    [SerializeField] private    Vector3         m_defaultCharRotation       = new Vector3(-348.0f, 315.0f, 13.0f);
    [SerializeField] private    Vector3         m_screenshotAnimStartPos    = Vector3.zero;

    #endregion // Serialized Variables

    #region Variables

    private SoomlaStoreManager      m_storeManager          = new SoomlaStoreManager();
    private SoomlaProfileManager    m_profileManager        = new SoomlaProfileManager();
    private SoomlaDataSystem        m_dataSystem            = null;
    private GiftManager             m_giftManager           = new GiftManager();
    
    private CharacterResource       m_characterResource     = new CharacterResource();
    private CharacterType           m_latestGachaCharacter  = 0;

    #endregion // Variables

    #region Constants
    
    private const   string          SCREENSHOT_FILE_NAME        = "CRCScreenshot";
    private const   int             MIN_SCORE_FOR_SCREENSHOT    = 50;

    #endregion // Constants

    #region State

    public enum MetaGameState
    {
        NONE,
        LOADING,
        START,
        GAME,
        PAUSE,
        SETTINGS,
        CREDITS,
        CHAR_SELECT,
        RESULTS,
        TOP_SCORE,
        GACHA,
        GIFT
    }
    // TODO: Unserialize
    [SerializeField]
    private MetaGameState m_state = MetaGameState.NONE;
    
    private bool m_hasStartedLoadNewGame = false;

    /// <summary>
    /// Changes metagame state to the specified state.
    /// </summary>
    /// <param name="newState">The new state.</param>
    private void ChangeState(MetaGameState newState)
    {
        // Do nothing if already in the specified state
        if (m_state == newState)
        {
            return;
        }
        switch (newState)
        {
            case MetaGameState.NONE:        /* Empty */                 break;
            case MetaGameState.LOADING:     ChangeStateLoading();       break;
            case MetaGameState.START:       ChangeStateStart();         break;
            case MetaGameState.GAME:        ChangeStateGame();          break;
            case MetaGameState.PAUSE:       ChangeStatePause();         break;
            case MetaGameState.SETTINGS:    ChangeStateSettings();      break;
            case MetaGameState.CREDITS:     ChangeStateCredits();       break;
            case MetaGameState.CHAR_SELECT: ChangeStateCharSelect();    break;
            case MetaGameState.RESULTS:     ChangeStateResults();       break;
            case MetaGameState.TOP_SCORE:   ChangeStateTopScore();      break;
            case MetaGameState.GACHA:       ChangeStateGacha();         break;
            case MetaGameState.GIFT:        ChangeStateGift();          break;
        }
        // Update state
        m_state = newState;
    }

    /// <summary>
    /// Changes metagame state to LOADING (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateLoading()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.NONE:
                // Empty
                break;
            case MetaGameState.CHAR_SELECT:
                m_gameUI.CharSelectUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.RESULTS:
                m_gameUI.ResultsUI.Hide();
                m_gameUI.ScoreUI.Hide();
                m_gameUI.CoinsUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.GACHA:
                m_gameUI.GachaUI.Reset();
                m_gameUI.GachaUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.LOADING);
                return;
        }

        m_gameUI.TitleUI.ShowTitle();
        if (m_state != MetaGameState.NONE)
        {
            m_gameUI.TitleUI.StartTitleEnter();
            m_gameUI.TitleUI.ShowBG(true);
        }
        // Start loading a new game when background has faded in
        m_hasStartedLoadNewGame = false;
    }

    /// <summary>
    /// Changes metagame state to START (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateStart()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.LOADING:
                // Once loaded, fade out the UI background
                m_gameUI.TitleUI.HideBG(true);
                break;
            case MetaGameState.RESULTS:
                m_gameUI.ResultsUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.CHAR_SELECT:
                m_gameUI.CharSelectUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.START);
                return;
        }

        m_gameUI.TitleUI.ShowTitle();
        m_gameUI.CoinsUI.SetCoins(m_storeManager.GetCoinBalance());
        m_gameUI.CoinsUI.Show();
        // If there is a new/unused character in the character list,
        //  enable the character select button animation
        m_gameUI.StartUI.EnableCharSelectBtnAnim(m_gameUI.CharSelectUI.IsNewCharacterAvailable());
        m_gameUI.StartUI.Show();
        m_gameUI.TapUI.Show();
    }

    /// <summary>
    /// Changes metagame state to GAME (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateGame()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.START:
                // Scroll out the title text
                if (m_gameUI.TitleUI.IsTitleShown)
                {
                    m_gameUI.TitleUI.StartTitleExit();
                }
                m_gameUI.StartUI.Hide();
                m_gameUI.TapUI.Hide();
                break;
            case MetaGameState.PAUSE:
                m_gameUI.PauseUI.Reset();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.GAME);
                return;
        }

        m_gameUI.ScoreUI.Reset();
        m_gameUI.ScoreUI.Show();
        m_gameUI.PauseUI.ShowPauseBtn();
    }

    /// <summary>
    /// Changes metagame state to PAUSE (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStatePause()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.GAME:
                // Nothing to hide
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.PAUSE);
                return;
        }

        m_gameManager.Pause();
        m_gameUI.PauseUI.StartPause();
    }

    /// <summary>
    /// Changes metagame state to SETTINGS (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateSettings()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.RESULTS:
                m_gameUI.ResultsUI.Hide();
                m_gameUI.CoinsUI.Hide();
                m_gameUI.ScoreUI.Hide();
                break;
            case MetaGameState.CREDITS:
                m_gameUI.CreditsUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.SETTINGS);
                return;
        }

        m_gameUI.SettingsUI.Show();
        m_gameUI.ShowBackButton();
        m_gameUI.TitleUI.ShowBG(false);
    }

    /// <summary>
    /// Changes metagame state to CREDITS (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateCredits()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.SETTINGS:
                m_gameUI.SettingsUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.CREDITS);
                return;
        }

        m_gameUI.CreditsUI.Show();
        m_gameUI.ShowBackButton();
        m_gameUI.TitleUI.ShowBG(false);
    }

    /// <summary>
    /// Changes metagame state to CHAR_SELECT (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateCharSelect()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.START:
                m_gameUI.StartUI.Hide();
                m_gameUI.CoinsUI.Hide();
                m_gameUI.TapUI.Hide();
                m_gameUI.TitleUI.HideTitle();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.CHAR_SELECT);
                return;
        }

        // Show the character list centered on the selected character
        m_gameUI.CharSelectUI.ScrollToIndex((int)EquippedCharacter, false);
        m_gameUI.CharSelectUI.Show();
        m_gameUI.ShowBackButton();
        m_gameUI.TitleUI.ShowBG(false);
    }

    /// <summary>
    /// Changes metagame state to RESULTS (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateResults()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.GAME:
                m_gameUI.PauseUI.Hide();
                m_gameUI.ResultsUI.Reset();
                // Scroll in top score text and enlarge score text
                m_gameUI.ScoreUI.ScrollInTopScoreText();
                m_gameUI.ScoreUI.EnlargeScoreText();
                break;
            case MetaGameState.SETTINGS:
                m_gameUI.SettingsUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.GIFT:
                // State cannot be changed while opening gift
                if (m_gameUI.GiftUI.IsOpeningGift)
                {
                    return;
                }
                m_gameUI.GiftUI.Hide();
                m_gameUI.CoinsUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.GACHA:
                // State cannot be changed while opening gacha
                if (m_gameUI.GachaUI.IsOpeningGacha)
                {
                    return;
                }
                m_gameUI.GachaUI.Hide();
                m_gameUI.CoinsUI.Hide();
                m_gameUI.HideBackButton();
                m_gameUI.TitleUI.HideBG(false);
                break;
            case MetaGameState.TOP_SCORE:
                m_gameUI.ResultsUI.Reset();
                m_gameUI.TopScoreUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.RESULTS);
                return;
        }

        // Update results strips
        bool adAvailable = Advertisement.IsReady(m_unityAdsZoneID);
        UpdateResultsStrips(adAvailable, false, 0);

        // Scroll in results strips only once, either just after game has ended
        //  or after closing the top score UI (previous state = GAME or TOP_SCORE)
        if (m_state == MetaGameState.GAME || m_state == MetaGameState.TOP_SCORE)
        {
            // Scroll in the video ads strip only if an ad is available
            if (adAvailable)
            {
                m_gameUI.ResultsUI.StartVideoAdsStripAnim();
            }
            // Scroll in the gift and gacha strips
            m_gameUI.ResultsUI.StartGiftStripAnim();
            m_gameUI.ResultsUI.StartGachaStripAnim();
        }

        m_gameUI.ResultsUI.Show();
        m_gameUI.ScoreUI.Show();
        m_gameUI.CoinsUI.Show();
    }

    /// <summary>
    /// Changes metagame state to TOP_SCORE (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateTopScore()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.GAME:
                m_gameUI.PauseUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.TOP_SCORE);
                return;
        }

        m_gameUI.TopScoreUI.Show();
        // Play top score sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.TopScore);
        // Scroll in top score text, enlarge score text, assign values
        m_gameUI.ScoreUI.ScrollInTopScoreText();
        m_gameUI.ScoreUI.EnlargeScoreText();
    }

    /// <summary>
    /// Changes metagame state to GACHA (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateGacha()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.RESULTS:
                m_gameUI.ResultsUI.Hide();
                m_gameUI.ScoreUI.Hide();
                m_gameUI.CoinsUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.GACHA);
                return;
        }

        m_gameUI.GachaUI.Reset();
        m_gameUI.GachaUI.Show();
        m_gameUI.CoinsUI.Show();
        m_gameUI.ShowBackButton();
        m_gameUI.TitleUI.ShowBG(false);
    }

    /// <summary>
    /// Changes metagame state to GIFT (should only be called from ChangeState method).
    /// </summary>
    private void ChangeStateGift()
    {
        // Show/hide other UI depending on previous state
        switch (m_state)
        {
            case MetaGameState.RESULTS:
                m_gameUI.ResultsUI.Hide();
                m_gameUI.ScoreUI.Hide();
                m_gameUI.CoinsUI.Hide();
                break;
            default:
                LogStateChangeWarning(m_state, MetaGameState.GIFT);
                return;
        }

        m_gameUI.GiftUI.Reset();
        m_gameUI.GiftUI.Show();
        m_gameUI.CoinsUI.Show();
        m_gameUI.ShowBackButton();
        m_gameUI.TitleUI.ShowBG(false);

        // Start gift animation
        m_gameUI.GiftUI.StartGiftDropAnimation();
    }

    /// <summary>
    /// Updates the metagame state.
    /// </summary>
    private void UpdateState()
    {
        switch (m_state)
        {
            case MetaGameState.NONE:
                break;
            case MetaGameState.LOADING:
                // Start loading new game map when old map has faded out
                if (!m_hasStartedLoadNewGame && m_gameUI.TitleUI.IsBGShown)
                {
                    m_gameManager.LoadNewGame();
                    m_hasStartedLoadNewGame = true;
                }
                // Check if game map has finished loading
                if (m_gameManager.HasGameLoaded)
                {
                    // Keep the background shown for a specified minimum duration
                    if (m_gameUI.TitleUI.IsReadyToFadeOutBG)
                    {
                        // Switch to START state
                        ChangeState(MetaGameState.START);
                    }
                }
                break;
            case MetaGameState.START:
                if (m_gameManager.HasGameStarted)
                {
                    ChangeState(MetaGameState.GAME);
                }
                break;
            case MetaGameState.GAME:
                // TODO: Remove
                if (BuildInfo.IsDebugMode)
                {
                    DebugGameEventSimulators();
                }
                break;
            case MetaGameState.PAUSE:
                // If unpause countdown has finished, unpause the game
                if (!m_gameUI.PauseUI.IsCountingDown && m_gameUI.PauseUI.IsCountdownFinished)
                {
                    m_gameUI.PauseUI.Reset();
                    m_gameUI.Unpause();
                    m_gameManager.Unpause();
                    // Resume play
                    ChangeState(MetaGameState.GAME);
                }
                break;
            case MetaGameState.SETTINGS:
                break;
            case MetaGameState.CREDITS:
                break;
            case MetaGameState.CHAR_SELECT:
                break;
            case MetaGameState.RESULTS:
                break;
            case MetaGameState.TOP_SCORE:
                break;
            case MetaGameState.GACHA:
                break;
            case MetaGameState.GIFT:
                break;
        }
    }

    /// <summary>
    /// Logs the warning for unsupported metagame state changes.
    /// </summary>
    /// <param name="fromState">State to switch from.</param>
    /// <param name="toState">State to switch to.</param>
    private void LogStateChangeWarning(MetaGameState fromState, MetaGameState toState)
    {
        Debug.LogWarning("Unsupported state change: " +
                         fromState.ToString() + " to " + toState.ToString());
    }

    /// <summary>
    /// Handles the "back" command, from either UI or device back button.
    /// </summary>
    private void Back()
    {
        // If sign-in UI is open, close it first
        if (m_gameUI.SignInUI.IsShown)
        {
            m_gameUI.SignInUI.Hide();
            return;
        }

        // If screenshot UI is open, close it first
        if (m_gameUI.ScreenshotUI.IsShown)
        {
            m_gameUI.ScreenshotUI.HideStillScreenshot();
        }

        switch (m_state)
        {
            case MetaGameState.NONE:
                // Empty
                break;
            case MetaGameState.LOADING:
                Application.Quit();
                break;
            case MetaGameState.START:
                Application.Quit();
                break;
            case MetaGameState.GAME:
                ChangeState(MetaGameState.PAUSE);
                break;
            case MetaGameState.PAUSE:
                // If counting down, cancel countdown
                if (m_gameUI.PauseUI.IsCountingDown)
                {
                    m_gameUI.PauseUI.CancelUnpauseCountdown();
                }
                // If not counting down, start countdown
                else
                {
                    m_gameUI.PauseUI.StartUnpauseCountdown();
                }
                break;
            case MetaGameState.SETTINGS:
                ChangeState(MetaGameState.RESULTS);
                break;
            case MetaGameState.CREDITS:
                ChangeState(MetaGameState.SETTINGS);
                break;
            case MetaGameState.CHAR_SELECT:
                ChangeState(MetaGameState.START);
                break;
            case MetaGameState.RESULTS:
                ChangeState(MetaGameState.LOADING);
                break;
            case MetaGameState.TOP_SCORE:
                ChangeState(MetaGameState.RESULTS);
                break;
            case MetaGameState.GACHA:
                // Use the previous character instead of the character from gacha
                ChangeState(MetaGameState.RESULTS);
                break;
            case MetaGameState.GIFT:
                ChangeState(MetaGameState.RESULTS);
                break;
        }
    }

    /// <summary>
    /// Updates the results strips - video ads, gift, and gacha.
    /// </summary>
    /// <param name="adAvailable">if set to <c>true</c> a video ad is available for watching.</param>
    /// <param name="watchedAd">if set to <c>true</c> the video ad was watched.</param>
    /// <param name="coinsEarned">Amount of coins earned for watching the videdo ad.</param>
    private void UpdateResultsStrips(bool adAvailable, bool watchedAd, int coinsEarned)
    {
        //  Set values for the video ads strip;
        m_gameUI.ResultsUI.SetUpVideoAdsStrip(adAvailable, watchedAd, coinsEarned);
        //  Set values for the free gift
        bool giftAvailable = m_giftManager.IsGiftAvailable;
        int minsUntilNextGift = m_giftManager.GetMinutesUntilNextGift();
        m_gameUI.ResultsUI.SetUpGiftStrip(giftAvailable, minsUntilNextGift);
        //  Set values for the gacha
        bool gachaAvailable = m_storeManager.CanAffordGacha();
        int coinsNeeded = CRCAssets.GACHA_PRICE - m_storeManager.GetCoinBalance();
        m_gameUI.ResultsUI.SetUpGachaStrip(gachaAvailable, coinsNeeded); 
    }

    #endregion // State

    #region Button Delegates

    /// <summary>
    /// Called when the character select button in start UI is tapped.
    /// </summary>
    private void OnCharSelectBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.CHAR_SELECT);
    }

    /// <summary>
    /// Called when the leaderboards button in start or results UI is tapped.
    /// </summary>
    private void OnLeaderboardsBtnTap(object sender, System.EventArgs e)
    {
        // If signed in, show leaderboards
        if (Locator.GetPlayServicesSystem().IsSignedInToGoogle)
        {
            Locator.GetPlayServicesSystem().ShowLeaderboardUI(LeaderboardType.TopScore);
        }
        // Else, show sign-in UI
        else
        {
            m_gameUI.SignInUI.Show();
        }
    }

    /// <summary>
    /// Called when the achievements button in start UI is tapped.
    /// </summary>
    private void OnAchievementsBtnTap(object sender, System.EventArgs e)
    {
        // If signed in, show achievements
        if (Locator.GetPlayServicesSystem().IsSignedInToGoogle)
        {
            Locator.GetPlayServicesSystem().ShowAchievementsUI();
        }
        // Else, show sign-in UI
        else
        {
            m_gameUI.SignInUI.Show();
        }
    }

    /// <summary>
    /// Called when the pause button is tapped.
    /// </summary>
    private void OnPauseBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.PAUSE);
    }

    /// <summary>
    /// Called when the screen is touched while game is paused.
    /// </summary>
    private void OnUnpauseScreenTouch(object sender, System.EventArgs e)
    {
        // If game is paused and countdown has not yet begun, check for any input
        if (m_state == MetaGameState.PAUSE && !m_gameUI.PauseUI.IsCountingDown)
        {
            // If input is detected, start unpause countown
            m_gameUI.PauseUI.StartUnpauseCountdown();
        }
    }

    /// <summary>
    /// Called when the play character button in character select UI is tapped.
    /// </summary>
    private void OnPlayCharacterBtnTap(object sender, System.EventArgs e)
    {
        // Get the focused item in the character scroll list
        bool changedCharacter = false;
        CharacterSelectItem focusedItem = m_gameUI.CharSelectUI.FocusedItem;
        if (focusedItem != null)
        {
            // Get the scroll item
            UIScrollItem scrollItem = focusedItem.ScrollItem;
            if (scrollItem != null)
            {
                // Update selected character
                m_storeManager.EquipCharacter((CharacterType)scrollItem.Index);
                changedCharacter = true;

                // Set character "used" state in character select
                m_gameUI.CharSelectUI.SetUsed((CharacterType)scrollItem.Index);
            }
        }
        // If a new character is selected, load a new game map
        // Else, just return to the START screen
        ChangeState(changedCharacter ? MetaGameState.LOADING : MetaGameState.START);
    }

    /// <summary>
    /// Called when the buy character button in character select UI is tapped.
    /// </summary>
    private void OnBuyCharacterBtnTap(object sender, System.EventArgs e)
    {
        // Get the focused item in the character scroll list
        CharacterSelectItem focusedItem = m_gameUI.CharSelectUI.FocusedItem;
        if (focusedItem != null)
        {
            // Get the scroll item
            UIScrollItem scrollItem = focusedItem.ScrollItem;
            if (scrollItem != null)
            {
                CharacterType character = (CharacterType)scrollItem.Index;
                CharacterResource.CharacterStruct charInfo = m_characterResource.GetCharacterStruct(character);
                // Make sure character is buyable
                if (charInfo.IsBuyable)
                {
#if ENABLE_TEST_PURCHASES
                    m_storeManager.GiveCharacter(character);
                    OnCharPurchaseSucceeded(charInfo.ItemID);
#else
                    m_storeManager.BuyCharacter(character);
#endif
                }
            }
        }
    }

    /// <summary>
    /// Called when the share button in character select UI is tapped.
    /// </summary>
    private void OnCharSelectShareBtnTap(object sender, System.EventArgs e)
    {
        // Get name of focused character in character select
        CharacterSelectItem focusedItem = m_gameUI.CharSelectUI.FocusedItem;
        if (focusedItem != null)
        {
            CharacterType focusedChar = (CharacterType)focusedItem.ScrollItem.Index;
            string charName = m_characterResource.GetCharacterStruct(focusedChar).Name;
            string shareMessage = charName + "! #roadycross";
            m_profileManager.MultiShareText(shareMessage);

            if (BuildInfo.IsDebugMode)
            {
                Debug.Log("Share: \"" + shareMessage + "\"");
            }
        }
    }

    /// <summary>
    /// Called when the the number of characters owned by the player changes.
    /// </summary>
    private void OnOwnedCharCountChanged(int ownedCharCount)
    {
        switch (ownedCharCount)
        {
            case 5:
                NotifyAchievementUnlocked(AchievementType.CollectCharacters00);
                break;
            case 10:
                NotifyAchievementUnlocked(AchievementType.CollectCharacters01);
                break;
            case 20:
                NotifyAchievementUnlocked(AchievementType.CollectCharacters02);
                break;
        }
    }

    /// <summary>
    /// Called when the credits button in settings UI is tapped.
    /// </summary>
    private void OnCreditsBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.CREDITS);
    }

    /// <summary>
    /// Called when the mute button in settings UI is tapped.
    /// </summary>
    private void OnMuteBtnTap(object sender, System.EventArgs e)
    {
        // Toggle mute state
        if (m_dataSystem.GetMuteState())
        {
            m_dataSystem.SetMuteState(false);
            Locator.GetSoundManager().UnmuteAllSounds();
        }
        else
        {
            m_dataSystem.SetMuteState(true);
            Locator.GetSoundManager().MuteAllSounds();
        }
    }

    /// <summary>
    /// Called when the rotation lock button in settings UI is tapped.
    /// </summary>
    private void OnRotationLockBtnTap(object sender, System.EventArgs e)
    {
        ScreenOrientation orientationPref = m_dataSystem.GetOrientationPref();
        bool rotationLocked = orientationPref == ScreenOrientation.Portrait ||
                              orientationPref == ScreenOrientation.PortraitUpsideDown ||
                              orientationPref == ScreenOrientation.Landscape ||
                              orientationPref == ScreenOrientation.LandscapeRight;
        
        // If not locked, lock to the current orientation
        if (!rotationLocked)
        {
            LockScreenRotation(Screen.orientation);
            if (orientationPref != Screen.orientation)
            {
                m_dataSystem.SetOrientationPref(Screen.orientation);
            }
        }
        // If locked, enable auto rotation
        else
        {
            UnlockScreenRotation();
            m_dataSystem.SetOrientationPref(ScreenOrientation.AutoRotation);
        }
    }

    /// <summary>
    /// Called when the restore purchases button in settings UI is tapped.
    /// </summary>
    private void OnRestorePurchasesBtnTap(object sender, System.EventArgs e)
    {
        m_storeManager.RestoreTransactions();
    }

    /// <summary>
    /// Called when the sign in button in settings UI is tapped.
    /// </summary>
    private void OnSettingsSignInBtnTap(object sender, System.EventArgs e)
    {
        // Show sign-in UI
        m_gameUI.SignInUI.Show();
    }

    /// <summary>
    /// Called when the sign out button in settings UI is tapped.
    /// </summary>
    private void OnSignOutBtnTap(object sender, System.EventArgs e)
    {
        // If signed in, sign out from Google
        if (Locator.GetPlayServicesSystem().IsSignedInToGoogle)
        {
            Locator.GetPlayServicesSystem().SignOutFromGoogle();
        }
        // Update settings UI
        m_gameUI.SettingsUI.SetSignedInState(false);
    }

    /// <summary>
    /// Called when the sign in button in sign-in UI is tapped.
    /// </summary>
    private void OnSignInBtnTap(object sender, System.EventArgs e)
    {
        // If not signed in, sign in to Google
        if (!Locator.GetPlayServicesSystem().IsSignedInToGoogle)
        {
            Locator.GetPlayServicesSystem().SignInToGoogle(OnSignInFinished);
        }
        // Else, hide sign-in UI and update settings UI
        else
        {
            m_gameUI.SignInUI.Hide();
            m_gameUI.SettingsUI.SetSignedInState(true);
        }
    }

    /// <summary>
    /// Called when the video ads button in results UI is tapped.
    /// </summary>
    private void OnVideoAdsBtnTap(object sender, System.EventArgs e)
    {
        var options = new ShowOptions { resultCallback = OnVideoAdPlaybackEnded };
        Advertisement.Show(m_unityAdsZoneID, options);
    }
    
    /// <summary>
    /// Called when video ad has ended.
    /// </summary>
    /// <param name="result">Whether video ad playback finished, failed, or was skipped.</param>
    private void OnVideoAdPlaybackEnded(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                int videoAdReward = m_storeManager.GetVideoAdRewardAmount();
                // Reward player for watching the video ad
                m_storeManager.GiveVideoAdReward();
                // Start "win coins" anim
                m_gameUI.ResultsUI.StartWinCoinsAnim(videoAdReward);
                // Update coins display
                m_gameUI.CoinsUI.SetCoins(m_storeManager.GetCoinBalance());
                // Animate coins text
                m_gameUI.CoinsUI.StartEnlargeAndShrinkAnim();
                // Update results strips
                UpdateResultsStrips(false, true, videoAdReward);
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown");
                break;
        }
    }

    /// <summary>
    /// Called when the gift button in results UI is tapped.
    /// </summary>
    private void OnGiftBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.GIFT);
    }

    /// <summary>
    /// Called when the gacha button in results UI is tapped.
    /// </summary>
    private void OnGachaBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.GACHA);
    }

    /// <summary>
    /// Called when the settings button in results UI is tapped.
    /// </summary>
    private void OnSettingsBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.SETTINGS);
    }

    /// <summary>
    /// Called when the share button in results UI is tapped.
    // </summary>
    private void OnResultsShareBtnTap(object sender, System.EventArgs e)
    {
        // Show screenshot UI
        m_gameUI.ResultsUI.ShowStillScreenshot();

        OnShareBtnTap();
    }
    
    /// <summary>
    /// Called when the play again button in results UI is tapped.
    // </summary>
    private void OnPlayAgainBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.LOADING);
    }
    
    /// <summary>
     /// Called when the share button in top score UI is tapped.
    // </summary>
    private void OnTopScoreShareBtnTap(object sender, System.EventArgs e)
    {
        // Show screenshot UI
        m_gameUI.TopScoreUI.ShowStillScreenshot();

        OnShareBtnTap();
    }

    /// <summary>
    /// Called when the share button in results or top score UI is tapped.
    /// </summary>
    private void OnShareBtnTap()
    {
        // Save screenshot
        string filePath = Application.persistentDataPath + SCREENSHOT_FILE_NAME;
        m_gameUI.ScreenshotTaker.SaveLastScreenshot(filePath);

        string shareMessage = "Play Roady Cross now!";
        m_profileManager.MultiShareImage(shareMessage, filePath);

        if (BuildInfo.IsDebugMode)
        {
            Debug.Log("Share: \"" + shareMessage + "\"");
        }
    }

    /// <summary>
    /// Called when the kuneho site button in credits UI is tapped.
    /// </summary>
    private void OnKunehoSiteBtnTap(object sender, System.EventArgs e)
    {
        Application.OpenURL(m_kunehoSiteAddress);
    }

    /// <summary>
    /// Called when the gacha open button in gacha UI is tapped.
    /// </summary>
    private void OnGachaOpenBtnTap(object sender, System.EventArgs e)
    {
        // Make sure player can afford gacha
        if (!m_storeManager.CanAffordGacha())
        {
            return;
        }

        // Deduct gacha cost from coin balance
        m_storeManager.BuyGachaRound();

        // Get a random character from the list of characters available in gacha
        var gachaCharacters = m_characterResource.GachaCharacterIndices;
        int typeIndex = gachaCharacters[Random.Range(0, gachaCharacters.Count)];
        CharacterType character = (CharacterType)typeIndex;
        
        // Get whether selected character is already owned by player
        bool isOwned = m_storeManager.IsOwned(character);
        // If not yet owned, give character to the player
        if (!isOwned)
        {
            m_storeManager.GiveCharacter(character);
            // Update character list in character select
            m_gameUI.CharSelectUI.OnGetNewGachaCharacter(character);
        }

        // Start gacha animation
        m_gameUI.GachaUI.StartGachaOpenAnimation(character, isOwned);
        // Store gacha character type
        m_latestGachaCharacter = character;
        // Hide back button
        m_gameUI.HideBackButton();
    }

    /// <summary>
    /// Called when play button in gacha UI is tapped.
    /// </summary>
    private void OnGachaPlayBtnTap(object sender, System.EventArgs e)
    {
        // Equip the character obtained from gacha
        m_storeManager.EquipCharacter(m_latestGachaCharacter);
        // Set character "used" state in character select
        m_gameUI.CharSelectUI.SetUsed(m_latestGachaCharacter);
        // Load new game using the new selected character
        ChangeState(MetaGameState.LOADING);
    }

    /// <summary>
    /// Called when gacha share button in gacha UI is tapped.
    /// </summary>
    private void OnGachaShareBtnTap(object sender, System.EventArgs e)
    {
        // Get name of latest gacha character
        string charName = m_characterResource.GetCharacterStruct(m_latestGachaCharacter).Name;
        string shareMessage = "Just got " + charName + "! #roadycross";
        m_profileManager.MultiShareText(shareMessage);

        if (BuildInfo.IsDebugMode)
        {
            Debug.Log("Share: \"" + shareMessage + "\"");
        }
    }

    /// <summary>
    /// Called when repeat gacha button in gacha UI is tapped.
    /// </summary>
    private void OnRepeatGachaBtnTap(object sender, System.EventArgs e)
    {
        m_gameUI.GachaUI.Reset();
        OnGachaOpenBtnTap(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Called when the gacha prize is opened (in gacha UI).
    /// </summary>
    private void OnGachaOpened(object sender, System.EventArgs e)
    {
        // Show back button
        m_gameUI.ShowBackButton();
    }

    /// <summary>
    /// Called when the gift open button in gift UI is tapped.
    /// </summary>
    private void OnGiftOpenBtnTap(object sender, System.EventArgs e)
    {
        // Get values from GiftManager
        int giftAmount = 0;
        int minutesUntilNextGift = 0;
        if (m_giftManager.ClaimGift(ref giftAmount, ref minutesUntilNextGift))
        {
            m_gameUI.GiftUI.StartGiftOpenAnimation(giftAmount, minutesUntilNextGift);
            // Hide back button
            m_gameUI.HideBackButton();
        }
    }

    /// <summary>
    /// Called when play button in gift UI is tapped.
    /// </summary>
    private void OnGiftPlayBtnTap(object sender, System.EventArgs e)
    {
        ChangeState(MetaGameState.RESULTS);
    }

    /// <summary>
    /// Called when the free gift is opened (in gift UI).
    /// </summary>
    private void OnGiftOpened(object sender, GiftUI.GiftOpenedEventArgs e)
    {
        // Add to player's coins
        m_storeManager.AddToCoinBalance(e.GiftAmount);
        // Animate coins text
        m_gameUI.CoinsUI.StartEnlargeAndShrinkAnim();
        // Show back button
        m_gameUI.ShowBackButton();
    }

    /// <summary>
    /// Called when the coin balance changes.
    /// </summary>
    private void OnCoinBalanceChanged(int balance)
    {
        // Ignore balance change events during initialization
        if (!m_isInitialized)
        {
            return;
        }
        // Update coins display
        m_gameUI.CoinsUI.SetCoins(balance);
        // Update gacha strip in results UI
        int coinsNeeded = CRCAssets.GACHA_PRICE - balance;
        bool gachaAvailable = coinsNeeded <= 0;
        m_gameUI.ResultsUI.SetUpGachaStrip(gachaAvailable, coinsNeeded);
        // Update gacha UI
        m_gameUI.GachaUI.UpdateRepeatGachaUI(gachaAvailable);
    }

    /// <summary>
    /// Called when the gacha balance changes.
    /// </summary>
    private void OnGachaBalanceChanged(int balance)
    {
        // Ignore balance change events during initialization
        if (!m_isInitialized)
        {
            return;
        }
        // Check if gacha achievement is unlocked
        if (m_storeManager.GetGachaPlayCount() == 10)
        {
            NotifyAchievementUnlocked(AchievementType.PlayGacha00);
        }
    }

    /// <summary>
    /// Called when the screen is touched while the screenshot UI is shown.
    /// </summary>
    private void OnScreenshotScreenTouch(object sender, System.EventArgs e)
    {
        m_gameUI.ScreenshotUI.HideStillScreenshot();
    }

    /// <summary>
    /// Called when the back button in settings, credits,
    /// character select, gift, gacha, or top score UI is tapped.
    // </summary>
    private void OnBackBtnTap(object sender, System.EventArgs e)
    {
        Back();
    }

#endregion // Button Delegates

#region Screen Rotation

    /// <summary>
    /// Locks the screen to the specified rotation.
    /// </summary>
    /// <param name="lockOrientation">The lock orientation.</param>
    private void LockScreenRotation(ScreenOrientation lockOrientation)
    {
        if (lockOrientation == ScreenOrientation.AutoRotation)
        {
            return;
        }
        // Disable auto rotation
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        // Switch to the specified screen orientation
        Screen.orientation = lockOrientation;
    }

    /// <summary>
    /// Enables screen auto rotation.
    /// </summary>
    private void UnlockScreenRotation()
    {
        // Enable auto rotation
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

#endregion // Screen Rotation

#region IAP Delegates

    /// <summary>
    /// Called when character purchase succeeded.
    /// </summary>
    private void OnCharPurchaseSucceeded(string characterID)
    {
        // Play new character sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.NewCharacter);
        // Notify character select UI to update the status of the newly-bought character
        m_gameUI.CharSelectUI.OnBuyFocusedCharacter();
    }

    /// <summary>
    /// Called when character purchase is cancelled by the player.
    /// </summary>
    private void OnCharPurchaseCancelled(string characterID)
    {
        // Nothing happens
        Debug.Log("Purchase cancelled");
    }

    /// <summary>
    /// Called when restore purchases has completed.
    /// </summary>
    private void OnRestorePurchasesCompleted()
    {
        // TODO
    }

    /// <summary>
    /// Called when restore purchases has failed.
    /// </summary>
    private void OnRestorePurchasesFailed()
    {
        // TODO
    }

#endregion // IAP Delegates

#region Play Games Service
 
    /// <summary>
    /// Called when report score finishes.
    /// </summary>
    /// <param name="scorePosted">if set to <c>true</c> score was posted on the leaderboard.</param>
    private void OnReportScoreFinished(bool scorePosted)
    {
        if (scorePosted)    Debug.Log("Score posted on leaderboard");
        else                Debug.Log("Report score failed");
    }
    
    /// <summary>
    /// Called when achievement unlock finishes.
    /// </summary>
    /// <param name="unlocked">if set to <c>true</c> achievement was unlocked.</param>
    private void OnUnlockAchievementFinished(bool unlocked)
    {
        if (BuildInfo.IsDebugMode)
        {
            if (unlocked)   Debug.Log("Achievement unlocked");
            else            Debug.Log("Achievement unlock failed");
        }
    }
    
    /// <summary>
    /// Called when Google sign-in finishes.
    /// </summary>
    /// <param name="signedIn">if set to <c>true</c> sign-in was successful.</param>
    private void OnSignInFinished(bool signedIn)
    {
        if (BuildInfo.IsDebugMode)
        {
            if (signedIn)   Debug.Log("Signed in to Google");
            else            Debug.Log("Google sign-in failed");
        }
        // Update sign-in UI in settings
        m_gameUI.SettingsUI.SetSignedInState(signedIn);
        // If sign is successful...
        if (signedIn)
        {
            // Hide sign-in UI
            m_gameUI.SignInUI.Hide();

            // Attempt to unlock achievements
            //  (Google will handle attempts to unlocked already-unlocked achievements)
            if (m_dataSystem.GetAchievLockState_CollectChars00())
            {
                NotifyAchievementUnlocked(AchievementType.CollectCharacters00);
            }
            if (m_dataSystem.GetAchievLockState_CollectChars01())
            {
                NotifyAchievementUnlocked(AchievementType.CollectCharacters01);
            }
            if (m_dataSystem.GetAchievLockState_CollectChars02())
            {
                NotifyAchievementUnlocked(AchievementType.CollectCharacters02);
            }
            if (m_dataSystem.GetAchievLockState_PlayGacha00())
            {
                NotifyAchievementUnlocked(AchievementType.PlayGacha00);
            }
            if (m_dataSystem.GetAchievLockState_TrainTracks00())
            {
                NotifyAchievementUnlocked(AchievementType.StayOnTrainTracks00);
            }
            if (m_dataSystem.GetAchievLockState_StraightLine00())
            {
                NotifyAchievementUnlocked(AchievementType.MoveInStraightLine00);
            }
        }
    }

#endregion // Play Games Service

#region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected override void Awake()
	{
		base.Awake();

        // For testing: Easy way to reset game progress
        // When ClearPlayerPrefs is set to true upon entering play mode, there will be errors
        if (ClearPlayerPrefs)
        {
            // Start game with this flag cleared
            ClearPlayerPrefs = false;
        }
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start()
	{
		base.Start();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	protected override void Update()
	{
		base.Update();

        if (!m_isInitialized)
        {
            // Check if systems have initialized
            if (m_storeManager.IsInitialized &&
                m_profileManager.IsInitialized &&
                m_gameManager.IsInitialized)
            {
                m_isInitialized = true;
            }
            // If already initialized, only allow update to run in the next frame
            return;
        }

        // Check device back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }

        // Update the metagame state
        UpdateState();

        // For testing: Easy way to reset game progress
        CheckClearPlayerPrefs();
    }

    // For testing: Easy way to reset progress
    public bool ClearPlayerPrefs = false;
    private void CheckClearPlayerPrefs()
    {
        if (ClearPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs has been cleared");
            ClearPlayerPrefs = false;
        }
    }

    // TODO: Remove
    /*private void OnGUI()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }

        if (!m_isInitialized)
        {
            return;
        }

        float x = 20.0f;
        float y = 70.0f;
        float width = 100.0f;
        float height = 30.0f;
        float spacing = 5.0f;
        
        //y += height + spacing;
        if (GUI.Button(new Rect(x, y, width, height), "Get Top Score"))
        {
            DebugGameOverTopScore();
        }
        y += height + spacing;
        if (GUI.Button(new Rect(x, y, width, height), "Collect coin"))
        {
            NotifyCollectCoin();
        }
        y += height + spacing;
        if (GUI.Button(new Rect(x, y, width, height), "Collect 5 coins"))
        {
            NotifyCollectCoin(5);
        }
    }*/

    private void DebugGameOverTopScore()
    {
        if (m_gameManager.CharacterInstance == null)
        {
            return;
        }
        int presetScore = 120;
        int presetTopScore = 100;
        Debug.Log("Testing top score UI: Preset score = " + presetScore.ToString() +
                  ", preset top score = " + presetTopScore.ToString());
        m_gameUI.ScoreUI.SetScore(presetScore);
        m_dataSystem.SetTopScore(presetTopScore);
        Vector3 screenshotWorldPos = m_gameManager.CharacterInstance.transform.position;
        // Take screenshot
        string topScoreString = (presetScore > TopScore) ? "NEW TOP" : "TOP" + TopScore.ToString();
        Texture2D screenshot = m_gameUI.ScreenshotTaker.TakeScreenshot(screenshotWorldPos, ScreenshotDir.Front,
                                                                       presetScore.ToString(), topScoreString);
        NotifyGameOver(presetScore, screenshot);
    }

    // TODO: Temporary game event simulators that should be called by GameManager instead.
    private void DebugGameEventSimulators()
    {
        // Simulate game end
        if (Input.GetKeyDown(KeyCode.A))
        {
            DebugGameOverTopScore();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            NotifyCollectCoin();
        }
        // Simulate collecting 1 coin
        else if (Input.GetKeyDown(KeyCode.D))
        {
            NotifyCollectCoin(5);
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected override void OnDestroy()
	{
		base.OnDestroy();
	}

#endregion // MonoBehaviour
}
