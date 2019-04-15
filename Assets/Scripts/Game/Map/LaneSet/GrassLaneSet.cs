/******************************************************************************
*  @file       GrassLaneSet.cs
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

public class GrassLaneSet : LaneSet
{
    #region Public Interface

    /// <summary>
    /// Create a new GrassLaneSet
    /// </summary>
    /// <param name="mapManager">instance that holds map data</param>
    /// <param name="mapAssetPool">pool of map assets</param>
    /// <param name="startRowCoord">row coordinate of the first lane</param>
    /// <param name="onLaneCreated">called each time a lane is created</param>
    public GrassLaneSet(MapManager mapManager, MapAssetPool mapAssetPool,
                        int startRowCoord, OnLaneCreated onLaneCreated)
        : base(mapManager, mapAssetPool, startRowCoord, onLaneCreated)
	{
		m_type = LaneSetType.Grass;
	}

    #endregion // Public Interface

    #region Lane Count

    // x = lane count
    // y = probability
    private Vector2[] m_laneCountProb = new Vector2[]
    {
        new Vector2(1, 0.3f),
        new Vector2(2, 0.3f),
        new Vector2(3, 0.2f),
        new Vector2(4, 0.1f),
        new Vector2(5, 0.05f),
        new Vector2(6, 0.05f)
    };

	/// <summary>
	/// Initializes the lane count.
	/// </summary>
	protected override void InitializeLaneCount(int startRowCoord)
	{
        float rand = Random.value;
        float cumulative = 0f;
        m_targetCount = 1;
        for (int i = 0; i < m_laneCountProb.Length; ++i)
        {
            cumulative += m_laneCountProb[i].y;
            if (rand < cumulative)
            {
                break;
            }
            m_targetCount = (int)m_laneCountProb[i].x;
        }
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