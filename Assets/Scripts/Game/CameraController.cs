/******************************************************************************
*  @file       CameraController.cs
*  @brief      Handles the game camera
*  @author     Lori
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class CameraController : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize()
    {
        // Get the camera component
        if (m_camera == null)
        {
            m_camera = GetComponent<Camera>();
            if (m_camera == null)
            {
                m_camera = Camera.main;
                if (m_camera == null)
                {
                    return;
                }
            }
        }

        SetInitVariables();

        // Set the initialized flag
        m_isInitialized = true;
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
        ResetToInitState();
        m_target = null;
        m_state = State.NONE;
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

    /// <summary>
    /// Sets the follow target
    /// </summary>
    /// <param name="followTarget"></param>
    public void SetFollowTarget(Transform followTarget)
    {
        m_target = followTarget;
        m_state = State.FOLLOW;
    }

    /// <summary>
    /// Stop following the target
    /// </summary>
    public void StopFollowTarget()
    {
        m_target = null;
        m_state = State.NONE;
    }

    /// <summary>
    /// Start focusing on the target
    /// </summary>
    public void StartFocusTarget(Transform focusTarget, bool isFocusZ, bool isZooming, bool isShaking)
    {
        m_target = focusTarget;
        m_isFocusZ = isFocusZ;
        m_isZooming = isZooming;
        m_isShaking = isShaking;
        m_focusTimer = 0f;
        m_focusStartPos = m_camera.transform.position;
        m_focusStartDistance = m_target.position - m_camera.transform.position;
        m_state = State.FOCUS;
    }

    /// <summary>
    /// Stop focusing on the target
    /// </summary>
    public void StopFocusTarget()
    {
        m_target = null;
        m_state = State.NONE;
    }

    /// <summary>
    /// Gets the target position
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTargetPosition()
    {
        if (m_state == State.FOLLOW)
        {
            return m_target.transform.position;
        }
        return transform.position + m_defaultDist;
    }

    /// <summary>
    /// Gets whether the camera has followed the target
    /// </summary>
    public bool IsTargetFollowed
    {
        get
        {
            if (m_target != null)
            {
                return m_target.position.z - m_camera.transform.position.z <= m_minDistZ;
            }
            return false;
        }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private Camera          m_camera                = null;

    [Header("Movement Bounds")]
    [SerializeField]private float           m_minPosX               = -30f;
    [SerializeField]private float           m_maxPosX               = 60;
    [Header("Distance from target")]
    [SerializeField]private Vector3         m_defaultDist           = new Vector3(-20f, -100f, 50f);
    [SerializeField]private float           m_minDistX              = 0f;
    [SerializeField]private float           m_maxDistX              = 15f;
    [SerializeField]private float           m_minDistZ              = 50f;
    [SerializeField]private float           m_maxDistZ              = 65f;
    [Header("Move speed")]
    [SerializeField]private float           m_minSpeedX             = 0f;
    [SerializeField]private float           m_maxSpeedX             = 50.0f;
    [SerializeField]private float           m_minSpeedZ             = 5.0f;
    [SerializeField]private float           m_maxSpeedZ             = 70.0f; // Depends on character jump speed
    [Header("Focus")]
    [SerializeField]private Vector3         m_focusDist             = new Vector3(-20f, -100f, 75f);
    [SerializeField]private float           m_focusTime             = 0.25f;
    [Header("Zoom")]
    [SerializeField]private float           m_portraitSize          = 80f;
    [SerializeField]private float           m_landscapeSize         = 50f;
    [SerializeField]private float           m_portraitZoomSize      = 40f;
    [SerializeField]private float           m_landscapeZoomSize     = 40f;
    [SerializeField]private float           m_zoomTime              = 0.25f;
    [Header("Shake")]
    [SerializeField]private float           m_shakeFactor           = 3.0f;
    [SerializeField]private float           m_shakeTime             = 0.5f;


    #endregion // Serialized Variables

    #region Variables
    
    private bool m_isInitialized    = false;
    private bool m_isPaused         = false;

    #endregion // Variables

    #region MonoBehaviour
	
	/// <summary>
	/// Late Update this instance.
	/// </summary>
	private void LateUpdate()
	{
        if (!m_isInitialized)
        {
            return;
        }

		if (m_isPaused)
        {
            return;
        }

        if (Locator.GetUIManager().HasScreenOrientationChanged)
        {
#if !UNITY_EDITOR
            m_isLandscape = Screen.width > Screen.height;
#endif
            if (m_isZoomedIn)
            {
                m_camera.orthographicSize = m_isLandscape ? m_landscapeZoomSize : m_portraitZoomSize;
            }
            else if (!m_isZooming)
            {
                m_camera.orthographicSize = m_isLandscape ? m_landscapeSize : m_portraitSize;
            }
        }

        switch (m_state)
        {
            case State.NONE:
                break;

            case State.FOLLOW:
                UpdateFollowMovement();
                break;

            case State.FOCUS:
                UpdateFocusMovement();
                break;
        }
    }

#endregion // MonoBehaviour

#region State

    private enum State
    {
        NONE,
        FOLLOW,
        FOCUS
    }

    private     State       m_state             = State.NONE;

#endregion // State

#region Movement
    private     bool        m_isLandscape           = true;
    private     Vector3     m_initPosition          = Vector3.zero;
    private     Transform   m_target                = null;
    private     float       m_focusTimer            = 0f;
    private     Vector3     m_focusStartPos         = Vector3.zero;
    private     Vector3     m_focusStartDistance    = Vector3.zero;
    private     bool        m_isFocusZ              = false;
    private     bool        m_isZooming             = false;
    private     bool        m_isZoomedIn            = false;
    private     bool        m_isShaking             = false;

    /// <summary>
    /// Sets the variables for the camera's initial state
    /// </summary>
    private void SetInitVariables()
    {
        m_initPosition = m_camera.transform.position;

#if !UNITY_EDITOR
        m_isLandscape = Screen.width > Screen.height;
#endif
        m_camera.orthographicSize = m_isLandscape ? m_landscapeSize : m_portraitSize;
    }

    /// <summary>
    /// Resets the camera to its initial state
    /// </summary>
    private void ResetToInitState()
    {
        m_camera.transform.position = m_initPosition;
        m_camera.orthographicSize = m_isLandscape ? m_landscapeSize : m_portraitSize;
        m_isZoomedIn = false;
    }

    /// <summary>
    /// Updates the follow movement
    /// </summary>
    private void UpdateFollowMovement()
    {
        Vector3 moveVec = Vector3.zero;
        Vector3 currentDistance = m_target.position - m_camera.transform.position;
        float lerpT = 0f;
        float moveSpeed = 0f;

        // Check difference in x positions
        //moveVec.x = currentDistance.x - m_defaultDist.x;

        float distX = currentDistance.x - m_defaultDist.x;
        if (Mathf.Abs(distX) > m_minDistX)
        {
            lerpT = (Mathf.Abs(distX) - m_minDistX) / (m_maxDistX - m_minDistX);
            moveSpeed = Mathf.Lerp(m_minSpeedX, m_maxSpeedX, lerpT);
            if (distX < 0)
            {
                moveSpeed = -moveSpeed;
            }
            moveVec.x = moveSpeed * Time.deltaTime;
        }

        // Check difference in z positions
        float deltaZ = currentDistance.z - m_minDistZ;
        lerpT = deltaZ / (m_maxDistZ - m_minDistZ);
        moveSpeed = Mathf.Lerp(m_minSpeedZ, m_maxSpeedZ, lerpT);
        moveVec.z = moveSpeed * Time.deltaTime;

        // Assign the new position
        Vector3 newPos = m_camera.transform.position + moveVec;
        newPos.x = Mathf.Clamp(newPos.x, m_minPosX, m_maxPosX);
        m_camera.transform.position = newPos;
    }

    /// <summary>
    /// Updates the focus movement
    /// </summary>
    private void UpdateFocusMovement()
    {
        m_focusTimer += Time.deltaTime;
        float lerpT = m_focusTimer / m_focusTime;

        // Focus
        Vector3 newPos = m_focusStartPos;
        newPos.x = m_target.transform.position.x - Mathf.Lerp(m_focusStartDistance.x, m_focusDist.x, lerpT);
        newPos.x = Mathf.Clamp(newPos.x, m_minPosX, m_maxPosX);
        newPos.y = m_initPosition.y;
        if (m_isFocusZ)
        {
            newPos.z = m_target.transform.position.z - Mathf.Lerp(m_focusStartDistance.z, m_focusDist.z, lerpT);
        }
        m_camera.transform.position = newPos;
        // Zoom
        if (m_isZooming)
        {
            m_camera.orthographicSize = Mathf.Lerp(m_isLandscape ? m_landscapeSize : m_portraitSize,
                                                    m_isLandscape ? m_landscapeZoomSize : m_portraitZoomSize, lerpT);
            if (m_focusTimer >= m_zoomTime)
            {
                m_isZooming = false;
                m_isZoomedIn = true;
            }
        }
        // Shake
        if (m_isShaking)
        {
            Vector2 shakeVec = Random.insideUnitCircle * m_shakeFactor;
            newPos.x += shakeVec.x;
            newPos.z += shakeVec.y;
            m_camera.transform.position = newPos;
            if (m_focusTimer >= m_shakeTime)
            {
                m_isShaking = false;
            }
        }
    }

#endregion // Movement
}
