/******************************************************************************
*  @file       AchievementInfo.cs
*  @brief      Holds values to be used in the achievement system
*  @author     Ron
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> How to use:
*           1. Design and document in-game achievements.
*           2. Fill in AchievementType enum with the list of achievements.
*           3. Create achievements in Google Play Developer Console.
*           4. Copy the achievement IDs and paste into the achievement IDs array,
*               making sure each element matches the corresponding achievement
*               in the AchievementType enum.
*           5. Fill in the achievement unlock requirements array with values
*               needed for determining when each achievement is unlocked. This
*               script only provides a central location for these values--how
*               each value is interpreted must be programmed based on the
*               achievements document.
******************************************************************************/

public enum AchievementType
{
    CollectCharacters00,
    CollectCharacters01,
    CollectCharacters02,
    PlayGacha00,
    StayOnTrainTracks00,
    MoveInStraightLine00,

    SIZE
}

public class AchievementInfo
{
    #region Public Interface

    /// <summary>
    /// Gets the ID of the specified achievement.
    /// </summary>
    /// <param name="achievement">The achievement.</param>
    /// <returns>The achievement ID</returns>
    public string GetID(AchievementType achievement)
    {
        return m_achievementIDs[(int)achievement];
    }

    /// <summary>
    /// Gets the achievement with the specified ID.
    /// </summary>
    /// <param name="achievementID">The achievement ID.</param>
    /// <returns>The achievement</returns>
    public AchievementType GetAchievement(string achievementID)
    {
        // Find the ID string in the array of achievement IDs
        for (int index = 0; index < m_achievementIDs.Length; ++index)
        {
            // If ID is found, return the achievement with the same index
            if (m_achievementIDs[index] == achievementID)
            {
                return (AchievementType)index;
            }
        }
        // ID not found - return an invalid value
        return AchievementType.SIZE;
    }

    /// <summary>
    /// Gets an array of ints that specify the requirements to unlock the specified achievement.
    /// See the game design document for the meaning for each int.
    /// </summary>
    /// <param name="achievement">The achievement.</param>
    /// <returns>The unlock requirements</returns>
    public int[] GetUnlockReqts(AchievementType achievement)
    {
        return m_achievementUnlockReqts[(int)achievement];
    }

    #endregion // Public Interface

    #region Variables

    private string[] m_achievementIDs =
    {
        CRCPlayGamesConstants.achievement_hobbyist,
        CRCPlayGamesConstants.achievement_accumulator,
        CRCPlayGamesConstants.achievement_exhibitionist,
        CRCPlayGamesConstants.achievement_becoming_a_hoarder,
        CRCPlayGamesConstants.achievement_train_tracks,
        CRCPlayGamesConstants.achievement_tunnel_vision
    };

    private int[][] m_achievementUnlockReqts =
    {
        new int[] { 5 },
        new int[] { 10 },
        new int[] { 15 },
        new int[] { 10 },
        new int[] { 6 },
        new int[] { 20 }
    };

    #endregion // Variables
}
