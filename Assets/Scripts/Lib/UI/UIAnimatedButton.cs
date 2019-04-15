/******************************************************************************
*  @file       UIAnimatedButton.cs
*  @brief      UI class for an animated button element
*  @author     Ron
*  @date       October 4, 2015
*      
*  @par [explanation]
*		> UIButton with a set of sprites for unpressed and pressed states
*       > Sprites for each state are cycled through and applied to the button
*           every set interval
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIAnimatedButton : UIButton
{
    #region Public Interface

    /// <summary>
    /// Enables the button animation.
    /// </summary>
    /// <param name="resetToFirstSprite">if set to <c>true</c> button animation
    ///     begins from the first sprite.</param>
    public void EnableAnimation(bool resetToFirstSprite = true)
    {
        if (resetToFirstSprite)
        {
            ResetAnimation();
        }
        m_enableAnimation = true;
    }

    /// <summary>
    /// Disables the button animation.
    /// </summary>
    /// <param name="resetToFirstSprite">if set to <c>true</c> button is reset to the first sprite.</param>
    public void DisableAnimation(bool resetToFirstSprite = true)
    {
        if (resetToFirstSprite)
        {
            ResetAnimation();
        }
        m_enableAnimation = false;
    }

    /// <summary>
    /// Resets the button animation.
    /// </summary>
    public void ResetAnimation()
    {
        // Reset animation values
        m_lastUnpressedSpriteIndex = 0;
        m_lastPressedSpriteIndex = 0;
        m_timeSinceLastChange = 0.0f;

        // Reset button to the first sprite in the animation
        m_unpressedSprite = m_unpressedSpriteArray[m_lastUnpressedSpriteIndex];
        m_pressedSprite = m_pressedSpriteArray[m_lastPressedSpriteIndex];
        m_spriteRenderer.sprite = !IsPressed ? m_unpressedSprite : m_pressedSprite;
    }

    /// <summary>
    /// Resets this button to its default state.
    /// </summary>
    public override void Reset()
    {
        // Reset sprites to the first sprite in the array
        if (m_unpressedSpriteArray != null && m_unpressedSpriteArray.Length > 0)
        {
            m_unpressedSprite = m_unpressedSpriteArray[0];
        }
        if (m_pressedSpriteArray != null && m_pressedSpriteArray.Length > 0)
        {
            m_pressedSprite = m_pressedSpriteArray[0];
        }

        // UIButton's Reset will update the actual button texture
        base.Reset();
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Sprites when in \"inactive\" state. Same as pressed sprites by default if left empty")]
    [SerializeField] protected Sprite[] m_unpressedSpriteArray  = null;
    [Tooltip("Sprites when in \"active\" state")]
    [SerializeField] protected Sprite[] m_pressedSpriteArray    = null;
    [Tooltip("Intervals at which the button sprite is changed")]
    [SerializeField] protected float    m_animCycleInterval     = 0.5f;

    #endregion // Serialized Variables

    #region Animation

    private bool    m_enableAnimation           = true;

    private int     m_lastPressedSpriteIndex    = 0;
    private int     m_lastUnpressedSpriteIndex  = 0;
    private float   m_timeSinceLastChange       = 0.0f;

    /// <summary>
    /// Updates the button animation.
    /// </summary>
    private void UpdateButtonAnim()
    {
        if (!m_enableAnimation)
        {
            return;
        }

        // Make sure both arrays contain sprites
        if (m_unpressedSpriteArray == null || m_unpressedSpriteArray.Length == 0 ||
            m_pressedSpriteArray == null || m_pressedSpriteArray.Length == 0)
        {
            return;
        }

        // After every set interval, update the unpressed and pressed sprites of UIButton
        //  to the next ones in the respective sprite arrays from UIAnimatedButton
        m_timeSinceLastChange += Time.deltaTime;
        if (m_timeSinceLastChange > m_animCycleInterval)
        {
            // For the next animation cycle, use the next sprite in the array
            m_lastUnpressedSpriteIndex++;
            m_lastPressedSpriteIndex++;

            // If sprite index goes past the last sprite in the array, cycle back to the first sprite
            if (m_lastUnpressedSpriteIndex >= m_unpressedSpriteArray.Length)
            {
                m_lastUnpressedSpriteIndex = 0;
            }
            if (m_lastPressedSpriteIndex >= m_pressedSpriteArray.Length)
            {
                m_lastPressedSpriteIndex = 0;
            }

            // Update UIButton sprites
            m_unpressedSprite = m_unpressedSpriteArray[m_lastUnpressedSpriteIndex];
            m_pressedSprite = m_pressedSpriteArray[m_lastPressedSpriteIndex];
            m_spriteRenderer.sprite = !IsPressed ? m_unpressedSprite : m_pressedSprite;

            m_timeSinceLastChange = 0.0f;
        }
    }

    #endregion // Animation

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

        UpdateButtonAnim();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();

        ResetAnimation();
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
