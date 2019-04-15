/******************************************************************************
*  @file       BeginningLaneSet.cs
*  @brief      
*  @author     Lori
*  @date       September 9, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class BeginningLaneSet : LaneSet
{
    #region Public Interface

    /// <summary>
    /// Create a new BeginningLaneSet
    /// </summary>
    /// <param name="mapManager">instance that holds map data</param>
    /// <param name="mapAssetPool">pool of map assets</param>
    /// <param name="startRowCoord">row coordinate of the first lane</param>
    /// <param name="onLaneCreated">called each time a lane is created</param>
    public BeginningLaneSet(MapManager mapManager, MapAssetPool mapAssetPool,
                            int startRowCoord, OnLaneCreated onLaneCreated)
        : base(mapManager, mapAssetPool, startRowCoord, onLaneCreated)
	{
        m_type = LaneSetType.Beginning;
	}

	#endregion // Public Interface

	#region Lane Count

	/// <summary>
	/// Initializes the lane count.
	/// </summary>
	protected override void InitializeLaneCount(int startRowCoord)
	{
        int rowsBehindChar = 0;
        if (m_mapManager.StartRowIndex < 0)
        {
            rowsBehindChar = Mathf.Abs(m_mapManager.StartRowIndex);
        }
        m_targetCount = rowsBehindChar + Random.Range(2, 4); // TODO: Make this better
    }

	#endregion // Lane Count

	#region Lane Creation

	/// <summary>
	/// Creates a lane.
	/// </summary>
	protected override Lane CreateLane()
	{
		// Alternate between dark and light grass assets
		LaneResourceType laneResType = LaneResourceType.Grass_Dark;
		if ((Mathf.Abs(m_startRowCoord) + m_currentCount) % 2 == 1)
		{
			laneResType = LaneResourceType.Grass_Light;
		}

		Lane newLane = m_mapAssetPool.GetLaneAssetPool(laneResType).GetAsset();
        ActivateLane(newLane);
        return newLane;
	}

	#endregion Lane Creation
}