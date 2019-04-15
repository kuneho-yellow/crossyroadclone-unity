/******************************************************************************
*  @file       GiftManager.cs
*  @brief      Handles the free gift system
*  @author     Ron
*  @date       October 9 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System;

#endregion // Namespaces

public class GiftManager
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(SoomlaDataSystem dataSystem)
    {
        m_dataSystem = dataSystem;

        // Read gift count and next gift time from save file
        m_claimedGiftCount = m_dataSystem.GetGiftCount();
        long nextGiftTimeUTC = m_dataSystem.GetNextGiftTime();
        // Create DateTime struct from the saved UTC value
        if (nextGiftTimeUTC > 0)
        {
            m_nextGiftTime = new DateTime(nextGiftTimeUTC, DateTimeKind.Utc);
        }
        // If no gift time is saved, recalculate next gift time
        else
        {
            UpdateNextGiftTime(m_claimedGiftCount);
        }

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Gets the amount of coins to give in the free gift.
    /// </summary>
    public bool ClaimGift(ref int giftAmount, ref int minutesUntilNextGift)
    {
        if (!IsGiftAvailable)
        {
            return false;
        }
        // Simplified gift system: Gifts are in multiples of 10 coins,
        //  with equal probabilities of getting any amount
        giftAmount = (int)UnityEngine.Random.Range(MIN_GIFT_AMOUNT * 0.1f, MAX_GIFT_AMOUNT * 0.1f) * 10;
        // Update number of gifts claimed
        m_claimedGiftCount++;
        // Save claimed gift count to file
        m_dataSystem.SetGiftCount(m_claimedGiftCount);
        
        // Get the number of minutes until the next gift
        minutesUntilNextGift = m_giftWaitTimes[m_claimedGiftCount];

        UpdateNextGiftTime(m_claimedGiftCount);

        return true;
    }

    /// <summary>
    /// Gets the time in minutes until the next gift.
    /// </summary>
    public int GetMinutesUntilNextGift()
    {
        DateTime now = DateTime.UtcNow;
        int minutesToNextGift = Mathf.CeilToInt((float)m_nextGiftTime.Subtract(now).TotalMinutes);
        // Clamp to positive value
        minutesToNextGift = Mathf.Max(0, minutesToNextGift);
        return minutesToNextGift;
    }

    /// <summary>
    /// Gets whether the next free gift available.
    /// </summary>
    public bool IsGiftAvailable
    {
        get { return GetMinutesUntilNextGift() == 0; }
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    #endregion // Public Interface

    #region Variables

    private bool m_isInitialized = false;

    private SoomlaDataSystem m_dataSystem = null;

    private DateTime m_nextGiftTime;

    #endregion // Variables

    #region Gift

    // Free gift wait times in minutes
    //  The first item is the wait time for the first gift
    //  The second item is the wait time for the second gift after claiming the first
    //  The last item is the wait time for the "last" gift as well as all succeeding gifts
    private int[] m_giftWaitTimes = new int[] { 0, 1, 3, 5, 8, 15, 30, 60, 120, 240, 360 };

    // Number of gifts the player has collected
    private int m_claimedGiftCount = 0;

    // Range of free gift amounts in coins that could be given to the player
    private const int MIN_GIFT_AMOUNT = 40;
    private const int MAX_GIFT_AMOUNT = 150;

    /// <summary>
    /// Updates the time when the next gift will be available.
    /// </summary>
    /// <param name="giftNo">The number of gifts claimed so far.</param>
    /// <returns></returns>
    private DateTime UpdateNextGiftTime(int claimedGiftCount)
    {
        // Clamp gift count to the number of wait times per gift number available
        claimedGiftCount = Mathf.Clamp(claimedGiftCount, 0, m_giftWaitTimes.Length - 1);
        int nextGiftWaitTime = m_giftWaitTimes[claimedGiftCount];
        // Add wait time to the current time to get the time of the next gift
        m_nextGiftTime = DateTime.UtcNow.AddMinutes(nextGiftWaitTime);
        // Save next gift time to file
        long nextGiftTimeUTC = m_nextGiftTime.Ticks;
        m_dataSystem.SetNextGiftTime(nextGiftTimeUTC);
        
        return m_nextGiftTime;
    }

    #endregion // Gift
}
