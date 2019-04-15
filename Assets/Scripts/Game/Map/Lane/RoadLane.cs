/******************************************************************************
*  @file       RoadLane.cs
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

public class RoadLane : Lane
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
        CreateVehicles(prevDir);
        SpawnCoin();
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

        if (m_vehicles != null)
        {
            foreach (Vehicle vehicle in m_vehicles)
            {
                vehicle.Pause();
            }
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Pause();
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

        if (m_vehicles != null)
        {
            foreach (Vehicle vehicle in m_vehicles)
            {
                vehicle.Unpause();
            }
        }
        if (m_coinInstance != null)
        {
            m_coinInstance.Unpause();
        }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Probabilities")]
    [SerializeField]private     float   m_oppositeDirProb   = 60.0f;
    //[Tooltip("Probability spawning a police car")]
    //[SerializeField]private     float   m_policeCarProb     = 0.01f;
    //[Tooltip("Probability spawning a race car")]
    //[SerializeField]private     float   m_raceCarProb       = 0.01f;
    //[Tooltip("Probability spawning a gas truck")]
    //[SerializeField]private     float   m_gasTruckProb      = 0.01f;
    [Header("Space between vehicles in % tile size")]
    [SerializeField]private     float   m_minSpace          = 1.5f;
    [SerializeField]private     float   m_maxSpace          = 9.0f;

    #endregion // Serialized Variables

    #region Items and Obstacles

    private         Vehicle[]           m_vehicles          = null;
    private         Vehicle             m_specialVehicle    = null;

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
        // Empty the vehicle holder array
        if (m_vehicles != null)
        {
            for (int i = 0; i < m_vehicles.Length; ++i)
            {
                if (m_vehicles[i] != null)
                {
                    m_vehicles[i].Deactivate();
                }
                m_vehicles[i] = null;
            }
        }

        // Return the special vehicle
        if (m_specialVehicle != null)
        {
            m_specialVehicle.Deactivate();
            m_specialVehicle = null;
        }

        // Return the coin instance
        if (m_coinInstance != null)
        {
            m_coinInstance.Deactivate();
            m_coinInstance = null;
        }
    }

    /// <summary>
    /// Creates vehicles
    /// </summary>
    private void CreateVehicles(LaneDirection prevDir)
    {
        // Determine a random "regular" vehicle type
        VehicleType vehType = (VehicleType)Random.Range(0, (int)VehicleType.Train);
        Vehicle vehSample = m_mapAssetPool.GetVehicleAssetPool(vehType).GetAsset();

        // Determine speed and no. of vehicles
        float speed = vehSample.GetRandomSpeed();
        float spaceBetweenVehicles = Random.Range(m_minSpace, m_maxSpace) * m_tileSize;
        float totalMoveSpace = (m_activeTileCount + (m_edgeTileCount * 2.0f)) * m_tileSize;
        float vehicleAndSpaceSize = spaceBetweenVehicles + vehSample.VehicleLength;
        totalMoveSpace += vehSample.VehicleLength;
        int vehicleCount = Mathf.FloorToInt(totalMoveSpace / vehicleAndSpaceSize);

        // Randomly determine lane direction
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

        // Spawn the vehicles properly spaced apart
        Vector3 pos1 = new Vector3(totalMoveSpace * -0.5f, m_height, 0f) + transform.position;
        Vector3 pos2 = new Vector3(totalMoveSpace * 0.5f, m_height, 0f) + transform.position;
        if (m_direction == LaneDirection.LEFT)
        {
            Vector3 temp = pos1;
            pos1 = pos2;
            pos2 = temp;
        }

        // Create the array of vehicles
        m_vehicles = new Vehicle[vehicleCount];

        m_vehicles[0] = vehSample;
        m_vehicles[0].gameObject.SetActive(true);
        m_vehicles[0].SetMapManagerInstance(m_mapManager);
        m_vehicles[0].StartMovement(speed, pos1, pos2, 0f);

        for (int i = 1; i < vehicleCount; ++i)
        {
            m_vehicles[i] = m_mapAssetPool.GetVehicleAssetPool(vehType).GetAsset();
            m_vehicles[i].gameObject.SetActive(true);
            float timeFactor = i / (float)vehicleCount;
            m_vehicles[i].SetMapManagerInstance(m_mapManager);
            m_vehicles[i].StartMovement(speed, pos1, pos2, timeFactor);
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
            // Get a random spawnable tile
            int tileIndex = Random.Range(0, m_activeTileCount);

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

    #endregion // Coins
}