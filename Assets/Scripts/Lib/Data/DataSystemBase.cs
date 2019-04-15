/******************************************************************************
*  @file       DataSystemBase.cs
*  @brief      Abstract base class for data system classes
*  @author     
*  @date       July 21, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public abstract class DataSystemBase
{
	#region Public Interface

	public abstract bool Initialize();
	public abstract bool Save();
	public abstract bool Load();

	public bool IsInitialized
	{
		get { return m_isInitialized; }
	}
	
	#endregion // Public Interface
	
	#region Variables
	
	protected bool m_isInitialized = false;
	
	#endregion // Variables
}
