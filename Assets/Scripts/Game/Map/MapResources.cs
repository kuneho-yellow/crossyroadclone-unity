/******************************************************************************
*  @file       MapResources.cs
*  @brief      Manages map resources
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> Handles the loading of map resource prefabs
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion // Namespaces

public class MapResources : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Load the specified mapType.
	/// </summary>
	/// <param name="mapType">Map type.</param>
	public void Load(MapType mapType)
	{
		if (m_loadedMapType == mapType)
		{
			return;
		}

		bool isSuccess = true;
		isSuccess &= LoadLanePrefabs(mapType);
        isSuccess &= LoadItemPrefabs(mapType);
        isSuccess &= LoadVehiclePrefabs(mapType);

        if (isSuccess)
		{
			m_loadedMapType = mapType;
		}
	}

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
		m_loadedMapType = MapType.SIZE;
		m_lanePrefabs = new GameObject[(int)LaneResourceType.SIZE];
        m_itemPrefabs = new GameObject[(int)MapItemType.SIZE];
        m_vehiclePrefabs = new GameObject[(int)VehicleType.SIZE];
        LoadSharedPrefabs();
    }

	/// <summary>
	/// Gets the lane prefab.
	/// </summary>
	/// <returns>The lane prefab.</returns>
	/// <param name="laneResType">Lane res type.</param>
	public GameObject GetLanePrefab(LaneResourceType laneResType)
	{
		if (laneResType == LaneResourceType.SIZE || m_loadedMapType == MapType.SIZE)
		{
			return null;
		}
		return m_lanePrefabs[(int)laneResType];
	}

    /// <summary>
	/// Gets the item prefab.
	/// </summary>
	/// <returns>The item prefab.</returns>
	/// <param name="itemType">Item type.</param>
	public GameObject GetItemPrefab(MapItemType itemType)
    {
        if (itemType == MapItemType.SIZE || m_loadedMapType == MapType.SIZE)
        {
            return null;
        }
        return m_itemPrefabs[(int)itemType];
    }

    /// <summary>
	/// Gets the vehicle prefab.
	/// </summary>
	/// <returns>The vehicle prefab.</returns>
	/// <param name="vehicleType">Vehicle type.</param>
	public GameObject GetVehiclePrefab(VehicleType vehicleType)
    {
        if (vehicleType == VehicleType.SIZE || m_loadedMapType == MapType.SIZE)
        {
            return null;
        }
        return m_vehiclePrefabs[(int)vehicleType];
    }

    /// <summary>
    /// Gets the coin prefab
    /// </summary>
    /// <returns></returns>
    public GameObject GetCoinPrefab()
    {
        return m_coinPrefab;
    }

    /// <summary>
    /// Gets the eagle prefab
    /// </summary>
    /// <returns></returns>
    public GameObject GetEaglePrefab()
    {
        return m_eaglePrefab;
    }

    /// <summary>
    /// Gets the water splash prefab
    /// </summary>
    /// <returns></returns>
    public GameObject GetWaterSplashPrefab()
    {
        return m_waterSplashPrefab;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Header("Skinned Prefabs")]
    [SerializeField]private		string		m_mapSkinFilePath   = "Prefabs/Map/";
	[SerializeField]private		string		m_laneFilePath      = "/Lanes/";
    [SerializeField]private     string      m_itemFilePath      = "/Items/";
    [SerializeField]private     string      m_vehicleFilePath   = "/Vehicles/";
    [Header("Shared Prefabs")]
    [SerializeField]private     string      m_coinFilePath      = "Prefabs/Items/Coin";
    [SerializeField]private     string      m_eagleFilePath     = "Prefabs/Items/Eagle";
    [SerializeField]private     string      m_waterSplashPath   = "Prefabs/Items/WaterSplash";

	#endregion // Serialized Variables

	#region File Paths

	/// <summary>
	/// Gets the lane prefab path.
	/// </summary>
	/// <returns>The lane prefab path.</returns>
	private string GetLanePrefabPath(LaneResourceType laneResType,
	                                 MapType mapType = MapType.Default)
	{
		if (laneResType == LaneResourceType.SIZE || mapType == MapType.SIZE)
		{
			return "";
		}
		return m_mapSkinFilePath + mapType.ToString() + m_laneFilePath + laneResType.ToString();
	}

    /// <summary>
    /// Gets the item prefab path.
    /// </summary>
    /// <returns>The item prefab path.</returns>
    private string GetItemPrefabPath(MapItemType itemType,
                                     MapType mapType = MapType.Default)
    {
        if (itemType == MapItemType.SIZE || mapType == MapType.SIZE)
        {
            return "";
        }
        return m_mapSkinFilePath + mapType.ToString() + m_itemFilePath + itemType.ToString();
    }

    /// <summary>
    /// Gets the vehicle prefab path.
    /// </summary>
    /// <returns>The vehicle prefab path.</returns>
    private string GetVehiclePrefabPath(VehicleType vehicleType,
                                        MapType mapType = MapType.Default)
    {
        if (vehicleType == VehicleType.SIZE || mapType == MapType.SIZE)
        {
            return "";
        }
        return m_mapSkinFilePath + mapType.ToString() + m_vehicleFilePath + vehicleType.ToString();
    }

    #endregion // File Paths

    #region Prefab Loading

    private		MapType				m_loadedMapType		= MapType.SIZE;
	private		GameObject[]		m_lanePrefabs		= null;
    private     GameObject[]        m_itemPrefabs       = null;
    private     GameObject[]        m_vehiclePrefabs    = null;
    private     GameObject          m_coinPrefab        = null;
    private     GameObject          m_eaglePrefab       = null;
    private     GameObject          m_waterSplashPrefab = null;

	/// <summary>
	/// Loads the lane prefabs.
	/// </summary>
	private bool LoadLanePrefabs(MapType mapType)
	{
		if (mapType == MapType.SIZE)
		{
			return false;
		}

		string lanePath = m_mapSkinFilePath + mapType.ToString() + m_laneFilePath;
		int laneResSize = (int)LaneResourceType.SIZE;
		for (int i = 0; i < laneResSize; ++i)
		{
			if (m_lanePrefabs[i] != null)
			{
				// Unload previously loaded asset
				Resources.UnloadAsset(m_lanePrefabs[i]);
			}

			LaneResourceType laneResType = (LaneResourceType)i;
			m_lanePrefabs[i] = Resources.Load<GameObject>(lanePath + laneResType.ToString());
		}

		return true;
	}

    /// <summary>
	/// Loads the item prefabs.
	/// </summary>
	private bool LoadItemPrefabs(MapType mapType)
    {
        if (mapType == MapType.SIZE)
        {
            return false;
        }

        string itemPath = m_mapSkinFilePath + mapType.ToString() + m_itemFilePath;
        int itemCount = (int)MapItemType.SIZE;
        for (int i = 0; i < itemCount; ++i)
        {
            if (m_itemPrefabs[i] != null)
            {
                // Unload previously loaded asset
                Resources.UnloadAsset(m_itemPrefabs[i]);
            }

            MapItemType itemType = (MapItemType)i;
            m_itemPrefabs[i] = Resources.Load<GameObject>(itemPath + itemType.ToString());
        }

        return true;
    }

    /// <summary>
	/// Loads the vehicle prefabs.
	/// </summary>
	private bool LoadVehiclePrefabs(MapType mapType)
    {
        if (mapType == MapType.SIZE)
        {
            return false;
        }

        string vehiclePath = m_mapSkinFilePath + mapType.ToString() + m_vehicleFilePath;
        int vehicleCount = (int)VehicleType.SIZE;
        for (int i = 0; i < vehicleCount; ++i)
        {
            if (m_vehiclePrefabs[i] != null)
            {
                // Unload previously loaded asset
                Resources.UnloadAsset(m_vehiclePrefabs[i]);
            }

            VehicleType vehicleType = (VehicleType)i;
            m_vehiclePrefabs[i] = Resources.Load<GameObject>(vehiclePath + vehicleType.ToString());
        }

        return true;
    }

    /// <summary>
    /// Loads shared prefabs
    /// </summary>
    /// <returns></returns>
    private bool LoadSharedPrefabs()
    {
        if (m_coinPrefab == null)
        {
            m_coinPrefab = Resources.Load<GameObject>(m_coinFilePath);
        }

        if (m_eaglePrefab == null)
        {
            m_eaglePrefab = Resources.Load<GameObject>(m_eagleFilePath);
        }

        if (m_waterSplashPrefab == null)
        {
            m_waterSplashPrefab = Resources.Load<GameObject>(m_waterSplashPath);
        }

        return true;
    }

    #endregion // Prefab Loading
}