/******************************************************************************
*  @file       MapAssetPool.cs
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

public class MapAssetPool : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize(MapResources mapResources)
	{
		m_mapResources = mapResources;
		if (m_lanesRoot == null)
		{
			m_lanesRoot = new GameObject("Lanes").transform;
			m_lanesRoot.parent = transform;

        }
        if (m_grassEdgeItemsRoot == null)
        {
            m_grassEdgeItemsRoot = new GameObject("GrassEdgeItems").transform;
            m_grassEdgeItemsRoot.parent = transform;

        }
        if (m_grassObstaclesRoot == null)
        {
            m_grassObstaclesRoot = new GameObject("GrassObstacles").transform;
            m_grassObstaclesRoot.parent = transform;

        }
        if (m_platformsRoot == null)
        {
            m_platformsRoot = new GameObject("Platforms").transform;
            m_platformsRoot.parent = transform;
        }
        if (m_vehiclesRoot == null)
        {
            m_vehiclesRoot = new GameObject("Vehicles").transform;
            m_vehiclesRoot.parent = transform;
        }
        if (m_coinsRoot == null)
        {
            m_coinsRoot = new GameObject("Coins").transform;
            m_coinsRoot.parent = transform;
        }
        if (m_sharedItemsRoot == null)
        {
            m_sharedItemsRoot = new GameObject("SharedItems").transform;
            m_sharedItemsRoot.parent = transform;
        }

        // Create shared items
        CreateSharedAssets();
    }

	/// <summary>
	/// Creates asset pools.
	/// </summary>
	public void CreateAssetPools(MapManager mapManager)
	{
        m_mapManager = mapManager;
        m_tileSize = m_mapManager.TileSize;
		m_activeLaneCount = m_mapManager.ActiveLaneCount;
        m_activeTileCount = m_mapManager.ActiveTileCount;
        m_edgeTileCount = m_mapManager.EdgeTileCount;
		CreateLaneAssets();
        CreateMapItemAssets();
        CreateVehicleAssets();
    }

	/// <summary>
	/// Gets the lane asset pool.
	/// </summary>
	/// <returns>The lane asset pool.</returns>
	/// <param name="laneResType">Lane res type.</param>
	public AssetPool<Lane> GetLaneAssetPool(LaneResourceType laneResType)
	{
		switch (laneResType)
		{
		case LaneResourceType.Grass_Dark:
			return m_grassDarkLanes;

		case LaneResourceType.Grass_Light:
			return m_grassLightLanes;

		case LaneResourceType.Road_Single:
			return m_roadSingleLanes;

		case LaneResourceType.Road_Bottom:
			return m_roadBottomLanes;

		case LaneResourceType.Road_Middle:
			return m_roadMiddleLanes;

		case LaneResourceType.Road_Top:
			return m_roadTopLanes;

		case LaneResourceType.River:
			return m_riverLanes;

		case LaneResourceType.Railroad:
			return m_railroadLanes;

		default:
			return null;
		}
	}

    /// <summary>
    /// Gets the grass edge item asset pool
    /// </summary>
    /// <returns></returns>
    public AssetPool<Transform> GetGrassEdgeItemAssetPool()
    {
        return m_grassEdgeItems;
    }

    /// <summary>
    /// Gets the grass obstacle asset pool
    /// </summary>
    /// <returns></returns>
    public AssetPool<Transform> GetGrassObstacleAssetPool()
    {
        return m_grassObstacles;
    }

    /// <summary>
    /// Gets the platform asset pool
    /// </summary>
    /// <returns></returns>
    public AssetPool<Platform> GetPlatformAssetPool(MapItemType itemType)
    {
        switch(itemType)
        {
            case MapItemType.LilyPad:
                return m_lilyPads;

            case MapItemType.Log_1:
                return m_shortLogs;

            case MapItemType.Log_2:
                return m_mediumLogs;

            case MapItemType.Log_3:
                return m_longLogs;

            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the vehicle asset pool
    /// </summary>
    /// <param name="vehicleType"></param>
    /// <returns></returns>
    public AssetPool<Vehicle> GetVehicleAssetPool(VehicleType vehicleType)
    {
        int vehIndex = (int)vehicleType;
        if (vehIndex < 0 || vehIndex >= m_vehicles.Length)
        {
            return null;
        }
        return m_vehicles[vehIndex];
    }

    /// <summary>
    /// Gets the coin asset pool
    /// </summary>
    /// <returns></returns>
    public AssetPool<Coin> GetCoinAssetPool()
    {
        return m_coins;
    }

    /// <summary>
    /// Gets the eagle asset
    /// </summary>
    /// <returns></returns>
    public Eagle GetEagleAsset()
    {
        return m_eagle;
    }

    /// <summary>
    /// Gets the water splash asset
    /// </summary>
    /// <returns></returns>
    public ParticleSystem GetWaterSplashAsset()
    {
        return m_waterSplash;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Grass lane edges")]
    [Header("Asset use probability")]
    [SerializeField]private float       m_tallTreeEdgeProb          = 0.85f;
    [SerializeField]private float       m_mediumTreeEdgeProb        = 0.10f;
    [SerializeField]private float       m_shortTreeEdgeProb         = 0.03f;
    [SerializeField]private float       m_treeTrunkEdgeProb         = 0.01f;
    [SerializeField]private float       m_rocksEdgeProb             = 0.01f;
    [Header("Grass lane obstacles")]
    [SerializeField]private float       m_tallTreeObstacleProb      = 0.10f;
    [SerializeField]private float       m_mediumTreeObstacleProb    = 0.30f;
    [SerializeField]private float       m_shortTreeObstacleProb     = 0.30f;
    [SerializeField]private float       m_treeTrunkObstacleProb     = 0.10f;
    [SerializeField]private float       m_rockObstacleProb          = 0.30f;
    [Header("AssetCount")]
    [SerializeField]private int         m_vehicleMaxCount           = 50;
    [SerializeField]private int         m_lilyPadMaxCountPerLane    = 3;
    // TODO: ^ This should be somewhere else
    [Header("Coins")]
    [SerializeField]private int         m_coinCountMax              = 30;

    #endregion // Serialized Variables

    #region Asset 

    private         MapManager          m_mapManager            = null;
	private			MapResources		m_mapResources          = null;

    // Lanes
    private         AssetPool<Lane>		m_grassDarkLanes	    = null;
	private			AssetPool<Lane>		m_grassLightLanes	    = null;
	private			AssetPool<Lane>		m_roadSingleLanes	    = null;
	private			AssetPool<Lane>		m_roadBottomLanes	    = null;
	private			AssetPool<Lane>		m_roadMiddleLanes	    = null;
	private			AssetPool<Lane>		m_roadTopLanes		    = null;
	private			AssetPool<Lane>		m_riverLanes		    = null;
	private			AssetPool<Lane>		m_railroadLanes		    = null;

    // Map items
    private         AssetPool<Transform>    m_grassEdgeItems    = null;
    private         AssetPool<Transform>    m_grassObstacles    = null;
    private         AssetPool<Platform>     m_lilyPads          = null;
    private         AssetPool<Platform>     m_shortLogs         = null;
    private         AssetPool<Platform>     m_mediumLogs        = null;
    private         AssetPool<Platform>     m_longLogs          = null;

    // Vehicles
    private         AssetPool<Vehicle>[]    m_vehicles          = null;

    // Shared items
    private         AssetPool<Coin>         m_coins             = null;
    private         Eagle                   m_eagle             = null;
    private         ParticleSystem          m_waterSplash       = null;


    // Root transforms
	private			Transform	m_lanesRoot             = null;
    private         Transform   m_grassEdgeItemsRoot    = null;
    private         Transform   m_grassObstaclesRoot    = null;
    private         Transform   m_platformsRoot         = null;
    private         Transform   m_vehiclesRoot          = null;
    private         Transform   m_coinsRoot             = null;
    private         Transform   m_sharedItemsRoot       = null;

    // Map properties
    private         float       m_tileSize               = 15f;
	private			int         m_activeLaneCount	     = 20;
    private         int         m_activeTileCount        = 9;
    private         int         m_edgeTileCount          = 7;

    /// <summary>
    /// Creates the lane asset pools.
    /// </summary>
    private void CreateLaneAssets()
	{
		// Grass Dark Lane
		// Can only take up 1/2 of active lanes
		int laneCount = Mathf.CeilToInt(m_activeLaneCount * 0.5f);
		CreateLaneAssetPool(ref m_grassDarkLanes, laneCount, LaneResourceType.Grass_Dark);

		// Grass Light Lane
		// Can only take up 1/2 of active lanes
		laneCount = Mathf.CeilToInt(m_activeLaneCount * 0.5f);
		CreateLaneAssetPool(ref m_grassLightLanes, laneCount, LaneResourceType.Grass_Light);
		
		// Road Single Lane
		// Can only take up 1/2 of active lanes
		laneCount = Mathf.CeilToInt(m_activeLaneCount * 0.5f);
		CreateLaneAssetPool(ref m_roadSingleLanes, laneCount, LaneResourceType.Road_Single);
		
		// Road Bottom Lane
		// Can only take up 1/3 of active lanes
		laneCount = Mathf.CeilToInt(m_activeLaneCount / 3f);
		CreateLaneAssetPool(ref m_roadBottomLanes, laneCount, LaneResourceType.Road_Bottom);
		
		// Road Middle Lane
		// Can take all active lanes
		laneCount = m_activeLaneCount;
		CreateLaneAssetPool(ref m_roadMiddleLanes, laneCount, LaneResourceType.Road_Middle);
		
		// Road Top Lane
		// Can only take up 1/3 of active lanes
		laneCount = Mathf.CeilToInt(m_activeLaneCount / 3f);
		CreateLaneAssetPool(ref m_roadTopLanes, laneCount, LaneResourceType.Road_Top);

		// River Lane
		// Can take all active lanes
		laneCount = m_activeLaneCount;
		CreateLaneAssetPool(ref m_riverLanes, laneCount, LaneResourceType.River);

		// Railroad Lane
		// Can take all active lanes
		laneCount = m_activeLaneCount;
		CreateLaneAssetPool(ref m_railroadLanes, laneCount, LaneResourceType.Railroad);
	}

	/// <summary>
	/// Creates the lane asset pool.
	/// </summary>
	/// <param name="assetPool">Asset pool.</param>
	/// <param name="laneCount">Lane count.</param>
	private void CreateLaneAssetPool(ref AssetPool<Lane> assetPool, int laneCount, LaneResourceType laneResType)
	{
        // Prepare the pool
		if (assetPool == null)
		{
            // Create a new one if it hasn't been created yet
			assetPool = new AssetPool<Lane>(laneCount);
		}
		else
		{
            // Clear the pool if it already exists
			assetPool.ClearAllAssets();
			assetPool.Resize(laneCount);
		}
        // Populate the pool
		GameObject prefab = m_mapResources.GetLanePrefab(laneResType);
		for (int i = 0; i < laneCount; ++i)
		{
			GameObject newlaneObj = Instantiate<GameObject>(prefab);
			Lane newLane = newlaneObj.AddComponentNoDupe<Lane>();
            newLane.Initialize(m_mapManager, m_tileSize, m_activeTileCount, m_edgeTileCount, this);
			newlaneObj.transform.parent = m_lanesRoot;
			newlaneObj.SetActive(false);
			assetPool.AddAsset(newLane);
		}
	}

    /// <summary>
    /// Creates the map item assets
    /// </summary>
    private void CreateMapItemAssets()
    {
        CreateGrassEdgeItems();
        CreateGrassObstacles();
        CreatePlatforms();
    }

    /// <summary>
    /// Create grass edge items
    /// </summary>
    private void CreateGrassEdgeItems()
    {
        // Compute the maximum number of grass edge items
        int itemCount = m_activeLaneCount * m_edgeTileCount * 2;
        
        // Prepare the pool
        if (m_grassEdgeItems == null)
        {
            m_grassEdgeItems = new AssetPool<Transform>(itemCount);
        }
        else
        {
            m_grassEdgeItems.ClearAllAssets();
            m_grassEdgeItems.Resize(itemCount);
        }

        float[] prob =
        {
            m_tallTreeEdgeProb,
            m_mediumTreeEdgeProb,
            m_shortTreeEdgeProb,
            m_treeTrunkEdgeProb,
            m_rocksEdgeProb
        };

        // Populate the pool
        for (int i = 0; i < itemCount; ++i)
        {
            // Determine item type based on probability
            float rand = Random.value;
            float cumulative = prob[0];
            MapItemType itemType = MapItemType.Tree_Tall;
            for (int j = 0; rand > cumulative; ++j)
            {
                if (j + 1 == prob.Length)
                {
                    break;
                }
                cumulative += prob[j + 1];
                itemType = (MapItemType)((int)itemType + 1);
            }

            // Create a new instance
            GameObject prefab = m_mapResources.GetItemPrefab(itemType);
            GameObject newItem = Instantiate<GameObject>(prefab);
            newItem.transform.parent = m_grassEdgeItemsRoot;
            newItem.SetActive(false);
            m_grassEdgeItems.AddAsset(newItem.transform);
        }
    }

    /// <summary>
    /// Create grass edge items
    /// </summary>
    private void CreateGrassObstacles()
    {
        // TODO: Handle possible special obstacles

        // Compute the maximum number of obstacles
        // [- m_activeLaneCount] accounts for a single-tile open path
        int itemCount = (m_activeLaneCount * m_activeTileCount) - m_activeLaneCount;

        // Prepare the pool
        if (m_grassObstacles == null)
        {
            m_grassObstacles = new AssetPool<Transform>(itemCount);
        }
        else
        {
            m_grassObstacles.ClearAllAssets();
            m_grassObstacles.Resize(itemCount);
        }

        float[] prob =
        {
            m_tallTreeObstacleProb,
            m_mediumTreeObstacleProb,
            m_shortTreeObstacleProb,
            m_treeTrunkObstacleProb,
            m_rockObstacleProb
        };

        // Populate the pool
        for (int i = 0; i < itemCount; ++i)
        {
            // Determine item type based on probability
            float rand = Random.value;
            float cumulative = prob[0];
            MapItemType itemType = MapItemType.Tree_Tall;
            for (int j = 0; rand > cumulative; ++j)
            {
                if (j + 1 == prob.Length)
                {
                    break;
                }
                cumulative += prob[j + 1];
                itemType = (MapItemType)((int)itemType + 1);

            }

            // Create a new instance
            GameObject prefab = m_mapResources.GetItemPrefab(itemType);
            GameObject newItem = Instantiate<GameObject>(prefab);
            newItem.transform.parent = m_grassObstaclesRoot;
            newItem.SetActive(false);
            m_grassObstacles.AddAsset(newItem.transform);
        }
    }

    /// <summary>
    /// Creates platforms
    /// </summary>
    private void CreatePlatforms()
    {
        // Lily pads
        // Has as maximum number of instance per lane
        int itemCount = m_activeLaneCount * m_lilyPadMaxCountPerLane;
        CreatPlatformAssetPool(ref m_lilyPads, itemCount, MapItemType.LilyPad, m_platformsRoot);

        // Short logs
        // Assumed to be spaced by one tile per lane
        int spaceTaken = 2; // log size (1) + space size (1)
        itemCount = m_activeLaneCount * Mathf.CeilToInt(m_activeTileCount / spaceTaken);
        CreatPlatformAssetPool(ref m_shortLogs, itemCount, MapItemType.Log_1, m_platformsRoot);

        // Medium logs
        // Assumed to be spaced by one tile per lane
        spaceTaken = 3; // log size (2) + space size (1)
        itemCount = m_activeLaneCount * Mathf.CeilToInt(m_activeTileCount / spaceTaken);
        CreatPlatformAssetPool(ref m_mediumLogs, itemCount, MapItemType.Log_2, m_platformsRoot);

        // Long logs
        // Assumed to be spaced by one tile per lane
        spaceTaken = 4; // log size (3) + space size (1)
        itemCount = m_activeLaneCount * Mathf.CeilToInt(m_activeTileCount / spaceTaken);
        CreatPlatformAssetPool(ref m_longLogs, itemCount, MapItemType.Log_3, m_platformsRoot);
    }

    /// <summary>
    /// Creates a item asset pool
    /// </summary>
    /// <param name="assetPool"></param>
    /// <param name="itemCount"></param>
    /// <param name="itemType"></param>
    /// <param name="itemRoot"></param>
	private void CreateItemAssetPool(ref AssetPool<Transform> assetPool, int itemCount, MapItemType itemType, Transform itemRoot)
    {
        // Prepare the pool
        if (assetPool == null)
        {
            // Create a new one if it hasn't been created yet
            assetPool = new AssetPool<Transform>(itemCount);
        }
        else
        {
            // Clear the pool if it already exists
            assetPool.ClearAllAssets();
            assetPool.Resize(itemCount);
        }
        // Populate the pool
        GameObject prefab = m_mapResources.GetItemPrefab(itemType);
        for (int i = 0; i < itemCount; ++i)
        {
            GameObject newItem = Instantiate<GameObject>(prefab);
            newItem.transform.parent = itemRoot;
            newItem.SetActive(false);
            assetPool.AddAsset(newItem.transform);
        }
    }

    /// <summary>
    /// Creates a item asset pool
    /// </summary>
    /// <param name="assetPool"></param>
    /// <param name="itemCount"></param>
    /// <param name="itemType"></param>
    /// <param name="itemRoot"></param>
	private void CreatPlatformAssetPool(ref AssetPool<Platform> assetPool, int itemCount, MapItemType itemType, Transform itemRoot)
    {
        // Prepare the pool
        if (assetPool == null)
        {
            // Create a new one if it hasn't been created yet
            assetPool = new AssetPool<Platform>(itemCount);
        }
        else
        {
            // Clear the pool if it already exists
            assetPool.ClearAllAssets();
            assetPool.Resize(itemCount);
        }
        // Populate the pool
        GameObject prefab = m_mapResources.GetItemPrefab(itemType);
        for (int i = 0; i < itemCount; ++i)
        {
            GameObject newItem = Instantiate<GameObject>(prefab);
            Platform platformScript = newItem.AddComponentNoDupe<Platform>();
            platformScript.Initialize();
            newItem.transform.parent = itemRoot;
            newItem.SetActive(false);
            assetPool.AddAsset(platformScript);
        }
    }

    /// <summary>
    /// Create vehicle assets
    /// </summary>
    private void CreateVehicleAssets()
    {
        // Create the array of vehicle asset pools
        if (m_vehicles == null)
        {
            m_vehicles = new AssetPool<Vehicle>[(int)VehicleType.SIZE];
        }

        for (int i = 0; i < (int)VehicleType.SIZE; ++i)
        {
            int vehicleCount = m_vehicleMaxCount;
            if (i == (int)VehicleType.Train)
            {
                vehicleCount = 20;
            }
            if (m_vehicles[i] == null)
            {
                // Create the pool for one vehicle type
                m_vehicles[i] = new AssetPool<Vehicle>(vehicleCount);
            }
            else
            {
                // Clear the pool if it already exists
                m_vehicles[i].ClearAllAssets();
                m_vehicles[i].Resize(vehicleCount);
            }

            // Populate the pool
            GameObject prefab = m_mapResources.GetVehiclePrefab((VehicleType)i);
            for (int j = 0; j < vehicleCount; ++j)
            {
                GameObject vehicleObj = Instantiate<GameObject>(prefab);
                Vehicle vehicle = vehicleObj.AddComponentNoDupe<Vehicle>();
                vehicle.Initialize();
                vehicleObj.transform.parent = m_vehiclesRoot;
                vehicleObj.SetActive(false);
                m_vehicles[i].AddAsset(vehicle);
            }

        }
    }

    /// <summary>
    /// Creates shared assets
    /// </summary>
    private void CreateSharedAssets()
    {
        // Create coins

        // Prepare the pool
        if (m_coins == null)
        {
            // Create a new one if it hasn't been created yet
            m_coins = new AssetPool<Coin>(m_coinCountMax);
        }
        else
        {
            // Clear the pool if it already exists
            m_coins.ClearAllAssets();
            m_coins.Resize(m_coinCountMax);
        }
        // Populate the pool
        GameObject prefab = m_mapResources.GetCoinPrefab();
        for (int i = 0; i < m_coinCountMax; ++i)
        {
            GameObject coinObj = Instantiate<GameObject>(prefab);
            Coin coinScript = coinObj.AddComponentNoDupe<Coin>();
            coinObj.SetActive(false);
            coinObj.transform.parent = m_coinsRoot;
            coinScript.SetCoinRoot(m_coinsRoot);
            coinScript.Initialize();
            m_coins.AddAsset(coinScript);
        }

        // Create the eagle
        if (m_eagle == null)
        {
            prefab = m_mapResources.GetEaglePrefab();
            GameObject eagleObj = Instantiate<GameObject>(prefab);
            m_eagle = eagleObj.AddComponentNoDupe<Eagle>();
            eagleObj.transform.parent = m_sharedItemsRoot;
            eagleObj.SetActive(false);
        }

        // Create the water splash
        if (m_waterSplash == null)
        {
            prefab = m_mapResources.GetWaterSplashPrefab();
            GameObject waterSplashObj = Instantiate<GameObject>(prefab);
            m_waterSplash = waterSplashObj.GetComponent<ParticleSystem>();
            waterSplashObj.transform.parent = m_sharedItemsRoot;
            waterSplashObj.SetActive(false);
        }
    }

    #endregion // Asset pool
}