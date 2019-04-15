/******************************************************************************
*  @file       UIBox.cs
*  @brief      UI class for a non-interactive UI element
*  @author     Ron
*  @date       September 24, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIBox : UIElement
{
    #region Public Interface

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
        // Empty
    }

    #endregion // Public Interface

    #region Serialized Variables

    #endregion // Serialized Variables

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
	/// Raises the destroy event.
	/// </summary>
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	#endregion // MonoBehaviour
}
