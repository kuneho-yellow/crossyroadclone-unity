/******************************************************************************
*  @file       SceneReferences.cs
*  @brief      Holds references to objects in the generic scene hierarchy
*  @author     Ron
*  @date       September 24, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SceneReferences : MonoBehaviour
{
	#region Public Interface

	public Transform 	TouchDebugger	= null;
	public Transform 	Gameplay		= null;
	public Transform 	DynamicObjects	= null;
	public Transform 	World			= null;
	public Transform 	UI				= null;
	public UICamera		UICamera		= null;

	#endregion // Public Interface
}
