/******************************************************************************
*  @file       Eagle.cs
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

public class Eagle : MonoBehaviour
{
	#region Public Interface

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
    /// Starts the movement
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    public void StartMovement(Vector3 startPos, Vector3 endPos)
    {
        // Assign move variables
        m_startPos = startPos;
        m_endPos = endPos;
        m_startPos.y = m_moveHeight;
        m_startPos.z += m_zSize;
        m_endPos.y = m_moveHeight;
        m_endPos.z -= m_zSize;
        m_moveTimer = 0f;
        transform.position = m_startPos;
        m_isMoving = true;

        // Determine movement duration and start time for lerp
        float dist = Vector3.Distance(m_endPos, m_startPos);
        m_moveDuration = dist / m_moveSpeed;

        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.EagleScreech);
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     float           m_moveSpeed     = 90f;
    [SerializeField]private     float           m_moveHeight    = 45f;
    [SerializeField]private     float           m_zSize         = 60f;

    #endregion // Serialized Variables

    #region Variables

    private     bool        m_isPaused         = false;

    #endregion // Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
	{
		if (m_isPaused)
        {
            return;
        }

        if (m_isMoving)
        {
            UpdateMovement();
        }
	}

    #endregion // MonoBehaviour

    #region Movement

    private             float           m_moveTimer     = 0f;
    private             float           m_moveDuration  = 0f;
    private             Vector3         m_startPos      = Vector3.zero;
    private             Vector3         m_endPos        = Vector3.zero;
    private             bool            m_isMoving      = false;

    /// <summary>
    /// Updates the movement
    /// </summary>
    private void UpdateMovement()
    {
        m_moveTimer += Time.deltaTime;
        float lerpTime = m_moveTimer / m_moveDuration;
        transform.position = Vector3.Lerp(m_startPos, m_endPos, lerpTime);
        if (lerpTime >= 1.0f)
        {
            m_moveTimer = 0f;
            m_isMoving = false;
            gameObject.SetActive(false);
        }
    }

    #endregion // Movement
}
