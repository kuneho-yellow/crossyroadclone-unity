/******************************************************************************
*  @file       Lane.cs
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

#endregion // Namespaces

public class Lane : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Gets the lane type.
	/// </summary>
	/// <value>The type.</value>
	public LaneType Type
	{
		get { return m_type; }
	}

    /// <summary>
    /// Gets the lane height
    /// </summary>
    public float Height
    {
        get { return m_height; }
    }

    /// <summary>
    /// Gets the passable active tile array
    /// </summary>
    public bool[] PassableActiveTileArray
    {
        get { return m_isActiveTilePassable; }
    }

    /// <summary>
    /// Initializes the lane
    /// </summary>
    public void Initialize(MapManager mapManager, float tileSize, int activeTileCount, int edgeTileCount, MapAssetPool mapAssetPool)
    {
        m_mapManager = mapManager;
        m_tileSize = tileSize;
        m_activeTileCount = activeTileCount;
        m_edgeTileCount = edgeTileCount;
        m_mapAssetPool = mapAssetPool;
        InitializeItems();
    }

    /// <summary>
    /// Reset this lane
    /// </summary>
    public void Reset()
    {
        ResetPassableActiveTiles();
        ReturnItems();
    }

    /// <summary>
    /// Activate this lane
    /// </summary>
    /// <param name="rowNumber">Indacates how far into the map the lane is</param>
    /// <param name="prevPassableTileArray">Passable tile array of the previous lane</param>
    /// <param name="prevDir">Direction of previous lane</param>
    public virtual void Activate(int rowNumber, bool[] prevPassableTileArray, LaneDirection prevDir)
    {
        Reset();
        m_isActivated = true;
    }

    /// <summary>
    /// Deactivate this lane
    /// </summary>
    public virtual void Deactivate()
    {
        m_isActivated = false;
        ReturnItems();
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public virtual void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;

        if (m_coinInstance != null)
        {
            m_coinInstance.Pause();
        }
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public virtual void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;

        if (m_coinInstance != null)
        {
            m_coinInstance.Unpause();
        }
    }

    /// <summary>
    /// Determine wheteher the tile is open and the player can move to it.
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    public virtual bool IsTileOpen(int tileCoord)
    {
        // Check if outside the active area
        int leftmostActiveTile = -Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (tileCoord < leftmostActiveTile)
        {
            return false;
        }
        int rightmostActiveTile = Mathf.FloorToInt(m_activeTileCount * 0.5f);
        if (m_activeTileCount % 2 == 0)
        {
            rightmostActiveTile -= 1;
        }
        if (tileCoord > rightmostActiveTile)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Gets the lane direction
    /// </summary>
    public virtual LaneDirection Direction
    {
        get { return m_direction; }
    }

    /// <summary>
    /// Gets a platform in the given position
    /// </summary>
    public virtual Platform GetPlatform(Vector3 pos)
    {
        return null;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]protected   LaneType        m_type          = LaneType.SIZE;
    [SerializeField]protected   float           m_height        = 1f;
    [SerializeField]protected   LaneDirection   m_direction     = LaneDirection.NONE;
    [SerializeField]protected   float           m_coinSpawnProb = 0.1f;

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected virtual void Awake()
    {

    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    protected virtual void Start()
    {

    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected virtual void Update()
    {
        if (!m_isActivated)
        {
            return;
        }

        if (m_isPaused)
        {
            return;
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected virtual void OnDestroy()
    {

    }

    #endregion // MonoBehaviour

    #region State

    protected       bool        m_isPaused          = false;
    protected       bool        m_isActivated       = false;

    #endregion // State

    #region Tiles

    protected       float       m_tileSize              = 0f;
    protected       int         m_activeTileCount       = 0;
    protected       int         m_edgeTileCount         = 0;
    protected       bool[]      m_isActiveTilePassable  = null;

    /// <summary>
    /// Resets passable active tiles
    /// </summary>
    protected void ResetPassableActiveTiles()
    {
        if (m_isActiveTilePassable == null)
        {
            m_isActiveTilePassable = new bool[m_activeTileCount];
        }
        for (int i = 0; i < m_isActiveTilePassable.Length; ++i)
        {
            m_isActiveTilePassable[i] = true;
        }
    }

    #endregion // Tiles

    #region Items

    protected       MapAssetPool    m_mapAssetPool      = null;
    
    /// <summary>
    /// Initializes items
    /// </summary>
    protected virtual void InitializeItems()
    {

    }

    /// <summary>
    /// Returns all items to the pool
    /// </summary>
    protected virtual void ReturnItems()
    {

    }

    #endregion // Items

    #region Coins

    protected       Coin            m_coinInstance      = null;

    /// <summary>
    /// Spawns a coin
    /// </summary>
    protected virtual void SpawnCoin()
    {

    }

    /// <summary>
    /// Removes the coin instance
    /// </summary>
    protected void RemoveCoinInstance()
    {
        m_coinInstance.Deactivate();
        m_coinInstance = null;
    }

    #endregion // Coins

    
    #region MapManager

    protected   MapManager      m_mapManager        = null;

    #endregion MapManager
}