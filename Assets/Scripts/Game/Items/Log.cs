/******************************************************************************
*  @file       Log.cs
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

public class Log : Platform
{
    #region Public Interface

    public void StartMovement(float fastSpeed, float normalSpeed,
                              Vector3 fastStartPos, Vector3 normalStartPos,
                              Vector3 normalEndPos, Vector3 fastEndPos,
                              OnEndMovement onEndMovement, bool hasCustomStart,
                              Vector3 startPos)
    {
        m_fastSpeed = fastSpeed;
        m_normalSpeed = normalSpeed;
        m_fastStartPos = fastStartPos;
        m_normalStartPos = normalStartPos;
        m_normalEndPos = normalEndPos;
        m_fastEndPos = fastEndPos;
        m_onEndMovement = onEndMovement;
        if (m_fastStartPos.x < m_fastEndPos.x)
        {
            transform.SetRotY(0f);
            m_normalEndPos.x += PlatformLength;
            m_fastEndPos.x += PlatformLength;
        }
        else
        {
            transform.SetRotY(180f);
            m_normalEndPos.x -= PlatformLength;
            m_fastEndPos.x -= PlatformLength;
        }

        if (hasCustomStart)
        {
            transform.position = startPos;

            // Determine the current move state according to start position
            if (m_fastStartPos.x < m_fastEndPos.x)
            {
                // Left to right
                if (startPos.x < m_fastStartPos.x)
                {
                    m_moveState = MoveState.NONE;
                }
                else if (startPos.x < m_normalStartPos.x)
                {
                    m_moveState = MoveState.FAST_START;
                }
                else if (startPos.x < m_normalEndPos.x)
                {
                    m_moveState = MoveState.NORMAL;
                }
                else if (startPos.x < m_fastEndPos.x)
                {
                    m_moveState = MoveState.FAST_END;
                }
                else
                {
                    m_moveState = MoveState.NONE;
                }
            }
            else
            {
                // Right to left
                if (startPos.x > m_fastStartPos.x)
                {
                    m_moveState = MoveState.NONE;
                }
                else if (startPos.x > m_normalStartPos.x)
                {
                    m_moveState = MoveState.FAST_START;
                }
                else if (startPos.x > m_normalEndPos.x)
                {
                    m_moveState = MoveState.NORMAL;
                }
                else if (startPos.x > m_fastEndPos.x)
                {
                    m_moveState = MoveState.FAST_END;
                }
                else
                {
                    m_moveState = MoveState.NONE;
                }
            }
            StartMoveState();
        }
        else
        {
            m_moveState = MoveState.NONE;
            UpdateMoveState();
        } 
    }

    /// <summary>
    /// Gets the nearest landing position
    /// </summary>
    /// <param name="origPos"></param>
    /// <returns></returns>
    public override Vector3 GetNearestLandingPos(Vector3 origPos)
    {
        Vector3 landingPos = transform.position;
        landingPos.y += PlatformHeight;
        float deltaTile = m_tileSize;

        if (!IsFacingLeft)
        {
            deltaTile = -m_tileSize;
        }

        landingPos.x += deltaTile * 0.5f;
        float diff = Mathf.Abs(landingPos.x - origPos.x);

        for (int i = 1; i < m_tileCount; ++i)
        {
            float nextPosX = landingPos.x + deltaTile;
            float nextDiff = Mathf.Abs(nextPosX - origPos.x);
            if (nextDiff <= diff)
            {
                diff = nextDiff;
                landingPos.x = nextPosX;
            }
            else
            {
                break;
            }
        }

        return landingPos;
    }

    /// <summary>
    /// Gets the nearest landing position
    /// </summary>
    /// <param name="origPos"></param>
    /// <returns></returns>
    public override Vector3 GetRandomLandingPos()
    {
        Vector3 landingPos = transform.position;
        landingPos.y += PlatformHeight;
        float deltaTile = m_tileSize;

        if (!IsFacingLeft)
        {
            deltaTile = -m_tileSize;
        }

        landingPos.x += deltaTile * 0.5f;
        int tileIndex = Random.Range(0, m_tileCount);
        landingPos.x = landingPos.x + (deltaTile * tileIndex);

        return landingPos;
    }

    public delegate void OnEndMovement(Log thisLog);

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     int         m_tileCount     = 1;
    [SerializeField]private     float       m_tileSize      = 15f;

    #endregion // Serialized Variables

    #region Variables

    #endregion // Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance
    /// </summary>
    protected override void Update()
    {
        if (m_isPaused)
        {
            return;
        }

        if (m_isMoving)
        {
            UpdateMovement();
        }

        base.Update();
    }

    #endregion // MonoBehaviour

    #region Size

    #endregion // Size

    #region Movement


    private     float               m_moveSpeed         = 0f;
    private     float               m_moveTimer         = 0f;
    private     float               m_moveDuration      = 0f;
    private     Vector3             m_startPos          = Vector3.zero;
    private     Vector3             m_endPos            = Vector3.zero;
    private     bool                m_isMoving          = false;
    private     OnEndMovement       m_onEndMovement     = null;
    private     float               m_fastSpeed         = 0f;
    private     float               m_normalSpeed       = 0f;
    private     Vector3             m_fastStartPos      = Vector3.zero;
    private     Vector3             m_normalStartPos    = Vector3.zero;
    private     Vector3             m_normalEndPos      = Vector3.zero;
    private     Vector3             m_fastEndPos        = Vector3.zero;
    private     MoveState           m_moveState         = MoveState.NONE;

    private enum MoveState
    {
        FAST_START,
        NORMAL,
        FAST_END,
        NONE
    }

    /// <summary>
    /// Starts the move state
    /// </summary>
    private void StartMoveState()
    {
        switch (m_moveState)
        {
            case MoveState.NONE:
                m_isMoving = false;
                transform.position = m_startPos;
                break;

            case MoveState.FAST_START:
                m_isMoving = true;
                m_moveSpeed = m_fastSpeed;
                m_startPos = m_fastStartPos;
                m_endPos = m_normalStartPos;
                break;

            case MoveState.NORMAL:
                m_isMoving = true;
                 m_moveSpeed = m_normalSpeed;
                m_startPos = m_normalStartPos;
                m_endPos = m_normalEndPos;
                break;

            case MoveState.FAST_END:
                m_isMoving = true;
                m_moveSpeed = m_fastSpeed;
                m_startPos = m_normalEndPos;
                m_endPos = m_fastEndPos;
                break;
        }

        if (m_moveState != MoveState.NONE)
        {
            m_moveDuration = Vector3.Distance(m_startPos, m_endPos) / m_moveSpeed;
            float percTravel = (transform.position.x - m_startPos.x) / (m_endPos.x - m_startPos.x);
            m_moveTimer = m_moveDuration * percTravel;
        }
        else
        {
            m_moveDuration = 0f;
            m_moveTimer = 0f;
        }
    }

    /// <summary>
    /// Updates the move state
    /// </summary>
    private void UpdateMoveState()
    {
        switch (m_moveState)
        {
            case MoveState.NONE:
                m_isMoving = true;
                m_moveState = MoveState.FAST_START;
                m_moveSpeed = m_fastSpeed;
                m_startPos = m_fastStartPos;
                m_endPos = m_normalStartPos;
                transform.position = m_startPos;
                break;

            case MoveState.FAST_START:
                m_isMoving = true;
                m_moveState = MoveState.NORMAL;
                m_moveSpeed = m_normalSpeed;
                m_startPos = m_normalStartPos;
                m_endPos = m_normalEndPos;
                break;

            case MoveState.NORMAL:
                m_isMoving = true;
                m_moveState = MoveState.FAST_END;
                m_moveSpeed = m_fastSpeed;
                m_startPos = m_normalEndPos;
                m_endPos = m_fastEndPos;
                break;

            case MoveState.FAST_END:
                m_isMoving = false;
                m_moveState = MoveState.NONE;

                // Inform the passenger that the end of the map has been reached
                if (m_charPassenger != null)
                {
                    if (m_charPassenger.transform.parent == transform)
                    {
                        m_charPassenger.NotifyReachEndOfMap();
                    }
                    m_charPassenger = null;
                }

                if (m_onEndMovement != null)
                {
                    m_onEndMovement(this);
                }
                break;

        }

        m_moveTimer = 0f;
        m_moveDuration = Vector3.Distance(m_startPos, m_endPos) / m_moveSpeed;
    }

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
            UpdateMoveState();
        }
    }

    #endregion // Movement

    #region Sounds

    /// <summary>
    /// Plays the sinking sound
    /// </summary>
    protected override void PlaySinkingSound()
    {
        ((SoundManager)Locator.GetSoundManager()).PlayWaterLogStepSound();
    }

    #endregion // Sounds
}
