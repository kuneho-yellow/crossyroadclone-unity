/******************************************************************************
*  @file       GameUI.cs
*  @brief      Handles all game-specific UI
*  @author     Ron
*  @date       October 10, 2015
*      
*  @par [explanation]
*		> Exposes handles to all UI class instances in the scene
*       > Holds methods for handling title and background UI
*       > All other functions are done through the UI class handles
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class GameUI : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> screenshotScreenDelegate,
                           System.EventHandler<System.EventArgs> backDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        if (m_isInitialized)
        {
            return;
        }

        // If previous scene is Title scene, Title UI would already be initialized
        if (Locator.GetMain().GetPrevSceneEnum == SceneInfo.SceneEnum.TITLE)
        {
            // Destroy the Title UI in the current scene
            m_titleUI.Delete();
            m_titleUI = null;
            // Reference the Title UI from the previous scene
            GameObject titleUIObj = GameObject.Find(TITLE_UI_NAME);
            m_titleUI = titleUIObj.GetComponent<TitleUI>();
        }
        else
        {
            // If game started in Game scene, initialize Title UI
            m_titleUI.Initialize(true);
        }

        // Initialize back button
        m_backButton.Initialize(backDelegate, UIButton.TriggerType.ON_RELEASE);
        m_backButton.UpdateScreenPosition();
        // Set button sounds
        m_backButton.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize shared UI before the other UI classes
        m_screenshotUI.Initialize(screenshotScreenDelegate);
        m_coinAnim.Initialize();      // "Win coins" animation
        m_newCharAnim.Initialize();   // "Win new char" animation
        
        // Set the initialized flag
        m_isInitialized = true;
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
        
        // Pause UI instances
        m_startUI.Pause();
        m_signInUI.Pause();
        m_settingsUI.Pause();
        m_creditsUI.Pause();
        m_charSelectUI.Pause();
        m_resultsUI.Pause();
        m_topScoreUI.Pause();
        m_gachaUI.Pause();
        m_giftUI.Pause();
        m_scoreUI.Pause();
        m_coinsUI.Pause();
        m_screenshotUI.Pause();

        // Pause coin animation
        m_coinAnim.Pause();

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

        // Unpause UI instances
        m_startUI.Unpause();
        m_signInUI.Unpause();
        m_settingsUI.Unpause();
        m_creditsUI.Unpause();
        m_charSelectUI.Unpause();
        m_resultsUI.Unpause();
        m_topScoreUI.Unpause();
        m_gachaUI.Unpause();
        m_giftUI.Unpause();
        m_scoreUI.Unpause();
        m_coinsUI.Unpause();
        m_screenshotUI.Unpause();

        // Unpause coin animation
        m_coinAnim.Unpause();

        m_isPaused = false;
    }

    /// <summary>
    /// Shows the back button.
    /// </summary>
    public void ShowBackButton()
    {
        m_backButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the back button.
    /// </summary>
    public void HideBackButton()
    {
        m_backButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when score reaches (or exceed) multiples of 50.
    /// </summary>
    public void OnReach50Score()
    {
        // Start animation
        m_scoreUI.StartEnlargeAndShrinkAnim();
        // Play "reach 50 score" sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.Reach50Score);
    }

    /// <summary>
    /// Called when coin balance reaches (or exceeds) the amount needed for gacha.
    /// Note: Name assumes gacha price is fixed at 100 coins.
    /// </summary>
    public void OnReach100Coins()
    {
        // Start animation
        m_coinsUI.StartEnlargeAndShrinkAnim();
        // Play "reach 100 coins" sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.Reach100Coins);
    }

    // UI instance getters
    public TitleUI              TitleUI             { get { return m_titleUI; } }
    public ScoreUI              ScoreUI             { get { return m_scoreUI; } }
    public CoinsUI              CoinsUI             { get { return m_coinsUI; } }
    public StartUI              StartUI             { get { return m_startUI; } }
    public SignInUI             SignInUI            { get { return m_signInUI; } }
    public PauseUI              PauseUI             { get { return m_pauseUI; } }
    public SettingsUI           SettingsUI          { get { return m_settingsUI; } }
    public CreditsUI            CreditsUI           { get { return m_creditsUI; } }
    public CharacterSelectUI    CharSelectUI        { get { return m_charSelectUI; } }
    public ResultsUI            ResultsUI           { get { return m_resultsUI; } }
    public TopScoreUI           TopScoreUI          { get { return m_topScoreUI; } }
    public GiftUI               GiftUI              { get { return m_giftUI; } }
    public GachaUI              GachaUI             { get { return m_gachaUI; } }
    public TapUI                TapUI               { get { return m_tapUI; } }
    public SwipeUI              SwipeUI             { get { return m_swipeUI; } }
    public ScreenshotUI         ScreenshotUI        { get { return m_screenshotUI; } }
    public ScreenshotTaker      ScreenshotTaker     { get { return m_screenshotTaker; } }
    public CoinWinAnimator      CoinWinAnimator     { get { return m_coinAnim; } }
    public NewCharWinAnimator   NewCharWinAnimator  { get { return m_newCharAnim; } }

    #endregion // Public Interface

    #region Serialized Variables

    // Other UI classes and objects
    [SerializeField] private TitleUI            m_titleUI           = null;
    [SerializeField] private StartUI            m_startUI           = null;
    [SerializeField] private SignInUI           m_signInUI          = null;
    [SerializeField] private PauseUI            m_pauseUI           = null;
    [SerializeField] private SettingsUI         m_settingsUI        = null;
    [SerializeField] private CreditsUI          m_creditsUI         = null;
    [SerializeField] private CharacterSelectUI  m_charSelectUI      = null;
    [SerializeField] private ResultsUI          m_resultsUI         = null;
    [SerializeField] private TopScoreUI         m_topScoreUI        = null;
    [SerializeField] private GachaUI            m_gachaUI           = null;
    [SerializeField] private GiftUI             m_giftUI            = null;
    [SerializeField] private ScoreUI            m_scoreUI           = null;
    [SerializeField] private CoinsUI            m_coinsUI           = null;
    [SerializeField] private TapUI              m_tapUI             = null;
    [SerializeField] private SwipeUI            m_swipeUI           = null;
    [SerializeField] private ScreenshotUI       m_screenshotUI      = null;
    [SerializeField] private ScreenshotTaker    m_screenshotTaker   = null;
    [SerializeField] private CoinWinAnimator    m_coinAnim          = null;
    [SerializeField] private NewCharWinAnimator m_newCharAnim       = null;
    [SerializeField] private UIButton           m_backButton        = null;
    
    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isPaused          = false;
    private bool    m_isInitialized     = false;

    #endregion // Variables

    #region Constants

    private const string TITLE_UI_NAME = "TitleUI";

    #endregion // Constants

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
