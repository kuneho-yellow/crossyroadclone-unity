/******************************************************************************
*  @file       MapManager.cs
*  @brief      
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> Handles map creation
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion // Namespaces

public class MapManager : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
		// Initialize the map resources
		if (m_mapResources == null)
		{
			m_mapResources = gameObject.AddComponentNoDupe<MapResources>();
		}
		m_mapResources.Initialize();

		// Initialize the map asset pool
		if (m_mapAssetPool == null)
		{
			m_mapAssetPool = gameObject.AddComponentNoDupe<MapAssetPool>();
		}
		m_mapAssetPool.Initialize(m_mapResources);

        // Initialize the camera controller
        if (m_cameraController == null)
        {
            m_cameraController = gameObject.AddComponentNoDupe<CameraController>();
        }
        m_cameraController.Initialize();

        // Create collections
        if (m_activeLaneList == null)
        {
            m_activeLaneList = new List<Lane>(m_activeLaneCount);
        }

        // Ready the map resources
        m_mapResources.Load(m_mapType);
        m_mapAssetPool.CreateAssetPools(this);

        // Initialize the sound manager
        m_soundManager = (SoundManager)Locator.GetSoundManager();
        m_soundManager.InitializeRiverSoundAnim(m_riverNormalVolume, m_riverGameOverVolume,
                                                m_riverVolumeChangeTime, m_riverGameOverTime);
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

        m_isPaused = true;
        PauseLanes();
        m_cameraController.Pause();
        if (m_eagle != null)
        {
            m_eagle.Pause();
        }
        if (m_waterSplash != null)
        {
            m_waterSplash.Pause();
        }
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

        m_isPaused = false;
        UnpauseLanes();
        m_cameraController.Unpause();
        if (m_eagle != null)
        {
            m_eagle.Unpause();
        }
        if (m_waterSplash != null)
        {
            m_waterSplash.Play();
        }
    }

    /// <summary>
    /// Gets the tile size
    /// </summary>
    public int TileSize
    {
        get { return m_tileSize;  }
    }

    /// <summary>
    /// Gets the number of walkable tiles in a row
    /// </summary>
    public int ActiveTileCount
    {
        get { return m_activeTileCount; }
    }

    /// <summary>
    /// Gets the number of tiles of a single side in a row
    /// </summary>
    public int EdgeTileCount
    {
        get { return m_edgeTileCount; }
    }

    /// <summary>
    /// Gets the active lane count
    /// </summary>
    public int ActiveLaneCount
    {
        get { return m_activeLaneCount; }
    }
    
    /// <summary>
    /// Gets the row coordinate of the first spawned lane
    /// </summary>
    public int StartRowIndex
    {
        get { return m_startRowCoord; }
    }

    /// <summary>
    /// Creates a new map
    /// </summary>
    public void CreateNewMap(MapType mapType)
    {
        if (m_mapType != mapType)
        {
            m_mapType = mapType;
            m_mapResources.Load(m_mapType);
            m_mapAssetPool.CreateAssetPools(this);
        }
        StartCreateMap();
    }


    /// <summary>
    /// Initializes the character reference
    /// </summary>
    /// <param name="character"></param>
    public void InitializeCharacterReference(Character character)
    {
        m_character = character;
        m_characterCoord = m_characterStartCoord;
        m_characterFurthestRow = 0;
        m_characterIdleTimer = 0f;
        character.transform.position = GetPosInMap(m_characterCoord);
        m_cameraController.Reset();
    }

    /// <summary>
    /// Activate gameplay
    /// </summary>
    public void ActivateMap()
    {
        m_cameraController.SetFollowTarget(m_character.transform);
        m_character.SetOnStartJumpDelegate(CanCharacterJump);
        m_character.SetOnEndJumpDelegate(OnCharacterJumped);
    }

    /// <summary>
    /// Called when the game ends
    /// </summary>
    public void NotifyGameOver(Character.DeathType deathType)
    {
        switch (deathType)
        {
            case Character.DeathType.EAGLE:
                m_cameraController.StartFocusTarget(m_character.transform, true, true, false);
                break;

            case Character.DeathType.VEHICLE:
            default:
                m_cameraController.StartFocusTarget(m_character.transform, false, false, false);
                break;

            case Character.DeathType.DROWN:
                m_cameraController.StartFocusTarget(m_character.transform, true, true, false);
                break;

            case Character.DeathType.LOG:
                m_cameraController.StartFocusTarget(m_character.transform, false, false, true);
                break;
        }
    }

    /// <summary>
    /// Gets the characters current position
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCharacterCurrentPosition()
    {
        return m_cameraController.GetTargetPosition();
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Sizes")]
	[SerializeField]private	int				    m_tileSize			    = 15;
	[SerializeField]private	int				    m_activeLaneCount	    = 20;
    [SerializeField]private int                 m_activeTileCount       = 9;
    [SerializeField]private int                 m_edgeTileCount         = 7;
    [Header("Positions")]
    [SerializeField]private int                 m_startRowCoord         = -6;
    [SerializeField]private Vector2             m_characterStartCoord   = Vector2.zero;
    [SerializeField]private int                 m_minRowsFromCharacter  = 13;
	[Header("Resources")]
	[SerializeField]private	MapResources	    m_mapResources		    = null;
	[SerializeField]private	MapAssetPool	    m_mapAssetPool		    = null;
	[SerializeField]private	MapType			    m_mapType			    = MapType.Default;
    [Header("CameraController")]
    [SerializeField]private CameraController    m_cameraController      = null;
    [SerializeField]private int                 m_maxBackwardSteps      = 3;
    [SerializeField]private float               m_maxIdleTime           = 6f;
    [Header("Sounds")]
    [SerializeField]private float               m_riverNormalVolume     = 0.1f;
    [SerializeField]private float               m_riverGameOverVolume   = 1.0f;
    [SerializeField]private float               m_riverVolumeChangeTime = 0.8f;
    [SerializeField]private float               m_riverGameOverTime     = 4.0f;
    [Header("Probabilites")]
    [Tooltip("x : score\ny : probability of grass set after non-grass set")]
    [SerializeField]private Vector2[]           m_grassLaneSetProb      = new Vector2[]
    {
        new Vector2(0f, 0.9f),
        new Vector2(50f, 0.75f),
        new Vector2(100f, 0.5f),
        new Vector2(150f, 0.25f),
        new Vector2(200f, 0f)
    };

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
	{
        if (m_isPaused)
        {
            return;
        }

        UpdateCharacterStatus();
	}

    #endregion // MonoBehaviour

    #region State

    private         bool                m_isPaused          = false;

    #endregion  // State

    #region Map Creation

    private         List<Lane>			m_activeLaneList	= null;
	private			LaneSet				m_currentLaneSet	= null;
    private         int                 m_lowestRowCoord    = 0;
	private			int					m_highestRowCoord	= 0;

	/// <summary>
	/// Starts creating the map.
	/// </summary>
	private void StartCreateMap()
	{
        // Deactivate previously used lanes
		foreach (Lane lane in m_activeLaneList)
		{
            lane.Deactivate();
		}
		m_activeLaneList.Clear();

        // Initialize variables
        m_currentLaneSet = null;
        m_lowestRowCoord = m_startRowCoord;
        m_highestRowCoord = m_startRowCoord;

        // Create the starting set
		m_currentLaneSet = new BeginningLaneSet(this, m_mapAssetPool, m_highestRowCoord, OnNewLaneCreated);
		m_currentLaneSet.CreateLanes(m_activeLaneCount - m_highestRowCoord + m_startRowCoord);

        // Fill up the screen with random sets
        FillUpMap(m_activeLaneCount - m_highestRowCoord + m_lowestRowCoord);
	}

    /// <summary>
    /// Fills up the map by creating new lanes
    /// </summary>
    private void FillUpMap(int maxLaneCount)
    {
        // Fill up the screen with random sets
        int curLaneCount = 0;
        while (curLaneCount < maxLaneCount)
        {
            // Get a new lane set if the current set is complete
            if (m_currentLaneSet != null)
            {
                if (m_currentLaneSet.IsSetComplete)
                {
                    GetNewLaneSet();
                }
            }
            curLaneCount += m_currentLaneSet.CreateLanes(maxLaneCount - curLaneCount);
        }
    }

	/// <summary>
	/// Gets the new lane set.
	/// </summary>
	private void GetNewLaneSet()
	{
		// Determine the new lane set type
        LaneSet prevLaneSet = m_currentLaneSet;
        LaneSetType prevLaneSetType = prevLaneSet.Type;
        LaneSetType newLaneSetType = prevLaneSetType;
        if (prevLaneSetType == LaneSetType.Grass)
        {
            // Non-grass lane set
            newLaneSetType = (LaneSetType)Random.Range((int)LaneSetType.Road, (int)LaneSetType.River + 1);
        }
        else
        {
            if (Random.value <= GetCurrentGrassSetProb())
            {
                // Grass lane set
                newLaneSetType = LaneSetType.Grass;
            }
            else
            {
                // Get a random lane set, except the one used previously
                while (newLaneSetType == prevLaneSetType)
                {
                    newLaneSetType = (LaneSetType)Random.Range((int)LaneSetType.REGULAR_START + 1,
                                                                (int)LaneSetType.REGULAR_END);
                }
            }
        }

        switch (newLaneSetType)
		{
		default:
		case LaneSetType.Grass:
			m_currentLaneSet = new GrassLaneSet(this, m_mapAssetPool,
                                                m_highestRowCoord, OnNewLaneCreated);
                break;

		case LaneSetType.Road:
			m_currentLaneSet = new RoadLaneSet(this, m_mapAssetPool,
                                               m_highestRowCoord, OnNewLaneCreated);
                break;

		case LaneSetType.Railroad:
			m_currentLaneSet = new RailroadLaneSet(this, m_mapAssetPool,
                                                   m_highestRowCoord, OnNewLaneCreated);
                break;

		case LaneSetType.River:
			m_currentLaneSet = new RiverLaneSet(this, m_mapAssetPool,
                                                m_highestRowCoord, OnNewLaneCreated);
                break;
		}
        m_currentLaneSet.SetPrevPassableTileArray(prevLaneSet.GetPrevPassableTileArray());
        m_currentLaneSet.SetPrevLaneDirection(prevLaneSet.GetPrevLaneDirection());
	}

	/// <summary>
	/// Raises the new lane created event.
	/// </summary>
	/// <param name="newLane">New lane.</param>
	private void OnNewLaneCreated(Lane newLane)
	{
		if (m_activeLaneList.Count == m_activeLaneCount)
		{
            Lane oldLane = m_activeLaneList[0];
            m_activeLaneList.RemoveAt(0);
            if (oldLane != newLane)
            {
                oldLane.Deactivate();
            }
            m_lowestRowCoord++;
		}
		m_activeLaneList.Add(newLane);
		m_highestRowCoord++;
	}

    /// <summary>
    /// Pauses the lanes
    /// </summary>
    private void PauseLanes()
    {
        foreach(Lane lane in m_activeLaneList)
        {
            lane.Pause();
        }
    }

    /// <summary>
    /// Unpauses the lanes
    /// </summary>
    private void UnpauseLanes()
    {
        foreach (Lane lane in m_activeLaneList)
        {
            lane.Unpause();
        }
    }

    /// <summary>
    /// Gets the current grass lane set probability
    /// </summary>
    /// <returns></returns>
    private float GetCurrentGrassSetProb()
    {
        if (m_grassLaneSetProb == null)
        {
            return -1f;
        }
        float curProb = -1f;
        for (int i = 0; i < m_grassLaneSetProb.Length; ++i)
        {
            if (m_grassLaneSetProb[i].x > m_highestRowCoord)
            {
                break;
            }
            curProb = m_grassLaneSetProb[i].y;
        }
        return curProb;
    }

    #endregion // Map Creation

    #region Character

    private     Character               m_character             = null;
    private     Vector2				    m_characterCoord	    = Vector2.zero;
    private     int                     m_characterFurthestRow  = 0;
    private     float                   m_characterIdleTimer    = 0f;
    private     Eagle                   m_eagle                 = null;
    private     ParticleSystem          m_waterSplash           = null;
    private     float                   m_railroadComboTimer    = 0f;
    private     float                   m_railroadComboTimeMin  = 6f;
    private     bool                    m_isRailroadComboOn     = true;

    /// <summary>
    /// Updates the character status
    /// </summary>
    private void UpdateCharacterStatus()
    {
        if (m_character == null || !m_character.IsPlaying)
        {
            return;
        }

        // Update the character idle timer
        if (m_cameraController.IsTargetFollowed)
        {
            m_characterIdleTimer += Time.deltaTime;
        }

        // Update the character coordinates
        Vector2 oldCoord = m_characterCoord;
        m_characterCoord = GetTileCoord(m_character.transform.position);

        // Update the river sound
        UpdateRiverSound();

        // Compare the new coordinates with the old coordinates
        int oldRow = (int)oldCoord.y;
        int newRow = (int)m_characterCoord.y;

        // Update the score if needed
        if (newRow > m_characterFurthestRow)
        {
            m_characterFurthestRow = newRow;
            m_characterIdleTimer = 0f;
        }
        // Notify game manager of possible score change
        if (oldRow != newRow)
        {
            Locator.GetGameManager().NotifyCharacterNewScore(newRow);
        }

        // Check if eagle must be called
        if (m_characterFurthestRow - newRow >= m_maxBackwardSteps ||
            m_characterIdleTimer >= m_maxIdleTime)
        {
            ActivateEagle();
        }

        // Check if railroad combo has been met
        if (m_isRailroadComboOn)
        {
            if (GetActiveLane(newRow).Type == LaneType.Railroad)
            {
                m_railroadComboTimer += Time.deltaTime;
                if (m_railroadComboTimer >= m_railroadComboTimeMin)
                {
                    Locator.GetGameManager().NotifyCharacterRailroadStay();
                    m_isRailroadComboOn = false;
                }
            }
            else
            {
                m_railroadComboTimer = 0f;
            }
        }

        // Check if more lanes should be spawned
        if (m_highestRowCoord - newRow < m_minRowsFromCharacter)
        {
            FillUpMap(m_minRowsFromCharacter - m_highestRowCoord + newRow);
        }

        // Check if player is out of playing area
        if (!m_character.IsJumping && m_character.IsOnPlatform)
        {
            int newCol = (int)m_characterCoord.x;
            int leftEdge = -Mathf.FloorToInt(m_activeTileCount * 0.5f);
            int rightEdge = Mathf.FloorToInt(m_activeTileCount * 0.5f);
            if (m_activeTileCount % 2 == 0)
            {
                rightEdge--;
            }
            if ((newCol < leftEdge && m_character.ParentPlatform.IsFacingLeft) ||
                (newCol > rightEdge && !m_character.ParentPlatform.IsFacingLeft))
            {
                m_soundManager.PlayRiverGameOverSound();
                m_character.NotifyOutOfPlayableArea();
            }
        }
    }

    /// <summary>
    /// Determines whether the character can jump in the given direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private bool CanCharacterJump(Vector2 direction, out Vector3 targetPos, out Platform targetPlatform)
    {
        Vector2 targetCoord = GetTileCoord(m_character.transform.position) + direction;
        targetPos = m_character.transform.position + (new Vector3(direction.x, 0f, direction.y) * m_tileSize);
        bool canJump = IsTileOpen(targetCoord);
        targetPlatform = canJump ? GetPlatformInMap(targetPos) : null;
        if (targetPlatform != null)
        {
            targetPos = targetPlatform.GetNearestLandingPos(targetPos);
        }
        else
        {
            targetPos = canJump ? GetPosInMap(targetCoord) : Vector3.zero;
        }
        return canJump;
    }

    /// <summary>
    /// Called when the character has finished jumping
    /// </summary>
    private void OnCharacterJumped()
    {
        // Check if the character has jumped on water
        Lane charLane = GetActiveLane((int)m_characterCoord.y);
        if (charLane != null)
        {
            if (charLane.Type == LaneType.River && !m_character.HasTargetPlatform)
            {
                if (m_waterSplash == null)
                {
                    m_waterSplash = m_mapAssetPool.GetWaterSplashAsset();
                    m_waterSplash.gameObject.SetActive(true);
                }
                m_waterSplash.transform.position = m_character.transform.position;
                m_waterSplash.Play();
                Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.WaterSplash);
                m_character.NotifyJumpOnWater();
            }
        }
    }

    /// <summary>
    /// Activates the eagle
    /// </summary>
    private void ActivateEagle()
    {
        if (m_eagle == null)
        {
            m_eagle = m_mapAssetPool.GetEagleAsset();
        }
        Vector3 startPos = new Vector3(m_character.transform.position.x, 0f, m_highestRowCoord * m_tileSize);
        Vector3 endPos = new Vector3(m_character.transform.position.x, 0f, m_lowestRowCoord * m_tileSize);
        m_eagle.gameObject.SetActive(true);
        m_eagle.StartMovement(startPos, endPos);
        m_character.NotifyEagleArrived(m_eagle.gameObject);
    }

    #endregion // Character

    #region Tiles

    /// <summary>
    /// Get the tile coordinates from the given position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Vector2 GetTileCoord(Vector3 pos)
    {
        Vector2 coord = Vector2.zero;

        coord.x = Mathf.RoundToInt(pos.x / m_tileSize);
        coord.y = Mathf.RoundToInt(pos.z / m_tileSize);

        return coord;
    }

    /// <summary>
    /// Get the position from the given tile coordinates
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    private Vector3 GetPos(Vector2 tileCoord)
    {
        Vector3 pos = Vector3.zero;

        pos.x = Mathf.FloorToInt(tileCoord.x) * m_tileSize;
        pos.z = Mathf.FloorToInt(tileCoord.y) * m_tileSize;

        return pos;
    }

    /// <summary>
    /// Gets the ground-level position of a tile in the map
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    private Vector3 GetPosInMap(Vector2 tileCoord)
    {
        Lane thisLane = GetActiveLane((int)tileCoord.y);
        if (thisLane == null)
        {
            return Vector3.zero;
        }
        Vector3 pos = GetPos(tileCoord);
        pos.y = thisLane.Height;

        return pos;
    }

    /// <summary>
    /// Gets the lane in the active list with the given row coordinate
    /// </summary>
    /// <param name="rowCoord"></param>
    /// <returns></returns>
    private Lane GetActiveLane(int rowCoord)
    {
        // Current row is not in use
        if (rowCoord < m_lowestRowCoord || rowCoord > m_highestRowCoord)
        {
            return null;
        }

        return m_activeLaneList[rowCoord - m_lowestRowCoord];
    }

    /// <summary>
    /// Gets the platform present on a tile in the map
    /// </summary>
    private Platform GetPlatformInMap(Vector3 pos)
    {
        Lane thisLane = GetActiveLane((int)GetTileCoord(pos).y);
        if (thisLane == null)
        {
            return null;
        }
        return thisLane.GetPlatform(pos);
    }


    /// <summary>
    /// Returns whether a tile is open (passable) or not
    /// </summary>
    /// <param name="tileCoord"></param>
    /// <returns></returns>
    private bool IsTileOpen(Vector2 tileCoord)
    {
        Lane thisLane = GetActiveLane((int)tileCoord.y);
        if (thisLane == null)
        {
            return false;
        }
        return thisLane.IsTileOpen((int)tileCoord.x);
    }

    #endregion // Tiles

    #region Sounds

    private     SoundManager        m_soundManager      = null;
    private     int                 m_riverSoundRow     = 0;

    /// <summary>
    /// Updates the river sound
    /// </summary>
    private void UpdateRiverSound()
    {
        // Play the sound in the current lane if it's a river lane
        int curRow = (int)m_characterCoord.y;
        Lane curLane = GetActiveLane(curRow);
        if (curLane.Type == LaneType.River && m_riverSoundRow != curRow)
        {
            m_riverSoundRow = curRow;
            m_soundManager.PlayRiverSound(curLane.transform.position + (Vector3.up * curLane.Height));
            return;
        }

        // Play the sound from the nearest river lane
        for (int i = curRow; i <= m_highestRowCoord; ++i)
        {
            curLane = GetActiveLane(curRow);
            if (curLane.Type == LaneType.River)
            {
                if (Mathf.Abs(i - curRow) <= Mathf.Abs(curRow - m_riverSoundRow) &&
                    m_riverSoundRow != i)
                {
                    m_riverSoundRow = i;
                    m_soundManager.PlayRiverSound(curLane.transform.position + (Vector3.up * curLane.Height));
                }
                return;
            }
        }
    }

    #endregion // Sounds
}