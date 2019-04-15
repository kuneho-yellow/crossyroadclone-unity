/******************************************************************************
*  @file       CoinWinAnimator.cs
*  @brief      Handles the "win coins" animation
*  @author     Ron
*  @date       October 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;

#endregion // Namespaces

public class CoinWinAnimator : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Load coin resource
        m_coinPrefab = Resources.Load<GameObject>(COIN_PREFAB_PATH);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts the coin win animation.
    /// </summary>
    /// <param name="coinAmount">The coin amount.</param>
    /// <param name="waitTime">Time until coins animation will begin.</param>
    public void StartWinCoinsAnim(int coinAmount, float waitTime = 0.0f)
    {
        if (!m_isInitialized)
        {
            if (BuildInfo.IsDebugMode)
            {
                Debug.Log("CoinWinAnimUI is not initialized");
            }
            return;
        }

        // Animate only a fraction of the amount of coins won
        m_coinAmount = (int)(coinAmount * m_fractionOfCoinsToAnimate);
        m_animWaitTime = waitTime;

        // Initialize time since last collect sound such that a sound would
        //  play at the start of the collect animation
        m_timeSinceLastCollectSound = m_collectSoundIntervals;

        // Start anim
        m_state = CoinAnimState.WAIT_SPAWN;
    }

    /// <summary>
    /// Notifies that a coin has been collected ("collect" animation finished).
    /// </summary>
    /// <param name="coin">The coin.</param>
    public void NotifyCoinCollected(CoinUIObject coin)
    {
        // Remove the coin from the list and delete it
        m_coins.Remove(coin);
        coin.Delete();

        // If all coins have been collected, end the animation
        if (m_coins.Count == 0)
        {
            m_state = CoinAnimState.IDLE;
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

        // Pause coin objects
        foreach (CoinUIObject coin in m_coins)
        {
            coin.Pause();
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

        // Unpause coin objects
        foreach (CoinUIObject coin in m_coins)
        {
            coin.Unpause();
        }

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        // Clear all coins
        foreach (CoinUIObject coin in m_coins)
        {
            coin.Delete();
        }
        m_coins.Clear();
        // Reset values
        m_state = CoinAnimState.IDLE;
        m_timeSinceStart = 0.0f;
        m_timeSinceLastCollect = 0.0f;
        m_timeSinceLastCollectSound = 0.0f;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        foreach (CoinUIObject coin in m_coins)
        {
            coin.Delete();
        }
        m_coins.Clear();
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
    /// Gets whether the "win coins" animation is animating.
    /// </summary>
    public bool IsAnimating
    {
        get { return m_state != CoinAnimState.IDLE; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private float  m_minSpeed              = 30.0f;
    [SerializeField] private float  m_maxSpeed_landscape    = 80.0f;
    [SerializeField] private float  m_maxSpeed_portrait     = 130.0f;
    [SerializeField] private float  m_minRotSpeed           = 20.0f;
    [SerializeField] private float  m_maxRotSpeed           = 50.0f;
    [SerializeField] private float  m_deceleration          = 150.0f;

    [Tooltip("Time from coin spawn to coin collection")]
    [SerializeField] private float  m_timeFromSpawnToCollect    = 0.7f;
    [Tooltip("Time between collection of each coin")]
    [SerializeField] private float  m_collectIntervals          = 0.01f;
    [Tooltip("Time between playing coin collect sounds")]
    [SerializeField] private float  m_collectSoundIntervals     = 0.1f;
    [Tooltip("Max offset of a coin object's target position to its spawn position")]
    [SerializeField] private float  m_maxTargetPosOffset        = 5.0f;
    [Tooltip("Fraction of coins won that would be animated")]
    [SerializeField] private float  m_fractionOfCoinsToAnimate  = 0.25f;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isInitialized    = false;
    private bool    m_isPaused         = false;

    #endregion // Variables

    #region Coins

    private List<CoinUIObject>  m_coins         = new List<CoinUIObject>();

    private GameObject      m_coinPrefab        = null;
    private const string    COIN_PREFAB_PATH    = "Prefabs/UI/CoinUIObject";
    
    private float   m_spawnOffsetFromTopEdge    = 1.0f;

    // Amount of coins to spawn
    private int     m_coinAmount        = 0;
    // Time until the animation begins
    private float   m_animWaitTime      = 0.0f;
    // Time since anim started
    private float   m_timeSinceStart    = 0.0f;
    // Time since coins were spawned
    private float   m_timeSinceSpawn    = 0.0f;
    // Time since the last coin was collected
    private float   m_timeSinceLastCollect      = 0.0f;
    // Time since the last coin collect sound was played
    private float   m_timeSinceLastCollectSound = 0.0f;
    // Index of the last coin collected
    private int     m_lastCollectedCoinIndex    = 0;

    private const float COIN_POS_Z = 0.0f;

    private enum CoinAnimState
    {
        IDLE,
        WAIT_SPAWN,
        SPAWN,
        WAIT_COLLECT,
        COLLECT
    }
    [SerializeField] private CoinAnimState m_state = CoinAnimState.IDLE;

    /// <summary>
    /// Updates the wait time until animation start.
    /// </summary>
    private void UpdateAnimState()
    {
        switch (m_state)
        {
            case CoinAnimState.IDLE:
                break;
            case CoinAnimState.WAIT_SPAWN:
                // Spawn coins after the specified delay
                m_timeSinceStart += Time.deltaTime;
                if (m_timeSinceStart > m_animWaitTime)
                {
                    m_timeSinceStart = 0.0f;
                    // Proceed to the next state
                    m_state = CoinAnimState.SPAWN;
                }
                break;
            case CoinAnimState.SPAWN:
                SpawnCoins();
                // Proceed to the next state
                m_state = CoinAnimState.WAIT_COLLECT;
                break;
            case CoinAnimState.WAIT_COLLECT:
                m_timeSinceSpawn += Time.deltaTime;
                if (m_timeSinceSpawn > m_timeFromSpawnToCollect)
                {
                    m_timeSinceSpawn = 0.0f;

                    // Collect starting from the last coin in the list
                    m_lastCollectedCoinIndex = m_coins.Count;

                    // Proceed to the next state
                    m_state = CoinAnimState.COLLECT;
                }
                break;
            case CoinAnimState.COLLECT:
                // Check if there are still coins to collect
                if (m_lastCollectedCoinIndex == 0)
                {
                    // Anim will be ended once the last coin has been collected (NotifyCoinCollected)
                    break;
                }
                // Collect one coin every fixed interval
                m_timeSinceLastCollect += Time.deltaTime;
                if (m_timeSinceLastCollect > m_collectIntervals)
                {
                    m_timeSinceLastCollect = 0.0f;
                    // Collect the next coin (in order from the last coin in the list to the first)
                    m_lastCollectedCoinIndex--;
                    m_coins[m_lastCollectedCoinIndex].Collect();
                }
                // Play coin collect sound every fixed interval
                m_timeSinceLastCollectSound += Time.deltaTime;
                if (m_timeSinceLastCollectSound > m_collectSoundIntervals)
                {
                    m_timeSinceLastCollectSound = 0.0f;
                    // Play coin collect (pickup) sound
                    SoundManager soundManager = (SoundManager)Locator.GetSoundManager();
                    soundManager.PlayCoinRewardSound();
                }
                break;
        }
    }

    /// <summary>
    /// Starts the actual "win coins" animation.
    /// </summary>
    private void SpawnCoins()
    {
        UICamera uiCam = Locator.GetUIManager().UICamera;
        // Spawn coins from the top edge of the screen
        float leftEdge = uiCam.TopLeftWorld.x;
        float rightEdge = uiCam.TopRightWorld.x;
        float bottomEdge = uiCam.ScreenMinWorld.y;
        // Add an offset to the top edge to spawn coins outside screen view
        float spawnPosY = uiCam.ScreenMaxWorld.y + m_spawnOffsetFromTopEdge;
        // Max speed of coins depends on screen orientation
        float maxSpeed = uiCam.IsLandscape ? m_maxSpeed_landscape : m_maxSpeed_portrait;
#if UNITY_EDITOR
        // Screen orientation does not apply to the Editor, so just use max speed for landscape
        maxSpeed = m_maxSpeed_landscape;
#endif
        // Spawn a number of coins equal to the specified coin amount
        //  (assuming 1 coin object = 1 coin value)
        for (int coinNo = 0; coinNo < m_coinAmount; ++coinNo)
        {
            // Randomize start position from top edge of screen
            float spawnPosX = Random.Range(leftEdge, rightEdge);
            Vector3 spawnPos = new Vector3(spawnPosX, spawnPosY, COIN_POS_Z);

            // Limit the angle of movement from spawn to target position
            float targetPosMinX = Mathf.Max(leftEdge, spawnPosX - m_maxTargetPosOffset);
            float targetPosMaxX = Mathf.Min(rightEdge, spawnPosX + m_maxTargetPosOffset);
            // Randomize move direction, generally downward toward the bottom screen edge
            float targetPosX = Random.Range(targetPosMinX, targetPosMaxX);
            Vector3 targetPos = new Vector3(targetPosX, bottomEdge, COIN_POS_Z);
            Vector3 moveDir = Vector3.Normalize(targetPos - spawnPos);

            // Randomize initial move and rotation speeds
            float speed = Random.Range(m_minSpeed, maxSpeed);
            float rotSpeed = Random.Range(m_minRotSpeed, m_maxRotSpeed);

            // Spawn and initialize coin object
            CoinUIObject coin = SpawnCoin();
            coin.transform.position = spawnPos;
            coin.transform.parent = this.transform;
            coin.Initialize(this, speed, rotSpeed, moveDir, m_deceleration);
        }

        // Play win coins sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.WinCoins);
    }

    /// <summary>
    /// Spawns a coin object.
    /// </summary>
    /// <returns>The coin handler instance</returns>
    private CoinUIObject SpawnCoin()
    {
        GameObject coinObj = GameObject.Instantiate<GameObject>(m_coinPrefab);
        CoinUIObject coin = coinObj.AddComponentNoDupe<CoinUIObject>();
        m_coins.Add(coin);
        return coin;
    }

    #endregion // Coins

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

        UpdateAnimState();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
