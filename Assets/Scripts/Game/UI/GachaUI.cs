/******************************************************************************
*  @file       GachaUI.cs
*  @brief      Handles the gacha UI
*  @author     Ron
*  @date       October 19, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class GachaUI : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CharacterResource characterResource,
                           NewCharWinAnimator newCharWinAnim,
                           Vector3 initialCharacterRotation,
                           System.EventHandler<System.EventArgs> openGachaDelegate,
                           System.EventHandler<System.EventArgs> gachaPlayDelegate,
                           System.EventHandler<System.EventArgs> gachaShareDelegate,
                           System.EventHandler<System.EventArgs> repeatGachaDelegate,
                           System.EventHandler<System.EventArgs> gachaOpenedDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        m_characterResource = characterResource;
        m_newCharWinAnim = newCharWinAnim;

        // Set the initial rotation for characters in character select
        m_initialRotation = initialCharacterRotation;

        // Initialize buttons
        m_openGachaBtn.Initialize(openGachaDelegate, UIButton.TriggerType.ON_RELEASE);
        m_gachaPlayBtn.Initialize(gachaPlayDelegate, UIButton.TriggerType.ON_RELEASE);
        m_gachaShareBtn.Initialize(gachaShareDelegate, UIButton.TriggerType.ON_RELEASE);
        m_repeatGachaBtn.Initialize(repeatGachaDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set button sounds
        m_openGachaBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_gachaPlayBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_gachaShareBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_repeatGachaBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Initialize invisible button for gacha machine lever
        m_gachaMachineLever.Initialize(openGachaDelegate, UIButton.TriggerType.ON_PRESS);

        // Initialize text
        m_characterNameText.Initialize();
        m_prizeLabelText.Initialize();
        
        // Get delegate to call when gacha has been opened
        m_gachaOpenedDelegate = gachaOpenedDelegate;

        // Set scale of gacha machine depending on device orientation
        float newScale = Locator.GetUIManager().UICamera.IsLandscape ?
                            m_gachaMachineLandscapeScale :
                            m_gachaMachinePortraitScale;
        m_gachaAnim.transform.localScale = Vector3.one * newScale;

        // Initialize gacha overlay UI animator
        //  State 1: 1 alpha, covering gacha and entire screen
        //  State 2: 0 alpha, revealing gacha prize
        m_gachaOverlayAnimator = new UIAnimator(m_gachaOverlay);
        m_gachaOverlayAnimator.SetAlphaAnimation(1.0f, 0.0f);
        m_gachaOverlayAnimator.SetAnimTime(m_gachaOverlayAnimTime);
        m_gachaOverlayAnimator.ResetToState1();

        // Note gacha animator's initial speed
        m_gachaAnimInitialSpeed = m_gachaAnim.speed;

        // Reset state (should come after initializing other values)
        Reset();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Shows the gacha UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_openGachaBtn.UpdateScreenPosition();
        m_gachaPlayBtn.UpdateScreenPosition();
        m_gachaShareBtn.UpdateScreenPosition();
        m_repeatGachaBtn.UpdateScreenPosition();

        m_gachaRoot.SetActive(true);
        m_gachaAnim.Play(EMPTY_ANIM_NAME);
    }

    /// <summary>
    /// Hides the gacha UI.
    /// </summary>
    public void Hide()
    {
        m_newCharWinAnim.StopAnim();
        m_gachaRoot.SetActive(false);
    }

    /// <summary>
    /// Starts the gacha open animation.
    /// </summary>
    /// <param name="prizeCharacterType">Gacha prize character.</param>
    /// <param name="isOwned">if set to <c>true</c> character is already owned by the player.</param>
    public void StartGachaOpenAnimation(CharacterType prizeCharacterType, bool isOwned)
    {
        if (prizeCharacterType == CharacterType.SIZE)
        {
            return;
        }

        // Set character name text (but don't show yet)
        m_characterNameText.gameObject.SetActive(false);
        m_characterNameText.SetText(m_characterResource.GetCharacterStruct(prizeCharacterType).Name);

        // Store whether character is already owned
        m_isPrizeCharacterOwned = isOwned;

        // Set prize label text (but don't show yet)
        m_prizeLabelText.gameObject.SetActive(false);
        m_prizeLabelText.SetText(isOwned ? OWNED_CHARACTER_PRIZE_LABEL : NEW_CHARACTER_PRIZE_LABEL);

        // Store character prefab
        m_prizeCharacterType = prizeCharacterType;

        // Hide the open gacha button
        m_openGachaBtn.gameObject.SetActive(false);

        // Disable the gacha machine lever button
        m_gachaMachineLever.gameObject.SetActive(false);

        // Hide prize model and show gacha machine
        m_prizeModelRoot.SetActive(false);
        m_gachaMachineRoot.SetActive(true);

        // Play gacha opening animation
        m_gachaAnim.Play(OPEN_GACHA_ANIM_NAME);

        m_isOpeningGacha = true;
    }

    /// <summary>
    /// Notifies that the gacha open animation has ended.
    /// </summary>
    public void NotifyGachaOpenAnimationEnd()
    {
        // Show overlay to hide the rest of the screen
        m_gachaOverlay.gameObject.SetActive(true);
        
        // Hide gacha machine and show prize model
        m_gachaMachineRoot.SetActive(false);
        m_prizeModelRoot.SetActive(true);
        m_characterNameText.gameObject.SetActive(true);

        // Instantiate character
        m_prizeCharacter = m_characterResource.CreateCharacter(m_prizeCharacterType);
        m_prizeCharacter.transform.parent = m_prizeModelRoot.transform;
        m_prizeCharacter.transform.SetPosY(m_prizeCharacterPosY);
        m_prizeCharacter.transform.eulerAngles = m_initialRotation;
        m_prizeCharacter.transform.localScale *= m_prizeScaleFactor;
        // Set character to UI layer to make it visible in the gacha UI
        UIUtils.SetLayerRecursively(m_prizeCharacter.gameObject, m_prizeCharacterLayer);
        UIUtils.SetSortingLayerRecursively(m_prizeCharacter.ModelRenderer,
                                           m_prizeCharacterLayer,
                                           m_prizeCharacterSortingOrder);

        // Initialize new character animation
        m_newCharWinAnim.StartAnim(m_prizeCharacter.transform, !m_isPrizeCharacterOwned);

        // Show prize label, and play and share buttons
        m_prizeLabelText.gameObject.SetActive(true);
        m_gachaPlayBtn.gameObject.SetActive(true);
        m_gachaShareBtn.gameObject.SetActive(true);

        // Show/hide repeat gacha and locked sprite
        m_repeatGachaBtn.gameObject.SetActive(m_canRepeatGacha);
        m_repeatGachaLockedSprite.gameObject.SetActive(!m_canRepeatGacha);

        // Notify (scene master) that gacha opening has finished
        m_gachaOpenedDelegate.Invoke(this, System.EventArgs.Empty);

        // Start overlay animation
        m_gachaOverlayAnimator.AnimateToState2();

        m_isOpeningGacha = false;
    }

    /// <summary>
    /// Updates the repeat gacha UI - active button if can repeat, locked if can not repeat.
    /// </summary>
    public void UpdateRepeatGachaUI(bool canRepeat)
    {
        m_canRepeatGacha = canRepeat;
        // Show/hide repeat button or locked sprite only when either should be visible,
        //  i.e. just after opening gacha
        if (m_repeatGachaBtn.gameObject.activeInHierarchy ||
            m_repeatGachaLockedSprite.gameObject.activeInHierarchy)
        {
            m_repeatGachaBtn.gameObject.SetActive(canRepeat);
            m_repeatGachaLockedSprite.gameObject.SetActive(!canRepeat);
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

        m_gachaAnimEvents.Pause();
        m_gachaOverlayAnimator.Pause();
        m_gachaAnimSpeedBeforePause = m_gachaAnim.speed;
        m_gachaAnim.speed = 0.0f;

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

        m_gachaAnimEvents.Unpause();
        m_gachaOverlayAnimator.Unpause();
        m_gachaAnim.speed = m_gachaAnimSpeedBeforePause;

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        // Hide overlay
        m_gachaOverlay.gameObject.SetActive(false);
        m_gachaOverlayAnimator.ResetToState1();
        // Reset gacha animation
        if (m_gachaAnim.gameObject.activeInHierarchy)
        {
            m_gachaAnim.Play(EMPTY_ANIM_NAME);
        }
        m_gachaAnim.speed = m_gachaAnimInitialSpeed;
        m_isOpeningGacha = false;
        // Show gacha machine and hide prize model
        m_gachaMachineRoot.SetActive(true);
        m_prizeModelRoot.SetActive(false);
        // Delete prize character
        if (m_prizeCharacter != null)
        {
            GameObject.Destroy(m_prizeCharacter.gameObject);
            m_prizeCharacter = null;
        }
        // Reset prize animation
        m_newCharWinAnim.Reset();
        // Show open gacha button and hide gacha play button
        m_openGachaBtn.gameObject.SetActive(true);
        m_gachaPlayBtn.gameObject.SetActive(false);
        // Enable gacha machine lever button
        m_gachaMachineLever.gameObject.SetActive(true);
        // Hide share and repeat gacha buttons
        m_gachaShareBtn.gameObject.SetActive(false);
        m_repeatGachaBtn.gameObject.SetActive(false);
        m_repeatGachaLockedSprite.gameObject.SetActive(false);

        // Reset text
        m_prizeLabelText.SetText("");
        m_characterNameText.SetText("");
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        m_openGachaBtn.Delete();
        m_gachaPlayBtn.Delete();
        m_gachaShareBtn.Delete();
        m_repeatGachaBtn.Delete();
        m_gachaMachineLever.Delete();
        m_prizeLabelText.Delete();
        m_characterNameText.Delete();
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets whether the gacha is being opened.
    /// </summary>
    public bool IsOpeningGacha
    {
        get { return m_isOpeningGacha; }
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

    [SerializeField] private GameObject         m_gachaRoot             = null;
    [SerializeField] private Animator           m_gachaAnim             = null;
    [SerializeField] private GachaAnimEvents    m_gachaAnimEvents       = null;
    [SerializeField] private SpriteRenderer     m_gachaOverlay          = null;
    [SerializeField] private float              m_gachaOverlayAnimTime  = 0.3f;
    [SerializeField] private GameObject         m_gachaMachineRoot      = null;
    [SerializeField] private GameObject         m_prizeModelRoot        = null;

    [SerializeField] private UIText         m_characterNameText         = null;
    [SerializeField] private UIText         m_prizeLabelText            = null;
    [SerializeField] private UIButton       m_openGachaBtn              = null;
    [SerializeField] private UIButton       m_gachaPlayBtn              = null;
    [SerializeField] private UIButton       m_gachaShareBtn             = null;
    [SerializeField] private UIButton       m_repeatGachaBtn            = null;
    [SerializeField] private SpriteRenderer m_repeatGachaLockedSprite   = null;
    [Tooltip("Invisible button on the gacha lever that can be pressed to open the gacha")]
    [SerializeField] private UIButton       m_gachaMachineLever         = null;
    
    [Tooltip("Scale of gacha machine when in landscape screen orientation")]
    [SerializeField] private float      m_gachaMachineLandscapeScale    = 1.0f;
    [Tooltip("Scale of gacha machine when in portrait screen orientation")]
    [SerializeField] private float      m_gachaMachinePortraitScale     = 1.5f;
    [SerializeField] private string     m_prizeCharacterLayer           = "UI";
    [SerializeField] private int        m_prizeCharacterSortingOrder    = 1;
    [Tooltip("Factor by which the original character model is scaled for display in gacha UI")]
    [SerializeField] private float      m_prizeScaleFactor              = 0.7f;
    [Tooltip("Vertical position of the prize character for display in gacha UI")]
    [SerializeField] private float      m_prizeCharacterPosY            = -3.0f;

    #endregion // Serialized Variables

    #region Variables

    private bool        m_isInitialized             = false;
    private bool        m_isPaused                  = false;

    private bool        m_isOpeningGacha            = false;

    private bool        m_canRepeatGacha            = false;

    // Initial rotation of characters in character select (in euler angles)
    private Vector3     m_initialRotation           = Vector3.zero;

    private float       m_gachaAnimInitialSpeed     = 0.0f;
    private float       m_gachaAnimSpeedBeforePause = 0.0f;

    private CharacterResource   m_characterResource         = null;
    private CharacterType       m_prizeCharacterType        = CharacterType.SIZE;
    private Character           m_prizeCharacter            = null;
    private bool                m_isPrizeCharacterOwned     = false;
    
    private NewCharWinAnimator  m_newCharWinAnim           = null;

    private System.EventHandler<System.EventArgs> m_gachaOpenedDelegate = null;

    #endregion // Variables

    #region Constants

    private const string OWNED_CHARACTER_PRIZE_LABEL    = "TRY AGAIN";
    private const string NEW_CHARACTER_PRIZE_LABEL      = "NEW!";

    #endregion // Constants

    #region Animation

    private UIAnimator      m_gachaOverlayAnimator  = null;
    
    private const string    OPEN_GACHA_ANIM_NAME    = "OpenGacha";
    private const string    EMPTY_ANIM_NAME         = "Empty";

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

        // Update overlay animator
        m_gachaOverlayAnimator.Update(Time.deltaTime);
        // When overlay animates to state 2, disable it upon reaching state 2
        if (m_gachaOverlayAnimator.IsInState2)
        {
            m_gachaOverlay.gameObject.SetActive(false);
        }

        // If device orientation changes, update size of gacha machine
        if (Locator.GetUIManager().HasScreenOrientationChanged)
        {
            float newScale = Locator.GetUIManager().UICamera.IsLandscape ?
                                m_gachaMachineLandscapeScale :
                                m_gachaMachinePortraitScale;
            m_gachaAnim.transform.localScale = Vector3.one * newScale;
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {

    }

    #endregion // MonoBehaviour
}
