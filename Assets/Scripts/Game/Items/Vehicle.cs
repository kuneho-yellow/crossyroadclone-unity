/******************************************************************************
*  @file       Vehicle.cs
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

public class Vehicle : MonoBehaviour
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
            m_length = col.bounds.size.x;
            m_width = col.bounds.size.z;
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
    /// Deactivates this instance
    /// </summary>
    public virtual void Deactivate()
    {
        StopSounds();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts the movement
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="startTime"></param>
    /// <param name="isLooping"></param>
    public void StartMovement(float speed, Vector3 startPos, Vector3 endPos,
                              float startTime = 0f, bool isLooping = true)
    {
        // Assign move variables
        m_moveSpeed = speed;
        m_startPos = startPos;
        m_endPos = endPos;
        m_isLooping = isLooping;

        // Determine movement duration and start time for lerp
        float dist = Vector3.Distance(endPos, startPos);
        m_moveDuration = dist / speed;
        m_moveTimer = startTime * m_moveDuration;

        // Make this instance face the movement direction
        if (m_startPos.x < m_startPos.y)
        {
            transform.SetRotY(0f);
        }
        else
        {
            transform.SetRotY(180f);
        }

        // Position the vehicle at the right place
        transform.position = Vector3.Lerp(m_startPos, m_endPos, startTime);

        // Prepare sounds
        StartSounds();

        m_isMoving = true;
    }

    /// <summary>
    /// Ends the movement
    /// </summary>
    public void EndMovement(bool moveToEndPosition = false)
    {
        if (m_isMoving)
        {
            return;
        }
        if (moveToEndPosition)
        {
            transform.position = m_endPos;
        }
        m_isMoving = false;
    }

    /// <summary>
    /// Setsthe passenger
    /// </summary>
    /// <param name="passenger"></param>
    public void SetPassenger(Character passenger)
    {
        m_charPassenger = passenger;
    }

    /// <summary>
    /// Sets the map manager instance
    /// </summary>
    /// <param name="mapManager"></param>
    public void SetMapManagerInstance(MapManager mapManager)
    {
        m_mapManager = mapManager;
    }

    /// <summary>
    /// Gets a random speed value
    /// </summary>
    /// <returns></returns>
    public float GetRandomSpeed()
    {
        return Random.Range(m_minSpeed, m_maxSpeed);
    }

    /// <summary>
    /// Returns the move speed
    /// </summary>
    public float MoveSpeed
    {
        get { return m_isMoving ? m_moveSpeed : 0f; }
    }

    /// <summary>
    /// Returns the vehicle length
    /// </summary>
    public float VehicleLength
    {
        get { return m_length; }
    }

    /// <summary>
    /// Returns the vehicle width
    /// </summary>
    public float VehicleWidth
    {
        get { return m_width;  }
    }

    /// <summary>
    /// Determine if this instance is facing left
    /// </summary>
    public bool IsFacingLeft
    {
        get { return Mathf.Approximately(transform.eulerAngles.y, 180f); }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]protected       float       m_minSpeed              = 0f;
    [SerializeField]protected       float       m_maxSpeed              = 0f;
    [SerializeField]protected       float       m_passingSoundDistance  = 30f;
    [SerializeField]protected       float       m_hornTimeMin           = 20f;
    [SerializeField]protected       float       m_hornTimeMax           = 50f;
    [SerializeField]protected       float       m_hornProb              = 0.01f;

    #endregion // Serialized Variables

    #region Variables

    protected bool m_isPaused         = false;

    #endregion // Variables

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected void Awake()
	{

	}

    /// <summary>
    /// Start this instance.
    /// </summary>
    protected void Start()
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

        if (m_isMoving)
        {
            UpdateMovement();
            UpdateSounds();
        }
	}

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected void OnDestroy()
	{

	}

    #endregion // MonoBehaviour

    #region Size

    protected           float           m_length        = 30f;
    protected           float           m_width         = 15f;

    #endregion // Size

    #region Movement

    protected           float           m_moveSpeed     = 0f;
    protected           float           m_moveTimer     = 0f;
    protected           float           m_moveDuration  = 0f;
    protected           Vector3         m_startPos      = Vector3.zero;
    protected           Vector3         m_endPos        = Vector3.zero;
    protected           bool            m_isMoving      = false;
    protected           bool            m_isLooping     = true;

    /// <summary>
    /// Updates the movement
    /// </summary>
    protected void UpdateMovement()
    {
        m_moveTimer += Time.deltaTime;
        float lerpTime = m_moveTimer / m_moveDuration;
        transform.position = Vector3.Lerp(m_startPos, m_endPos, lerpTime);
        if (lerpTime >= 1.0f)
        {
            m_moveTimer = 0f;
            m_isMoving = m_isLooping;

            if (!m_isMoving)
            {
                StopSounds();
            }

            // Inform the passenger that the end of the map has been reached
            if (m_charPassenger != null)
            {
                m_charPassenger.NotifyReachEndOfMap();
                m_charPassenger = null;
            }
        }
    }

    #endregion // Movement

    #region Sounds

    protected       MapManager          m_mapManager        = null;
    protected       SoundManager        m_soundManager      = null;
    protected       float               m_hornTimer         = 0f;
    protected       float               m_hornTime          = 0f;
    protected       SoundObject         m_passingSound      = null; 

    /// <summary>
    /// Starts the sounds
    /// </summary>
    protected void StartSounds()
    {
        if (m_soundManager == null)
        {
            m_soundManager = (SoundManager)Locator.GetSoundManager();
        }

        StopSounds();

        m_hornTime = Random.Range(m_hornTimeMin, m_hornTimeMax);
        m_hornTimer = Random.Range(0f, m_hornTime);
    }

    /// <summary>
    /// Updates the sounds
    /// </summary>
    protected void UpdateSounds()
    {
        UpdatePassingSound();
        UpdateHornSound();
    }

    /// <summary>
    /// Stops the sounds
    /// </summary>
    protected void StopSounds()
    {
        if (m_passingSound != null)
        {
            m_passingSound.Delete();
            m_passingSound = null;
        }

        m_hornTime = 0f;
        m_hornTimer = 0f;
    }

    /// <summary>
    /// Updates the passing sound
    /// </summary>
    protected virtual void UpdatePassingSound()
    {
        if (Vector3.Distance(m_mapManager.GetCharacterCurrentPosition(), transform.position)
            <= m_passingSoundDistance)
        {
            if (m_passingSound == null || !m_passingSound.IsPlaying)
            {
                if (m_passingSound != null)
                {
                    m_passingSound.Delete();
                }
                m_passingSound = m_soundManager.PlayCarPassingSound();
                m_passingSound.transform.parent = transform;
            }
        }
        else
        {
            if (m_passingSound != null && !m_passingSound.IsPlaying)
            {
                m_passingSound.Delete();
            }
        }
    }

    /// <summary>
    /// Updates the horn sound
    /// </summary>
    protected virtual void UpdateHornSound()
    {
        m_hornTimer += Time.deltaTime;
        if (m_hornTimer >= m_hornTime && Random.value <= m_hornProb)
        {
            m_soundManager.PlayCarHornSound(transform.position);
            m_hornTimer = 0f;
        }
    }

    #endregion // Sounds

    #region Character

    protected       Character       m_charPassenger     = null;

    #endregion // Character
}
