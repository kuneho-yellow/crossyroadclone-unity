/******************************************************************************
*  @file       UIToggleButton.cs
*  @brief      UI class for a toggle button element
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

#endregion // Namespaces

public class UIToggleButton : UIElement
{
	#region Public Interface

	/// <summary>
	/// Creates a toggle button with the specified button sprites for the two toggle button states.
	/// </summary>
	/// <param name="state1TriggerDelegate">Delegate to call when button is switched to state 1.</param>
	/// <param name="state2TriggerDelegate">Delegate to call when button is switched to state 2.</param>
	/// <param name="triggerType">When is button "triggered"?</param>
	/// <param name="state1UnpressedSprite">Unpressed sprite for toggle button state 1.</param>
	/// <param name="state1PressedSprite">Pressed sprite for toggle button state 1.</param>
	/// <param name="state2UnpressedSprite">Unpressed sprite for toggle button state 2.</param>
	/// <param name="state2PressedSprite">Pressed sprite for toggle button state 2.</param>
	/// <param name="initialState">Starting state of toggle button.</param>
	public void Initialize(System.EventHandler<System.EventArgs> state1TriggerDelegate,
	                       System.EventHandler<System.EventArgs> state2TriggerDelegate,
	                       TriggerType triggerType,
						   Sprite state1UnpressedSprite,
	                       Sprite state1PressedSprite,
	                   	   Sprite state2UnpressedSprite,
	                       Sprite state2PressedSprite,
	                       ToggleButtonState initialState = ToggleButtonState.STATE1)
	{
		Initialize(state1TriggerDelegate, state2TriggerDelegate, triggerType);

		// If pressed sprite parameters are left empty, use the unpressed sprite
		if (state1PressedSprite == null)
		{
			state1PressedSprite = state1UnpressedSprite;
		}
		if (state2PressedSprite == null)
		{
			state2PressedSprite = state2UnpressedSprite;
		}
		// Set sprites for the appropriate toggle button states
		SetSprites(state1UnpressedSprite, state1PressedSprite, ToggleButtonState.STATE1);
		SetSprites(state2UnpressedSprite, state2PressedSprite, ToggleButtonState.STATE2);
	}

	/// <summary>
	/// Creates a toggle button.
	/// </summary>
	/// <param name="state1TriggerDelegate">Delegate to call when button is switched to state 1.</param>
	/// <param name="state2TriggerDelegate">Delegate to call when button is switched to state 2.</param>
	/// <param name="triggerType">When is button "triggered"?</param>
	/// <param name="initialState">Starting state of toggle button.</param>
	public void Initialize(System.EventHandler<System.EventArgs> state1TriggerDelegate,
	                       System.EventHandler<System.EventArgs> state2TriggerDelegate,
	                       TriggerType triggerType = TriggerType.ON_PRESS,
	                       ToggleButtonState initialState = ToggleButtonState.STATE1)
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
			AddPressDelegate(state1TriggerDelegate, ToggleButtonState.STATE1);
			AddPressDelegate(state2TriggerDelegate, ToggleButtonState.STATE2);
			break;
		case TriggerType.ON_RELEASE:
			AddReleaseDelegate(state1TriggerDelegate, ToggleButtonState.STATE1);
			AddReleaseDelegate(state2TriggerDelegate, ToggleButtonState.STATE2);
			break;
		}

		m_toggleButtonState = initialState;
		// Apply button sprite for the starting state
		m_spriteRenderer.sprite = (initialState == ToggleButtonState.STATE1) ?
									m_state1UnpressedSprite :
									m_state2UnpressedSprite;

        // If element has an active anchor mode, update position on initialize
        UpdateScreenPosition();

        // Set the initialized flag
        m_isInitialized = true;
	}

	/// <summary>
	/// Adds a press delegate for the specified toggle button state.
	/// </summary>
	/// <returns><c>true</c>, if pressed delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="pressDelegate">Function delegate to call on button press.</param>
	/// <param name="state">Toggle button state the delegate would be used for.</param>
	public bool AddPressDelegate(System.EventHandler<System.EventArgs> pressDelegate, ToggleButtonState state)
	{
		switch (state)
		{
		case ToggleButtonState.STATE1:
		default:
			if (m_state1PressDelegateSet.Add(pressDelegate))
			{
				if (IsInState1)
				{
					m_pressGesture.Pressed += pressDelegate;
				}
				return true;
			}
			break;
		case ToggleButtonState.STATE2:
			if (m_state2PressDelegateSet.Add(pressDelegate))
			{
				if (IsInState2)
				{
					m_pressGesture.Pressed += pressDelegate;
				}
				return true;
			}
			break;
		}
		return false;
	}

	/// <summary>
	/// Adds a release delegate for the specified toggle button state.
	/// </summary>
	/// <returns><c>true</c>, if released delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="releaseDelegate">Function delegate to call on button release.</param>
	/// /// <param name="state">Toggle button state the delegate would be used for.</param>
	public bool AddReleaseDelegate(System.EventHandler<System.EventArgs> releaseDelegate, ToggleButtonState state)
	{
		if (state == ToggleButtonState.STATE1)
		{
			return m_state1ReleaseDelegateSet.Add(releaseDelegate);
		}
		return m_state2ReleaseDelegateSet.Add(releaseDelegate);
	}

	/// <summary>
	/// Removes a press delegate for the specified toggle button state.
	/// </summary>
	/// <returns><c>true</c>, if press delegate was removed, <c>false</c> otherwise.</returns>
	/// <param name="pressDelegate">Press delegate to remove.</param>
	/// <param name="state">State to remove delegate from.</param>
	public bool RemovePressDelegate(System.EventHandler<System.EventArgs> pressDelegate, ToggleButtonState state)
	{
		switch (state)
		{
		case ToggleButtonState.STATE1:
		default:
			if (m_state1PressDelegateSet.Remove(pressDelegate))
			{
				if (IsInState1)
				{
					m_pressGesture.Pressed -= pressDelegate;
				}
				return true;
			}
			break;
		case ToggleButtonState.STATE2:
			if (m_state2PressDelegateSet.Remove(pressDelegate))
			{
				if (IsInState2)
				{
					m_pressGesture.Pressed -= pressDelegate;
				}
				return true;
			}
			break;
		}
		return false;
	}

	/// <summary>
	/// Removes a release delegate for the specified toggle button state.
	/// </summary>
	/// <returns><c>true</c>, if release delegate was removed, <c>false</c> otherwise.</returns>
	/// <param name="releaseDelegate">Release delegate to remove.</param>
	/// <param name="state">State to remove delegate from.</param>
	public bool RemoveReleaseDelegate(System.EventHandler<System.EventArgs> releaseDelegate, ToggleButtonState state)
	{
		if (state == ToggleButtonState.STATE1)
		{
			return m_state1ReleaseDelegateSet.Remove(releaseDelegate);
		}
		return m_state2ReleaseDelegateSet.Remove(releaseDelegate);
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
		bool addedState1Sound = AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate, ToggleButtonState.STATE1);
		bool addedState2Sound = AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate, ToggleButtonState.STATE2);
		return addedState1Sound && addedState2Sound;
	}

	/// <summary>
	/// Adds button press and release sound delegates for the specified toggle button state.
	/// </summary>
	/// <returns><c>true</c>, if at least one sound delegate was added, <c>false</c> otherwise.</returns>
	/// <param name="pressSoundDelegate">Press sound delegate.</param>
	/// <param name="releaseSoundDelegate">Release sound delegate.</param>
	public bool AddSoundDelegates(System.EventHandler<System.EventArgs> pressSoundDelegate,
	                              System.EventHandler<System.EventArgs> releaseSoundDelegate,
	                              ToggleButtonState state)
	{
		bool delegateAdded = false;
		if (pressSoundDelegate != null && AddPressDelegate(pressSoundDelegate, state))
		{
			delegateAdded = true;
		}
		if (releaseSoundDelegate != null)
		{
			if (state == ToggleButtonState.STATE1)
			{
				m_state1ButtonReleaseSoundDelegate = releaseSoundDelegate;
			}
			else
			{
				m_state2ButtonReleaseSoundDelegate = releaseSoundDelegate;
			}
			delegateAdded = true;
		}
		return delegateAdded;
	}

	/// <summary>
	/// Sets button sprites for the specified toggle button state.
	/// </summary>
	/// <param name="state1UnpressedSprite">Unpressed sprite for state 1.</param>
	/// <param name="state1UnpressedSprite">Pressed sprite for state 1.</param>
	/// <param name="state">State to assign button sprites for.</param>
	public void SetSprites(Sprite unpressedSprite, Sprite pressedSprite, ToggleButtonState state)
	{
		if (state == ToggleButtonState.STATE1)
		{
			m_state1UnpressedSprite = unpressedSprite;
			m_state1PressedSprite = pressedSprite;
		}
		else
		{
			m_state2UnpressedSprite = unpressedSprite;
			m_state2PressedSprite = pressedSprite;
		}
	}

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
        m_spriteRenderer.sprite = (m_toggleButtonState == ToggleButtonState.STATE1) ?
                                    m_state1UnpressedSprite :
                                    m_state2UnpressedSprite;
        m_activeTouch = null;
        m_isPressed = false;
    }

	/// <summary>
	/// Deletes this instance.
	/// </summary>
	public void Delete()
	{
		// Unsubscribe from events
		switch (m_triggerType)
		{
		case TriggerType.ON_PRESS:
		default:
			if (IsInState1)		RemoveDelegatesFromPressGesture(m_state1PressDelegateSet);
			else 				RemoveDelegatesFromPressGesture(m_state2PressDelegateSet);
			// Clear press delegate sets
			m_state1PressDelegateSet.Clear();
			m_state2PressDelegateSet.Clear();
			break;
		case TriggerType.ON_RELEASE:
			// Clear release delegate sets
			m_state1ReleaseDelegateSet.Clear();
			m_state2ReleaseDelegateSet.Clear();
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

	/// <summary>
	/// Gets whether the toggle button is in state 1.
	/// </summary>
	public bool IsInState1
	{
		get { return m_toggleButtonState == ToggleButtonState.STATE1; }
	}

	/// <summary>
	/// Gets whether the toggle button is in state 2.
	/// </summary>
	public bool IsInState2
	{
		get { return m_toggleButtonState == ToggleButtonState.STATE2; }
	}

    #endregion // Public Interface

    #region Serialized Variables
    
    [Tooltip("Sprite when in unpressed state (in toggle button state 1)\n" +
             "Same as state 1 pressed sprite by default if left empty")]
    [SerializeField] protected	Sprite		m_state1UnpressedSprite	= null;
    [Tooltip("Sprite when in unpressed state (in toggle button state 2)\n" +
             "Same as state 2 pressed sprite by default if left empty")]
    [SerializeField] protected	Sprite		m_state2UnpressedSprite	= null;
    [Tooltip("Sprite when in pressed state (in toggle button state 1)")]
    [SerializeField] protected	Sprite		m_state1PressedSprite	= null;
    [Tooltip("Sprite when in pressed state (in toggle button state 2)")]
    [SerializeField] protected	Sprite		m_state2PressedSprite	= null;

	#endregion // Serialized Variables

	#region Trigger Type

	public enum TriggerType
	{
		ON_PRESS,
		ON_RELEASE
	}
	private TriggerType m_triggerType = TriggerType.ON_PRESS;

	#endregion // Trigger Type

	#region Toggle Button State

	public enum ToggleButtonState
	{
		STATE1,
		STATE2
	}
	private ToggleButtonState m_toggleButtonState = ToggleButtonState.STATE1;

	#endregion // Toggle Button State

	#region Delegates

	// Delegates for toggle button state 1
	private HashSet<System.EventHandler<System.EventArgs>> m_state1PressDelegateSet 	= new HashSet<System.EventHandler<System.EventArgs>>();
	private HashSet<System.EventHandler<System.EventArgs>> m_state1ReleaseDelegateSet 	= new HashSet<System.EventHandler<System.EventArgs>>();
	// Delegates for toggle button state 2
	private HashSet<System.EventHandler<System.EventArgs>> m_state2PressDelegateSet 	= new HashSet<System.EventHandler<System.EventArgs>>();
	private HashSet<System.EventHandler<System.EventArgs>> m_state2ReleaseDelegateSet 	= new HashSet<System.EventHandler<System.EventArgs>>();
	// Delegate for playing button sound on release in toggle button state 1
	private System.EventHandler<System.EventArgs> 	m_state1ButtonReleaseSoundDelegate 	= null;
	// Delegate for playing button sound on release in toggle button state 2
	private System.EventHandler<System.EventArgs> 	m_state2ButtonReleaseSoundDelegate 	= null;

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

		m_spriteRenderer.sprite = IsInState1 ? m_state1PressedSprite : m_state2PressedSprite;
		m_isPressed = true;

		// Find and store a reference to the touch on this button
		foreach (ITouch touch in m_pressGesture.ActiveTouches)
		{
			if (m_pressGesture.HasTouch(touch))
			{
				m_activeTouch = touch;
				break;
			}
		}
	}

	/// <summary>
	/// Raises the button release event.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="e">E.</param>
	private void OnButtonRelease(object sender, System.EventArgs e)
	{
		m_activeTouch = null;
		m_isPressed = false;

		if (IsInState1)
		{
			m_spriteRenderer.sprite = m_state1UnpressedSprite;
			// Play button release sound (if available)
			if (m_state1ButtonReleaseSoundDelegate != null)
			{
				m_state1ButtonReleaseSoundDelegate(this, new System.EventArgs());
			}
		}
		else
		{
			m_spriteRenderer.sprite = m_state2UnpressedSprite;
			// Play button release sound (if available)
			if (m_state2ButtonReleaseSoundDelegate != null)
			{
				m_state2ButtonReleaseSoundDelegate(this, new System.EventArgs());
			}
		}
	}

	/// <summary>
	/// Adds delegates to the press gesture.
	/// </summary>
	/// <param name="delegatesToAdd">Delegates to add.</param>
	private void AddDelegatesToPressGesture(HashSet<System.EventHandler<System.EventArgs>> delegatesToAdd)
	{
		foreach (System.EventHandler<System.EventArgs> delegateToAdd in delegatesToAdd)
		{
			m_pressGesture.Pressed += delegateToAdd;
		}
	}

	/// <summary>
	/// Removes delegates from the press gesture.
	/// </summary>
	/// <param name="delegatesToRemove">Delegates to remove.</param>
	private void RemoveDelegatesFromPressGesture(HashSet<System.EventHandler<System.EventArgs>> delegatesToRemove)
	{
		foreach (System.EventHandler<System.EventArgs> delegateToRemove in delegatesToRemove)
		{
			m_pressGesture.Pressed -= delegateToRemove;
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
			// On release, toggle between state 1 and state 2
			if (IsInState1)
			{
				// Toggle to state 2 (*before* calling release delegates)
				m_toggleButtonState = ToggleButtonState.STATE2;
				// Call release delegates for state 1
				foreach (System.EventHandler<System.EventArgs> releaseDelegate in m_state1ReleaseDelegateSet)
				{
					releaseDelegate(this, new System.EventArgs());
				}
				// Update press delegates for state 2
				RemoveDelegatesFromPressGesture(m_state1PressDelegateSet);
				AddDelegatesToPressGesture(m_state2PressDelegateSet);
			}
			else 
			{
				// Toggle to state 1 (*before* calling release delegates)
				m_toggleButtonState = ToggleButtonState.STATE1;
				// Call release delegates for state 2
				foreach (System.EventHandler<System.EventArgs> releaseDelegate in m_state2ReleaseDelegateSet)
				{
					releaseDelegate(this, new System.EventArgs());
				}
				// Update press delegates for state 1
				RemoveDelegatesFromPressGesture(m_state2PressDelegateSet);
				AddDelegatesToPressGesture(m_state1PressDelegateSet);
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

		AddPressDelegate(OnButtonPress, ToggleButtonState.STATE1);
		AddPressDelegate(OnButtonPress, ToggleButtonState.STATE2);
		AddReleaseDelegate(OnButtonRelease, ToggleButtonState.STATE1);
		AddReleaseDelegate(OnButtonRelease, ToggleButtonState.STATE2);
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
