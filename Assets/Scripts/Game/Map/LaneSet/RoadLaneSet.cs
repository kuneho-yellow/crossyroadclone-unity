/******************************************************************************
*  @file       RoadLaneSet.cs
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

public class RoadLaneSet : LaneSet
{
    #region Public Interface

    /// <summary>
    /// Create a new RoadLaneSet
    /// </summary>
    /// <param name="mapManager">instance that holds map data</param>
    /// <param name="mapAssetPool">pool of map assets</param>
    /// <param name="startRowCoord">row coordinate of the first lane</param>
    /// <param name="onLaneCreated">called each time a lane is created</param>
    public RoadLaneSet(MapManager mapManager, MapAssetPool mapAssetPool,
                       int startRowCoord, OnLaneCreated onLaneCreated)
        : base(mapManager, mapAssetPool, startRowCoord, onLaneCreated)
	{
		m_type = LaneSetType.Road;
	}

    #endregion // Public Interface

    #region Lane Count

    /// <summary>
    /// Initializes the lane count.
    /// </summary>
    protected override void InitializeLaneCount(int startRowCoord)
	{
        // TODO: Do properly!
        if (startRowCoord <= 25)
            m_targetCount = Random.Range(1, 3);
        else if (startRowCoord <= 50)
            m_targetCount = Random.Range(1, 4);
        else if (startRowCoord <= 100)
            m_targetCount = Random.Range(1, 5);
        else if (startRowCoord <= 150)
            m_targetCount = Random.Range(1, 10);
        else if (startRowCoord <= 200)
            m_targetCount = Random.Range(1, 15);
        else
            m_targetCount = Random.Range(1, 20);
    }

	#endregion // Lane Count     

	#region Lane Creation

	/// <summary>
	/// Creates a lane.
	/// </summary>
	protected override Lane CreateLane()
	{
		// Choose assets so road lines are properly placed
		LaneResourceType laneResType = LaneResourceType.Road_Middle;
		if (m_targetCount == 1)
		{
			laneResType = LaneResourceType.Road_Single;
		}
		else if (m_currentCount == 0)
		{
			laneResType = LaneResourceType.Road_Bottom;
		}
		else if (m_currentCount == m_targetCount - 1)
		{
			laneResType = LaneResourceType.Road_Top;
		}

		Lane newLane = m_mapAssetPool.GetLaneAssetPool(laneResType).GetAsset();
        ActivateLane(newLane);
        return newLane;
	}

	#endregion Lane Creation
}