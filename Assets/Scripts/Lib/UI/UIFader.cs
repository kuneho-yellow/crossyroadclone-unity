/******************************************************************************
*  @file       UIFader.cs
*  @brief      Handles the fader UI
*  @author     Ron
*  @date       September 24, 2015
*      
*  @par [explanation]
*		> Fades current scene out and fades next scene in when switching scenes
*		> Note: "Fade" in this script is always from the perspective of the game
*			i.e.	"Fade in" means the game becomes visible
*						(fader overlay becomes transparent)
*					"Fade out" means the game becomes hidden
*						(fader overlay becomes opaque)
******************************************************************************/

#region Namespaces

using UnityEngine;
using TouchScript.Hit;

#endregion // Namespaces

public class UIFader : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Initialize this instance.
	/// </summary>
	public void Initialize()
	{
		// Start hidden
		Hide();

		// Make fader overlay either fully transparent or fully opaque
		float a = Mathf.Clamp(Mathf.Round(m_faderOverlay.color.a), 0.0f, 1.0f);
		m_faderOverlay.SetAlpha(a);
		// Set fade animation state
		m_fadeAnimState = (a == 1.0f) ? FadeAnimationState.FADED_OUT : FadeAnimationState.FADED_IN;
		// Block input if opaque, allow input if transparent
		SetBlockInput((a == 1.0f) ? true : false);

		// Set the initialized flag
		m_isInitialized = true;
	}

	/// <summary>
	/// Begins fading out.
	/// </summary>
	/// <param name="startFadedIn">Whether fader overlay should be transparent before fade animation begins.</param>
	/// <param name="fadeAnimSpeed">Fade animation speed.</param>
	public void FadeOut(bool startFadedIn = false, float fadeAnimSpeed = DEFAULT_FADE_ANIM_SPEED)
	{
		if (startFadedIn)
		{
			m_faderOverlay.SetAlpha(0.0f);
		}
		SetFadeAnimationSpeed(fadeAnimSpeed);

		// Store fader overlay's alpha at the beginning of the fade animation
		m_startingFaderAlpha = m_faderOverlay.color.a;
		// Reset fade animation time tracker
		m_timeSinceFadeStart = 0.0f;

		Show();

		// Start fade animation
		m_fadeAnimState = FadeAnimationState.FADING_OUT;
	}

	/// <summary>
	/// Begins fading in.
	/// </summary>
	/// <param name="startFadedOut">Whether fader overlay should be opaque before fade animation begins.</param>
	/// <param name="fadeAnimSpeed">Fade animation speed.</param>
	/// <param name="blockInput">Whether input should be blocked during fade in.</param>
	public void FadeIn(bool startFadedOut = false, float fadeAnimSpeed = DEFAULT_FADE_ANIM_SPEED)
	{
		if (startFadedOut)
		{
			m_faderOverlay.SetAlpha(1.0f);
		}
		SetFadeAnimationSpeed(fadeAnimSpeed);

		// Store fader overlay's alpha at the beginning of the fade animation
		m_startingFaderAlpha = m_faderOverlay.color.a;
		// Reset fade animation time tracker
		m_timeSinceFadeStart = 0.0f;

		Show();

		// Start fade animation
		m_fadeAnimState = FadeAnimationState.FADING_IN;
	}

	/// <summary>
	/// Sets the fade animation speed.
	/// </summary>
	/// <param name="fadeAnimSpeed">Fade animation speed.</param>
	public void SetFadeAnimationSpeed(float fadeAnimSpeed)
	{
		m_fadeAnimSpeed = fadeAnimSpeed;
	}

	/// <summary>
	/// Sets the fader overlay to block or allow input.
	/// </summary>
	/// <param name="blockInput">If set to <c>true</c> block input. Else, allow input.</param>
	public void SetBlockInput(bool blockInput = true)
	{
		m_untouchable.DiscardTouch = blockInput;
	}

	/// <summary>
	/// Shows the UIFader.
	/// </summary>
	public void Show()
	{
		m_faderOverlay.gameObject.SetActive(true);
	}

	/// <summary>
	/// Hides the UIFader.
	/// </summary>
	public void Hide()
	{
		m_faderOverlay.gameObject.SetActive(false);
	}

	/// <summary>
	/// Gets whether fader is fading in.
	/// </summary>
	public bool IsFadingIn
	{
		get { return m_fadeAnimState == FadeAnimationState.FADING_IN; }
	}

	/// <summary>
	/// Gets whether fader is fading out.
	/// </summary>
	public bool IsFadingOut
	{
		get { return m_fadeAnimState == FadeAnimationState.FADING_OUT; }
	}

	/// <summary>
	/// Gets the state of the fade animation.
	/// </summary>
	public FadeAnimationState FaderState
	{
		get { return m_fadeAnimState; }
	}

	/// <summary>
	/// Gets whether PauseUI is initialized.
	/// </summary>
	public bool IsInitialized
	{
		get { return m_isInitialized; }
	}

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("The overlay object to use in the fade animation")]
    [SerializeField] private SpriteRenderer	m_faderOverlay	= null;
    [Tooltip("The untouchable behavior that blocks input during the fade animation")]
    [SerializeField] private Untouchable 	m_untouchable 	= null;

	#endregion // Serialized Variables

	#region Variables

	private bool m_isInitialized = false;

	#endregion // Variables

	#region Fade Animation

	public enum FadeAnimationState
	{
		NONE = 0,

		FADED_IN,
		FADED_OUT,

		FADING_IN,
		FADING_OUT
	}
	private FadeAnimationState m_fadeAnimState = FadeAnimationState.NONE;

	private float m_fadeAnimSpeed = 0.0f;
	private float m_timeSinceFadeStart = 0.0f;
	private float m_startingFaderAlpha = 0.0f;

	private const float DEFAULT_FADE_ANIM_SPEED = 5.0f;

	/// <summary>
	/// Updates the fade animation.
	/// </summary>
	private void UpdateFadeAnimation()
	{
		switch (m_fadeAnimState)
		{
		case FadeAnimationState.NONE:
		case FadeAnimationState.FADED_IN:
		case FadeAnimationState.FADED_OUT:
		default:
			break;
		case FadeAnimationState.FADING_IN:
			m_timeSinceFadeStart += Time.deltaTime;
			m_faderOverlay.SetAlpha(Mathf.Lerp(m_startingFaderAlpha, 0.0f, m_timeSinceFadeStart * m_fadeAnimSpeed));
			if (m_faderOverlay.color.a == 0.0f)
			{
				m_fadeAnimState = FadeAnimationState.FADED_IN;
				// Reallow input when scene has faded in
				SetBlockInput(false);
			}
			break;
		case FadeAnimationState.FADING_OUT:
			m_timeSinceFadeStart += Time.deltaTime;
			m_faderOverlay.SetAlpha(Mathf.Lerp(m_startingFaderAlpha, 1.0f, m_timeSinceFadeStart * m_fadeAnimSpeed));
			if (m_faderOverlay.color.a == 1.0f)
			{
				m_fadeAnimState = FadeAnimationState.FADED_OUT;
				// Block input when scene has faded out
				SetBlockInput(true);
			}
			break;
		}
	}

	#endregion // Fade Animation

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
		UpdateFadeAnimation();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
