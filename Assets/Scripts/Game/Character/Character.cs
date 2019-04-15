/******************************************************************************
*  @file       Character.cs
*  @brief      
*  @author     Lori
*  @date       September 10, 2015
*      
*  @par [explanation]
*		> Holds the character settings
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class Character : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initialize this instance
    /// </summary>
    public void Initialize()
    {
        m_collider = GetComponent<Collider>();
        GetSoundManagerInstance();
        m_state = State.NONE;
    }

    /// <summary>
    /// Reset this instances
    /// </summary>
    public void Reset()
    {
        m_state = State.NONE;
        transform.parent = null;
        m_modelRoot.gameObject.SetActive(true);
        transform.position = Vector3.zero;
        transform.eulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;
        m_modelRoot.localPosition = Vector3.zero;
        m_modelRoot.localEulerAngles = Vector3.zero;
        m_modelRoot.localScale = Vector3.one;
        m_facingDir = Vector2.down;
        EnablePhysics();
        m_isPaused = false;
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
    /// Gets the character type.
    /// </summary>
    /// <value>The type.</value>
    public CharacterType Type
    {
        get { return m_type; }
    }

    /// <summary>
    /// Gets the map type.
    /// </summary>
    /// <value>The type.</value>
    public MapType MapType
    {
        get { return m_mapType; }
    }

    /// <summary>
    /// Gets the renderer
    /// </summary>
    public Renderer ModelRenderer
    {
        get
        {
            if (m_modelRenderer != null)
            {
                return m_modelRenderer;
            }
            return GetComponent<Renderer>();
        }
    }

    /// <summary>
    /// Gets whether the character is on a platform
    /// </summary>
    public bool IsOnPlatform
    {
        get
        {
            return m_targetPlatform != null && transform.parent == m_targetPlatform.transform;
        }
    }

    /// <summary>
    /// Gets whether the character has a target platform
    /// </summary>
    public bool HasTargetPlatform
    {
        get
        {
            return m_targetPlatform != null;
        }
    }

    /// <summary>
    /// Gets the parent platform
    /// </summary>
    public Platform ParentPlatform
    {
        get { return IsOnPlatform ? m_targetPlatform : null; }
    }

    /// <summary>
    /// Gets whether the character is jumping
    /// </summary>
    public bool IsJumping
    {
        get { return m_isJumping; }
    }

    /// <summary>
    /// Attaches a controller
    /// </summary>
    /// <param name="thisPlayer"></param>
    public void AttachController(Player thisPlayer)
    {
        m_player = thisPlayer;
        m_player.SetPressDelegate(NotifyPress);
        m_player.SetReleaseDelegate(NotifyRelease);
        m_player.SetSwipeDelegate(NotifySwipe);
        m_state = State.INPUT;
    }

    /// <summary>
    /// Detaches the controller
    /// </summary>
    public void DetachController()
    {
        m_player.SetPressDelegate(null);
        m_player.SetReleaseDelegate(null);
        m_player.SetSwipeDelegate(null);
    }

    /// <summary>
    /// Called when the character has jumped on water
    /// </summary>
    public void NotifyJumpOnWater()
    {
        StartDeath(DeathType.DROWN, ScreenshotDir.Front);
    }

    /// <summary>
    /// Called when the character has been idle for too long
    /// or has jumped back 3 lanes
    /// </summary>
    /// <param name="eagle"></param>
    public void NotifyEagleArrived(GameObject eagle)
    {
        m_eagle = eagle;
        StartDeath(DeathType.EAGLE, ScreenshotDir.Front);
    }

    /// <summary>
    /// Called when the player exits the playable area (by riding a log)
    /// </summary>
    public void NotifyOutOfPlayableArea()
    {
        StartDeath(DeathType.LOG, ScreenshotDir.Front);
    }

    /// <summary>
    /// Called when the player has reached the end of the map
    /// </summary>
    public void NotifyReachEndOfMap()
    {
        transform.parent = null;
    }

    public delegate bool OnStartJump(Vector2 direction, out Vector3 targetPos, out Platform targetPlatform);
    public delegate void OnEndJump();

    /// <summary>
    /// Set the OnStartJump delegate
    /// </summary>
    /// <param name="onStartJump"></param>
    public void SetOnStartJumpDelegate(OnStartJump onStartJump)
    {
        m_onStartJump = onStartJump;
    }

    /// <summary>
    /// Set the OnEndJump delegate
    /// </summary>
    /// <param name="onEndJump"></param>
    public void SetOnEndJumpDelegate(OnEndJump onEndJump)
    {
        m_onEndJump = onEndJump;
    }

    /// <summary>
    /// Returns whether the character is active
    /// </summary>
    public bool IsPlaying
    {
        get { return m_state == State.INPUT; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private		CharacterType   m_type		    = CharacterType.Chicken;
	[SerializeField]private		MapType		    m_mapType	    = MapType.Default;
    [SerializeField]private     Transform       m_modelRoot     = null;
    [SerializeField]private     Renderer        m_modelRenderer = null;
    [Header("Controller")]
    [SerializeField]private     Player          m_player        = null;
    [Header("Movement")]
    [SerializeField]private     float           m_rotateTime    = 0.2f;
    [SerializeField]private     float           m_jumpTime      = 0.2f;
    [SerializeField]private     Vector3         m_jumpScale     = new Vector3(1.1f, 0.8f, 1.1f);
    [SerializeField]private     float           m_jumpScaleTime = 0.2f;
    [SerializeField]private     float           m_jumpHeight    = 5f;
    [Tooltip("The maximum number of blocked jumps before swipe ui appears")]
    [SerializeField]private     int             m_maxBlockJumps = 3;
    [Header("Death")]
    [SerializeField]private     Vector3         m_dieScaleFlat  = new Vector3(1.2f, 0.1f, 1.2f);
    [SerializeField]private     Vector3         m_dieScaleVert  = new Vector3(0.1f, 1.2f, 1.2f);
    [SerializeField]private     Vector3         m_dieScaleHoriz = new Vector3(1.2f, 1.2f, 0.1f);
    [SerializeField]private     Vector3         m_dieRotVert    = new Vector3(10f, 0f, 0f);
    [SerializeField]private     Vector3         m_dieRotHoriz   = new Vector3(0f, 0f, 10f);
    [SerializeField]private     float           m_dieScaleTime  = 0.2f;
    [Tooltip("The maximum vehicle speed to get flattened")]
    [SerializeField]private     float           m_maxSpeedFlat  = 100f;
    [Tooltip("The minimum z distance between the eagle and the character for screenshot")]
    [SerializeField]private     float           m_eagleScreenshotDist   = 30f;
    [Tooltip("The minimum z distance between the eagle and the character for the character to disappear")]
    [SerializeField]private     float           m_eagleDisappearDist    = -30f;
    [SerializeField]private     float           m_drownSpeed            = 90f;
    [SerializeField]private     float           m_deathTime             = 1f;
    [Header("Sounds")]
    [SerializeField]private     float           m_charSoundProb         = 0.2f;

    #endregion // Serialized Variables

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
        if (m_isPaused)
        {
            return;
        }

        switch (m_state)
        {
            case State.NONE:
                break;

            case State.IDLE:
                break;

            case State.INPUT:
                UpdateMovement();
                break;

            case State.DEATH:
                UpdateDeath();
                break;
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {

    }

    /// <summary>
    /// Called when touching trigger colliders
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickup"))
        {
            OnCollideWithPickup(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Vehicle"))
        {
            OnCollideWithVehicle(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            OnCollideWithPlatform(other.gameObject);
        }
    }

    #endregion // MonoBehaviour

    #region State

    /// <summary>
    /// Character states
    /// </summary>
    private enum State
    {
        NONE,
        IDLE,
        INPUT,
        DEATH
    }

    private         State       m_state             = State.NONE;
    private         bool        m_isPaused          = false;

    #endregion // State

    #region Movement

    /// <summary>
    /// Updates the movement
    /// </summary>
    private void UpdateMovement()
    {
        if (m_isJumping)
        {
            UpdateJump();
        }
        if (m_isScaling)
        {
            UpdateScale();
        }
        if (m_isRotating)
        {
            UpdateRotation();
        }
    }

    #region Jump

    private         bool        m_isJumping         = false;
    private         int         m_pendingJumpCount  = 0;
    private         int         m_blockedJumpCount  = 0;
    private         Vector3     m_jumpStartPos      = Vector3.zero;
    private         Vector3     m_jumpTargetPos     = Vector3.zero;
    private         Platform    m_targetPlatform    = null;
    private         float       m_jumpTimer         = 0f;
    private         OnStartJump m_onStartJump       = null;
    private         OnEndJump   m_onEndJump         = null;
    private         SoundObject m_charSound         = null;
    private         int         m_forwardComboCount = 0;
    private         int         m_forwardComboMin   = 20;
    private         bool        m_isForwardComboOn  = true;

    /// <summary>
    /// Prepares to jump
    /// </summary>
    private void PrepareJump()
    {
        // Squish the character downward
        StartScale(m_jumpScale, m_jumpScaleTime);
    }

    /// <summary>
    /// Try to jump
    /// </summary>
    private void TryJump()
    {
        m_pendingJumpCount++;
        if (!m_isJumping)
        {
            StartJump();
        }
    }

    /// <summary>
    /// Starts the jump
    /// </summary>
    private void StartJump()
    {
        m_jumpStartPos = transform.position;
        m_jumpTargetPos = m_jumpStartPos;
        Platform newTargetPlatform = null;
        bool canJump = true;
        // Get the jump target position and determine if character can jump there
        if (m_onStartJump != null)
        {
            canJump = m_onStartJump(m_facingDir, out m_jumpTargetPos, out newTargetPlatform);
        }
        if (canJump)
        {
            // Start the jump
            m_jumpTimer = 0f;
            m_isJumping = true;
            m_targetPlatform = newTargetPlatform;
            if (m_targetPlatform == null || transform.parent != m_targetPlatform.transform)
            {
                transform.parent = null;
            }
            m_blockedJumpCount = 0;

            // Play sounds
            PlayCharacterMoveSound();
            PlayCharacterCallSound();

            // Inform game manager
            Locator.GetGameManager().NotifyCharacterJump();
        }
        else
        {
            EndJump();
            if (m_facingDir == Vector2.up)
            {
                m_blockedJumpCount++;
                if (m_blockedJumpCount == m_maxBlockJumps)
                {
                    Locator.GetGameManager().NotifyCharacterBlockedJump();
                }
            }
        }
    }

    /// <summary>
    /// Updates the jump
    /// </summary>
    private void UpdateJump()
    {
        m_jumpTimer += Time.deltaTime;
        float lerpTime = m_jumpTimer / m_jumpTime;
        if (m_targetPlatform != null)
        {
            m_jumpTargetPos = m_targetPlatform.GetNearestLandingPos(m_jumpTargetPos);
        }
        transform.position = Vector3.Lerp(m_jumpStartPos, m_jumpTargetPos, lerpTime);
        float newYPos = 0f;
        if (lerpTime <= 0.5f)
        {
            newYPos = Mathf.Lerp(0f, m_jumpHeight, lerpTime * 2f);
        }
        else
        {
            newYPos = Mathf.Lerp(m_jumpHeight, 0f, (lerpTime - 0.5f) * 2f);
        }
        m_modelRoot.SetLocalPosY(newYPos);

        if (lerpTime >= 1.0f)
        {
            transform.position = m_jumpTargetPos;
            m_modelRoot.SetLocalPosY(0f);
            m_pendingJumpCount--;

            // Update the achievement
            if (m_isForwardComboOn)
            {
                if (m_facingDir == Vector2.up)
                {
                    m_forwardComboCount++;
                    if (m_forwardComboCount >= m_forwardComboMin)
                    {
                        Locator.GetGameManager().NotifyCharacterTunnelVision();
                        m_isForwardComboOn = false;
                    }
                }
                else
                {
                    m_forwardComboCount = 0;
                }
            }

            // Update the platform
            if (m_targetPlatform != null)
            {
                m_targetPlatform.StartSinking();
            }

            if (m_onEndJump != null)
            {
                m_onEndJump();
            }

            if (m_pendingJumpCount > 0)
            {
                StartJump();
            }
            else
            {
                EndJump();
            }
        }
    }

    /// <summary>
    /// Ends the jump
    /// </summary>
    private void EndJump()
    {
        // Unsquish the character
        if (!m_isPressed)
        {
            StartScale(Vector3.one, m_jumpScaleTime);
        }
        m_pendingJumpCount = 0;
        m_isJumping = false;
    }

    #endregion // Jump

    #region Scaling

    private         bool        m_isScaling         = false;
    private         Vector3     m_startScale        = Vector3.one;
    private         Vector3     m_targetScale       = Vector3.one;
    private         float       m_scaleTimer        = 0f;
    private         float       m_scaleTime         = 0f;

    /// <summary>
    /// Scale instantly
    /// </summary>
    /// <param name="newScale"></param>
    private void ScaleInstantly(Vector3 newScale)
    {
        m_modelRoot.localScale = newScale;
        m_isScaling = false;
    }

    /// <summary>
    /// Start scaling
    /// </summary>
    /// <param name="targetScale">The target scale</param>
    /// <param name="scaleTime">The time to scale</param>
    private void StartScale(Vector3 targetScale, float scaleTime)
    {
        m_startScale = m_modelRoot.localScale;
        m_targetScale = targetScale;
        m_scaleTimer = 0f;
        m_scaleTime = scaleTime;
        m_isScaling = true;
    }

    /// <summary>
    /// Updates the scale
    /// </summary>
    private void UpdateScale()
    {
        m_scaleTimer += Time.deltaTime;
        float lerpTime = m_scaleTimer / m_scaleTime;
        m_modelRoot.localScale = Vector3.Lerp(m_startScale, m_targetScale, lerpTime);
        if (lerpTime >= 1.0f)
        {
            m_modelRoot.localScale = m_targetScale;
            m_isScaling = false;
        }
    }

    #endregion // Scaling

    #region Rotation

    private         bool        m_isRotating        = false;
    private         Vector2     m_facingDir         = Vector2.down;
    private         Vector3     m_startRot          = Vector3.zero;
    private         Vector3     m_targetRot         = Vector3.zero;
    private         float       m_rotateTimer       = 0f;

    /// <summary>
    /// Assign a new facing direction
    /// </summary>
    /// <param name="dir"></param>
    private void AssignFacing(Vector2 dir)
    {
        if (m_isRotating)
        {
            return;
        }

        if (dir != m_facingDir)
        {
            // Character needs to turn
            m_facingDir = dir;
            m_rotateTimer = 0f;
            m_startRot = m_modelRoot.localEulerAngles;
            float yRot = (-m_facingDir.x + ((m_facingDir.y + 1) * Mathf.Abs(m_facingDir.y))) * 90f;
            if (yRot > 180f && m_startRot.y < 90f)
            {
                yRot -= 360f;
            }
            if (m_startRot.y > 180f && yRot < 90f)
            {
                m_startRot.y -= 360f;
            }
            m_targetRot = new Vector3(0f, yRot, 0f);
            m_isRotating = true;
        } 
    }

    /// <summary>
    /// Updates the rotation
    /// </summary>
    private void UpdateRotation()
    {
        m_rotateTimer += Time.deltaTime;
        float lerpTime = m_rotateTimer / m_rotateTime;
        m_modelRoot.localEulerAngles = Vector3.Lerp(m_startRot, m_targetRot, lerpTime);
        if (lerpTime >= 1.0f)
        {
            m_modelRoot.localEulerAngles = m_targetRot;
            m_isRotating = false;
        }
    }

    #endregion // Rotation

    #endregion // Movement

    #region Collision
    
    private         Collider    m_collider          = null;

    /// <summary>
    /// Enables the physics
    /// </summary>
    private void EnablePhysics()
    {
        m_collider.enabled = true;
    }

    /// <summary>
    /// Disables the physics
    /// </summary>
    private void DisablePhysics()
    {
        m_collider.enabled = false;
    }

    /// <summary>
    /// Called when the player collides with a vehicle
    /// </summary>
    /// <param name="vehicleObj"></param>
    private void OnCollideWithVehicle(GameObject vehicleObj)
    {
        Vehicle vehicle = vehicleObj.GetComponent<Vehicle>();

        Vector3 dist = vehicleObj.transform.position - transform.position;
        dist.Normalize();
        Vector3 cornerVec = new Vector3(vehicle.VehicleLength, 0f, vehicle.VehicleWidth);
        cornerVec.Normalize();

        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.CarHit);

        if (Mathf.Abs(dist.x / dist.z) >= Mathf.Abs(cornerVec.x / cornerVec.z))
        {
            if (Mathf.Abs(vehicle.MoveSpeed) <= m_maxSpeedFlat &&
                ((vehicle.IsFacingLeft && transform.position.x < vehicleObj.transform.position.x) ||
                (!vehicle.IsFacingLeft && transform.position.x > vehicleObj.transform.position.x)))
            {
                // Front hit / Flattened
                m_modelRoot.localPosition = Vector3.zero;
                StartScale(m_dieScaleFlat, m_dieScaleTime);
            }
            else
            {
                // Back hit
                transform.Rotate(m_dieRotVert);
                transform.localScale = m_dieScaleVert;
                transform.parent = vehicleObj.transform;
                vehicle.SetPassenger(this);
                if (transform.position.x < vehicleObj.transform.position.x)
                {
                    transform.SetPosX(vehicleObj.transform.position.x - (vehicle.VehicleLength * 0.5f));
                }
                else
                {
                    transform.SetPosX(vehicleObj.transform.position.x + (vehicle.VehicleLength * 0.5f));
                }
            }

            if (transform.position.x < vehicleObj.transform.position.x)
            {
                StartDeath(DeathType.VEHICLE, ScreenshotDir.Side_Left);
            }
            else
            {
                StartDeath(DeathType.VEHICLE, ScreenshotDir.Side_Right);
            }
        }
        else
        {
            // Side hit
            transform.Rotate(m_dieRotHoriz);
            transform.localScale = m_dieScaleHoriz;
            transform.parent = vehicleObj.transform;
            vehicle.SetPassenger(this);

            if (transform.position.z < vehicleObj.transform.position.z)
            {
                transform.SetPosZ(vehicleObj.transform.position.z - (vehicle.VehicleWidth * 0.5f));
                StartDeath(DeathType.VEHICLE, ScreenshotDir.Front);
            }
            else
            {
                transform.SetPosZ(vehicleObj.transform.position.z + (vehicle.VehicleWidth * 0.5f));
                StartDeath(DeathType.VEHICLE, ScreenshotDir.Back);
            }
        }
    }

    /// <summary>
    /// Called when the player collides with a pickup
    /// </summary>
    /// <param name="vehicle"></param>
    private void OnCollideWithPickup(GameObject pickup)
    {
        Locator.GetGameManager().NotifyCoinGet();
    }

    /// <summary>
    /// Called when the player collides with a platform
    /// </summary>
    /// <param name="platformObj"></param>
    private void OnCollideWithPlatform(GameObject platformObj)
    {
        if (m_targetPlatform != null && m_targetPlatform.gameObject == platformObj)
        {
            transform.parent = platformObj.transform;
            m_targetPlatform.SetPassenger(this);
        }
    }

    #endregion // Collision

    #region Input

    private     bool        m_isPressed     = false;

    /// <summary>
    /// Called when the character has been pressed
    /// </summary>
    private void NotifyPress()
    {
        m_isPressed = true;
        PrepareJump();
    }

    /// <summary>
    /// Called when the character has been released
    /// </summary>
    private void NotifyRelease()
    {
        m_isPressed = false;
        TryJump();
    }

    /// <summary>
    /// Called when the character has been swiped
    /// </summary>
    /// <param name="normDir">The swipe direction</param>
    private void NotifySwipe(Vector2 normDir)
    {
        AssignFacing(normDir);
    }

    #endregion // Input

    #region Death

    public enum DeathType
    {
        EAGLE,      // Idled too much, or took 3 steps back
        VEHICLE,    // Hit by vehicles or train
        DROWN,      // Drowned
        LOG         // Rode log into unplayable area
    }

    private         DeathType           m_deathType     = DeathType.EAGLE;
    private         ScreenshotDir       m_deathDir      = ScreenshotDir.Front;
    private         GameObject          m_eagle         = null;
    private         bool                m_eagleNearFlag = false;
    private         float               m_deathTimer    = 0f;

    /// <summary>
    /// Starts the death
    /// </summary>
    private void StartDeath(DeathType deathType, ScreenshotDir deathDir)
    {
        // Enter death state
        m_state = State.DEATH;
        m_deathType = deathType;
        m_deathDir = deathDir;
        DisablePhysics();
        DetachController();

        // Cancel jumps
        m_isJumping = false;
        m_isRotating = false;
        m_pendingJumpCount = 0;

        // Inform game manager
        Locator.GetGameManager().NotifyCharacterDeathStart(m_deathType);
        m_deathTimer = 0f;

        switch (m_deathType)
        {
            case DeathType.EAGLE:
                m_eagleNearFlag = false;
                // Get out of platform
                if (m_targetPlatform != null)
                {
                    m_targetPlatform.SetPassenger(null);
                }
                transform.parent = null;
                break;

            case DeathType.VEHICLE:
                // Take the screenshot immediately
                Locator.GetGameManager().CreateCharacterScreenshot(m_deathDir);
                PlayDeathSound();
                break;

            case DeathType.DROWN:
                // Get out of platform
                if (m_targetPlatform != null)
                {
                    m_targetPlatform.SetPassenger(null);
                }
                transform.parent = null;
                PlayDeathSound();
                break;

            case DeathType.LOG:
                // Take the screenshot immediately
                Locator.GetGameManager().CreateCharacterScreenshot(m_deathDir);
                break;
        }
    }

    /// <summary>
    /// Updates the death
    /// </summary>
    private void UpdateDeath()
    {
        m_deathTimer += Time.deltaTime;

        switch (m_deathType)
        {
            case DeathType.EAGLE:
                if (!m_eagleNearFlag &&
                    m_eagle.transform.position.z - transform.position.z <= m_eagleScreenshotDist)
                {
                    m_eagleNearFlag = true;
                    Locator.GetGameManager().CreateCharacterScreenshot(m_deathDir);
                    PlayDeathSound();
                }
                else if (m_eagleNearFlag &&
                         m_eagle.transform.position.z - transform.position.z <= m_eagleDisappearDist)
                {
                    // Make the character disappear, as if taken by the eagle
                    m_modelRoot.gameObject.SetActive(false);
                }
                if (m_eagleNearFlag && !m_eagle.activeSelf)
                {
                    m_state = State.NONE;
                    Locator.GetGameManager().NotifyCharacterDeathEnd();
                }
                break;

            case DeathType.VEHICLE:
                if (m_isScaling)
                {
                    UpdateScale();
                }
                else if (m_deathTimer >= m_deathTime)
                {
                    m_state = State.NONE;
                    Locator.GetGameManager().NotifyCharacterDeathEnd();
                }
                break;

            case DeathType.DROWN:
                m_modelRoot.Translate(Vector3.down * m_drownSpeed * Time.deltaTime);
                if (m_deathTimer >= m_deathTime)
                {
                    m_state = State.NONE;
                    Locator.GetGameManager().CreateCharacterScreenshot(m_deathDir);
                    Locator.GetGameManager().NotifyCharacterDeathEnd();
                }
                break;

            case DeathType.LOG:
                if (m_deathTimer >= m_deathTime)
                {
                    m_state = State.NONE;
                    Locator.GetGameManager().NotifyCharacterDeathEnd();
                }
                break;
        }
    }

    #endregion // Death

    #region Sounds

    private     SoundManager        m_soundManager      = null;

    /// <summary>
    /// Gets the sound manager instance
    /// </summary>
    private void GetSoundManagerInstance()
    {
        m_soundManager = (SoundManager)Locator.GetSoundManager();
    }

    /// <summary>
    /// Plays the character move sound
    /// </summary>
    private void PlayCharacterMoveSound()
    {
        m_soundManager.PlayOneShot(SoundInfo.SFXID.CharacterMove);
    }

    /// <summary>
    /// Plays the character call sound
    /// </summary>
    private void PlayCharacterCallSound()
    {
        if (Random.value <= m_charSoundProb && (m_charSound == null || !m_charSound.IsPlaying))
        {
            if (m_charSound != null)
            {
                m_charSound.Delete();
            }
            m_charSound = m_soundManager.PlayCharacterCallSound(m_type);
        }
    }
    /// <summary>
    /// Plays the death sound
    /// </summary>
    private void PlayDeathSound()
    {
        if (m_charSound != null)
        {
            m_charSound.Delete();
        }
        m_soundManager.PlayCharacterCrySound(m_type);
    }

    #endregion // Sounds
}