/******************************************************************************
*  @file       Player.cs
*  @brief      
*  @author     Lori
*  @date       September 10, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using TouchScript;
using TouchScript.Gestures;
using TouchScript.Gestures.Simple;

#endregion // Namespaces

public class Player : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
		InitializeComponents();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    /// <summary>
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

	#region Input Delegates

	public delegate void OnPressDelegate();
	public delegate void OnReleaseDelegate();
    public delegate void OnSwipeDeleagete(Vector2 dir);

	/// <summary>
	/// Sets the press delegate.
	/// </summary>
	/// <param name="onPress">On press.</param>
	public void SetPressDelegate(OnPressDelegate onPress)
	{
		m_onPress = onPress;
	}

	/// <summary>
	/// Sets the release delegate.
	/// </summary>
	/// <param name="onRelease">On release.</param>
	public void SetReleaseDelegate(OnReleaseDelegate onRelease)
	{
		m_onRelease = onRelease;
	}
	
    /// <summary>
	/// Sets the swipe delegate.
	/// </summary>
	/// <param name="onSwipe">On swipe.</param>
	public void SetSwipeDelegate(OnSwipeDeleagete onSwipe)
    {
        m_onSwipe = onSwipe;
    }

    /// <summary>
    /// Calls the OnPress delegate
    /// </summary>
    public void CallOnPressDelegate()
    {
        if (m_onPress != null)
        {
            m_onPress();
        }
    }

    #endregion // Input Delegates

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     float       m_swipeMinCentimeters   = 1.0f;

    #endregion // Serialized Variables

    #region Variables

    private bool m_isInitialized    = false;
    private bool m_isPaused         = false;

    #endregion

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
	{

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
		
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour

	#region Components

	private			Collider2D			m_collider2D			= null;
	private			PressGesture		m_pressGesture			= null;
	private			ReleaseGesture		m_releaseGesture		= null;
    private         SimplePanGesture    m_panGesture            = null;

	/// <summary>
	/// Initializes the components.
	/// </summary>
	private void InitializeComponents()
	{
		m_collider2D = gameObject.AddDerivedIfNoBase<Collider2D, BoxCollider2D>();
		m_collider2D.isTrigger = true;
		m_pressGesture = gameObject.AddComponentNoDupe<PressGesture>();
		m_releaseGesture = gameObject.AddComponentNoDupe<ReleaseGesture>();
        m_panGesture = gameObject.AddComponentNoDupe<SimplePanGesture>();
		m_pressGesture.Pressed += OnPlayerPress;
		m_releaseGesture.Released += OnPlayerRelease;
        m_panGesture.Panned += OnPlayerSwipe;
	}

	#endregion // Components

	#region Input

	private			OnPressDelegate		m_onPress				= null;
	private			OnReleaseDelegate	m_onRelease				= null;
    private         OnSwipeDeleagete    m_onSwipe               = null;
    private         Vector2             m_startPos              = Vector2.zero;
    // True if player has swiped in this touch instance before release
    private         bool                m_hasSwiped             = false;

	/// <summary>
	/// Raises the player press event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnPlayerPress(object sender, System.EventArgs e)
	{
        m_startPos = m_pressGesture.ScreenPosition;
		if (m_onPress != null)
		{
			m_onPress();
		}
        m_hasSwiped = false;
    }

	/// <summary>
	/// Raises the player release event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnPlayerRelease(object sender, System.EventArgs e)
	{
		if (m_onRelease != null)
		{
            if (!m_hasSwiped)
            {
                ForceSwipeUp();
            }
			m_onRelease();
		}
	}

    /// <summary>
	/// Raises the player swipe event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnPlayerSwipe(object sender, System.EventArgs e)
    {
        Vector2 swipeVector = m_panGesture.ScreenPosition - m_startPos;
        float minDistance = m_swipeMinCentimeters * TouchScript.TouchManager.Instance.DotsPerCentimeter;
        if (swipeVector.sqrMagnitude >= minDistance)
        {
            if (m_onSwipe != null)
            {
                // Choose between left/right or up/down
                if (Mathf.Abs(swipeVector.x) >= Mathf.Abs(swipeVector.y))
                {
                    swipeVector.y = 0f;
                }
                else
                {
                    swipeVector.x = 0f;
                }
                swipeVector.Normalize();

                m_onSwipe(swipeVector);
                m_hasSwiped = true;
            }
        }
    }

    /// <summary>
    /// Do a "swipe up" action
    /// "Corrects" the move direction of a single tap
    /// </summary>
    private void ForceSwipeUp()
    {
        if (m_onSwipe != null)
        {
            m_onSwipe(Vector2.up);
        }
    }

    #endregion // Input
}
