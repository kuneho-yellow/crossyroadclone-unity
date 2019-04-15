/******************************************************************************
*  @file       LilyPad.cs
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

public class LilyPad : Platform
{
    #region Public Interface

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     float       m_rotateSpeed   = 10f;
    [SerializeField]private     float       m_minYRotation  = 0f;
    [SerializeField]private     float       m_maxYRotation  = 90f;

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Start this instance
    /// </summary>
    protected override void Start()
    {
        StartRotation();
    }

    /// <summary>
    /// Update this instance
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (m_isPaused)
        {
            return;
        }

        UpdateRotation();
    }

    #endregion // MonoBehaviour

    #region Movement

    /// <summary>
    /// Starts the rotation
    /// </summary>
    private void StartRotation()
    {
        // Randomize the initial rotation
        float randRotY = Random.Range(m_minYRotation, m_maxYRotation);
        m_modelRoot.SetRotY(randRotY);

        // Randomize the initial direction
        m_rotateSpeed = Random.Range(0, 2) == 0 ? m_rotateSpeed : -m_rotateSpeed;
    }

    /// <summary>
    /// Updates the rotation
    /// </summary>
    private void UpdateRotation()
    {
        m_modelRoot.Rotate(Vector3.up * m_rotateSpeed * Time.deltaTime);
        if (m_modelRoot.eulerAngles.y <= m_minYRotation ||
            m_modelRoot.eulerAngles.y >= m_maxYRotation)
        {
            m_rotateSpeed *= -1f;
        }
    }

    #endregion // Movement

    /// <summary>
    /// Plays the sinking sound
    /// </summary>
    protected override void PlaySinkingSound()
    {
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.WaterLilyStep);
    }
}
