/******************************************************************************
*  @file       UIContainer.cs
*  @brief      Instantiable UI element
*  @author     Ron
*  @date       October 4, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIContainer : UIElement
{
    #region Public Interface

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {

    }
    
	#endregion // Public Interface

	#region Serialized Variables

	#endregion // Serialized Variables

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

        // Since containers typically won't be initialized via script,
        //  position should be automatically updated
        m_enableAutoUpdate = true;
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
