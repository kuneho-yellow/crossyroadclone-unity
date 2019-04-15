/******************************************************************************
*  @file       UIUtils.cs
*  @brief      Holds utility functions for UI
*  @author     Ron
*  @date       October 1, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public static class UIUtils
{
	#region Sprite Resize

	/// <summary>
	/// Calculates the sprite scale to fill the specified fractions of the given camera's viewport.
	/// To leave a dimension unscaled, specify a fraction of 0.
	/// </summary>
	/// <param name="sprite">Sprite.</param>
	/// <param name="camera">Camera.</param>
	/// <param name="screenFractionX">Horizontal fraction of screen to fill.</param>
	/// <param name="screenFractionY">Vertical fraction of screen to fill.</param>
	public static Vector3 GetSpriteScaleResizedToScreen(SpriteRenderer sprite, Camera camera, float screenFractionX, float screenFractionY)
	{
		// Sprite resize supports only orthographic cameras for now
		if (!camera.orthographic)
		{
			return Vector3.zero;
		}

		// If both fraction parameters are 0 or less, no scaling would happen
		if (screenFractionX <= 0.0f && screenFractionY <= 0.0f)
		{
			return Vector3.zero;
		}

		// Store the sprite's scale
		Vector3 spriteScale = sprite.transform.localScale;

		// Set scale to one to normalize calculation
		sprite.transform.localScale = Vector3.one;
		
		// Get sprite dimensions
		float spriteWidth = sprite.bounds.size.x;
		float spriteHeight = sprite.bounds.size.y;
		
		// Height of world seen in viewport is twice the camera's orthographic size
		float worldScreenHeight = camera.orthographicSize * 2.0f;
		// Get the width of world seen in viewport by multiplying height by the aspect ratio
		float worldScreenWidth = worldScreenHeight * ((float)Screen.width / Screen.height);
		
		// Set new scale
		float scaleX = spriteScale.x;
		if (screenFractionX > 0.0f)
		{
			scaleX = (worldScreenWidth / spriteWidth) * screenFractionX;
		}
		float scaleY = spriteScale.y;
		if (screenFractionY > 0.0f)
		{
			scaleY = (worldScreenHeight / spriteHeight) * screenFractionY;
		}

		// Restore the sprite's scale
		sprite.transform.localScale = spriteScale;

		// Return the computed scale
		return new Vector3(scaleX, scaleY, 1.0f);
	}

	/// <summary>
	/// Calculates the sprite scale to fill the given camera's viewport.
	/// </summary>
	/// <param name="sprite">Sprite.</param>
	/// <param name="camera">Camera.</param>
	public static Vector3 GetSpriteScaleToFillScreen(SpriteRenderer sprite, Camera camera)
	{
		return GetSpriteScaleResizedToScreen(sprite, camera, 1.0f, 1.0f);
	}

    /// <summary>
    /// Sets the layer recursively.
    /// </summary>
    /// <param name="obj">The root object of the hierarchy whose layer would be set.</param>
    /// <param name="layerName">The layer to assign.</param>
    public static void SetLayerRecursively(GameObject obj, string layerName)
    {
        foreach (Transform child in obj.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    /// <summary>
    /// Sets the sorting layer recursively.
    /// </summary>
    /// <param name="renderer">The root renderer of the hierarchy whose sorting layer and sorting order would be set.</param>
    /// <param name="sortingLayerName">The sorting layer to assign.</param>
    /// <param name="sortingOrder">The sorting order to assign.</param>
    public static void SetSortingLayerRecursively(Renderer renderer, string sortingLayerName, int sortingOrder)
    {
        foreach (Renderer childRenderer in renderer.GetComponentsInChildren<Renderer>())
        {
            childRenderer.sortingLayerName = sortingLayerName;
            childRenderer.sortingOrder = sortingOrder;
        }
    }

	#endregion // Sprite Resize
}
