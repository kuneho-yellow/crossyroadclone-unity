/******************************************************************************
*  @file       LaneSet.cs
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

public class LaneSet
{
	#region Public Interface

	/// <summary>
    /// Create a new LaneSet
    /// </summary>
    /// <param name="mapManager">instance that holds map data</param>
    /// <param name="mapAssetPool">pool of map assets</param>
    /// <param name="startRowCoord">row coordinate of the first lane</param>
    /// <param name="onLaneCreated">called each time a lane is created</param>
	public LaneSet(MapManager mapManager, MapAssetPool mapAssetPool,
                   int startRowCoord, OnLaneCreated onLaneCreated)
	{
        m_mapManager = mapManager;
		m_mapAssetPool = mapAssetPool;
		m_startRowCoord = startRowCoord;
		m_onLaneCreated = onLaneCreated;
		InitializeLaneCount(startRowCoord);
	}

	// Delegate called when a lane has been created
	public delegate void OnLaneCreated(Lane newLane);

	/// <summary>
	/// Gets the lane type.
	/// </summary>
	/// <value>The type.</value>
	public LaneSetType Type
	{
		get { return m_type; }
	}

	/// <summary>
	/// Creates the lanes.
	/// </summary>
	/// <returns>The number of new lanes created.</returns>
	/// <param name="maxLaneCount">Max lane count.</param>
	public int CreateLanes(int maxLaneCount)
	{
		int newLanesCount = 0;
		while (newLanesCount < maxLaneCount)
		{
			Lane newLane = CreateLane();
			newLanesCount++;
			if (m_onLaneCreated != null)
			{
				m_onLaneCreated(newLane);
			}
			if (IsSetComplete)
			{
				return newLanesCount;
			}
		}
		return newLanesCount;
	}

    /// <summary>
    /// Gets the previous lane's passable tile array
    /// </summary>
    /// <returns></returns>
    public bool[] GetPrevPassableTileArray()
    {
        return m_prevPassableTileArray;
    }

    /// <summary>
    /// Sets the previous lane's passable tile array
    /// </summary>
    /// <param name="prevPassableTileArray"></param>
    public void SetPrevPassableTileArray(bool[] prevPassableTileArray)
    {
        m_prevPassableTileArray = prevPassableTileArray;
    }

    /// <summary>
    /// Gets the previous lane direction
    /// </summary>
    /// <returns></returns>
    public LaneDirection GetPrevLaneDirection()
    {
        return m_prevDir;
    }

    /// <summary>
    /// Sets the previous lane direction
    /// </summary>
    public void SetPrevLaneDirection(LaneDirection prevDir)
    {
        m_prevDir = prevDir;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is set complete.
    /// </summary>
    /// <value><c>true</c> if this instance is set complete; otherwise, <c>false</c>.</value>
    public bool IsSetComplete
	{
		get { return m_targetCount == m_currentCount; }
	}

	/// <summary>
	/// Gets the target lane count.
	/// </summary>
	/// <value>The target lane count.</value>
	public int TargetLaneCount
	{
		get { return m_targetCount; }
	}

	/// <summary>
	/// Gets the current lane count.
	/// </summary>
	/// <value>The current lane count.</value>
	public int CurrentLaneCount
	{
		get { return m_currentCount; }
	}

	#endregion // Public Interface

	#region Type

	protected		LaneSetType			m_type				= LaneSetType.Grass;

	#endregion // Type

	#region Lane Count

	protected		int					m_targetCount		= 0;
	protected		int					m_currentCount		= 0;


	/// <summary>
	/// Initializes the lane count.
	/// </summary>
	protected virtual void InitializeLaneCount(int startRowCoord)
	{
		m_targetCount = 1;
	}

	#endregion Lane Count

	#region Lane Creation

    protected       MapManager          m_mapManager                = null;
	protected		MapAssetPool		m_mapAssetPool		        = null;
    protected		int					m_startRowCoord	            = 0;
    protected		OnLaneCreated		m_onLaneCreated		        = null;
    protected       bool[]              m_prevPassableTileArray     = null;
    protected       LaneDirection       m_prevDir                   = LaneDirection.NONE;

	/// <summary>
	/// Creates a lane.
	/// </summary>
	protected virtual Lane CreateLane()
	{
		return null;
	}

    /// <summary>
    /// Positions the new lane and activates it
    /// </summary>
    /// <param name="newLane"></param>
    protected void ActivateLane(Lane newLane)
    {
        float laneZPos = (m_startRowCoord + m_currentCount) * m_mapManager.TileSize;
        newLane.transform.SetPosZ(laneZPos);
        newLane.gameObject.SetActive(true);
        newLane.Activate(m_startRowCoord + m_currentCount, m_prevPassableTileArray, m_prevDir);
        m_prevPassableTileArray = newLane.PassableActiveTileArray;
        m_prevDir = newLane.Direction;
        m_currentCount++;
    }

	#endregion // Lane Creation
}