/******************************************************************************
*  @file       NewCharWinAnimator.cs
*  @brief      Handles the animation played upon obtaining a new character
*  @author     Ron
*  @date       October 13, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class NewCharWinAnimator : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Create confetti object pool
        for (int index = 0; index < CONFETTI_COUNT; ++index)
        {
            GameObject confettiObj = GameObject.Instantiate<GameObject>(m_confettiObjectPrototype);
            confettiObj.transform.parent = m_newCharWinAnimRoot.transform;
            ConfettiObject confetti = confettiObj.AddComponentNoDupe<ConfettiObject>();
            // Give confetti a random color from the provided set of colors
            Color color = m_confettiObjectColors[Random.Range(0, m_confettiObjectColors.Length)];
            confetti.SetColor(color);
            
            // Store confetti object in confetti array
            m_confettiObjects[index] = confetti;
        }

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts the animation.
    /// </summary>
    /// <param name="charToAnimate">The character to animate.</param>
    /// <param name="isNewChar">if set to <c>true</c> this is the first time the character was won.</param>
    public void StartAnim(Transform charToAnimate, bool isNewChar)
    {
        m_charToAnimate = charToAnimate;
        m_rotSpeed = m_initialRotSpeed;

        // Play one of two sounds, depending on whether character is already owned or not
        Locator.GetSoundManager().PlayOneShot(isNewChar ?
                                                SoundInfo.SFXID.NewCharacter :
                                                SoundInfo.SFXID.OldCharacter);

        m_timeSinceConfettiAnimStart = 0.0f;

        // Enable character animation
        m_enableCharAnim = true;
        // Enable confetti animation if new character
        m_enableConfettiAnim = isNewChar;
    }

    /// <summary>
    /// Stops the character and confetti animations.
    /// </summary>
    public void StopAnim()
    {
        StopCharAnim();
        m_enableConfettiAnim = false;
        // Hide confetti objects
        for (int index = 0; index < m_confettiObjects.Length; index++)
        {
            m_confettiObjects[index].gameObject.SetActive(false);
        }
        // Reset values
        m_timeSinceConfettiAnimStart = 0.0f;
        m_timeSinceLastConfetti = 0.0f;
    }

    /// <summary>
    /// Stops the character animation.
    /// </summary>
    public void StopCharAnim()
    {
        // Disable animation
        m_enableCharAnim = false;
        // Remove reference to character
        m_charToAnimate = null;
    }

    /// <summary>
    /// Notifies of a confetti object reaching the bottom of the screen.
    /// </summary>
    public void NotifyConfettiReachedTarget(int confettiIndex)
    {
        // Disable confetti
        m_confettiObjects[confettiIndex].gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the new char win animator objects.
    /// </summary>
    public void Show()
    {
        m_newCharWinAnimRoot.SetActive(true);
    }

    /// <summary>
    /// Hides the new char win animator objects.
    /// </summary>
    public void Hide()
    {
        m_newCharWinAnimRoot.SetActive(false);
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
        StopAnim();
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        // Delete confetti objects
        for (int index = 0; index < m_confettiObjects.Length; index++)
        {
            m_confettiObjects[index].Delete();
        }
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
    /// Gets whether the new character animation is active.
    /// </summary>
    public bool IsCharAnimActive
    {
        get { return m_enableCharAnim; }
    }

    /// <summary>
    /// Gets whether the confetti animation is active.
    /// </summary>
    public bool IsConfettiAnimActive
    {
        get { return m_enableConfettiAnim; }
    }

    /// <summary>
    /// Gets whether the character animation is finished.
    /// </summary>
    public bool IsCharAnimFinished
    {
        get { return m_enableCharAnim && m_rotSpeed <= m_finalRotSpeed; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GameObject     m_newCharWinAnimRoot        = null;

    [SerializeField] private GameObject     m_confettiObjectPrototype   = null;
    [SerializeField] private Color[]        m_confettiObjectColors      = null;
    [SerializeField] private float          m_confettiAnimDuration      = 6.0f;
    [SerializeField] private float          m_confettiSpawnIntervalMin  = 0.1f;
    [SerializeField] private float          m_confettiSpawnIntervalMax  = 0.35f;
    [SerializeField] private float          m_confettiSpeedMin          = 12.0f;
    [SerializeField] private float          m_confettiSpeedMax          = 25.0f;
    [SerializeField] private float          m_confettiDestRadiusMin     = 10.0f;
    [SerializeField] private float          m_confettiDestRadiusMax     = 20.0f;

    #endregion // Serialized Variables

    #region Variables

    private bool        m_isInitialized     = false;
    private bool        m_isPaused          = false;
    
    #endregion // Variables

    #region Animation

    private bool        m_enableCharAnim        = false;
    private bool        m_enableConfettiAnim    = false;
    private Transform   m_charToAnimate         = null;
    private float       m_initialRotSpeed       = 500.0f;
    private float       m_finalRotSpeed         = 40.0f;
    private float       m_rotDeceleration       = 250.0f;
    private float       m_rotSpeed              = 0.0f;
    private float       m_timeSinceConfettiAnimStart = 0.0f;
    private float       m_timeUntilNextConfetti = 0.0f;
    private float       m_timeSinceLastConfetti = 0.0f;
    private int         m_curConfettiIndex      = 0;
    private const float SPAWN_POS_OFFSET        = 3.0f;
    private const int   CONFETTI_COUNT          = 25;
    private ConfettiObject[] m_confettiObjects  = new ConfettiObject[CONFETTI_COUNT];

    /// <summary>
    /// Updates the animation.
    /// </summary>
    private void UpdateAnim()
    {
        if (!m_enableCharAnim)
        {
            return;
        }
        // Apply deceleration
        m_rotSpeed -= m_rotDeceleration * Time.deltaTime;
        // Clamp to the final rotation speed
        m_rotSpeed = Mathf.Max(m_rotSpeed, m_finalRotSpeed);
        // Rotate prize character
        m_charToAnimate.transform.Rotate(Vector3.up, m_rotSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Updates the confetti animation.
    /// </summary>
    private void UpdateConfettiAnim()
    {
        if (!m_enableConfettiAnim)
        {
            return;
        }
        // Stop confetti anim after a specified duration
        m_timeSinceConfettiAnimStart += Time.deltaTime;
        if (m_timeSinceConfettiAnimStart > m_confettiAnimDuration)
        {
            m_enableConfettiAnim = false;
            return;
        }

        m_timeSinceLastConfetti += Time.deltaTime;
        if (m_timeSinceLastConfetti > m_timeUntilNextConfetti)
        {
            // Spawn confetti above the top center of the screen
            Vector3 spawnPos = Vector3.zero;
            spawnPos.y = Locator.GetUIManager().UICamera.ScreenMaxWorld.y;
            
            // Get random move direction towards a position in a circular area at the bottom of the screen
            Vector3 spawnCenterToTargetPosDir = new Vector3(Random.Range(-1.0f, 1.0f),
                                                            0.0f,
                                                            Random.Range(-1.0f, 1.0f));
            float spawnCenterToTargetPosDist = Random.Range(m_confettiDestRadiusMin, m_confettiDestRadiusMax);
            Vector3 targetPos = spawnPos + spawnCenterToTargetPosDir * spawnCenterToTargetPosDist;
            // Set target pos to the bottom of the screen
            targetPos.y = Locator.GetUIManager().UICamera.ScreenMinWorld.y;
            // Get move direction
            Vector3 moveDir = Vector3.Normalize(targetPos - spawnPos);

            // Set rotation based on move direction
            Vector3 rotation = new Vector3(Mathf.Atan(moveDir.z / moveDir.y) * Mathf.Rad2Deg,
                                      Mathf.Atan(moveDir.z / moveDir.x) * Mathf.Rad2Deg,
                                      Mathf.Atan(moveDir.x / moveDir.y) * Mathf.Rad2Deg);

            // Randomize speed
            float speed = Random.Range(m_confettiSpeedMin, m_confettiSpeedMax);

            // Initialize a confetti object
            ConfettiObject confetti = InitNextConfetti(moveDir, speed, targetPos.y, rotation);
            confetti.transform.position = spawnPos;
            confetti.gameObject.SetActive(true);

            // Reset timer
            m_timeSinceLastConfetti = 0.0f;
            // Randomize the time until the next confetti
            m_timeUntilNextConfetti = Random.Range(m_confettiSpawnIntervalMin, m_confettiSpawnIntervalMax);
        }
    }

    /// <summary>
    /// Gets and initializes the next confetti object.
    /// </summary>
    private ConfettiObject InitNextConfetti(Vector3 moveDir, float speed, float targetPosY, Vector3 rotation)
    {
        // Make sure index stays within range of the confetti array
        if (m_curConfettiIndex >= CONFETTI_COUNT)
        {
            m_curConfettiIndex = 0;
        }
        // Initialize confetti object
        m_confettiObjects[m_curConfettiIndex].Initialize(moveDir, speed, targetPosY, rotation,
                                                         this, m_curConfettiIndex);

        // Return confetti, and incrememt index for the next spawn interval
        return m_confettiObjects[m_curConfettiIndex++];
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
        if (!m_isInitialized || m_isPaused)
        {
            return;
        }

        UpdateAnim();
        UpdateConfettiAnim();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {

    }

    #endregion // MonoBehaviour
}
