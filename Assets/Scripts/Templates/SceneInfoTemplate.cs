#if false
/******************************************************************************
*  @file       SceneInfo.cs
*  @brief      Holds basic data and methods for game scenes
*  @author     
*  @date       January 1, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SceneInfo
{
	#region Scene Identifiers
	
	// Enum of all scenes that can be switched to in the game
	// (does not include Main, as you can only switch *from* it to another scene)
	public enum SceneEnum
	{
		//TEST_SCENE = 0,
		
		SIZE
	}
	
	// The order scene names and master scripts are listed should match GameScene enum
	private string[] m_sceneNames =
	{
		"",
	};
	
	#endregion // Scene Identifiers

	#region Public Interface

	/// <summary>
	/// Gets the name of the specified scene.
	/// </summary>
	/// <returns>Name of specified scene.</returns>
	/// <param name="gameScene">Game scene.</param>
	public string GetSceneNameOf(SceneEnum gameScene)
	{
		if (gameScene == SceneEnum.SIZE)
		{
			Debug.Log("Specified item is not a scene");
			return null;
		}
		return m_sceneNames[(int)gameScene];
	}

	#endregion // Public Interface
}
#endif