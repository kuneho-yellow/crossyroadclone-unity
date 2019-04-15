/******************************************************************************
*  @file       Train.cs
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

public class Train : Vehicle
{
    #region Sounds

    /// <summary>
    /// Updates the passing sound
    /// </summary>
    protected override void UpdatePassingSound()
    {
        if (Mathf.Abs(m_mapManager.GetCharacterCurrentPosition().z - transform.position.z)
            <= m_passingSoundDistance)
        {
            if (m_passingSound == null || !m_passingSound.IsPlaying)
            {
                if (m_passingSound != null)
                {
                    m_passingSound.Delete();
                }
                m_passingSound = m_soundManager.PlaySound(SoundInfo.SFXID.TrainPassing);
                m_passingSound.transform.parent = transform;
            }
        }
        else
        {
            if (m_passingSound != null)
            {
                m_passingSound.Delete();
            }
        }
    }

    /// <summary>
    /// Updates the horn sound
    /// </summary>
    protected override void UpdateHornSound()
    {
        m_hornTimer += Time.deltaTime;
        if (m_hornTimer >= m_hornTime && Random.value <= m_hornProb)
        {
            m_soundManager.PlayOneShot(SoundInfo.SFXID.TrainHorn, this.transform.position);
            m_hornTimer = 0f;
        }
    }

    #endregion // Sounds
}
