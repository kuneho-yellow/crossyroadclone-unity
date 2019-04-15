/******************************************************************************
*  @file       UIScrollItem.cs
*  @brief      UI class for a scroll item element
*  @author     Ron
*  @date       September 11, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

using TouchScript;

#endregion // Namespaces

public class UIScrollItem : UIElement
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="scrollPanel">The scroll panel this scroll item is part of.</param>
    /// <param name="includeInactive">if set to <c>true</c> inactive objects will
    ///                                 be considered in bounds calculations.</param>
    public void Initialize(UIScrollPanel scrollPanel, bool includeInactive)
	{
        base.InitializeElement();

        m_scrollPanel = scrollPanel;
        UpdateBounds(includeInactive);

        // Set initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Calculates the bounds of this scroll item based on the renderer bounds of its contents.
    /// </summary>
    /// <param name="includeInactive">if set to <c>true</c> inactive objects will
    ///                                 be considered in bounds calculations.</param>
    public void UpdateBounds(bool includeInactive)
    {
        m_min = CalculateMinBounds(includeInactive);
        m_max = CalculateMaxBounds(includeInactive);
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
        // Reset UI elements under this scroll item
        foreach (UIElement elem in this.GetComponentsInChildren<UIElement>())
        {
            if (this == elem)
            {
                continue;
            }
            elem.Reset();
        }
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete(bool deletedByScrollPanel = false)
	{
        // If deleted by anything other than the owner scroll panel, notify scroll panel
        if (!deletedByScrollPanel)
        {
            m_scrollPanel.NotifyDeleteScrollItem(this);
        }
        GameObject.Destroy(this.gameObject);
	}

    /// <summary>
    /// Gets the minimum bounds.
    /// </summary>
    public Vector2 MinBounds(bool includeInactive)
    {
        // Recalculate bounds
        m_min = CalculateMinBounds(includeInactive);
        return m_min;
    }

    /// <summary>
    /// Gets the maximum bounds.
    /// </summary>
    public Vector2 MaxBounds(bool includeInactive)
    {
        // Recalculate bounds
        m_max = CalculateMaxBounds(includeInactive);
        return m_max;
    }
    
    /// <summary>
    /// Sets the size of the collider.
    /// </summary>
    /// <param name="sizeX">The horizontal size.</param>
    /// <param name="sizeY">The vertical size.</param>
    public void SetColliderSize(float sizeX, float sizeY)
    {
        this.GetComponent<BoxCollider2D>().size = new Vector2(sizeX, sizeY);
    }

    /// <summary>
    /// Gets the width.
    /// (assumes that objects in the scroll item do not move relative to each other)
    /// </summary>
    public float Width
    {
        get { return m_max.x - m_min.x; }
    }

    /// <summary>
    /// Gets the height.
    /// (assumes that objects in the scroll item do not move relative to each other)
    /// </summary>
    public float Height
    {
        get { return m_max.y - m_min.y; }
    }

    /// <summary>
    /// Gets the index of this item in the scroll panel it belongs to.
    /// Returns -1 if not part of a scroll panel.
    /// </summary>
    public int Index
    {
        get
        {
            if (m_scrollPanel == null)
            {
                return -1;
            }
            return m_scrollPanel.GetItemIndex(this);
        }
    }

    #endregion // Public Interface

    #region Variables

    private UIScrollPanel m_scrollPanel = null;

    #endregion // Variables

    #region Contents

    private Vector2 m_min   = Vector2.one * Mathf.Infinity;
    private Vector2 m_max   = Vector2.one * Mathf.NegativeInfinity;

    /// <summary>
    /// Calculates the minimum bounds of this scroll item based on
    /// the renderer bounds of its contents.
    /// </summary>
    /// <param name="includeInactive">if set to <c>true</c> inactive objects will
    ///                                 be considered in bounds calculations.</param>
    private Vector2 CalculateMinBounds(bool includeInactive)
    {
        Vector2 min = Vector2.one * Mathf.Infinity;
        Renderer thisRenderer = this.GetComponent<Renderer>();
        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>(includeInactive))
        {
            if (renderer == thisRenderer)
            {
                continue;
            }
            min.x = Mathf.Min(min.x, renderer.bounds.min.x);
            min.y = Mathf.Min(min.y, renderer.bounds.min.y);
        }
        return min;
    }

    /// <summary>
    /// Calculates the maximum bounds of this scroll item based on
    /// the renderer bounds of its contents.
    /// </summary>
    /// <param name="includeInactive">if set to <c>true</c> inactive objects will
    ///                                 be considered in bounds calculations.</param>
    private Vector2 CalculateMaxBounds(bool includeInactive)
    {
        Vector2 max = Vector2.one * Mathf.NegativeInfinity;
        Renderer thisRenderer = this.GetComponent<Renderer>();
        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>(includeInactive))
        {
            if (renderer == thisRenderer)
            {
                continue;
            }
            max.x = Mathf.Max(max.x, renderer.bounds.max.x);
            max.y = Mathf.Max(max.y, renderer.bounds.max.y);
        }
        return max;
    }

    #endregion // Contents

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected override void Awake()
	{
        base.InitializeElement();

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
