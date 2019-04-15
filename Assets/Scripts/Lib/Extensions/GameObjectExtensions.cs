/******************************************************************************
*  @file       GameObjectExtensions.cs
*  @brief      Extensions for Unity's GameObject component
*  @author     Ron
*  @date       July 31, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public static class GameObjectExtensions
{
	#region Public Interface

	/// <summary>
	/// Adds the specified component to the game object if component is not attached.
	/// Else, gets the attached component and returns it.
	/// </summary>
	/// <returns>The attached component.</returns>
	/// <param name="checkChildren">If set to <c>true</c> also look for component in the game object's children.</param>
	/// <param name="replaceExisting">If set to <c>true</c> replace existing attached component, if any.</param>
	/// <typeparam name="T">The component type to add or get.</typeparam>
	public static T AddComponentNoDupe<T>(this GameObject gameObject, bool checkChildren = false,
	                                      bool replaceExisting = false) where T : Component
	{
		T component = null;
		if (checkChildren)
		{
			component = gameObject.GetComponentInChildren<T>();
		}
		else
		{
			component = gameObject.GetComponent<T>();
		}
		if (component != null && replaceExisting)
		{
			Object.Destroy(component);
			component = null;
		}
		if (component == null)
		{
			component = gameObject.AddComponent<T>();
		}
		return component;
	}

	/// <summary>
	/// Adds the specified derived component if the base component is not attached.
	/// Else, gets the attached component and returns it.
	/// </summary>
	/// <typeparam name="B">The base class to find in this object.</typeparam>
	/// <typeparam name="D">The class derived from base that would be added to this object.</typeparam>
	public static B AddDerivedIfNoBase<B, D>(this GameObject gameObject) where D : Component, B
	{
		// See if component of the specified base class is already attached
		B var = gameObject.GetComponent<B>();
		// If not, add the specified derived component
		if (var == null)
		{
			var = gameObject.AddComponent<D>();
		}
		return var;
	}

	#endregion // Public Interface
}
