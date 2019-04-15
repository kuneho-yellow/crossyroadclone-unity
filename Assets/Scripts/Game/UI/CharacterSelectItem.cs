/******************************************************************************
*  @file       CharacterSelectItem.cs
*  @brief      Handles a character in the character scroll panel
*  @author     Ron
*  @date       October 15, 2015
*      
*  @par [explanation]
*		> Rotates characters that the player already owns
*       > Sets non-owned characters to grayscale
*       > Sets owned characters to normal color
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class CharacterSelectItem : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CharacterSelectUI charSelectUI, UIScrollItem scrollItem,
                           Character character, bool isUsed, bool isOwned, float ownedCharRotSpeed,
                           float normalCharScale, float focusCharScale,
                           float focusAnimTime, Vector3 focusPos,
                           NewCharWinAnimator newCharWinAnim,
                           Animator newCharAnimReference)
    {
        // Store references to character select classes
        m_charSelectUI = charSelectUI;
        m_scrollItem = scrollItem;
        m_character = character;

        SetUsed(isUsed);
        SetOwned(isOwned);
        SetOwnedCharRotSpeed(ownedCharRotSpeed);
        SetCharScale(normalCharScale, focusCharScale);
        SetFocusPos(focusPos);
        SetFocusAnimTime(focusAnimTime);

        m_newCharWinAnim = newCharWinAnim;

        // Store reference to animator to follow for new/unused characters
        m_newCharAnimReference = newCharAnimReference;

        // Store the character's starting transform properties
        m_originalRot = m_character.transform.localEulerAngles;
        m_originalPosZ = m_character.transform.localPosition.z;

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Plays the new character animation.
    /// </summary>
    public void PlayNewCharAnim()
    {
        m_newCharWinAnim.StartAnim(m_character.transform, true);
    }

    /// <summary>
    /// Sets whether this character has been used by the player at least once.
    /// </summary>
    /// <param name="isUsed">if set to <c>true</c> is used.</param>
    public void SetUsed(bool isUsed = true)
    {
        m_isUsed = isUsed;
        // Reset vertical position to 0
        m_character.transform.SetLocalPosY(0.0f);
    }

    /// <summary>
    /// Sets whether this character is owned by the player.
    /// </summary>
    /// <param name="isOwned">if set to <c>true</c> is owned.</param>
    public void SetOwned(bool isOwned = true)
    {
        m_isOwned = isOwned;
        // Set to color if owned, to grayscale if not owned
        m_character.ModelRenderer.material.shader = isOwned ? m_ownedShader : m_unownedShader;
    }

    /// <summary>
    /// Sets the rotation speed of owned characters in character select.
    /// </summary>
    /// <param name="ownedCharRotSpeed">The rotation speed.</param>
    public void SetOwnedCharRotSpeed(float ownedCharRotSpeed)
    {
        m_ownedRotSpeed = ownedCharRotSpeed;
    }

    /// <summary>
    /// Sets the factor by which the character scales when focused in character select.
    /// </summary>
    /// <param name="normalScale">Normal scale.</param>
    /// <param name="focusScale">Scale when in focus.</param>
    public void SetCharScale(float normalScale, float focusScale)
    {
        m_normalScale = normalScale;
        m_focusScale = focusScale;
    }

    /// <summary>
    /// Sets the z position of focused characters in character select.
    /// </summary>
    /// <param name="focusPos">The focus position.</param>
    public void SetFocusPos(Vector3 focusPos)
    {
        m_focusPos = focusPos;
    }

    /// <summary>
    /// Sets the animation time for focusing on character in character select.
    /// </summary>
    /// <param name="focusAnimTime">The animation time.</param>
    public void SetFocusAnimTime(float focusAnimTime)
    {
        m_focusAnimTime = focusAnimTime;
    }

    /// <summary>
    /// Sets/unsets the focus on this character in character select.
    /// </summary>
    /// <param name="isFocused">if set to <c>true</c> is focused.</param>
    public void SetFocus(bool isFocused, bool lerpToNewState = true)
    {
        if (m_isFocused == isFocused)
        {
            return;
        }
        m_isFocused = isFocused;
        // If focused, remove character from the scroll panel hierarchy
        // If not focused, return character to the scroll panel hierarchy
        m_character.transform.parent = isFocused ? m_charSelectUI.transform : m_scrollItem.transform;
        // Check if lerp is enabled
        if (lerpToNewState)
        {
            // Reset lerp values
            m_timeSinceLerpStart = 0.0f;
            m_lerpStartScale = m_character.transform.localScale;
            m_lerpStartPos = m_character.transform.position;
            // If start and target animation values are different, start focus animation
            if (m_lerpStartScale.x != (isFocused ? m_focusScale : m_normalScale) ||
                m_lerpStartPos != (isFocused ? m_focusPos : m_scrollItem.transform.position))
            {
                m_isFocusUpdating = true;
            }
        }
        // Else, go directly to target scale and position
        else
        {
            if (isFocused)
            {
                // Change to focus position and scale
                m_character.transform.position = m_focusPos;
                m_character.transform.localScale = Vector3.one * m_focusScale;
            }
            else
            {
                // Change to scroll item position and normal scale
                Vector3 scrollItemPos = m_scrollItem.transform.position;
                scrollItemPos.z = m_originalPosZ;
                m_character.transform.position = scrollItemPos;
                m_character.transform.localScale = Vector3.one * m_normalScale;
            }
            // Disable focus animation
            m_isFocusUpdating = false;
        }
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
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        m_character.transform.parent = m_scrollItem.transform;
        m_character.transform.localPosition = new Vector3(0.0f, 0.0f, m_originalPosZ);
        m_character.transform.localEulerAngles = m_originalRot;
        m_character.transform.localScale = Vector3.one * m_normalScale;
        m_newCharWinAnim.Reset();
        m_isFocused = false;
        m_isFocusUpdating = false;
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        // This script only handles color and rotation of scroll items
        // The actual handlers are still UIScrollItem and UIScrollPanel
        // To delete the scroll item, call UIScrollItem.Delete
        Destroy(this);
    }

    /// <summary>
    /// Gets whether the character this char select item handles is owned by the player.
    /// </summary>
    public bool IsOwned
    {
        get { return m_isOwned; }
    }

    /// <summary>
    /// Gets whether the character this char select item handles has been used by the player.
    /// </summary>
    public bool IsUsed
    {
        get { return m_isUsed; }
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
    /// Gets the scroll item associated with this character select item instance.
    /// </summary>
    public UIScrollItem ScrollItem
    {
        get { return m_scrollItem; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized     = false;
    private bool    m_isPaused          = false;

    #endregion // Variables

    #region Character Handling

    private CharacterSelectUI   m_charSelectUI  = null;
    private UIScrollItem        m_scrollItem    = null;
    private Character           m_character     = null;

    private bool        m_isUsed                = false;
    private bool        m_isOwned               = false;
    private bool        m_isFocused             = false;
    private bool        m_isFocusUpdating       = false;

    private float       m_ownedRotSpeed         = 0.0f;
    private float       m_focusAnimTime         = 0.0f;
    private float       m_normalScale           = 0.0f;
    private float       m_focusScale            = 0.0f;
    private Vector3     m_focusPos              = Vector3.zero;
    private float       m_originalPosZ          = 0.0f;         // Original position is centered on scroll item
    private Vector3     m_originalRot           = Vector3.zero;
    private Vector3     m_lerpStartPos          = Vector3.zero;
    private Vector3     m_lerpStartScale        = Vector3.zero;
    private float       m_timeSinceLerpStart    = 0.0f;

    private Shader      m_ownedShader           = Shader.Find("Standard");
    private Shader      m_unownedShader         = Shader.Find("Custom/Grayscale");

    private Animator    m_newCharAnimReference  = null;

    private NewCharWinAnimator  m_newCharWinAnim   = null;

    /// <summary>
    /// Updates the rotation of owned characters in character select.
    /// </summary>
    private void UpdateOwnedCharAnim()
    {
        if (!m_isOwned)
        {
            return;
        }
        // If character was just unlocked, let the new char animator handle the animation first
        if (m_newCharWinAnim.IsCharAnimActive)
        {
            return;
        }

        m_character.transform.Rotate(Vector3.up, Time.deltaTime * m_ownedRotSpeed);
    }

    /// <summary>
    /// Updates scaling and positioning of focused character.
    /// </summary>
    private void UpdateFocus()
    {
        if (!m_isFocusUpdating)
        {
            return;
        }

        // If time since start has exceeded animation time, the character scroll item
        //  should be in the target position and have the target scale already
        if (m_timeSinceLerpStart > m_focusAnimTime)
        {
            m_isFocusUpdating = false;
            return;
        }
        m_timeSinceLerpStart += Time.deltaTime;
        Vector3 lerpTargetPos = Vector3.zero;
        Vector3 lerpTargetScale = Vector3.zero;
        if (m_isFocused)
        {
            // Lerp to focus position
            lerpTargetPos = m_focusPos;
            // Lerp to larger scale
            lerpTargetScale = Vector3.one * m_focusScale;
        }
        else
        {
            // Lerp back to position in character select scroll panel
            Vector3 scrollItemPos = m_scrollItem.transform.position;
            scrollItemPos.z = m_originalPosZ;
            lerpTargetPos = scrollItemPos;
            // Lerp back to normal scale
            lerpTargetScale = Vector3.one * m_normalScale;
        }

        // Lerp to the target position and scale
        if (m_focusAnimTime != 0.0f)
        {
            m_character.transform.position = Vector3.Lerp(m_lerpStartPos, lerpTargetPos,
                                                   m_timeSinceLerpStart / m_focusAnimTime);
            m_character.transform.localScale = Vector3.Lerp(m_lerpStartScale, lerpTargetScale,
                                                 m_timeSinceLerpStart / m_focusAnimTime);
        }
        // If anim time is 0, just go straight to the focus position and scale
        else
        {
            m_character.transform.position = lerpTargetPos;
            m_character.transform.localScale = lerpTargetScale;
        }
    }

    /// <summary>
    /// Updates the new character idle animation.
    /// </summary>
    private void UpdateNewCharIdleAnim()
    {
        if (m_isUsed || !m_isOwned || m_isFocused)
        {
            return;
        }

        // Follow the hopping animation of the "new character" animation reference
        float animPosY = m_newCharAnimReference.transform.position.y;
        m_character.transform.SetLocalPosY(animPosY);
    }

    #endregion // Character Handling

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
		if (!m_isInitialized || m_isPaused)
        {
            return;
        }
        
        UpdateOwnedCharAnim();
        UpdateNewCharIdleAnim();
        UpdateFocus();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
