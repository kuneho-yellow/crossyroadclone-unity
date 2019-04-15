/******************************************************************************
*  @file       Platform.cs
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

public class Platform : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initialize this instance
    /// </summary>
    public void Initialize()
    {
        // Get the collider size
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            m_colliderSize = col.bounds.size;
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

        if (m_coinInstance != null)
        {
            m_coinInstance.Pause();
        }
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

        if (m_coinInstance != null)
        {
            m_coinInstance.Unpause();
        }
    }

    /// <summary>
    /// Activates this instance
    /// </summary>
    public virtual void Activate()
    {
        m_isSinking = false;
    }

    /// <summary>
    /// Deactivates this instance
    /// </summary>
    public virtual void Deactivate()
    {
        if (m_coinInstance != null)
        {
            m_coinInstance.Deactivate();
            m_coinInstance = null;
        }

        m_charPassenger = null;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

    /// <summary>
    /// Returns the platform length
    /// </summary>
    public float PlatformLength
    {
        get { return m_colliderSize.x; }
    }

    /// <summary>
    /// Returns the platform width
    /// </summary>
    public float PlatformWidth
    {
        get { return m_colliderSize.z;  }
    }

    /// <summary>
    /// Returns the platform height
    /// </summary>
    public float PlatformHeight
    {
        get { return m_colliderSize.y; }
    }

    /// <summary>
    /// Determine if this instance is facing left
    /// </summary>
    public bool IsFacingLeft
    {
        get { return Mathf.Approximately(transform.eulerAngles.y, 180f); }
    }

    /// <summary>
    /// Returns the model root
    /// </summary>
    public Transform ModelRoot
    {
        get { return m_modelRoot; }
    }

    /// <summary>
    /// Gets the nearest landing position
    /// </summary>
    /// <param name="origPos"></param>
    /// <returns></returns>
    public virtual Vector3 GetNearestLandingPos(Vector3 origPos)
    {
        return new Vector3(transform.position.x,
                           transform.position.y + PlatformHeight,
                           transform.position.z);
    }

    /// <summary>
    /// Gets the nearest landing position
    /// </summary>
    /// <param name="origPos"></param>
    /// <returns></returns>
    public virtual Vector3 GetRandomLandingPos()
    {
        return new Vector3(transform.position.x,
                           transform.position.y + PlatformHeight,
                           transform.position.z);
    }

    /// <summary>
    /// Starts the sinking
    /// </summary>
    public virtual void StartSinking()
    {
        if (!m_isSinking)
        {
            m_isSinking = true;
            m_sinkTimer = 0f;
            m_startSinkYPos = transform.position.y;
            m_endSinkPos = m_startSinkYPos - m_sinkDepth;

            PlaySinkingSound();
        }
    }

    /// <summary>
    /// Sets the coin instance
    /// </summary>
    /// <param name="coinInstance"></param>
    public virtual void SetCoinInstance(Coin coinInstance)
    {
        m_coinInstance = coinInstance;

        if (m_coinInstance != null)
        {
            m_coinInstance.transform.position = GetRandomLandingPos();
            m_coinInstance.transform.parent = transform;
            m_coinInstance.gameObject.SetActive(true);
            m_coinInstance.SetOnCoinGetDelegate(RemoveCoinInstance);
            m_coinInstance.Activate();
        }
    }

    /// <summary>
    /// Setsthe passenger
    /// </summary>
    /// <param name="passenger"></param>
    public void SetPassenger(Character passenger)
    {
        m_charPassenger = passenger;
    }

    #endregion // Public Interface
    
    #region Serialized Variables

    [SerializeField]protected     Transform         m_modelRoot     = null;
    [SerializeField]protected     float             m_sinkDuration  = 0.5f;
    [SerializeField]protected     float             m_sinkDepth     = 2f;

    #endregion // Serialized Variables

    #region Variables

    protected bool m_isPaused         = false;

    #endregion // Variables

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected virtual void Awake()
	{

	}

    /// <summary>
    /// Start this instance.
    /// </summary>
    protected virtual void Start()
	{
		
	}

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected virtual void Update()
	{
		if (m_isPaused)
        {
            return;
        }

        if (m_isSinking)
        {
            UpdateSinking();
        }
	}

    #endregion // MonoBehaviour

    #region Size

    protected           Vector3         m_colliderSize  = Vector3.zero;

    #endregion // Size

    #region Movement

    // Sinking movement
    protected           float           m_sinkTimer     = 0f;
    protected           float           m_startSinkYPos = 0f;
    protected           float           m_endSinkPos    = 0f;
    protected           bool            m_isSinking     = false;

    /// <summary>
    /// Updates the sinking
    /// </summary>
    protected virtual void UpdateSinking()
    {
        float halfDur = m_sinkDuration * 0.5f;
        m_sinkTimer += Time.deltaTime;
        float newYPos = 0f;

        if (m_sinkTimer <= halfDur)
        {
            newYPos = Mathf.Lerp(m_startSinkYPos, m_endSinkPos, m_sinkTimer / halfDur);
        }
        else if (m_sinkTimer < m_sinkDuration)
        {
            newYPos = Mathf.Lerp(m_endSinkPos, m_startSinkYPos, (m_sinkTimer - halfDur) / halfDur);
        }
        else
        {
            m_isSinking = false;
            newYPos = m_startSinkYPos;
        }

        transform.SetPosY(newYPos);
    }

    #endregion // Movement

    #region Coin

    protected           Coin            m_coinInstance      = null;

    /// <summary>
    /// Removes the coin instance
    /// </summary>
    protected void RemoveCoinInstance()
    {
        m_coinInstance.Deactivate();
        m_coinInstance = null;
    }

    #endregion // Coin

    #region Character

    protected Character m_charPassenger = null;

    #endregion // Character

    #region Sounds

    /// <summary>
    /// Plays the sinking sound
    /// </summary>
    protected virtual void PlaySinkingSound()
    {
        
    }

    #endregion // Sounds
}
