/******************************************************************************
*  @file       SpriteRendererExtensions.cs
*  @brief      Extensions for Unity's SpriteRenderer component
*  @author     Ron
*  @date       July 22, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public static class SpriteRendererExtensions
{
	#region Public Interface

	/// <summary>
	/// Set the alpha component of the sprite renderer's color.
	/// </summary>
	public static void SetAlpha(this SpriteRenderer renderer, float a)
	{
		Color newColor = renderer.color;
		newColor.a = a;
		renderer.color = newColor;
	}

	#endregion // Public Interface
}
