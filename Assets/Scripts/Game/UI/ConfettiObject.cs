/******************************************************************************
*  @file       ConfettiObject.cs
*  @brief      Handles a confetti object in the new character win animation
*  @author     Ron
*  @date       October 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class ConfettiObject : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="moveDir">Move direction of this confetti.</param>
    /// <param name="speed">Speed at which this confetti moves.</param>
    /// <param name="targetPosY">Target vertical level to move to.</param>
    /// <param name="rotation">The confetti model's rotation.</param>
    /// <param name="newCharAnim">New character anim instance handling this confetti.</param>
    /// <param name="arrayIndex">Index of this confetti in the array of confetti objects.</param>
    public void Initialize(Vector3 moveDir, float speed, float targetPosY, Vector3 rotation,
                           NewCharWinAnimator newCharAnim, int arrayIndex)
    {
        m_moveDir = moveDir;
        m_speed = speed;
        m_targetPosY = targetPosY;
        m_confettiRoot.eulerAngles = rotation;
        m_newCharAnim = newCharAnim;
        m_arrayIndex = arrayIndex;

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Sets this confetti object's color.
    /// </summary>
    public void SetColor(Color color)
    {
        m_modelRenderer.material.color = color;
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

    [SerializeField] private Transform  m_confettiRoot  = null;
    [SerializeField] private Renderer   m_modelRenderer = null;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isInitialized = false;
    private bool    m_isPaused      = false;

    private int     m_arrayIndex    = 0;

    private NewCharWinAnimator m_newCharAnim = null;

    #endregion // Variables

    #region Movement

    private Vector3 m_moveDir       = Vector3.zero;
    private float   m_speed         = 0.0f;
    private float   m_targetPosY    = 0.0f;

    /// <summary>
    /// Updates the movement.
    /// </summary>
    private void UpdateMovement()
    {
        this.transform.Translate(m_moveDir * m_speed * Time.deltaTime);
        // Check if target pos is reached
        if (this.transform.position.y < m_targetPosY)
        {
            m_newCharAnim.NotifyConfettiReachedTarget(m_arrayIndex);
        }
    }

    #endregion // Movement

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

        UpdateMovement();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
