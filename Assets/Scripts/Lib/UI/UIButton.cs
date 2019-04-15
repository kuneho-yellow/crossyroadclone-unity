/******************************************************************************
*  @file       UIButton.cs
*  @brief      UI class for a button element
*  @author     Ron
*  @date       October 6, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;
using TouchScript.Gestures;

using TouchScript;

#endregion // Namespaces

public class UIButton : UIElement
{
	#region Public Interface

	/// <summary>
	/// Creates a button using the specified button sprites.
	/// </summary>
	/// <param name="triggerDelegate">Delegate to call when button is triggered.</param>
	/// <param name="triggerType">When is button "triggered"?</param>
	/// <param name="unpressedTexture">Texture when button is not pressed down.</param>
	/// <param name="pressedTexture">Texture when button is pressed down.</param>
	public void Initialize(System.EventHandler<System.EventArgs> triggerDelegate, TriggerType triggerType,
	                   Sprite unpressedSprite, Sprite pressedSprite)
	{
		Initialize(triggerDelegate, triggerType);

		m_unpressedSprite = unpressedSprite;
		if (m_pressedSprite == null)
		{
			m_pressedSprite = unpressedSprite;
		}
	}

	/// <summary>
	/// Creates a button using the default button sprites.
	/// </summary>
	/// <param name="triggerDelegate">Delegate to call when button is triggered.</param>
	/// <param name="triggerType">When is button "triggered"?</param>
	public void Initialize(System.EventHandler<System.EventArgs> triggerDelegate, TriggerType triggerType = TriggerType.ON_PRESS)
	{
		base.InitializeElement();

		// Make sure element has a PressGesture component
		m_pressGesture = this.gameObject.AddComponentNoDupe<PressGesture>();
		// No need for ReleaseGesture - this script has a different implementation for release

		m_triggerType = triggerType;

		switch (triggerType)
		{
		case TriggerType.ON_PRESS:
		default:
			AddPressDelegate(triggerDelegate);
			break;
		case TriggerType.ON_RELEASE:
			AddReleaseDelegate(triggerDelegate);
			break;
		}

        // If element has an active anchor mode, update position on initialize
        UpdateScreenPosition();

        // Set the initialized flag
        m_isInitialized = true;
	}

	/// <summary>
	/// Adds the press delegate.
	/// </summary>
	/// <returns><c>true</c>, if pressed delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="pressDelegate">Function delegate to call on button press.</param>
	public bool AddPressDelegate(System.EventHandler<System.EventArgs> pressDelegate)
	{
		if (m_pressDelegateSet.Add(pressDelegate))
		{
			m_pressGesture.Pressed += pressDelegate;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Adds the release delegate.
	/// </summary>
	/// <returns><c>true</c>, if released delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="releaseDelegate">Function delegate to call on button release.</param>
	public bool AddReleaseDelegate(System.EventHandler<System.EventArgs> releaseDelegate)
	{
		return m_releaseDelegateSet.Add(releaseDelegate);
	}

	/// <summary>
	/// Removes the press delegate.
	/// </summary>
	/// <returns><c>true</c>, if pressed delegate was removed, <c>false</c> otherwise.</returns>
	/// <param name="pressDelegate">Pressed delegate to remove.</param>
	public bool RemovePressDelegate(System.EventHandler<System.EventArgs> pressDelegate)
	{
		if (m_pressDelegateSet.Contains(pressDelegate))
		{
			m_pressDelegateSet.Remove(pressDelegate);
			m_pressGesture.Pressed -= pressDelegate;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes the release delegate.
	/// </summary>
	/// <returns><c>true</c>, if released delegate was removed, <c>false</c> otherwise.</returns>
	/// <param name="releaseDelegate">Released delegate to remove.</param>
	public bool RemoveReleaseDelegate(System.EventHandler<System.EventArgs> releaseDelegate)
	{
		if (m_releaseDelegateSet.Contains(releaseDelegate))
		{
			m_releaseDelegateSet.Remove(releaseDelegate);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Adds button press and release sound delegates.
	/// </summary>
	/// <returns><c>true</c>, if at least one sound delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="pressSoundDelegate">Press sound delegate.</param>
	/// <param name="releaseSoundDelegate">Release sound delegate.</param>
	public bool AddSoundDelegates(System.EventHandler<System.EventArgs> pressSoundDelegate,
	                              System.EventHandler<System.EventArgs> releaseSoundDelegate)
	{
		bool delegateAdded = false;
		if (pressSoundDelegate != null && AddPressDelegate(pressSoundDelegate))
		{
			delegateAdded = true;
		}
		if (releaseSoundDelegate != null)
		{
			m_buttonReleaseSoundDelegate = releaseSoundDelegate;
			delegateAdded = true;
		}
		return delegateAdded;
	}

	/// <summary>
	/// Sets the button sprite for unpressed state.
	/// </summary>
	/// <param name="unpressedSprite">Unpressed sprite.</param>
	public void SetUnpressedSprite(Sprite unpressedSprite)
	{
		m_unpressedSprite = unpressedSprite;
	}

	/// <summary>
	/// Sets the button sprite for pressed state.
	/// </summary>
	/// <param name="pressedSprite">Pressed sprite.</param>
	public void SetPressedSprite(Sprite pressedSprite)
	{
		m_pressedSprite = pressedSprite;
	}
	
	/// <summary>
	/// Resets this button to its default state.
	/// </summary>
	public override void Reset()
	{
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.sprite = m_unpressedSprite;
        }
		m_activeTouch = null;
		m_isPressed = false;
	}

	/// <summary>
	/// Delete this instance.
	/// </summary>
	public void Delete()
	{
		// Unsubscribe from events
		switch (m_triggerType)
		{
		case TriggerType.ON_PRESS:
		default:
			foreach (System.EventHandler<System.EventArgs> pressDelegate in m_pressDelegateSet)
			{
				m_pressGesture.Pressed -= pressDelegate;
			}
			m_pressDelegateSet.Clear();
			break;
		case TriggerType.ON_RELEASE:
			m_releaseDelegateSet.Clear();
			break;
		}
	}

	/// <summary>
	/// Gets whether this button is pressed.
	/// </summary>
	public bool IsPressed
	{
		get { return m_isPressed; }
	}

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Sprite when in \"inactive\" state. Same as pressed sprite by default if left empty")]
    [SerializeField] protected	Sprite		m_unpressedSprite	= null;
    [Tooltip("Sprite when in \"active\" state")]
	[SerializeField] protected	Sprite		m_pressedSprite		= null;

	#endregion // Serialized Variables

	#region Trigger Type

	public enum TriggerType
	{
		ON_PRESS,
		ON_RELEASE
	}
	private TriggerType m_triggerType = TriggerType.ON_PRESS;

	#endregion // Trigger Type

	#region Delegates

	// Button press and release delegates
	private HashSet<System.EventHandler<System.EventArgs>> m_pressDelegateSet 	= new HashSet<System.EventHandler<System.EventArgs>>();
	private HashSet<System.EventHandler<System.EventArgs>> m_releaseDelegateSet = new HashSet<System.EventHandler<System.EventArgs>>();
	// Delegate for playing button sound on release
	private System.EventHandler<System.EventArgs> m_buttonReleaseSoundDelegate 	= null;

	/// <summary>
	/// Raises the button press event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnButtonPress(object sender, System.EventArgs e)
	{
		// Process only one touch per button
		if (m_activeTouch != null)
		{
			return;
		}

		m_spriteRenderer.sprite = m_pressedSprite;
		m_isPressed = true;

		// Find and store a reference to the touch on this button
		m_activeTouch = m_pressGesture.ActiveTouches[0];
	}

	/// <summary>
	/// Raises the button release event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnButtonRelease(object sender, System.EventArgs e)
	{
		Reset();

		// Play button release sound (if available)
		if (m_buttonReleaseSoundDelegate != null)
		{
			m_buttonReleaseSoundDelegate(this, new System.EventArgs());
		}
	}

	#endregion // Delegates

	#region Input Handling

	private PressGesture m_pressGesture = null;

	private ITouch 	m_activeTouch 	= null;
	private bool	m_isPressed		= false;

	/// <summary>
	/// Updates the touch that is on this button.
	/// </summary>
	private void UpdateActiveTouch()
	{
		if (m_activeTouch == null)
		{
			return;
		}
		// If touch is released while over this button, trigger the release event
		if (!TouchManager.Instance.ActiveTouches.Contains(m_activeTouch))
		{
			// Make sure this button is still the touch target on release
			if (this.transform == TouchManager.Instance.GetHitTarget(m_activeTouch.Position))
			{
				foreach (System.EventHandler<System.EventArgs> releaseDelegate in m_releaseDelegateSet)
				{
					releaseDelegate(this, new System.EventArgs());
				}
			}
		}
		// If touch is moved off this button, call the OnButtonRelease method only
		else if (m_activeTouch.Hit == null || m_activeTouch.Hit.Transform != this.transform)
		{
			OnButtonRelease(this, new System.EventArgs());
		}
	}

	#endregion // Input Handling

	#region MonoBehaviour

	/// <summary>
	/// Awake this instance.
	/// </summary>
	protected override void Awake()
	{
		base.Awake();

		// Make sure element has a PressGesture component
		m_pressGesture = this.gameObject.AddComponentNoDupe<PressGesture>();
		// No need for ReleaseGesture - this script has a different implementation for release
		
		AddPressDelegate(OnButtonPress);
		AddReleaseDelegate(OnButtonRelease);
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

		UpdateActiveTouch();
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
