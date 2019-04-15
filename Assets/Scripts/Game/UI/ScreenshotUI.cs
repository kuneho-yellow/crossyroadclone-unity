/******************************************************************************
*  @file       ScreenshotUI.cs
*  @brief      Handles the screenshot UI
*  @author     Ron
*  @date       October 13, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class ScreenshotUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(System.EventHandler<System.EventArgs> screenshotScreenTouchDelegate)
    {
        // Initialize screenshot screen (giant button)
        m_screenshotScreen.Initialize(screenshotScreenTouchDelegate, UIButton.TriggerType.ON_PRESS);

        // Create animator instances
        m_scaleUpAnimator = new UIAnimator(m_screenshotImageRoot.transform);
        m_slideAnimator = new UIAnimator(m_screenshotImageRoot.transform);
        m_scaleDownAnimator = new UIAnimator(m_screenshotImageRoot.transform);
        m_stillShotAnimator = new UIAnimator(m_screenshotImageRoot.transform);

        // Initialize scale animators
        //  State 1: Zero scale (at the start when screenshot is just taken,
        //              and at the end when screenshot is hidden in the share button)
        //  State 2: Normal scale
        m_scaleUpAnimator.SetScaleAnimation(Vector3.zero, Vector3.one * m_animatedScreenshotScale);
        m_scaleUpAnimator.SetAnimTime(m_scaleAnimTime);
        //  [States are the reverse of scale up animator's]
        m_scaleDownAnimator.SetScaleAnimation(Vector3.one * m_animatedScreenshotScale, Vector3.zero);
        m_scaleDownAnimator.SetAnimTime(m_scaleAnimTime);

        // Initialize position and rotation animator
        m_slideAnimator.SetAnimTime(m_slideAnimTime);
        // Start and end values are set when the animation is started

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts the screenshot anim.
    /// </summary>
    /// <param name="startPos">Start position, typically at the position the screenshot was taken.</param>
    /// <param name="endPos">The position where the screenshot will "hide" in (i.e. scale down to 0).</param>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void StartScreenshotAnim(Vector2 startPos, Vector2 endPos, Texture2D screenshotTexture = null)
    {
        // Get the start position
        m_slideStartPos = new Vector3(startPos.x, startPos.y, SCREENSHOT_POS_Z);
        // Get a random end position within a certain distance from the start position
        Vector3 dir = new Vector3(Random.value, Random.value, 0.0f);
        m_slideEndPos = m_slideStartPos + dir * Random.Range(-m_maxAnimDeltaPos, m_maxAnimDeltaPos);
        
        // Initialize position animation using the specified start position and the random end position
        m_slideAnimator.SetPositionAnimation(m_slideStartPos, m_slideEndPos);
        // Initialize rotation animation, starting from 0 tilt to a random rotation in z
        m_slideAnimator.SetRotationAnimation(Vector3.zero, GetRandomScreenshotTilt());

        // Add position animation to the scale down animator, from the slide end pos to the specified anim end pos
        Vector3 animEndPos = new Vector3(endPos.x, endPos.y, SCREENSHOT_POS_Z);
        m_scaleDownAnimator.SetPositionAnimation(m_slideEndPos, animEndPos);

        SetScreenshotTexture(screenshotTexture);

        // Show UI
        m_screenshotRoot.SetActive(true);

        // Play screenshot sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.Screenshot);

        // Reset animation to state 1
        m_slideAnimator.ResetToState1();
        m_scaleUpAnimator.ResetToState1();

        // Start animation
        m_scaleUpAnimator.AnimateToState2();
        m_state = ScreenshotAnimState.SCALE_UP;
    }

    /// <summary>
    /// Shows the screenshot at the center of the screen.
    /// </summary>
    public void ShowStillScreenshot(Vector3 startPos, Texture2D screenshotTexture = null)
    {
        // If the screenshot animation is playing, wait for it to finish first
        if (m_state != ScreenshotAnimState.IDLE)
        {
            return;
        }

        // Enable screenshot "screen"
        m_screenshotScreen.gameObject.SetActive(true);

        // If animation is still playing, just play "show" animation without resetting to state 1
        if (m_stillShotAnimator.IsAnimating)
        {
            m_stillShotAnimator.AnimateToState2(false);
            return;
        }

        startPos.z = SCREENSHOT_POS_Z;
        SetScreenshotTexture(screenshotTexture);

        // Give screenshot a random tilt
        m_screenshotImageRoot.transform.eulerAngles = GetRandomScreenshotTilt();

        // Set values for the show/hide screenshot animation
        //  State 1: Hidden in share button
        //  State 2: Shown at the center of the screen
        m_stillShotAnimator.SetPositionAnimation(startPos, Vector3.zero);
        m_stillShotAnimator.SetScaleAnimation(Vector3.zero, Vector3.one * m_stillScreenshotScale);
        m_stillShotAnimator.SetAnimTime(m_scaleAnimTime);
        m_stillShotAnimator.ResetToState1();

        // Show screenshot
        Show();
        m_screenshotImageRoot.SetActive(true);

        // Start animation
        m_stillShotAnimator.AnimateToState2(true);
    }

    /// <summary>
    /// Updates the screenshot animations' positions.
    /// </summary>
    /// <param name="animPos">The anim position.</param>
    public void UpdateAnimPos(Vector3 animPos)
    {
        Vector3 animStartPos = new Vector3(animPos.x, animPos.y, SCREENSHOT_POS_Z);
        m_stillShotAnimator.SetPositionAnimation(animStartPos, Vector3.zero);

        Vector3 animEndPos = new Vector3(animPos.x, animPos.y, SCREENSHOT_POS_Z);
        m_slideAnimator.SetPositionAnimation(m_slideStartPos, m_slideEndPos);
        m_scaleDownAnimator.SetPositionAnimation(m_slideEndPos, animEndPos);
    }

    /// <summary>
    /// Hides the still screenshot.
    /// </summary>
    public void HideStillScreenshot()
    {
        // If the screenshot animation is playing, wait for it to finish first
        if (m_state != ScreenshotAnimState.IDLE)
        {
            return;
        }

        // Disable screenshot "screen"
        m_screenshotScreen.gameObject.SetActive(false);

        m_stillShotAnimator.AnimateToState1(false); 
    }

    /// <summary>
    /// Sets the screenshot texture.
    /// </summary>
    /// <param name="screenshotTexture">The screenshot texture.</param>
    public void SetScreenshotTexture(Texture2D screenshotTexture)
    {
        // Apply screenshot texture if specified. Else, show the last used screenshot.
        if (screenshotTexture != null)
        {
            // Convert texture to sprite
            Rect spriteRect = new Rect(0.0f, 0.0f, screenshotTexture.width, screenshotTexture.height);
            Vector2 spritePivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(screenshotTexture, spriteRect, spritePivot);
            // Apply sprite
            m_screenImageRenderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Shows the screenshot UI.
    /// </summary>
    public void Show()
    {
        m_screenshotRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the screenshot UI.
    /// </summary>
    public void Hide()
    {
        m_screenshotRoot.SetActive(false);
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

        m_scaleUpAnimator.Pause();
        m_slideAnimator.Pause();

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

        m_scaleUpAnimator.Unpause();
        m_slideAnimator.Unpause();

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_screenshotRoot.SetActive(true);

        // Default state is screenshot image at the center of the screen, random tilt, and normal scale
        m_screenshotImageRoot.SetActive(true);
        m_screenshotImageRoot.transform.position = Vector3.zero;
        m_screenshotImageRoot.transform.eulerAngles = GetRandomScreenshotTilt();
        m_screenshotImageRoot.transform.localScale = Vector3.one * m_stillScreenshotScale;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
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

    /// <summary>
    /// Gets whether the screenshot UI is shown.
    /// </summary>
    public bool IsShown
    {
        get { return m_screenshotRoot.activeInHierarchy; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject     m_screenshotRoot        = null;
    [SerializeField] private GameObject     m_screenshotImageRoot   = null;
    [Tooltip("Part of the screenshot image that fits in the screenshot frame")]
    [SerializeField] private SpriteRenderer m_screenImageRenderer   = null;
    [Tooltip("Invisible giant button that detects when screenshot UI should be hidden")]
    [SerializeField] private UIButton       m_screenshotScreen      = null;
    [Tooltip("Camera used for rendering the screenshot image in screenshot UI")]
    [SerializeField] private Camera         m_screenshotCamera      = null;

    [Tooltip("Time for screenshot to get from zero to normal scale (and back) during the scale animation")]
    [SerializeField] private float      m_scaleAnimTime             = 0.3f;
    [Tooltip("Time for screenshot to get from start to end position/rotation during the slide animation")]
    [SerializeField] private float      m_slideAnimTime             = 1.5f;
    [Tooltip("Maximum amount the screenshot can move while it is displayed during the slide animation")]
    [SerializeField] private float      m_maxAnimDeltaPos           = 2.0f;
    [Tooltip("Maximum amount the screenshot can rotate while it is displayed during the slide animation")]
    [SerializeField] private float      m_maxAnimDeltaRot           = 5.0f;
    [Tooltip("Screenshot (max) scale when in animated mode (e.g. after getting top/great score)")]
    [SerializeField] private float      m_animatedScreenshotScale   = 0.8f;
    [Tooltip("Screenshot scale when in still mode (e.g. when share button is tapped)")]
    [SerializeField] private float      m_stillScreenshotScale      = 1.0f;

    #endregion // Serialized Variables

    #region Variables

    private bool        m_isInitialized    = false;
    private bool        m_isPaused         = false;

    #endregion // Variables

    #region Animation

    // Animators for the initial animation when screenshot is taken on game over
    private UIAnimator  m_scaleUpAnimator   = null;
    private UIAnimator  m_slideAnimator     = null;
    private UIAnimator  m_scaleDownAnimator = null;
    // Animator for showing/hiding a still screenshot
    private UIAnimator  m_stillShotAnimator = null;
    
    private Vector3     m_slideStartPos     = Vector3.zero;
    private Vector3     m_slideEndPos       = Vector3.zero;

    private const float SCREENSHOT_POS_Z    = 0.0f;

    private enum ScreenshotAnimState
    {
        IDLE,
        SCALE_UP,
        SLIDE,
        SCALE_DOWN
    }
    private ScreenshotAnimState m_state = ScreenshotAnimState.IDLE;

    /// <summary>
    /// Updates the screenshot animation.
    /// </summary>
    private void UpdateAnimState()
    {
        m_stillShotAnimator.Update(Time.deltaTime);

        switch (m_state)
        {
            case ScreenshotAnimState.IDLE:
                break;
            case ScreenshotAnimState.SCALE_UP:
                // Update scale up animation
                m_scaleUpAnimator.Update(Time.deltaTime);
                // When screenshot reaches normal scale, proceed to next anim state
                if (m_scaleUpAnimator.IsInState2)
                {
                    // Start slide animation
                    m_slideAnimator.AnimateToState2();
                    m_state = ScreenshotAnimState.SLIDE;
                }
                break;
            case ScreenshotAnimState.SLIDE:
                // Update slide animation
                m_slideAnimator.Update(Time.deltaTime);
                // When screenshot reaches target position, proceed to next anim state
                if (m_slideAnimator.IsInState2)
                {
                    // Start scale down animation
                    m_scaleDownAnimator.AnimateToState2();
                    m_state = ScreenshotAnimState.SCALE_DOWN;
                }
                break;
            case ScreenshotAnimState.SCALE_DOWN:
                // Update scale down animation
                m_scaleDownAnimator.Update(Time.deltaTime);
                // When screenshot reaches zero scale (becomes hidden), end the animation
                if (m_scaleDownAnimator.IsInState2)
                {
                    m_state = ScreenshotAnimState.IDLE;

                    // Hide screenshot UI
                    Hide();
                }
                break;
        }
    }

    /// <summary>
    /// Gets a random tilt within the allowed range for the screenshot.
    /// </summary>
    private Vector3 GetRandomScreenshotTilt()
    {
        float rotZ = Random.Range(-m_maxAnimDeltaRot, m_maxAnimDeltaRot);
        return Vector3.forward * rotZ;
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

        UpdateAnimState();

        // The screenshot shader clips the screenshot image so that only part of it is shown in the frame.
        //  However, it has a side effect of messing up the sorting order of UI, always making the
        //  screenshot image render under all other UI.
        // Workaround:  Use a new camera just for rendering the screenshot image above everything else
        // Make screenshot camera copy the values of UICamera, specifically orthographic size
        m_screenshotCamera.orthographicSize = Locator.GetUIManager().UICamera.Camera.orthographicSize;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
