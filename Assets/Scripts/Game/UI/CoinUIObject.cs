/******************************************************************************
*  @file       CoinUIObject.cs
*  @brief      Handles a coin object in the "win coins" animation
*  @author     Ron
*  @date       September 29, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class CoinUIObject : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CoinWinAnimator coinWinUI, float initialSpeed, float initialRotSpeed,
                           Vector3 moveDir, float deceleration)
    {
        m_coinWinUI = coinWinUI;
        m_speed = initialSpeed;
        m_rotSpeed = initialRotSpeed;
        m_moveDir = moveDir;
        m_deceleration = deceleration;

        // Initialize coin animator
        //  State 1: Zero scale
        //  State 2: Normal scale
        m_coinScaleAnimator = new UIAnimator(m_coinSprite.transform);
        m_coinScaleAnimator.SetScaleAnimation(Vector3.zero, m_coinSprite.transform.localScale);
        m_coinScaleAnimator.SetAnimSpeed(m_coinScaleAnimSpeed);

        // Initialize collect animator
        //  State 1: Original scale
        //  State 2: Zero scale
        m_collectAnimator = new UIAnimator(m_collectSprite.transform);
        m_collectAnimator.SetScaleAnimation(m_collectSprite.transform.localScale, Vector3.zero);
        m_collectAnimator.SetAnimSpeed(m_collectAnimSpeed);

        Reset();

        // Start coin scale animation
        m_coinScaleAnimator.AnimateToState2();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts the "collect" animation.
    /// </summary>
    public void Collect()
    {
        // Hide the coin sprite and show the "collect" sprite
        m_coinSprite.gameObject.SetActive(false);
        m_collectSprite.gameObject.SetActive(true);
        // Start "collect" anim
        m_collectAnimator.AnimateToState2();
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
        // Show the coin sprite and hide the "collect" sprite
        m_coinSprite.gameObject.SetActive(true);
        m_collectSprite.gameObject.SetActive(false);

        // Reset coin sprite to zero scale
        m_coinScaleAnimator.ResetToState1();
        // Reset collect sprite to original scale
        m_collectAnimator.ResetToState1();

        // Randomize the collect sprite's rotation
        m_collectSprite.transform.eulerAngles = new Vector3(Random.Range(-RANDOM_ROT_ANGLE_LIMIT, RANDOM_ROT_ANGLE_LIMIT),
                                                            Random.Range(-RANDOM_ROT_ANGLE_LIMIT, RANDOM_ROT_ANGLE_LIMIT),
                                                            Random.Range(-RANDOM_ROT_ANGLE_LIMIT, RANDOM_ROT_ANGLE_LIMIT));
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

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private SpriteRenderer m_coinSprite        = null;
    [SerializeField] private SpriteRenderer m_collectSprite     = null;

    [Tooltip("Speed at which the coin sprite grows from nothing to normal size")]
    [SerializeField] private float      m_coinScaleAnimSpeed    = 5.0f;

    [Tooltip("Speed at which the collect sprite shrinks from normal size to nothing")]
    [SerializeField] private float      m_collectAnimSpeed      = 10.0f;

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized     = false;
    private bool    m_isPaused          = false;

    private CoinWinAnimator m_coinWinUI   = null;

    #endregion // Variables

    #region Movement

    // Coin
    private float m_speed = 0.0f;
    private float m_rotSpeed = 0.0f;
    private Vector3 m_moveDir = Vector3.zero;
    // Rate at which both linear and angular speed decrease to 0
    private float m_deceleration = 0.0f;

    /// <summary>
    /// Updates the coin's movement.
    /// </summary>
    private void UpdateMovement()
    {
        // If coin sprite is hidden, stop movement
        if (!m_coinSprite.gameObject.activeInHierarchy)
        {
            return;
        }
        // Apply deceleration
        if (m_speed > 0)
        {
            m_speed -= m_deceleration * Time.deltaTime;
            m_rotSpeed -= m_deceleration * Time.deltaTime;
            this.transform.Translate(m_speed * m_moveDir * Time.deltaTime);
            this.transform.Rotate(m_rotSpeed * Vector3.forward * Time.deltaTime);
        }
    }

    #endregion // Movement

    #region Coin Scale Animation

    private UIAnimator m_coinScaleAnimator = null;

    #endregion // Coin Scale Animation

    #region Collect Animation

    private const float RANDOM_ROT_ANGLE_LIMIT = 70.0f;

    private UIAnimator m_collectAnimator = null;

    /// <summary>
    /// Updates the "collect" animation.
    /// </summary>
    private void UpdateCollectAnim()
    {
        m_collectAnimator.Update(Time.deltaTime);
        // If animation reaches target state (collect sprite at zero scale), notify coin win UI
        if (m_collectAnimator.IsInState2)
        {
            m_coinWinUI.NotifyCoinCollected(this);
        }
    }

    #endregion // Collect Animation

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

        UpdateMovement();
        UpdateCollectAnim();
        m_coinScaleAnimator.Update(Time.deltaTime);
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
