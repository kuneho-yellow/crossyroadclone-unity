/******************************************************************************
*  @file       GiftAnimEvents.cs
*  @brief      Holds events for the gift UI animation
*  @author     Ron
*  @date       September 14, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class GiftAnimEvents : MonoBehaviour
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

    [SerializeField] private GiftUI m_giftUI = null;

    #endregion // Serialized Variables

    #region Animation Events

    /// <summary>
    /// Called when the gift drop animation starts.
    /// </summary>
    private void OnGiftDropAnimStart()
    {
        // Empty
    }

    /// <summary>
    /// Called when the gift drop animation ends.
    /// </summary>
    private void OnGiftDropAnimEnd()
    {
        // Notify GiftUI
        m_giftUI.NotifyGiftDropAnimationEnd();
    }

    /// <summary>
    /// Called when the gift open animation starts.
    /// </summary>
    private void OnGiftOpenAnimStart()
    {
        // Play gift open sound
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.GiftOpen);
    }

    /// <summary>
    /// Called when the gift open animation ends.
    /// </summary>
    private void OnGiftOpenAnimEnd()
    {
        // Notify GiftUI
        m_giftUI.NotifyGiftOpenAnimationEnd();
    }

    #endregion // Animation Events
}
