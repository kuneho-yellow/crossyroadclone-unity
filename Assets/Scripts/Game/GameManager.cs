/******************************************************************************
*  @file       GameManager.cs
*  @brief      
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> Handles the game
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class GameManager : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
        m_isInitialized = false;

        // Initialize the map manager
		m_mapManager.Initialize();

		// Initialize the player
		m_player.Initialize();

        m_isInitialized = true;
    }

    /// <summary>
    /// Pause this instance.
    /// </summary>
    public void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;
        m_player.Pause();
        m_character.Pause();
        m_mapManager.Pause();
    }

    /// <summary>
    /// Unpause this instance.
    /// </summary>
    public void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;
        m_player.Unpause();
        m_character.Unpause();
        m_mapManager.Unpause();
    }

    /// <summary>
    /// Load a new game
    /// </summary>
    public void LoadNewGame()
    {
        m_hasGameLoaded = false;
        m_hasGameStarted = false;
        CreateCharacter();
        CreateMap();
        InitializeGame();
    }

    #region Properties

    /// <summary>
    /// Returns whether initialization has finished
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    /// <summary>
    /// Returns whether the game has finished loading
    /// </summary>
    public bool HasGameLoaded
    {
        get { return m_hasGameLoaded; }
    }

    /// <summary>
    /// Returns whether the game started
    /// </summary>
    public bool HasGameStarted
    {
        get { return m_hasGameStarted; }
    }

    /// <summary>
    /// Sets the current score.
    /// </summary>
    /// <value>The current score.</value>
    public int CurrentScore
    {
        get { return m_currentScore; }
    }

    /// <summary>
    /// Gets the character instance
    /// </summary>
    public Character CharacterInstance
    {
        get { return m_character; }
    }

    #endregion // Properties

    #region Game Events

    /// <summary>
    /// Called when the first game input has beed done
    /// </summary>
    public void NotifyFirstGameInput()
    {
        StartGame();
    }

    /// <summary>
    /// Called when the character jumps
    /// </summary>
    public void NotifyCharacterJump()
    {
        m_gameUI.SwipeUI.Hide();
    }

    /// <summary>
    /// Called when the character tries to jump but is blocked by an obstacle
    /// </summary>
    public void NotifyCharacterBlockedJump()
    {
        m_gameUI.SwipeUI.Show(false);
    }

    /// <summary>
    /// Called when the character jumps to the furthest row it has reached
    /// </summary>
    public void NotifyCharacterNewScore(int newRow)
    {
        if (newRow > m_currentScore)
        {
            m_currentScore = newRow;
            m_gameUI.ScoreUI.SetScore(m_currentScore);
        }
    }

    /// <summary>
    /// Called when the character does the tunnel vision achievement
    /// </summary>
    public void NotifyCharacterTunnelVision()
    {
        if (!m_hasTunnelVision)
        {
            m_gameSceneMaster.NotifyAchievementUnlocked(AchievementType.MoveInStraightLine00);
            m_hasTunnelVision = true;
        }
    }

    /// <summary>
    /// Called when the character does the railroad stay achievement
    /// </summary>
    public void NotifyCharacterRailroadStay()
    {
        if (!m_hasRailroadStay)
        {
            m_gameSceneMaster.NotifyAchievementUnlocked(AchievementType.StayOnTrainTracks00);
            m_hasRailroadStay = true;
        }
    }

    /// <summary>
    /// Called to create the character's death screenshot
    /// </summary>
    /// <param name="deathDir"></param>
    public void CreateCharacterScreenshot(ScreenshotDir deathDir)
    {
        bool isNewTopScore = m_currentScore > m_gameSceneMaster.TopScore;
        string topScoreString = isNewTopScore ? "NEW TOP" : "TOP" + m_gameSceneMaster.TopScore.ToString();
        m_charScreenshot = m_gameUI.ScreenshotTaker.TakeScreenshot(m_character.transform.position, deathDir,
                                                                       m_currentScore.ToString(), topScoreString);
    }

    /// <summary>
    /// Called when the character's death sequence starts
    /// </summary>
    public void NotifyCharacterDeathStart(Character.DeathType deathType)
    {
        m_mapManager.NotifyGameOver(deathType);
    }

    /// <summary>
    /// Called when the character's death sequence ends
    /// </summary>
    public void NotifyCharacterDeathEnd()
    {
        m_gameSceneMaster.NotifyGameOver(m_currentScore, m_charScreenshot);
    }

    /// <summary>
    /// Called when a coin has been acquired
    /// </summary>
    public void NotifyCoinGet()
    {
        m_currentCoins++;
        m_gameSceneMaster.NotifyCollectCoin();
    }

    #endregion // Game Events

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private GameSceneMaster m_gameSceneMaster   = null;
    [SerializeField]private GameUI          m_gameUI            = null;
	[SerializeField]private	MapManager		m_mapManager		= null;
	[SerializeField]private	Player			m_player			= null;
	[SerializeField]private	int				m_currentScore		= 0;
    [SerializeField]private int             m_currentCoins      = 0;

	#endregion // Serialized Variables

	#region MonoBehaviour

	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
		if (Locator.GetGameManager() == null)
		{
			// Pass reference to Locator
			Locator.ProvideGameManager(this);
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			// Self-destruct if an instance is already present
			Destroy(this.gameObject);
		}
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

	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour

	#region State
	
    private         bool            m_isInitialized     = false;
    private         bool            m_isPaused          = false;

    #endregion // State

    #region Score

    #endregion // Score


    #region Gameplay

    private         Character       m_character         = null;
    private         bool            m_hasGameLoaded     = false;
    private         bool            m_hasGameStarted    = false;
    private         Texture2D       m_charScreenshot    = null;
    private         bool            m_hasTunnelVision   = false;
    private         bool            m_hasRailroadStay   = false;

    /// <summary>
    /// Creates the character
    /// </summary>
    private void CreateCharacter()
    {
        if (m_character == null)
        {
            // Create a new character if there's none
            m_character = m_gameSceneMaster.CharacterResource.CreateCharacter(m_gameSceneMaster.EquippedCharacter);
            m_character.Initialize();
        }
        else if (m_character.Type != m_gameSceneMaster.EquippedCharacter)
        {
            // Destroy the old character if it's the wrong one
            Destroy(m_character.gameObject);
            // Create the correct character
            m_character = m_gameSceneMaster.CharacterResource.CreateCharacter(m_gameSceneMaster.EquippedCharacter);
            m_character.Initialize();
        }
    }

    /// <summary>
    /// Creates the map
    /// </summary>
    private void CreateMap()
    {
        m_mapManager.CreateNewMap(m_character.MapType);
    }

    /// <summary>
    /// Initializes the game
    /// </summary>
    private void InitializeGame()
    {
        m_currentScore = 0;
        m_currentCoins = 0;
        m_gameUI.ScoreUI.SetScore(m_currentScore);
        m_gameUI.CoinsUI.SetCoins(m_currentCoins);
        m_character.Reset();
        m_player.SetPressDelegate(NotifyFirstGameInput);
        m_mapManager.InitializeCharacterReference(m_character);
        m_hasGameLoaded = true;
        m_hasGameStarted = false;
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    private void StartGame()
    {
        m_mapManager.ActivateMap();
        m_character.AttachController(m_player);
        m_hasGameStarted = true;
        m_player.CallOnPressDelegate();
    }

    #endregion // Gameplay
}