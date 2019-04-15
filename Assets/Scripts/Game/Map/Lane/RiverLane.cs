/******************************************************************************
*  @file       RiverLane.cs
*  @brief      
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion // Namespaces

public class RiverLane : Lane
{
    #region Public Interface

    /// <summary>
    /// Activate this lane
    /// </summary>
    /// <param name="rowNumber">Indacates how far into the map the lane is</param>
    /// <param name="prevPassableTileArray">Passable tile array of the previous lane</param>
    /// <param name="prevDir">Direction of previous lane</param>
    public override void Activate(int rowNumber, bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        base.Activate(rowNumber, prevPassableTileArray, prevDir);
        PlayRiverSound();
        CreatePlatforms(prevPassableTileArray, prevDir);
    }

    /// <summary>
    /// Deactivate this lane
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
        StopRiverSound();
    }

    /// <summary>
    /// Determine wheteher the tile is open and the player can move to it.
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    public override bool IsTileOpen(int tileCoord)
    {
        return base.IsTileOpen(tileCoord);
    }

    /// <summary>
    /// Pause this instance
    /// </summary>
    public override void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;

        if (m_lilyPads != null)
        {
            for (int i = 0; i < m_lilyPads.Length; ++i)
            {
                if (m_lilyPads[i] != null)
                {
                    m_lilyPads[i].Pause();
                }
            }
        }
        if (m_logs != null)
        {
            foreach(Log log in m_logs)
            {
                log.Pause();
            }
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Pause();
        }
        if (m_riverFrothObjs != null)
        {
            foreach (RiverFroth riverFroth in m_riverFrothObjs)
            {
                riverFroth.Pause();
            }
        }
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public override void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;

        if (m_lilyPads != null)
        {
            for (int i = 0; i < m_lilyPads.Length; ++i)
            {
                if (m_lilyPads[i] != null)
                {
                    m_lilyPads[i].Unpause();
                }
            }
        }
        if (m_logs != null)
        {
            foreach (Log log in m_logs)
            {
                log.Unpause();
            }
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Unpause();
        }
        if (m_riverFrothObjs != null)
        {
            foreach (RiverFroth riverFroth in m_riverFrothObjs)
            {
                riverFroth.Unpause();
            }
        }
    }

    /// <summary>
    /// Gets a platform in the given position
    /// </summary>
    public override Platform GetPlatform(Vector3 pos)
    {
        float halfTile = m_tileSize * 0.5f;
        if (m_direction == LaneDirection.NONE)
        {
            for (int i = 0; i < m_lilyPads.Length; ++i)
            {
                if (m_lilyPads[i] != null)
                {
                    // Check if position is within the lily pad's tile
                    if ((pos.x >= m_lilyPads[i].transform.position.x - halfTile) &&
                        (pos.x <= m_lilyPads[i].transform.position.x + halfTile))
                    {
                        return m_lilyPads[i];
                    }
                }
            }
        }
        else if (m_direction == LaneDirection.LEFT)
        {
            foreach (Log log in m_logs)
            {
                // Check if position is within the log's length
                // or within half a tile in front of the log
                if ((pos.x >= log.transform.position.x &&
                    pos.x <= log.transform.position.x + log.PlatformLength) ||
                    Mathf.Abs(log.transform.position.x - pos.x) < halfTile)
                {
                    return log;
                }
            }
        }
        else
        {
            foreach (Log log in m_logs)
            {
                // Check if position is within the log's length
                // or within half a tile in front of the log
                if ((pos.x >= log.transform.position.x - log.PlatformLength &&
                    pos.x <= log.transform.position.x) ||
                    Mathf.Abs(log.transform.position.x - pos.x) < halfTile)
                {
                    return log;
                }
            }
        }

        return null;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Direction")]
    [Tooltip("Probability of going at the opposite direction from the previous lane")]
    [SerializeField]private     float   m_oppositeDirProb   = 0.80f;
    [Header("Lily Pads")]
    [Tooltip("Probability of using lilypads instead of logs")]
    [SerializeField]private     float   m_lilyPadLaneProb   = 0.30f;
    [Tooltip("Probability of using a lilypad in an accessible tile")]
    [SerializeField]private     float   m_lilyPadProbOpen   = 0.3f;
    [Tooltip("Probability of using a lilypad in an inaccessible tile")]
    [SerializeField]private     float   m_lilyPadProbClosed = 0.1f;
    [Tooltip("Maximum number of lily pads in a lane")]
    [SerializeField]private     int     m_maxLilyPadCount   = 4;
    [Header("Logs")]
    [Tooltip("Probability of using each log type")]
    [SerializeField]private     float[] m_logTypeProb       = {0.1f, 0.4f, 0.5f};
    [Tooltip("Min speed of logs")]
    [SerializeField]private     float   m_minSpeed          = 20.0f;
    [Tooltip("Max speed of logs")]
    [SerializeField]private     float   m_maxSpeed          = 45.0f;
    [Tooltip("Speed of logs beyond active area")]
    [SerializeField]private     float   m_fastSpeed         = 45.0f;
    [Tooltip("Min spawn time of logs")]
    [SerializeField]private     float   m_minSpawnTime      = 1.0f;
    [Tooltip("Max spawn time of logs")]
    [SerializeField]private     float   m_maxSpawnTime      = 1.0f;
    [Header("Froth")]
    [SerializeField]private     RiverFroth[]   m_riverFrothObjs      = null;

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (!m_isActivated)
        {
            return;
        }

        if (m_isPaused)
        {
            return;
        }

        UpdateLogSpawning();
    }

    #endregion // MonoBehaviour

    #region Items and Obstacles

    /// <summary>
    /// Initializes items and obstacles
    /// </summary>
    protected override void InitializeItems()
    {

    }

    /// <summary>
    /// Returns all items to the pool
    /// </summary>
    protected override void ReturnItems()
    {
        if (m_lilyPads != null)
        {
            for (int i = 0; i < m_lilyPads.Length; ++i)
            {
                if (m_lilyPads[i] != null)
                {
                    m_lilyPads[i].Deactivate();
                }
                m_lilyPads[i] = null;
            }
        }
        if (m_logs != null)
        {
            foreach(Log log in m_logs)
            {
                log.Deactivate();
            }
            m_logs.Clear();
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Deactivate();
            m_coinInstance = null;
        }
    }

    /// <summary>
    /// Creates platforms
    /// </summary>
    private void CreatePlatforms(bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        // Determine whether to spawn logs or lily pads
        if (Random.value <= m_lilyPadLaneProb)
        {
            SpawnLilyPads(prevPassableTileArray, prevDir);
        }
        else
        {
            StartLogSpawning(prevPassableTileArray, prevDir);
        }
    }

    #endregion // Items

    #region Platform Spawning

    private         float               m_spawnTimer        = 0f;
    private         float               m_spawnTime         = 0f;
    private         float               m_logSpeed          = 0f;
    private         Vector3             m_edgeLeftPos       = Vector3.zero;
    private         Vector3             m_edgeRightPos      = Vector3.zero;
    private         Vector3             m_activeLeftPos     = Vector3.zero;
    private         Vector3             m_activeRightPos    = Vector3.zero;
    private         List<Log>           m_logs              = null;
    private         Platform[]          m_lilyPads          = null;


    /// <summary>
    /// Spawns lily pads
    /// </summary>
    private void SpawnLilyPads(bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        m_direction = LaneDirection.NONE;

        // Check the list of passable tiles and choose a path where you can pass
        // Choose the center tile by default
        int mainPassableTile = Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (prevPassableTileArray != null)
        {
            List<int> passableTiles = new List<int>(prevPassableTileArray.Length);
            for (int i = 0; i < prevPassableTileArray.Length; ++i)
            {
                if (prevPassableTileArray[i])
                {
                    passableTiles.Add(i);
                }
            }
            for (int i = 6; i >= 0; --i)
            {
                if (passableTiles.Count > i)
                {
                    // Avoid the edge columns if possible
                    int j = i / 2;
                    mainPassableTile = passableTiles[Random.Range(j, passableTiles.Count - j)];
                    break;
                }
            }
        }

        // Create the lily pad array
        m_lilyPads = new Platform[m_maxLilyPadCount];

        // Spawn the lily pads
        int lilyPadCount = 0;
        float leftmostXPos = Mathf.FloorToInt(m_activeTileCount * 0.5f) * -m_tileSize;
        Vector3 leftmostPosition = new Vector3(leftmostXPos, m_height, 0f) + transform.position;
        // TODO: Make spawning logic better
        for (int i = 0; i < m_activeTileCount; ++i)
        {
            bool spawnFlag = false;

            // Determine whether to spawn
            if (i == mainPassableTile)
            {
                spawnFlag = true;
            }
            else
            {
                float prob = Random.value;
                if (prevPassableTileArray[i])
                {
                    if (prob <= m_lilyPadProbOpen && lilyPadCount < m_maxLilyPadCount - 1)
                    {
                        spawnFlag = true;
                    }
                }
                else
                {
                    if (prob <= m_lilyPadProbClosed && lilyPadCount < m_maxLilyPadCount - 1)
                    {
                        spawnFlag = true;
                    }
                }
            }

            if (spawnFlag)
            {
                // Spawn a lily pad and position it correctly
                m_lilyPads[lilyPadCount] = m_mapAssetPool.GetPlatformAssetPool(MapItemType.LilyPad).GetAsset();
                m_lilyPads[lilyPadCount].transform.position = leftmostPosition + (Vector3.right * i * m_tileSize);
                m_lilyPads[lilyPadCount].gameObject.SetActive(true);
                m_lilyPads[lilyPadCount].Activate();
                // Temporarily indicate where lily pads are
                m_isActiveTilePassable[i] = true;
                // Spawn a coin
                SpawnCoin(m_lilyPads[lilyPadCount]);
                lilyPadCount++;
            }
            else
            {
                // If no lily pad spawned, tile is not passable
                m_isActiveTilePassable[i] = false;
            }
        }

        // Evaluate passable tiles branching from the main tile
        m_isActiveTilePassable[mainPassableTile] = true;
        // Leftward
        for (int i = mainPassableTile - 1; i >= 0; --i)
        {
            m_isActiveTilePassable[i] = m_isActiveTilePassable[i] &&
                                        (m_isActiveTilePassable[i + 1] || prevPassableTileArray[i]);
        }
        // Rightward
        for (int i = mainPassableTile + 1; i < m_activeTileCount; ++i)
        {
            m_isActiveTilePassable[i] = m_isActiveTilePassable[i] &&
                                        (m_isActiveTilePassable[i - 1] || prevPassableTileArray[i]);
        }
    }

    /// <summary>
    /// Start spawning logs
    /// </summary>
    private void StartLogSpawning(bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        // Set a random spawn time
        m_spawnTime = Random.Range(m_minSpawnTime, m_maxSpawnTime);
        m_spawnTimer = Random.Range(0f, m_spawnTime);

        // Determine log direction
        switch (prevDir)
        {
            case LaneDirection.NONE:
                // Random direction
                m_direction = (Random.Range(0, 2) == 1) ? LaneDirection.RIGHT : LaneDirection.LEFT;
                break;

            case LaneDirection.LEFT:
            case LaneDirection.RIGHT:
                if (Random.value <= m_oppositeDirProb)
                {
                    m_direction = (prevDir == LaneDirection.LEFT) ? LaneDirection.RIGHT : LaneDirection.LEFT;
                }
                else
                {
                    m_direction = prevDir;
                }
                break;
        }

        // Update passable tiles
        if (m_direction == LaneDirection.RIGHT)
        {
            // Rightward
            m_isActiveTilePassable[0] = false;
            for (int i = 1; i < m_activeTileCount; ++i)
            {
                m_isActiveTilePassable[i] = m_isActiveTilePassable[i - 1] || prevPassableTileArray[i];
            }
        }
        else
        {
            // Leftward
            m_isActiveTilePassable[m_activeTileCount - 1] = false;
            for (int i = m_activeTileCount - 2; i <= 0; --i)
            {
                m_isActiveTilePassable[i] = m_isActiveTilePassable[i + 1] || prevPassableTileArray[i];
            }
        }

        // Determine log speed
        m_logSpeed = Random.Range(m_minSpeed, m_maxSpeed);

        // Ready the log list
        if (m_logs != null)
        {
            m_logs.Clear();
        }
        else
        {
            m_logs = new List<Log>();
        }

        // Determine log movement positions
        // Left
        int tileCountToEdge = Mathf.FloorToInt(m_activeTileCount * 0.5f) + 1;
        m_activeLeftPos = new Vector3(tileCountToEdge * -m_tileSize, m_height, transform.position.z);
        m_edgeLeftPos = new Vector3(m_activeLeftPos.x + (m_edgeTileCount * -m_tileSize),
                                    m_height, transform.position.z);
        // Right
        tileCountToEdge = Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (m_activeTileCount % 2 != 0)
        {
            tileCountToEdge++;
        }
        m_activeRightPos = new Vector3(tileCountToEdge * m_tileSize, m_height, transform.position.z);
        m_edgeRightPos = new Vector3(m_activeRightPos.x + (m_edgeTileCount * m_tileSize),
                                     m_height, transform.position.z);

        // Create logs to fill the initial space
        float randomXPos = Random.Range(m_activeLeftPos.x + m_tileSize * 3f, 0f);
        Vector3 startPos = new Vector3(randomXPos, m_height, transform.position.z);
        CreateLog(true, startPos);
        randomXPos = Random.Range(m_tileSize * 3f, m_activeRightPos.x);
        startPos = new Vector3(randomXPos, m_height, transform.position.z);
        CreateLog(true, startPos);
    }

    /// <summary>
    /// Update platform spawning
    /// </summary>
    private void UpdateLogSpawning()
    {
        if (m_direction == LaneDirection.NONE)
        {
            return;
        }

        m_spawnTimer += Time.deltaTime;
        if (m_spawnTimer >= m_spawnTime)
        {
            CreateLog(false, Vector3.zero);
            m_spawnTimer = 0f;
        }
    }

    /// <summary>
    /// Creates a log
    /// </summary>
    private void CreateLog(bool isCustomStart, Vector3 customStartPos)
    {
        // Determine log type
        MapItemType logType = MapItemType.Log_1;
        float rand = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < 3; ++i)
        {
            cumulative += m_logTypeProb[i];
            if (rand <= cumulative)
            {
                logType += i;
                break;
            }
        }
        
        // Get the log asset
        Platform newPlatform = m_mapAssetPool.GetPlatformAssetPool(logType).GetAsset();
        Log newLog = (Log)newPlatform;
        m_logs.Add(newLog);
        newLog.gameObject.SetActive(true);
        newLog.Activate();

        // Start the log's movement
        if (m_direction == LaneDirection.RIGHT)
        {
            newLog.StartMovement(m_fastSpeed, m_logSpeed, m_edgeLeftPos, m_activeLeftPos,
                                 m_activeRightPos, m_edgeRightPos, OnLogEndMovement,
                                 isCustomStart, customStartPos);
        }
        else if (m_direction == LaneDirection.LEFT)
        {
            newLog.StartMovement(m_fastSpeed, m_logSpeed, m_edgeRightPos, m_activeRightPos,
                                 m_activeLeftPos, m_edgeLeftPos, OnLogEndMovement,
                                 isCustomStart, customStartPos);
        }

        // Spawn a coin
        SpawnCoin(newLog);
    }

    /// <summary>
    /// Called when a log's movement has ended
    /// </summary>
    /// <param name="thisLog"></param>
    private void OnLogEndMovement(Log thisLog)
    {
        m_logs.Remove(thisLog);
        thisLog.Deactivate();
    }

    #endregion // Platform Spawning

    #region Sounds

    private         SoundObject     m_riverSound        = null;

    /// <summary>
    /// Plays the river sound
    /// </summary>
    private void PlayRiverSound()
    {
        if (m_riverSound == null)
        {
            m_riverSound = Locator.GetSoundManager().PlaySound(SoundInfo.SFXID.RiverRush);
            m_riverSound.transform.parent = transform;
            m_riverSound.transform.localPosition = Vector3.up * m_height;
        }
    }

    /// <summary>
    /// Stops the river sound
    /// </summary>
    private void StopRiverSound()
    {
        if (m_riverSound != null)
        {
            m_riverSound.Delete();
            m_riverSound = null;
        }
    }

    #endregion // Sounds

    #region Coins

    /// <summary>
    /// Spawns a coin
    /// </summary>
    private void SpawnCoin(Platform thisPlatform)
    {
        // Determine whether to spawn a coin
        if (Random.value <= m_coinSpawnProb)
        {
            thisPlatform.SetCoinInstance(m_mapAssetPool.GetCoinAssetPool().GetAsset());
        }
    }

    #endregion // Coins
}