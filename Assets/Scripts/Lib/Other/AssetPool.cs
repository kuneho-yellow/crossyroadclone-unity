/******************************************************************************
*  @file       AssetPool.cs
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
using System.Collections.Generic;

#endregion // Namespaces

public class AssetPool<T> where T : Component
{
	private		List<T>		m_pool			= null;
	private		int			m_curIndex		= 0;

	#region Public Interface

	/// <summary>
	/// Initializes a new instance of the <see cref="AssetPool`1"/> class.
	/// </summary>
	/// <param name="capacity">Capacity.</param>
	public AssetPool(int capacity)
	{
		m_pool = new List<T>(capacity);
		m_curIndex = 0;
	}

	/// <summary>
	/// Resize the specified capacity.
	/// </summary>
	/// <param name="capacity">Capacity.</param>
	public void Resize(int capacity)
	{
		if (m_pool == null)
		{
			return;
		}
		m_pool.Capacity = capacity;
	}
	
	/// <summary>
	/// Adds the asset.
	/// </summary>
	/// <param name="thisAsset">This asset.</param>
	public void AddAsset(T thisAsset)
	{
		m_pool.Add(thisAsset);
	}

	/// <summary>
	/// Gets the asset.
	/// </summary>
	/// <returns>The asset.</returns>
	public T GetAsset()
	{
		if (m_pool == null)
		{
			return null;
		}

		if (m_curIndex >= m_pool.Count)
		{
			ResetIndex();
		}
		m_curIndex++;
		return m_pool[m_curIndex - 1];
	}

	/// <summary>
	/// Clears all assets.
	/// </summary>
	public void ClearAllAssets()
	{
		if (m_pool == null)
		{
			return;
		}

		foreach (T asset in m_pool)
		{
			GameObject.Destroy(asset.gameObject);
		}
		m_pool.Clear();
	}

	/// <summary>
	/// Resets the index.
	/// </summary>
	public void ResetIndex()
	{
		m_curIndex = 0;
	}

	#endregion // Public Interface
}
