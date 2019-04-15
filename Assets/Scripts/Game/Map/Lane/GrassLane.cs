/******************************************************************************
*  @file       GrassLane.cs
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

public class GrassLane : Lane
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
        CreateEdgeItems();
        CreateObstacles(rowNumber, prevPassableTileArray);
        if (rowNumber > 1)
        {
            SpawnCoin();
        }
    }

    /// <summary>
    /// Deactivate this lane
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
    }

    /// <summary>
    /// Determine wheteher the tile is open and the player can move to it.
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    public override bool IsTileOpen(int tileCoord)
    {
        bool baseResult = base.IsTileOpen(tileCoord);
        if (!baseResult)
        {
            return false;
        }
        int tileIndex = tileCoord + Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (m_obstacles[tileIndex] != null)
        {
            return false;
        }
        return true;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Probabilities")]
    [Tooltip("Probability of not spawning an edge item. Does not include the immediate edges.")]
    [SerializeField]private     float   m_edgeItemNoSpawnProbability        = 0.1f;
    [Tooltip("Number of levels for each item in the probability array")]
    [SerializeField]private     int     m_probabilityLevelSize              = 25;
    [SerializeField]private     float[] m_obstacleSpawnProbability          = new float[]
    {
       0.1f, 0.1f, 0.2f, 0.2f, 0.25f, 0.25f, 0.3f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f
    };

    #endregion // Serialized Variables

    #region Items and Obstacles

    private         GameObject[]        m_obstacles     = null;
    private         GameObject[]        m_edgeItems     = null;

    /// <summary>
    /// Initializes items and obstacles
    /// </summary>
    protected override void InitializeItems()
    {
        if (m_obstacles == null)
        {
            // Create the holder array for obstacles
            m_obstacles = new GameObject[m_activeTileCount];
        }

        if (m_edgeItems == null)
        {
            // Create the holder array for edge items
            m_edgeItems = new GameObject[m_edgeTileCount * 2];
        }
    }

    /// <summary>
    /// Returns all items to the pool
    /// </summary>
    protected override void ReturnItems()
    {
        // Empty the obstacle holder array
        for (int i = 0; i < m_obstacles.Length; ++i)
        {
            if (m_obstacles[i] != null)
            {
                m_obstacles[i].SetActive(false);
            }
            m_obstacles[i] = null;
        }

        // Empty the edge item holder array
        for (int i = 0; i < m_edgeItems.Length; ++i)
        {
            if (m_edgeItems[i] != null)
            {
                m_edgeItems[i].SetActive(false);
            }
            m_edgeItems[i] = null;
        }

        // Return the coin instance
        if (m_coinInstance != null)
        {
            m_coinInstance.Deactivate();
            m_coinInstance = null;
        }
    }

    /// <summary>
    /// Creates edge items
    /// </summary>
    private void CreateEdgeItems()
    {
        // Left side
        int tileCountToEdge = Mathf.FloorToInt(m_activeTileCount * 0.5f) + 1;
        for (int i = 0; i < m_edgeTileCount; ++i)
        {
            // [i == 0] --> immediate map edge should have an item
            if (i == 0 || Random.value > m_edgeItemNoSpawnProbability)
            {
                // Spawn an edge item
                Transform itemTransform = m_mapAssetPool.GetGrassEdgeItemAssetPool().GetAsset();
                itemTransform.position = transform.position + new Vector3((tileCountToEdge + i) * -m_tileSize, m_height, 0f);
                m_edgeItems[i] = itemTransform.gameObject;
                itemTransform.gameObject.SetActive(true);
            }
        }
        // Right side
        tileCountToEdge = Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (m_activeTileCount % 2 != 0)
        {
            tileCountToEdge++;
        }
        for (int i = 0; i < m_edgeTileCount; ++i)
        {
            // [i == 0] --> immediate map edge should have an item
            if (i == 0 || Random.value > m_edgeItemNoSpawnProbability)
            {
                // Spawn an edge item
                Transform itemTransform = m_mapAssetPool.GetGrassEdgeItemAssetPool().GetAsset();
                itemTransform.position = transform.position + new Vector3((tileCountToEdge + i) * m_tileSize, m_height, 0f);
                m_edgeItems[i + m_edgeTileCount] = itemTransform.gameObject;
                itemTransform.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Create obstacles
    /// </summary>
    private void CreateObstacles(int rowNumber, bool[] prevPassableTileArray)
    {
        // Check the list of passable tiles and choose a path where you can pass
        // Choose the center tile by default... and if your around the starting area
        int mainPassableTile = Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (prevPassableTileArray != null && rowNumber > 1)
        {
            List<int> passableTiles = new List<int>(prevPassableTileArray.Length);
            for (int i = 0; i < prevPassableTileArray.Length; ++i)
            {
                if (prevPassableTileArray[i])
                {
                    passableTiles.Add(i);
                }
            }
            if (passableTiles.Count > 0)
            {
                mainPassableTile = passableTiles[Random.Range(0, passableTiles.Count)];
            }
        }

        // Spawn obstacles based on probability
        // Special case: no obstacles on start lane
        float prob = 0f;
        if (rowNumber > 0f)
        {
            int probIndex = Mathf.Clamp(rowNumber / m_probabilityLevelSize, 0, m_obstacleSpawnProbability.Length - 1);
            prob = m_obstacleSpawnProbability[probIndex];
        }
        // Special case: full obstacles 4 lanes and behind
        else if (rowNumber < -3)
        {
            mainPassableTile = -1;
            prob = 1f;
        }
        float leftmostXPos = Mathf.FloorToInt(m_activeTileCount * 0.5f) * -m_tileSize;
        Vector3 leftmostPosition = new Vector3(leftmostXPos, m_height, 0f) + transform.position;
        for (int i = 0; i < m_activeTileCount; ++i)
        {
            // Skip the main passable tile
            if (i == mainPassableTile)
            {
                continue;
            }
            if (Random.value <= prob)
            {
                // Spawn an obstacle and position it correctly
                m_obstacles[i] = m_mapAssetPool.GetGrassObstacleAssetPool().GetAsset().gameObject;
                m_obstacles[i].transform.position = leftmostPosition + (Vector3.right * i * m_tileSize);
                m_obstacles[i].SetActive(true);
            }
        }

        // If there is no main passable tile, determine passable tiles from obstacles spawned
        if (mainPassableTile < 0 || mainPassableTile >= m_activeTileCount)
        {
            for (int i = 0; i >= m_activeTileCount; ++i)
            {
                if (m_obstacles[i] != null)
                {
                    m_isActiveTilePassable[i] = false;
                }
            }
        }
        else
        {
            // Evaluate passable tiles branching from the main tile
            m_isActiveTilePassable[mainPassableTile] = true;
            // Leftward
            for (int i = mainPassableTile - 1; i >= 0; --i)
            {
                m_isActiveTilePassable[i] = m_obstacles[i] == null &&
                                                (m_isActiveTilePassable[i + 1] || prevPassableTileArray[i]);
            }
            // Rightward
            for (int i = mainPassableTile + 1; i < m_activeTileCount; ++i)
            {
                m_isActiveTilePassable[i] = m_obstacles[i] == null &&
                                                (m_isActiveTilePassable[i - 1] || prevPassableTileArray[i]);
            }
        }
    }

    #endregion // Items

    #region Coins

    /// <summary>
    /// Spawns a coin
    /// </summary>
    protected override void SpawnCoin()
    {
        // Determine whether to spawn a coin
        if (Random.value <= m_coinSpawnProb)
        {
            // Get the possible tiles to spawn the coin on
            List<int> coinTiles = new List<int>();
            for (int i = 0; i < m_activeTileCount; ++i)
            {
                if (m_obstacles[i] == null)
                {
                    coinTiles.Add(i);
                }
            }
            // Spawna coin if there's at least 1 extra space
            if (coinTiles.Count > 1)
            {
                // Get a random spawnable tile
                int tileIndex = coinTiles[Random.Range(0, coinTiles.Count)];

                // Get a coin instance
                m_coinInstance = m_mapAssetPool.GetCoinAssetPool().GetAsset();

                // Position the coin
                float leftmostXPos = Mathf.FloorToInt(m_activeTileCount * 0.5f) * -m_tileSize;
                Vector3 leftmostPosition = new Vector3(leftmostXPos, m_height, 0f) + transform.position;
                m_coinInstance.transform.position = leftmostPosition + (Vector3.right * tileIndex * m_tileSize);

                // Activate the coin
                m_coinInstance.gameObject.SetActive(true);
                m_coinInstance.SetOnCoinGetDelegate(RemoveCoinInstance);
                m_coinInstance.Activate();
            }
        }
    }

    #endregion // Coins
}