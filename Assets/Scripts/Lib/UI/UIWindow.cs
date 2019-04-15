/******************************************************************************
*  @file       UIWindow.cs
*  @brief      UI class for a window element
*  @author     Ron
*  @date       October 4, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;
using TouchScript.Gestures;

using TouchScript;
using System;

#endregion // Namespaces

public class UIWindow : UIElement
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
		//m_simplePanGesture.PanStarted
		//m_simplePanGesture.Panned
		//m_simplePanGesture.PanCompleted
	}

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
        // Empty
    }

    /// <summary>
    /// Delete this instance.
    /// </summary>
    public void Delete()
	{

	}

	#endregion // Public Interface

	#region Serialized Variables
	
	//[SerializeField] private bool m_freezeX	= false;
	//[SerializeField] private bool m_freezeY	= false;

	#endregion // Serialized Variables

	#region Delegates



	#endregion // Delegates

	#region Input Handling



	#endregion // Input Handling

	#region MonoBehaviour

	/// <summary>
	/// Awake this instance.
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start()
	{
		base.Start();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	protected override void Update()
	{
		base.Update();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	#endregion // MonoBehaviour
}
