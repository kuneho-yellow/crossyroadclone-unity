/******************************************************************************
*  @file       RiverLaneSet.cs
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

public class RiverLaneSet : LaneSet
{
    #region Public Interface

    /// <summary>
    /// Create a new RiverLaneSet
    /// </summary>
    /// <param name="mapManager">instance that holds map data</param>
    /// <param name="mapAssetPool">pool of map assets</param>
    /// <param name="startRowCoord">row coordinate of the first lane</param>
    /// <param name="onLaneCreated">called each time a lane is created</param>
    public RiverLaneSet(MapManager mapManager, MapAssetPool mapAssetPool,
                        int startRowCoord, OnLaneCreated onLaneCreated)
        : base(mapManager, mapAssetPool, startRowCoord, onLaneCreated)
	{
		m_type = LaneSetType.River;
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
            m_targetCount = Random.Range(1, 2);
        else if (startRowCoord <= 50)
            m_targetCount = Random.Range(1, 3);
        else if (startRowCoord <= 100)
            m_targetCount = Random.Range(1, 4);
        else if (startRowCoord <= 150)
            m_targetCount = Random.Range(1, 5);
        else if (startRowCoord <= 200)
            m_targetCount = Random.Range(1, 8);
        else
            m_targetCount = Random.Range(1, 10);
    }

	#endregion // Lane Count     

	#region Lane Creation

	/// <summary>
	/// Creates a lane.
	/// </summary>
	protected override Lane CreateLane()
	{
		Lane newLane = m_mapAssetPool.GetLaneAssetPool(LaneResourceType.River).GetAsset();
        ActivateLane(newLane);
        return newLane;
	}

	#endregion Lane Creation
}