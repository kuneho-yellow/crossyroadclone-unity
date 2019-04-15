/******************************************************************************
*  @file       UIAnimator.cs
*  @brief      Handles simple two-state animations for UI
*  @author     Ron
*  @date       September 16, 2015
*      
*  @par [explanation]
*		> Provides three kinds of animators: Transform, Color, and Text
*		 	Transform animator allows animation of position, rotation, and scale
*		 	Color animator allows animation of color and transparency
*           Text animator allows animation of font size
*		> How to use:
*			1. Create a UIAnimator instance by calling one of three constructors,
*				and passing in the object you need animated (either Transform, 
*               Renderer, or TextMesh)
*			2. Set values for the two animation states, e.g. 2 position values
*				if you're animating position
*			3. Set values for the animation speed (in speed, or time to animate)
*			4. Start the animation by calling either AnimateToState1 or
*				AnimateToState2
*		> NOTE: SpriteRenderer.color is different from Renderer.material.color.
*			Sprite objects ignore the value of Renderer.material.color. The
*			Color UIAnimator animates both of these different color fields.
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIAnimator
{
	#region Public Interface

	/// <summary>
	/// Initializes a Transform animator.
	/// </summary>
	public UIAnimator(Transform transformToAnimate) :
		this(transformToAnimate, UIAnimatorType.TRANSFORM, true, true)
	{
		// Empty
	}

	/// <summary>
	/// Initializes a Color animator.
	/// </summary>
	public UIAnimator(Renderer rendererToAnimate, bool includeChildren = true, bool includeInactive = true) :
		this(rendererToAnimate.transform, UIAnimatorType.COLOR, includeChildren, includeInactive)
	{
		// Empty
	}

    /// <summary>
    /// Initializes a Text animator.
    /// </summary>
    public UIAnimator(TextMesh textToAnimate, bool includeChildren = true, bool includeInactive = true) :
        this(textToAnimate.transform, UIAnimatorType.COLOR, includeChildren, includeInactive)
    {
        // Empty
    }

    /// <summary>
    /// Initializes a Transform, Color, or Text animator.
    /// </summary>
    public UIAnimator(Transform transformToAnimate, UIAnimatorType animatorType, bool includeChildren, bool includeInactive)
	{
		m_uiAnimatorType = animatorType;
		switch (animatorType)
		{
		case UIAnimatorType.TRANSFORM:
		default:
			m_transform = transformToAnimate;
			break;
		case UIAnimatorType.COLOR:
			m_spriteRenderers = includeChildren ? 
								transformToAnimate.GetComponentsInChildren<SpriteRenderer>(includeInactive) :
								new SpriteRenderer[]{ transformToAnimate.GetComponent<SpriteRenderer>() };
			m_renderers = includeChildren ?
						  transformToAnimate.GetComponentsInChildren<Renderer>(includeInactive) :
						  new Renderer[]{ transformToAnimate.GetComponent<Renderer>() };
			// Check if there are renderers to animate
			if (m_spriteRenderers.Length == 0 && m_renderers.Length == 0)
			{
				Debug.LogWarning("No sprite renderer(s) found");
			}
			break;
        case UIAnimatorType.TEXT:
            m_textMesh = transformToAnimate.GetComponent<TextMesh>();
            // Check if a text mesh is found
            if (m_textMesh == null)
            {
                Debug.LogWarning("No text mesh found");
            }
            break;
		}
		SetStartState(UIAnimationState.STATE1);
	}

	/// <summary>
	/// Sets the state the animation should start in.
	/// </summary>
	/// <param name="startState">Start state.</param>
	public void SetStartState(UIAnimationState startState)
	{
		m_uiAnimState = startState;
	}

	/// <summary>
	/// Starts animation from state 1 to state 2.
	/// </summary>
	public void AnimateToState2(bool resetToState1 = true)
	{
		if (resetToState1)
		{
			//ResetToState(UIAnimationState.STATE1);
			m_timeSinceAnimStart = 0.0f;
		}
		m_uiAnimState = UIAnimationState.TO_STATE2;
	}

	/// <summary>
	/// Starts animation from state 2 to state 1.
	/// </summary>
	public void AnimateToState1(bool resetToState2 = true)
	{
		if (resetToState2)
		{
			//ResetToState(UIAnimationState.STATE2);
			m_timeSinceAnimStart = 0.0f;
		}
		m_uiAnimState = UIAnimationState.TO_STATE1;
	}

    /// <summary>
    /// Resets the animation to state 1.
    /// </summary>
    public void ResetToState1()
    {
        ResetToState(UIAnimationState.STATE1);
    }

    /// <summary>
    /// Resets the animation to state 2.
    /// </summary>
    public void ResetToState2()
    {
        ResetToState(UIAnimationState.STATE2);
    }

    /// <summary>
    /// Resets the animation to the specified state.
    /// </summary>
    public void ResetToState(UIAnimationState state)
	{
		m_uiAnimState = state;
		switch (m_uiAnimatorType)
		{
		case UIAnimatorType.TRANSFORM:
		default:
			if (m_animatePosition)
			{
                if (m_animateLocal)
                {
                    m_transform.localPosition = (state == UIAnimationState.STATE1) ?
                                                m_animState1_position :
                                                m_animState2_position;
                }
                else
                {
                    m_transform.position = (state == UIAnimationState.STATE1) ?
                                            m_animState1_position :
                                            m_animState2_position;
                }
			}
			if (m_animateRotation)
			{
                if (m_animateLocal)
                {
                    m_transform.localEulerAngles = (state == UIAnimationState.STATE1) ?
                                                   m_animState1_rotation :
                                                   m_animState2_rotation;
                }
                else
                {
                    m_transform.eulerAngles = (state == UIAnimationState.STATE1) ?
                                              m_animState1_rotation :
                                              m_animState2_rotation;
                }
			}
			if (m_animateScale)
			{
				m_transform.localScale = (state == UIAnimationState.STATE1) ?
										 m_animState1_scale :
										 m_animState2_scale;
			}
			break;
		case UIAnimatorType.COLOR:
			if (m_animateAlpha)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.SetAlpha((state == UIAnimationState.STATE1) ?
											m_animState1_alpha :
											m_animState2_alpha);
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.SetAlpha((state == UIAnimationState.STATE1) ?
					                  m_animState1_alpha :
					                  m_animState2_alpha);
				}
			}
			if (m_animateColor)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.color = (state == UIAnimationState.STATE1) ?
										   m_animState1_color :
										   m_animState2_color;
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.material.color = (state == UIAnimationState.STATE1) ?
											  m_animState1_color :
											  m_animState2_color;
				}
			}
			break;
        case UIAnimatorType.TEXT:
            if (m_animateFontSize)
            {
                m_textMesh.fontSize = (state == UIAnimationState.STATE1) ?
									  m_animState1_fontSize :
									  m_animState2_fontSize;
            }
            break;
		}
	}

	/// <summary>
	/// Updates the UI animation.
	/// </summary>
	/// <param name="deltaTime">Delta time.</param>
	public void Update(float deltaTime)
	{
		UpdateUIAnimation(deltaTime);
	}

    /// <summary>
    /// Sets whether the transform animation should animate local values or global values (default global).
    /// Note: Scale animations always work on local scale, regardless of this method.
    /// </summary>
    /// <param name="animateLocal">if set to <c>true</c> animate local values. Else, global values.</param>
    public void SetAnimateLocal(bool animateLocal = true)
    {
        m_animateLocal = animateLocal;
    }

	/// <summary>
	/// Sets values for animating the position.
	/// </summary>
	/// <param name="positionInState1">Position in state 1.</param>
	/// <param name="positionInState2">Position in state 2.</param>
	public void SetPositionAnimation(Vector3 positionInState1, Vector3 positionInState2)
	{
		if (m_uiAnimatorType != UIAnimatorType.TRANSFORM)
		{
			if (BuildInfo.IsDebugMode)
			{
				Debug.LogWarning("Position animations can only be set in Transform animators");
			}
			return;
		}
		m_animatePosition = true;
		m_animState1_position = positionInState1;
		m_animState2_position = positionInState2;
        m_lerpAmount = Vector3.Distance(positionInState1, positionInState2);
	}

	/// <summary>
	/// Sets values for animating the rotation in euler angles.
	/// </summary>
	/// <param name="rotationInState1">Rotation in state 1.</param>
	/// <param name="rotationInState2">Rotation in state 2.</param>
	public void SetRotationAnimation(Vector3 rotationInState1, Vector3 rotationInState2)
	{
		if (m_uiAnimatorType != UIAnimatorType.TRANSFORM)
		{
			if (BuildInfo.IsDebugMode)
			{
				Debug.LogWarning("Rotation animations can only be set in Transform animators");
			}
			return;
		}
		m_animateRotation = true;
		m_animState1_rotation = rotationInState1;
		m_animState2_rotation = rotationInState2;
        m_lerpAmount = Vector3.Distance(rotationInState1, rotationInState2);
	}

	/// <summary>
	/// Sets values for animating the local scale.
	/// </summary>
	/// <param name="scaleInState1">Scale in state 1.</param>
	/// <param name="scaleInState2">Scale in state 2.</param>
	public void SetScaleAnimation(Vector3 scaleInState1, Vector3 scaleInState2)
	{
		if (m_uiAnimatorType != UIAnimatorType.TRANSFORM)
		{
			if (BuildInfo.IsDebugMode)
			{
				Debug.LogWarning("Scale animations can only be set in Transform animators");
			}
			return;
		}
		m_animateScale = true;
		m_animState1_scale = scaleInState1;
		m_animState2_scale = scaleInState2;
        m_lerpAmount = Vector3.Distance(scaleInState1, scaleInState2);
	}

	/// <summary>
	/// Sets values for animating the SpriteRenderer's transparency.
	/// </summary>
	/// <param name="alphaInState1">Alpha in state 1.</param>
	/// <param name="alphaInState2">Alpha in state 2.</param>
	public void SetAlphaAnimation(float alphaInState1, float alphaInState2)
	{
		if (m_uiAnimatorType != UIAnimatorType.COLOR)
		{
			if (BuildInfo.IsDebugMode)
			{
				Debug.LogWarning("Alpha animations can only be set in Color animators");
			}
			return;
		}
		m_animateAlpha = true;
		m_animState1_alpha = alphaInState1;
		m_animState2_alpha = alphaInState2;
        m_lerpAmount = Mathf.Abs(alphaInState1 - alphaInState2);
	}

	/// <summary>
	/// Sets values for animating the SpriteRenderer's color.
	/// </summary>
	/// <param name="alphaInState1">Color in state 1.</param>
	/// <param name="alphaInState2">Color in state 2.</param>
	public void SetColorAnimation(Color colorInState1, Color colorInState2)
	{
		if (m_uiAnimatorType != UIAnimatorType.COLOR)
		{
			if (BuildInfo.IsDebugMode)
			{
				Debug.LogWarning("Color animations can only be set in Color animators");
			}
			return;
		}
		m_animateColor = true;
		m_animState1_color = colorInState1;
		m_animState2_color = colorInState2;
        m_lerpAmount = Vector4.Distance(colorInState1, colorInState2);
	}

    /// <summary>
    /// Sets values for animating the TextMesh's font size.
    /// </summary>
    /// <param name="sizeInState1">Font size in state 1.</param>
    /// <param name="sizeInState2">Font size in state 2.</param>
    public void SetFontSizeAnimation(int sizeInState1, int sizeInState2)
    {
        if (m_uiAnimatorType != UIAnimatorType.TEXT)
        {
            if (BuildInfo.IsDebugMode)
            {
                Debug.LogWarning("Font size animations can only be set in Text animators");
            }
            return;
        }
        m_animateFontSize = true;
        m_animState1_fontSize = sizeInState1;
        m_animState2_fontSize = sizeInState2;
        m_lerpAmount = Mathf.Abs(sizeInState1 - sizeInState2);
    }

	/// <summary>
	/// Sets the animation speed.
	/// Overrides any previous SetAnimTime calls.
	/// </summary>
	/// <param name="animSpeed">Animation speed.</param>
	public void SetAnimSpeed(float animSpeed)
	{
		m_useAnimSpeed = true;
		m_animSpeed = animSpeed;
        // If lerp amount has been set, compute the animation time.
        // Else, compute it just before the actual lerp begins.
		if (animSpeed != 0.0f && m_lerpAmount != UNASSIGNED_VALUE)
		{
			m_animTime = m_lerpAmount / animSpeed;
		}
		// If animation speed is 0, animation stays in current state
	}

	/// <summary>
	/// Sets the time the animation should take to get from state to state.
	/// Overrides any previous SetAnimSpeed calls.
	/// </summary>
	/// <param name="animTime">Animation time.</param>
	public void SetAnimTime(float animTime)
	{
		m_useAnimSpeed = false;
		m_animTime = animTime;
        // If lerp amount has been set, compute the animation time.
        // Else, compute it just before the actual lerp begins.
        if (animTime != 0.0f && m_lerpAmount != UNASSIGNED_VALUE)
		{
			m_animSpeed = m_lerpAmount / animTime;
		}
		// If animation time is 0, animation will switch immediately to the target state
	}

	/// <summary>
	/// Removes all animations.
	/// </summary>
	public void RemoveAllAnimations()
	{
		RemovePositionAnimation();
		RemoveRotationAnimation();
		RemoveScaleAnimation();
		RemoveAlphaAnimation();
		RemoveColorAnimation();
        RemoveFontSizeAnimation();
		m_animSpeed = 0.0f;
		m_animTime = 0.0f;
	}

	/// <summary>
	/// Removes the position animation.
	/// </summary>
	public void RemovePositionAnimation()
	{
		m_animatePosition = false;
	}

	/// <summary>
	/// Removes the rotation animation.
	/// </summary>
	public void RemoveRotationAnimation()
	{
		m_animateRotation = false;
	}

	/// <summary>
	/// Removes the scale animation.
	/// </summary>
	public void RemoveScaleAnimation()
	{
		m_animateScale = false;
	}

	/// <summary>
	/// Removes the color animation.
	/// </summary>
	public void RemoveColorAnimation()
	{
		m_animateColor = false;
	}

	/// <summary>
	/// Removes the alpha animation.
	/// </summary>
	public void RemoveAlphaAnimation()
	{
		m_animateAlpha = false;
	}

    /// <summary>
	/// Removes the font size animation.
	/// </summary>
	public void RemoveFontSizeAnimation()
    {
        m_animateFontSize = false;
    }

    /// <summary>
    /// Pauses the UI animation.
    /// </summary>
    public void Pause()
	{
		m_uiAnimStateBeforePause = m_uiAnimState;
	}

	/// <summary>
	/// Unpauses the UI animation.
	/// </summary>
	public void Unpause()
	{
		m_uiAnimState = m_uiAnimStateBeforePause;
	}

	/// <summary>
	/// Gets the UI animation state.
	/// </summary>
	public UIAnimationState UIAnimState
	{
		get { return m_uiAnimState; }
	}

	/// <summary>
	/// Gets whether animator has any active animations.
	/// </summary>
	public bool IsActive
	{
		get { return m_animatePosition ||
					 m_animateRotation ||
					 m_animateScale ||
					 m_animateAlpha ||
					 m_animateColor ||
                     m_animateFontSize; }
	}

	/// <summary>
	/// Gets whether this UI is currently animating.
	/// </summary>
	public bool IsAnimating
	{
		get { return m_uiAnimState == UIAnimationState.TO_STATE1 ||
					 m_uiAnimState == UIAnimationState.TO_STATE2; }
	}

	/// <summary>
	/// Gets whether animation is in State 1.
	/// </summary>
	public bool IsInState1
	{
		get { return m_uiAnimState == UIAnimationState.STATE1; }
	}

	/// <summary>
	/// Gets whether animation is in State 2.
	/// </summary>
	public bool IsInState2
	{
		get { return m_uiAnimState == UIAnimationState.STATE2; }
	}

	#endregion // Public Interface

	#region UI Animator Type

	public enum UIAnimatorType
	{
		TRANSFORM,
		COLOR,
        TEXT
	}
	private UIAnimatorType m_uiAnimatorType = UIAnimatorType.TRANSFORM;

	#endregion // UI Animator Type

	#region Animations

	// Transform
	private Transform	m_transform				= null;
    private bool        m_animateLocal          = false;
	// Position (world position)
	private bool		m_animatePosition		= false;
	private Vector3 	m_animState1_position	= Vector3.zero;
	private Vector3 	m_animState2_position	= Vector3.zero;
	// Rotation (euler angles)
	private bool		m_animateRotation		= false;
	private Vector3 	m_animState1_rotation 	= Vector3.zero;
	private Vector3 	m_animState2_rotation 	= Vector3.zero;
	// Scale (local scale)
	private bool		m_animateScale			= false;
	private Vector3 	m_animState1_scale		= Vector3.zero;
	private Vector3 	m_animState2_scale		= Vector3.zero;

	// Renderer
	private	SpriteRenderer[] m_spriteRenderers	= null;	// Used specifically for sprites
	private Renderer[]	m_renderers				= null;
	// Alpha
	private bool		m_animateAlpha			= false;
	private float 		m_animState1_alpha		= 0.0f;
	private float 		m_animState2_alpha		= 0.0f;
	// Color
	private bool		m_animateColor 			= false;
	private Color		m_animState1_color 		= Color.white;
	private Color		m_animState2_color 		= Color.white;

    // Text
    private TextMesh    m_textMesh              = null;
    private bool        m_animateFontSize       = false;
    private int         m_animState1_fontSize   = 0;
    private int         m_animState2_fontSize   = 0;

	private float 		m_timeSinceAnimStart 	= 0.0f;
	private bool		m_useAnimSpeed			= false;
	private float		m_animSpeed				= UNASSIGNED_VALUE;
    private float		m_animTime				= UNASSIGNED_VALUE;
    private float       m_lerpAmount            = UNASSIGNED_VALUE;

    private const float UNASSIGNED_VALUE        = Mathf.Infinity;

    public enum UIAnimationState
	{
		NONE,
		TO_STATE1,
		STATE1,
		TO_STATE2,
		STATE2
	}
	private UIAnimationState m_uiAnimState = UIAnimationState.NONE;

	private UIAnimationState m_uiAnimStateBeforePause = UIAnimationState.NONE;

	/// <summary>
	/// Updates the UI animation.
	/// </summary>
	private void UpdateUIAnimation(float deltaTime)
    {
        if (!IsActive || (m_useAnimSpeed && m_animSpeed == 0.0f))
        {
            return;
        }

        // Make sure to use updated values
        if (m_useAnimSpeed && m_animTime == UNASSIGNED_VALUE)
		{
            // s = d/t, t = d/s -> Animation time = amount to lerp / animation speed
            if (m_lerpAmount != UNASSIGNED_VALUE)
            {
                m_animTime = m_lerpAmount / m_animSpeed;
            }
            // If lerp amount is unassigned, i.e. start and end values have not been set, return
            else
            {
                return;
            }
        }

		switch (m_uiAnimState)
		{
		case UIAnimationState.TO_STATE1:
			// Animate from state 2 to state 1
			m_timeSinceAnimStart += deltaTime;
            float percentToState1 = (m_animTime != 0.0f) ? (m_timeSinceAnimStart / m_animTime) : 1.0f;
            
			if (m_animatePosition)
			{
                Vector3 newPos = Vector3.Lerp(m_animState2_position, m_animState1_position, percentToState1);
                if (m_animateLocal)     m_transform.localPosition = newPos;
                else                    m_transform.position = newPos;
			}
			if (m_animateRotation)
			{
                Vector3 newRot = Vector3.Lerp(m_animState2_rotation, m_animState1_rotation, percentToState1);
                if (m_animateLocal)     m_transform.localEulerAngles = newRot;
                else                    m_transform.eulerAngles = newRot;
            }
			if (m_animateScale)
			{
				m_transform.localScale = Vector3.Lerp(m_animState2_scale, m_animState1_scale, percentToState1);
			}
			if (m_animateAlpha)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.SetAlpha(Mathf.Lerp(m_animState2_alpha, m_animState1_alpha, percentToState1));
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.SetAlpha(Mathf.Lerp(m_animState2_alpha, m_animState1_alpha, percentToState1));
				}
			}
			if (m_animateColor)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.color = Color.Lerp(m_animState2_color, m_animState1_color, percentToState1);
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.material.color = Color.Lerp(m_animState2_color, m_animState1_color, percentToState1);
				}
			}
            if (m_animateFontSize)
			{
				m_textMesh.fontSize = (int)Mathf.Lerp(m_animState2_fontSize, m_animState1_fontSize, percentToState1);
			}
			// Check if animation is done
			if (m_timeSinceAnimStart > m_animTime)
			{
				m_timeSinceAnimStart = 0.0f;
				m_uiAnimState = UIAnimationState.STATE1;
			}
			break;
		case UIAnimationState.STATE1:
			// Now in animation state 1
			break;

		case UIAnimationState.TO_STATE2:
			// Animate from state 1 to state 2
			m_timeSinceAnimStart += deltaTime;
            float percentToState2 = (m_animTime != 0.0f) ? (m_timeSinceAnimStart / m_animTime) : 1.0f;
            
			if (m_animatePosition)
			{
                Vector3 newPos = Vector3.Lerp(m_animState1_position, m_animState2_position, percentToState2);
                if (m_animateLocal)     m_transform.localPosition = newPos;
                else                    m_transform.position = newPos;
			}
			if (m_animateRotation)
			{
                Vector3 newRot = Vector3.Lerp(m_animState1_rotation, m_animState2_rotation, percentToState2);
                if (m_animateLocal)     m_transform.localEulerAngles = newRot;
                else                    m_transform.eulerAngles = newRot;
            }
			if (m_animateScale)
			{
				m_transform.localScale = Vector3.Lerp(m_animState1_scale, m_animState2_scale, percentToState2);
			}
			if (m_animateAlpha)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.SetAlpha(Mathf.Lerp(m_animState1_alpha, m_animState2_alpha, percentToState2));
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.SetAlpha(Mathf.Lerp(m_animState1_alpha, m_animState2_alpha, percentToState2));
				}
			}
			if (m_animateColor)
			{
				foreach (SpriteRenderer spriteRenderer in m_spriteRenderers)
				{
					spriteRenderer.color = Color.Lerp(m_animState1_color, m_animState2_color, percentToState2);
				}
				foreach (Renderer renderer in m_renderers)
				{
					renderer.material.color = Color.Lerp(m_animState1_color, m_animState2_color, percentToState2);
				}
			}
            if (m_animateFontSize)
			{
				m_textMesh.fontSize = (int)Mathf.Lerp(m_animState1_fontSize, m_animState2_fontSize, percentToState2);
			}
			// Check if animation is done
			if (m_timeSinceAnimStart > m_animTime)
			{
				m_timeSinceAnimStart = 0.0f;
				m_uiAnimState = UIAnimationState.STATE2;
			}
			break;
		case UIAnimationState.STATE2:
		default:
			// Now in animation state 2
			break;
		}
	}

	#endregion // Animations
}
