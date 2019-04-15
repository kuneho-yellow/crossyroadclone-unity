/******************************************************************************
*  @file       GachaAnimEvents.cs
*  @brief      Holds events for the gacha UI animation
*  @author     Ron
*  @date       September 17, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class GachaAnimEvents : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public void Pause()
    {
        // Empty
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public void Unpause()
    {
        // Empty
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField] private GachaUI m_gachaUI = null;

    #endregion // Serialized Variables

    #region Animation Events

    /// <summary>
    /// Called when player has just spent coins for the gacha.
    /// </summary>
    private void OnGachaBuy()
    {
        // Play gacha coin jingling sound (gacha open)
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.GachaOpen);
    }

    /// <summary>
    /// Called when the gacha open animation starts.
    /// </summary>
    private void OnGachaOpenAnimStart()
    {
        // Play gacha jingle
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.GachaJingle);
    }

    /// <summary>
    /// Called when the gacha open animation ends.
    /// </summary>
    private void OnGachaOpenAnimEnd()
    {
        // Notify GachaUI
        m_gachaUI.NotifyGachaOpenAnimationEnd();
    }

    #endregion // Animation Events
}
