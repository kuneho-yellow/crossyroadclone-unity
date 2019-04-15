/******************************************************************************
*  @file       UICamera.cs
*  @brief      Handles initialization and control of the UI camera
*  @author     Ron
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> Provides convenience properties for various camera and screen values,
*           such as the world coordinates corresponding to the screen corners
*       > Updates camera size when device screen orientation changes
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UICamera : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Sets the orthographic sizes for landscape and portrait screen orientation.
    /// </summary>
    /// <param name="landscapeOrthoSize">Orthographic size in landscape orientation.</param>
    /// <param name="portraitOrthoSize">Orthographic size in portrait orientation.</param>
    public void SetOrthographicSizes(int landscapeOrthoSize, int portraitOrthoSize)
    {
        m_landscapeOrthoSize = landscapeOrthoSize;
        m_portraitOrthoSize = portraitOrthoSize;
    }

	/// <summary>
	/// Gets the world coordinates at the lower left corner of the screen.
	/// </summary>
	public Vector2 ScreenMinWorld
	{
		get { return BottomLeftWorld; }
	}

	/// <summary>
	/// Gets the world coordinates at the center of the screen.
	/// </summary>
	public Vector2 ScreenCenterWorld
	{
		get { return m_uiCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1.0f)); }
	}

	/// <summary>
	/// Gets the world coordinates at the upper right corner of the screen.
	/// </summary>
	public Vector2 ScreenMaxWorld
	{
		get { return TopRightWorld; }
	}

    /// <summary>
	/// Gets the world coordinates at the upper left corner of the screen.
	/// </summary>
    public Vector2 TopLeftWorld
    {
        get { return m_uiCamera.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, 1.0f)); }
    }

    /// <summary>
	/// Gets the world coordinates at the upper right corner of the screen.
	/// </summary>
	public Vector2 TopRightWorld
    {
        get { return m_uiCamera.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, 1.0f)); }
    }

    /// <summary>
	/// Gets the world coordinates at the lower left corner of the screen.
	/// </summary>
    public Vector2 BottomLeftWorld
    {
        get { return m_uiCamera.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, 1.0f)); }
    }

    /// <summary>
    /// Gets the world coordinates at the lower right corner of the screen.
    /// </summary>
    public Vector2 BottomRightWorld
    {
        get { return m_uiCamera.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, 1.0f)); }
    }

    /// <summary>
    /// Gets the initial aspect ratio.
    /// </summary>
    public float AspectRatio
    {
        get { return m_aspectRatio; }
    }

    /// <summary>
    /// Gets whether the current screen orientation is landscape.
    /// </summary>
    public bool IsLandscape
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return Screen.width > Screen.height;
#endif
        }
    }

    /// <summary>
    /// Gets whether the current screen orientation is portrait.
    /// </summary>
    public bool IsPortrait
    {
        get
        {
#if UNITY_EDITOR
            return false;
#else
            return Screen.width < Screen.height;
#endif
        }
    }

    /// <summary>
    /// Gets the camera.
    /// </summary>
    public Camera Camera
	{
		get { return m_uiCamera; }
	}

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Orthographic size to use when in landscape screen orientation (unused when 0)")]
    [SerializeField] private float m_landscapeOrthoSize   = 0;
    [Tooltip("Orthographic size to use when in portrait screen orientation (unused when 0)")]
    [SerializeField] private float m_portraitOrthoSize    = 0;

    #endregion // Serialized Variables

    #region Variables

	private Camera  m_uiCamera          = null;

    private float   m_initialOrthoSize  = 0.0f;
    private bool    m_startInLandscape  = false;

    private float   m_aspectRatio       = 0.0f;

    #endregion // Variables

    #region Orthographic Size

    /// <summary>
    /// Initializes camera orthographic sizes for landscape and portrait screen orientations.
    /// </summary>
    private void InitializeOrthoSize()
    {
        // Set orthographic sizes for landscape and portrait orientation
        m_initialOrthoSize = m_uiCamera.orthographicSize;
        m_startInLandscape = (Screen.orientation == ScreenOrientation.Landscape) ||
                             (Screen.orientation == ScreenOrientation.LandscapeRight);
        // If user specified an orthographic size for only one orientation,
        //  calculate the equivalent size for the other orientation
        if (m_landscapeOrthoSize > 0.0f && m_portraitOrthoSize <= 0.0f)
        {
            m_portraitOrthoSize = m_landscapeOrthoSize * m_aspectRatio;
        }
        else if (m_portraitOrthoSize > 0.0f && m_landscapeOrthoSize <= 0.0f)
        {
            m_landscapeOrthoSize = m_portraitOrthoSize / m_aspectRatio;
        }
        // If user did not specify both orthographic sizes, use the camera's starting size for
        //  the current orientation and calculate the equivalent size for the other orientation
        else if (m_landscapeOrthoSize <= 0.0f && m_portraitOrthoSize <= 0.0f)
        {
            if (m_startInLandscape)
            {
                m_landscapeOrthoSize = m_initialOrthoSize;
                m_portraitOrthoSize = m_initialOrthoSize * m_aspectRatio;
            }
            else /* if start in portrait */
            {
                m_portraitOrthoSize = m_initialOrthoSize;
                m_landscapeOrthoSize = m_initialOrthoSize / m_aspectRatio;
            }
        }

        // Update camera size depending on starting orientation
        m_uiCamera.orthographicSize = m_startInLandscape ? m_landscapeOrthoSize : m_portraitOrthoSize;
    }

    /// <summary>
    /// Updates the orthographic size when device screen orientation changes.
    /// </summary>
    private void UpdateOrthoSize()
    {
        // Check if screen orientation changes
        UIManagerBase uimb = Locator.GetUIManager();
        if (uimb != null && uimb.HasScreenOrientationChanged)
        {
            // Update orthographic size according to the new orientation
            m_uiCamera.orthographicSize = IsLandscape ? m_landscapeOrthoSize : m_portraitOrthoSize;
        }
    }

    #endregion // Orthographic Size

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
	{
		// Make sure this object has a UI camera component
		m_uiCamera = this.gameObject.AddComponentNoDupe<Camera>();
		// Initialize UI camera settings
		m_uiCamera.orthographic = true;
        //m_uiCamera.orthographicSize = Screen.height * 0.5f;
        m_aspectRatio = Screen.width / Screen.height;

        // Screen orientation is only relevant on mobile platforms
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        InitializeOrthoSize();
#endif
    }

	/// <summary>
	/// Start this instance.
	/// </summary>
	private void Start()
	{
		
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	private void Update()
	{
        // Screen orientation is only relevant on mobile platforms
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        UpdateOrthoSize();
#endif
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
	{

	}

    #endregion // MonoBehaviour
}
