/******************************************************************************
*  @file       ScoreUI.cs
*  @brief      Handles the score UI
*  @author     Ron
*  @date       October 3, 2015
*      
*  @par [explanation]
*		> Sets the score and top score text
*       > Animates the score text when reaching multiples of 50 score
*       > Animates and displays the top score text on death
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class ScoreUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Initialize text
        m_scoreText.Initialize();
        m_topScoreText.Initialize();
        // Initialize score text positions
        m_scoreContainer.UpdateScreenPosition();

        // Initialize animators
        // Score
        //  State 1: Normal size (during normal play)
        //  State 2: Larger size (when reaching multiples of 50 score or when in results)
        m_scoreAnimator = new UIAnimator(m_scoreText.transform);
        m_scoreAnimator.SetScaleAnimation(Vector3.one * m_scoreTextSize1,
                                          Vector3.one * m_scoreTextSize2);
        m_scoreAnimator.SetAnimSpeed(m_scoreTextAnimSpeed);
        m_scoreAnimator.ResetToState(UIAnimator.UIAnimationState.STATE1);
        // Top score
        //  State 1: Hidden at the left edge of the screen (during normal play)
        //  State 2: Below score text (when in results)
        m_topScoreAnimator = new UIAnimator(m_topScoreText.transform);
        Vector3 state2Pos = m_topScoreText.transform.localPosition;
        Vector3 state1Pos = new Vector3(m_topScoreHiddenPos, state2Pos.y, state2Pos.z); 
        m_topScoreAnimator.SetPositionAnimation(state1Pos, state2Pos);
        m_topScoreAnimator.SetAnimateLocal(true);
        m_topScoreAnimator.SetAnimSpeed(m_topScoreTextAnimSpeed);
        m_topScoreAnimator.ResetToState(UIAnimator.UIAnimationState.STATE1);

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets the score display.
    /// </summary>
    /// <param name="score">The score to display</param>
    public void SetScore(int score)
    {
        m_scoreText.SetText(score.ToString());
    }

    /// <summary>
    /// Sets the top score display.
    /// </summary>
    /// <param name="topScore">The top score to display</param>
    public void SetTopScore(int topScore, bool isNewTop)
    {
        string topScoreStr = !isNewTop ? "TOP " + topScore.ToString() : "NEW TOP";
        m_topScoreText.SetText(topScoreStr);
    }

    /// <summary>
    /// Starts the enlarge and shrink animation of the score text.
    /// </summary>
    public void StartEnlargeAndShrinkAnim()
    {
        // Start animation
        m_scoreAnimator.AnimateToState2();
        m_scoreAnimState = ScoreAnimState.Enlarging;
    }

    /// <summary>
    /// Enlarges the score text.
    /// </summary>
    public void EnlargeScoreText()
    {
        m_scoreAnimator.AnimateToState2();
    }

    /// <summary>
    /// Shrinks the score text.
    /// </summary>
    public void ShrinkScoreText()
    {
        m_scoreAnimator.AnimateToState1();
    }

    /// <summary>
    /// Scrolls the top score text into view under the score text.
    /// </summary>
    public void ScrollInTopScoreText()
    {
        m_topScoreAnimator.AnimateToState2();
    }

    /// <summary>
    /// Shows the score UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_scoreText.UpdateScreenPosition();
        m_topScoreText.UpdateScreenPosition();
        m_scoreContainer.UpdateScreenPosition();

        m_scoreRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the score UI.
    /// </summary>
    public void Hide()
    {
        m_scoreRoot.SetActive(false);
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

        m_scoreAnimator.Pause();
        m_topScoreAnimator.Pause();

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

        m_scoreAnimator.Unpause();
        m_topScoreAnimator.Unpause();

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        if (m_scoreAnimator != null)
        {
            m_scoreAnimator.ResetToState(UIAnimator.UIAnimationState.STATE1);
        }
        if (m_topScoreAnimator != null)
        {
            m_topScoreAnimator.ResetToState(UIAnimator.UIAnimationState.STATE1);
        }
        m_timeSinceEnlarged = 0.0f;
        m_scoreAnimState = ScoreAnimState.Normal;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        m_scoreText.Delete();
        m_topScoreText.Delete();
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

    [SerializeField] private GameObject     m_scoreRoot         = null;
    [SerializeField] private UIContainer    m_scoreContainer    = null;
    [SerializeField] private UIText         m_scoreText         = null;
    [SerializeField] private UIText         m_topScoreText      = null;

    [Tooltip("Text size while playing in-game")]
    [SerializeField] private float      m_scoreTextSize1        = 0.5f;
    [Tooltip("Text size when reaching multiples of 50 score and when in results")]
    [SerializeField] private float      m_scoreTextSize2        = 1.0f;
    [Tooltip("Horizontal position of top score text when off-screen")]
    [SerializeField] private float      m_topScoreHiddenPos     = -7.5f;
    [Tooltip("Speed of the score text's enlarge and shrink animation")]
    [SerializeField] private float      m_scoreTextAnimSpeed    = 5.0f;
    [Tooltip("Speed of the top score text's scroll-in animation")]
    [SerializeField] private float      m_topScoreTextAnimSpeed = 5.0f;
    [Tooltip("Duration that the score text is held enlarged during the enlarge-shrink animation")]
    [SerializeField] private float      m_enlargeHoldDuration   = 0.5f;

    #endregion // Serialized Variables

    #region Variables
    
    private bool m_isInitialized    = false;
    private bool m_isPaused         = false;

    #endregion // Variables

    #region Animation

    private enum ScoreAnimState
    {
        Normal,
        Enlarging,
        Enlarged,
        Shrinking
    }
    private ScoreAnimState m_scoreAnimState = ScoreAnimState.Normal;

    private UIAnimator  m_scoreAnimator     = null;
    private UIAnimator  m_topScoreAnimator  = null;

    private float   m_timeSinceEnlarged     = 0.0f;

    /// <summary>
    /// Updates the score text animation.
    /// </summary>
    private void UpdateScoreTextAnim()
    {
        m_scoreAnimator.Update(Time.deltaTime);

        switch (m_scoreAnimState)
        {
            case ScoreAnimState.Normal:
                break;
            case ScoreAnimState.Enlarging:
                if (m_scoreAnimator.IsInState2)
                {
                    m_timeSinceEnlarged = 0.0f;
                    m_scoreAnimState = ScoreAnimState.Enlarged;
                }
                break;
            case ScoreAnimState.Enlarged:
                // Track time in enlarged state
                m_timeSinceEnlarged += Time.deltaTime;
                // Once it reaches the required hold time, start shrinking
                if (m_timeSinceEnlarged > m_enlargeHoldDuration)
                {
                    m_timeSinceEnlarged = 0.0f;
                    m_scoreAnimator.AnimateToState1();
                    m_scoreAnimState = ScoreAnimState.Shrinking;
                }
                break;
            case ScoreAnimState.Shrinking:
                // Check if coins text has shrunk back to normal size
                if (m_scoreAnimator.IsInState1)
                {
                    // End animation
                    m_scoreAnimState = ScoreAnimState.Normal;
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

        // Update animators
        UpdateScoreTextAnim();
        m_topScoreAnimator.Update(Time.deltaTime);
    }

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
