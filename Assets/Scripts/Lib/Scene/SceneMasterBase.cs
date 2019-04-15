/******************************************************************************
*  @file       SceneMasterBase.cs
*  @brief      Abstract base class for SceneMaster classes
*  @author     Ron
*  @date       August 30, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public abstract class SceneMasterBase : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Loads resources for this scene master's scene.
	/// </summary>
	public abstract bool Load();

	/// <summary>
	/// Unloads resources.
	/// </summary>
	public abstract bool Unload();

	/// <summary>
	/// Starts the scene.
	/// </summary>
	public abstract void StartScene();

	/// <summary>
	/// Notifies of scene pause.
	/// </summary>
	public virtual void NotifyPause() {}

	/// <summary>
	/// Notifies of scene unpause.
	/// </summary>
	public virtual void NotifyUnpause() {}

	/// <summary>
	/// Notifies of scene quit.
	/// </summary>
	public virtual void NotifyQuit() {}

	/// <summary>
	/// Gets whether this instance has finished initialization.
	/// </summary>
	/// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
	public bool IsInitialized
	{
		get { return m_isInitialized; }
	}

	// References to transforms in the base scene hierarchy
	public Transform TouchDebugger
	{
		get { return m_sceneReferences.TouchDebugger; }
	}
	public Transform Gameplay
	{
		get { return m_sceneReferences.Gameplay; }
	}
	public Transform DynamicObjects
	{
		get { return m_sceneReferences.DynamicObjects; }
	}
	public Transform World
	{
		get { return m_sceneReferences.World; }
	}
	public Transform UI
	{
		get { return m_sceneReferences.UI; }
	}
	public UICamera UICamera
	{
		get { return m_sceneReferences.UICamera; }
	}

	#endregion // Public Interface

	#region Variables

	protected bool m_isInitialized = false;

	protected SceneReferences m_sceneReferences = null;

	#endregion // Variables

	#region MonoBehaviour

	/// <summary>
	/// Awake this instance.
	/// </summary>
	protected virtual void Awake()
	{
        m_sceneReferences = this.GetComponent<SceneReferences>();
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected virtual void Start()
	{

	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	protected virtual void Update()
	{
		
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	protected virtual void OnDestroy()
	{
		
	}

	#endregion // MonoBehaviour
}
