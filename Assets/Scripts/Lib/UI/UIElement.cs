/******************************************************************************
*  @file       UIElement.cs
*  @brief      Base class for all UI elements
*  @author     Ron
*  @date       October 7, 2015
*      
*  @par [explanation]
*		> Holds common methods and fields for UI elements
******************************************************************************/

#region Namespaces

using UnityEngine;
using TouchScript.Behaviors;
using TouchScript.Gestures.Simple;

#endregion // Namespaces

public abstract class UIElement : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Updates this element's screen positioning.
    /// </summary>
    public void UpdateScreenPosition()
    {
        if (!m_enableScreenPositioning)
        {
            return;
        }

        UIManager uiManager = (UIManager)Locator.GetUIManager();
        if (uiManager == null)
        {
            return;
        }
        UICamera uiCam = uiManager.UICamera;

        Vector2 relativeOffset = Vector2.zero;
        switch (m_units)
        {
            case Units.PIXELS:
                relativeOffset = m_screenPosition;
                break;
            case Units.SCREEN_FRACTION:
                relativeOffset = new Vector2(m_screenPosition.x * Screen.width,
                                             m_screenPosition.y * Screen.height);
                // Convert relative offset from screen to world units
                relativeOffset = uiCam.Camera.ScreenToWorldPoint(relativeOffset) -
                                 uiCam.Camera.ScreenToWorldPoint(Vector3.zero);
                break;
        }

        switch (m_relativeTo)
        {
            case RelativeTo.TOP_LEFT:
                this.transform.SetPosXY(relativeOffset + uiCam.TopLeftWorld);
                break;
            case RelativeTo.TOP_RIGHT:
                this.transform.SetPosXY(relativeOffset + uiCam.TopRightWorld);
                break;
            case RelativeTo.BOTTOM_LEFT:
                this.transform.SetPosXY(relativeOffset + uiCam.BottomLeftWorld);
                break;
            case RelativeTo.BOTTOM_RIGHT:
                this.transform.SetPosXY(relativeOffset + uiCam.BottomRightWorld);
                break;
            case RelativeTo.TOP:
                this.transform.SetPosY(relativeOffset.y + uiCam.ScreenMaxWorld.y);
                break;
            case RelativeTo.BOTTOM:
                this.transform.SetPosY(relativeOffset.y + uiCam.ScreenMinWorld.y);
                break;
            case RelativeTo.LEFT:
                this.transform.SetPosX(relativeOffset.x + uiCam.ScreenMinWorld.x);
                break;
            case RelativeTo.RIGHT:
                this.transform.SetPosX(relativeOffset.x + uiCam.ScreenMaxWorld.x);
                break;
        }
    }

    /// <summary>
    /// Sets the screen corner or side the element will be positioned relative to.
    /// </summary>
    /// <param name="relativeTo">Screen corner or side.</param>
    /// <param name="updatePosition">if set to <c>true</c> update position.</param>
    public void SetRelativeTo(RelativeTo relativeTo, bool updatePosition = true)
    {
        bool changed = (m_relativeTo != relativeTo);
        m_relativeTo = relativeTo;
        if (updatePosition && changed)
        {
            UpdateScreenPosition();
        }
    }

    /// <summary>
    /// Sets the units in which this element's position is specified.
    /// </summary>
    /// <param name="units">Units.</param>
    /// <param name="updatePosition">if set to <c>true</c> update position.</param>
    public void SetScreenPositionUnits(Units units, bool updatePosition = true)
    {
        bool changed = (m_units != units);
        m_units = units;
        if (updatePosition)
        {
            if (changed)
            {
                UpdateScreenPosition();
            }
        }
    }

    /// <summary>
    /// Sets the screen position.
    /// </summary>
    /// <param name="screenPosition">Screen position.</param>
    /// <param name="updatePosition">if set to <c>true</c> update position.</param>
    public void SetScreenPosition(Vector2 screenPosition, bool updatePosition = true)
    {
        bool changed = (m_screenPosition != screenPosition);
        m_screenPosition = screenPosition;
        if (updatePosition)
        {
            if (changed)
            {
                UpdateScreenPosition();
            }
        }
    }

    /// <summary>
    /// Sets whether element position should be updated automatically.
    /// </summary>
    public void SetAutoUpdate(bool autoUpdate)
    {
        m_enableAutoUpdate = autoUpdate;
    }

    public void SetDraggable(bool isDraggable = true)
    {
        m_isDraggable = isDraggable;
    }
    public void SetRotatable(bool isRotatable = true)
    {
        m_isRotatable = isRotatable;
    }
    public void SetScalable(bool isScalable = true)
    {
        m_isScalable = isScalable;
    }
    /*public void SetBlockOtherInput(bool blockOtherInput = true)
	{
		m_blockOtherInput = blockOtherInput;
	}*/

    // Getters
    public bool IsDraggable
    {
        get { return m_isDraggable; }
    }
    public bool IsRotatable
    {
        get { return m_isRotatable; }
    }
    public bool IsScalable
    {
        get { return m_isScalable; }
    }
    /*public bool IsBlockOtherInput
	{
		get { return m_blockOtherInput; }
	}*/

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Allow this element to be dragged")]
    [SerializeField]
    protected bool m_isDraggable = false;
    [Tooltip("Allow this element to be rotated")]
    [SerializeField]
    protected bool m_isRotatable = false;
    [Tooltip("Allow this element to be scaled")]
    [SerializeField]
    protected bool m_isScalable = false;
    //[Tooltip("When input is on this element, block input to other UI elements")]
    // TODO: Uncomment when implementation is ready
    //[SerializeField] protected bool	m_blockOtherInput	= false;

    [Header("Screen Positioning")]

    [Tooltip("If enabled, the serialized variables following this would be used to position this element")]
    [SerializeField]
    protected bool m_enableScreenPositioning = false;

    [Tooltip("Units in which the UI element position is specified")]
    [SerializeField]
    protected Units m_units = Units.PIXELS;
    [Tooltip("Position the UI element relative to the specified screen corner or side")]
    [SerializeField]
    protected RelativeTo m_relativeTo = RelativeTo.BOTTOM_LEFT;
    [Tooltip("The UI element position relative to a screen corner or side, in either pixels or screen fraction")]
    [SerializeField]
    protected Vector2 m_screenPosition = Vector2.zero;

    [Tooltip("Use element's initial position to set the anchor values")]
    [SerializeField]
    protected bool m_useInitialPosition = false;
    [Tooltip("Update UI element position automatically")]
    [SerializeField]
    protected bool m_enableAutoUpdate = false;

    #endregion // Serialized Variables

    #region Initialization

    protected bool m_isInitialized = false;

    /// <summary>
    /// Initialize this instance.
    /// </summary>
    protected void InitializeElement()
    {
        m_spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        m_collider2D = this.gameObject.GetComponent<Collider2D>();
        if (m_isDraggable)
        {
            m_simplePanGesture = this.gameObject.AddComponentNoDupe<SimplePanGesture>();
            m_simplePanGesture.enabled = true;
        }
        if (m_isRotatable)
        {
            m_simpleRotateGesture = this.gameObject.AddComponentNoDupe<SimpleRotateGesture>();
            m_simpleRotateGesture.enabled = true;
        }
        if (m_isScalable)
        {
            m_simpleScaleGesture = this.gameObject.AddComponentNoDupe<SimpleScaleGesture>();
            m_simpleScaleGesture.enabled = true;
        }
        if (m_isDraggable || m_isRotatable || m_isScalable)
        {
            m_transformer2D = this.gameObject.AddComponentNoDupe<Transformer2D>();
            m_transformer2D.enabled = true;
        }

        if (m_enableScreenPositioning && m_useInitialPosition)
        {
            UICamera uiCam = Locator.GetUIManager().UICamera;

            Vector2 screenPos = Vector2.zero;
            switch (m_relativeTo)
            {
                case RelativeTo.TOP_LEFT:
                    screenPos = (Vector2)this.transform.position - uiCam.TopLeftWorld;
                    break;
                case RelativeTo.TOP_RIGHT:
                    screenPos = (Vector2)this.transform.position - uiCam.TopRightWorld;
                    break;
                case RelativeTo.BOTTOM_LEFT:
                    screenPos = (Vector2)this.transform.position - uiCam.BottomLeftWorld;
                    break;
                case RelativeTo.BOTTOM_RIGHT:
                    screenPos = (Vector2)this.transform.position - uiCam.BottomRightWorld;
                    break;
                case RelativeTo.TOP:
                    screenPos.y = this.transform.position.y - uiCam.ScreenMaxWorld.y;
                    break;
                case RelativeTo.BOTTOM:
                    screenPos.y = this.transform.position.y - uiCam.ScreenMinWorld.y;
                    break;
                case RelativeTo.LEFT:
                    screenPos.x = this.transform.position.x - uiCam.ScreenMinWorld.x;
                    break;
                case RelativeTo.RIGHT:
                    screenPos.x = this.transform.position.x - uiCam.ScreenMaxWorld.x;
                    break;
            }

            switch (m_units)
            {
                case Units.PIXELS:
                    m_screenPosition = screenPos;
                    break;
                case Units.SCREEN_FRACTION:
                    m_screenPosition = new Vector2(screenPos.x / Screen.width,
                                                   screenPos.y / Screen.height);
                    break;
            }
        }
    }

    #endregion // Initialization

    #region Input Handling

    protected Transformer2D m_transformer2D = null;
    protected SimplePanGesture m_simplePanGesture = null;
    protected SimpleRotateGesture m_simpleRotateGesture = null;
    protected SimpleScaleGesture m_simpleScaleGesture = null;

    /// <summary>
    /// Updates this element's draggable property.
    /// </summary>
    private void UpdateDraggable()
    {
        if (m_isDraggable && (m_simplePanGesture == null || !m_simplePanGesture.enabled))
        {
            m_simplePanGesture = this.gameObject.AddComponentNoDupe<SimplePanGesture>();
            m_simplePanGesture.enabled = true;
        }
        else if (!m_isDraggable && m_simplePanGesture != null)
        {
            Destroy(m_simplePanGesture);
            m_simplePanGesture = null;
        }
    }

    /// <summary>
    /// Updates this element's rotatable property.
    /// </summary>
    private void UpdateRotatable()
    {
        if (m_isRotatable && (m_simpleRotateGesture == null || !m_simpleRotateGesture.enabled))
        {
            m_simpleRotateGesture = this.gameObject.AddComponentNoDupe<SimpleRotateGesture>();
            m_simpleRotateGesture.enabled = true;
        }
        else if (!m_isRotatable && m_simpleRotateGesture != null)
        {
            Destroy(m_simpleRotateGesture);
            m_simpleRotateGesture = null;
        }
    }

    /// <summary>
    /// Updates this element's scalable property.
    /// </summary>
    private void UpdateScalable()
    {
        if (m_isScalable && (m_simpleScaleGesture == null || !m_simpleScaleGesture.enabled))
        {
            m_simpleScaleGesture = this.gameObject.AddComponentNoDupe<SimpleScaleGesture>();
            m_simpleScaleGesture.enabled = true;
        }
        else if (!m_isScalable && m_simpleScaleGesture != null)
        {
            Destroy(m_simpleScaleGesture);
            m_simpleScaleGesture = null;
        }
    }

    /// <summary>
    /// Updates this element's transformer component (for enabling draggable/rotatable/scalable properties).
    /// </summary>
    private void UpdateTransformer2D()
    {
        if ((m_isDraggable || m_isRotatable || m_isScalable) &&
            (m_transformer2D == null || !m_transformer2D.enabled))
        {
            m_transformer2D = this.gameObject.AddComponentNoDupe<Transformer2D>();
            m_transformer2D.enabled = true;
        }
        else if (m_transformer2D != null && !m_isDraggable && !m_isRotatable && !m_isScalable)
        {
            Destroy(m_transformer2D);
            m_transformer2D = null;
        }
    }

    #endregion // Input Handling

    #region Components

    protected SpriteRenderer m_spriteRenderer = null;
    protected Collider2D m_collider2D = null;

    #endregion // Components

    #region Screen Anchor

    public enum Units
    {
        PIXELS,
        SCREEN_FRACTION
    }

    public enum RelativeTo
    {
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    /// <summary>
    /// Updates UI position based on the device screen orientation.
    /// </summary>
    private void UpdateUIOrientation()
    {
        // Check if screen orientation changes
        UIManagerBase uimb = Locator.GetUIManager();
        if (uimb != null && uimb.HasScreenOrientationChanged)
        {
            UpdateScreenPosition();
        }
    }

    #endregion // Screen Anchor

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected virtual void Awake()
    {
        InitializeElement();
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
        UpdateDraggable();
        UpdateRotatable();
        UpdateScalable();
        UpdateTransformer2D();

        // Update element position
        if (m_enableAutoUpdate)
        {
            // If auto-update is enabled, UI position is continually updated
            //  regardless of changes in screen orientation
            UpdateScreenPosition();
        }
        else
        {
            // If auto-update is disabled, update position only when screen orientation changes
            UpdateUIOrientation();
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    protected virtual void OnEnable()
    {
        UpdateScreenPosition();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    protected virtual void OnDisable()
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