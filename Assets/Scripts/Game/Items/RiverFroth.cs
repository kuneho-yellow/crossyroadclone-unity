/******************************************************************************
*  @file       RiverFroth.cs
*  @brief      
*  @author     Lori
*  @date       January 1, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class RiverFroth : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initialize this instance
    /// </summary>
    public void Initialize()
    {
        
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
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     float           m_minDur        = 1f;
    [SerializeField]private     float           m_maxDur        = 5f;
    [SerializeField]private     float           m_minSpeed      = 15f;
    [SerializeField]private     float           m_maxSpeed      = 45f;
    [SerializeField]private     float           m_minXPos       = -6f;
    [SerializeField]private     float           m_maxXPos       = 6f;

    #endregion // Serialized Variables

    #region Variables

    private     bool        m_isPaused      = false;

    #endregion // Variables

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
        UpdateMovement();
	}

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
	{

	}

    #endregion // MonoBehaviour

    #region Movement

    private float           m_moveSpeed     = 0f;
    private float           m_moveTimer     = 0f;
    private float           m_moveDuration  = 0f;
 
    /// <summary>
    /// Starts the movement
    /// </summary>
    private void StartMovement()
    {
        if (m_moveSpeed < 0f)
        {
            m_moveSpeed = Random.Range(m_minSpeed, m_maxSpeed);
        }
        else if (m_moveSpeed > 0f)
        {
            m_moveSpeed = Random.Range(-m_minSpeed, -m_maxSpeed);
        }
        else
        {
            m_moveSpeed = Random.Range(-m_minSpeed, m_maxSpeed);
        }
        m_moveDuration = Random.Range(m_minDur, m_maxDur);
        m_moveTimer = 0f;
    }

    /// <summary>
    /// Updates the movement
    /// </summary>
    private void UpdateMovement()
    {
        m_moveTimer += Time.deltaTime;
        float newX = transform.position.x + (m_moveSpeed * Time.deltaTime);
        if (newX < m_minXPos)
        {
            transform.SetPosX(m_minXPos);
            StartMovement();
        }
        else if (newX > m_maxXPos)
        {
            transform.SetPosX(m_maxXPos);
            StartMovement();
        }
        else
        {
            transform.SetPosX(newX);
            if (m_moveTimer >= m_moveDuration)
            {
                StartMovement();
            }
        }
    }

    #endregion // Movement
}
