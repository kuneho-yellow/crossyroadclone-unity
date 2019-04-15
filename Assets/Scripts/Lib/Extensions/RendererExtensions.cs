/******************************************************************************
*  @file       RendererExtensions.cs
*  @brief      Extensions for Unity's Renderer component
*  @author     Ron
*  @date       July 29, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public static class RendererExtensions
{
	#region Public Interface

	/// <summary>
	/// Set the alpha component of the renderer's color.
	/// </summary>
	public static void SetAlpha(this Renderer renderer, float a)
	{
		Color newColor = renderer.material.color;
		newColor.a = a;
		renderer.material.color = newColor;
	}

	#endregion // Public Interface
}
