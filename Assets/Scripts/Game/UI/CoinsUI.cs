/******************************************************************************
*  @file       CoinsUI.cs
*  @brief      Handles the coins UI
*  @author     Ron
*  @date       October 3, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class CoinsUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Initialize text
        m_coinsText.Initialize();
        m_coinsText.UpdateScreenPosition();

        // Initialize coins animator
        //  State 1: Normal size (during normal play)
        //  State 2: Larger size (when reaching multiples of 100 coins)
        m_coinsAnimator = new UIAnimator(m_coinsAnimRoot.transform);
        m_coinsAnimator.SetScaleAnimation(Vector3.one * m_coinsTextSize1,
                                          Vector3.one * m_coinsTextSize2);
        m_coinsAnimator.SetPositionAnimation(m_coinsAnimRoot.localPosition, m_coinsTextPos2);
        m_coinsAnimator.SetAnimateLocal();
        m_coinsAnimator.SetAnimSpeed(m_coinsAnimSpeed);
        m_coinsAnimator.ResetToState1();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets the coins display.
    /// </summary>
    /// <param name="coins">Coin count to display</param>
    public void SetCoins(int coins)
    {
        m_coins = coins;
        m_coinsText.SetText(m_coins.ToString());
    }

    /// <summary>
    /// Adds one to the coin display.
    /// </summary>
    public void AddOneCoin()
    {
        SetCoins(m_coins + 1);
    }

    /// <summary>
    /// Starts the enlarge and shrink animation of the coins text.
    /// </summary>
    public void StartEnlargeAndShrinkAnim()
    {
        // Start animation
        m_coinsAnimator.AnimateToState2();
        m_coinsAnimState = CoinsAnimState.Enlarging;
    }
    
    /// <summary>
    /// Shows the coins UI.
    /// </summary>
    public void Show()
    {
        // Update position
        m_coinsText.UpdateScreenPosition();

        m_coinsRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the coins UI.
    /// </summary>
    public void Hide()
    {
        m_coinsRoot.SetActive(false);
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

        m_coinsAnimator.Pause();

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

        m_coinsAnimator.Unpause();

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        if (m_coinsAnimator != null)
        {
            m_coinsAnimator.ResetToState(UIAnimator.UIAnimationState.STATE1);
        }
        m_timeSinceEnlarged = 0.0f;
        m_coinsAnimState = CoinsAnimState.Normal;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        m_coinsText.Delete();
        GameObject.Destroy(this.gameObject);
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

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject m_coinsRoot         = null;

    [Tooltip("Root object used to animate both the coins text and the coin unit sprite")]
    [SerializeField] private Transform  m_coinsAnimRoot     = null;
    [Tooltip("Text object that displays the coin amount")]
    [SerializeField] private UIText     m_coinsText         = null;
    [Tooltip("Text size while playing in-game")]
    [SerializeField] private float      m_coinsTextSize1    = 1.0f;
    [Tooltip("Text size when reaching multiples of 100 coins")]
    [SerializeField] private float      m_coinsTextSize2    = 1.8f;
    [Tooltip("Text position when in enlarged form")]
    [SerializeField] private Vector3    m_coinsTextPos2     = new Vector3(-0.6f, 0.6f, 0.0f);
    [Tooltip("Coin animation speed")]
    [SerializeField] private float      m_coinsAnimSpeed    = 5.0f;
    [Tooltip("Duration that the coins text is held enlarged during the enlarge-shrink animation")]
    [SerializeField] private float      m_enlargeHoldDuration = 0.5f;

    #endregion // Serialized Variables

    #region Variables
    
    private bool m_isInitialized    = false;
    private bool m_isPaused         = false;

    private int  m_coins            = 0;

    #endregion // Variables

    #region Animation

    private enum CoinsAnimState
    {
        Normal,
        Enlarging,
        Enlarged,
        Shrinking
    }
    private CoinsAnimState m_coinsAnimState = CoinsAnimState.Normal;

    private UIAnimator  m_coinsAnimator     = null;
    
    private float   m_timeSinceEnlarged     = 0.0f;

    /// <summary>
    /// Updates the coins text animation.
    /// </summary>
    private void UpdateCoinsTextAnim()
    {
        m_coinsAnimator.Update(Time.deltaTime);

        switch (m_coinsAnimState)
        {
            case CoinsAnimState.Normal:
                break;
            case CoinsAnimState.Enlarging:
                if (m_coinsAnimator.IsInState2)
                {
                    m_timeSinceEnlarged = 0.0f;
                    m_coinsAnimState = CoinsAnimState.Enlarged;
                }
                break;
            case CoinsAnimState.Enlarged:
                // Track time in enlarged state
                m_timeSinceEnlarged += Time.deltaTime;
                // Once it reaches the required hold time, start shrinking
                if (m_timeSinceEnlarged > m_enlargeHoldDuration)
                {
                    m_timeSinceEnlarged = 0.0f;
                    m_coinsAnimator.AnimateToState1();
                    m_coinsAnimState = CoinsAnimState.Shrinking;
                }
                break;
            case CoinsAnimState.Shrinking:
                // Check if coins text has shrunk back to normal size
                if (m_coinsAnimator.IsInState1)
                {
                    // End animation
                    m_coinsAnimState = CoinsAnimState.Normal;
                }
                break;
        }
    }

    #endregion // Animation

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
		if (!m_isInitialized)
        {
            return;
        }
        
        UpdateCoinsTextAnim();
    }

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
