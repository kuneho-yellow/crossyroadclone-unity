/******************************************************************************
*  @file       UIText.cs
*  @brief      UI class for a text element
*  @author     Ron
*  @date       October 6, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIText : UIElement
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
        if (m_isInitialized)
        {
            return;
        }

        // Make sure object has a TextMesh component
        m_textMesh = this.gameObject.AddComponentNoDupe<TextMesh>();

        // Add a SortingLayerExposed component to allow the text mesh to be sorted along with sprites
        this.gameObject.AddComponentNoDupe<SortingLayerExposed>();

        // If element has an active anchor mode, update position on initialize
        UpdateScreenPosition();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets the text.
    /// </summary>
    public void SetText(string text)
    {
        m_textMesh.text = text;
    }

    /// <summary>
    /// Sets the font size.
    /// </summary>
    public void SetFontSize(int size)
    {
        m_textMesh.fontSize = size;
    }

    /// <summary>
    /// Sets the font color.
    /// </summary>
    public void SetColor(Color color)
    {
        m_textMesh.GetComponent<Renderer>().material.color = color;
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

    #region Variables

    private TextMesh m_textMesh = null;

    #endregion // Variables

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
