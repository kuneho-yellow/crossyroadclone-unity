/******************************************************************************
*  @file       LeaderboardInfo.cs
*  @brief      Holds values to be used in the leaderboard system
*  @author     Ron
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> How to use:
*           1. Design and document in-game leaderboards.
*           2. Fill in LeaderboardType enum with the list of leaderboards.
*           3. Create leaderboards in Google Play Developer Console.
*           4. Copy the leaderboard IDs and paste into the leaderboard IDs array,
*               making sure each element matches the corresponding leaderboard
*               in the LeaderboardType enum.
******************************************************************************/

public enum LeaderboardType
{
    TopScore,

    SIZE
}

public class LeaderboardInfo
{
    #region Public Interface

    /// <summary>
    /// Gets the ID of the specified leaderboard.
    /// </summary>
    /// <param name="leaderboard">The leaderboard.</param>
    /// <returns>The achievement ID</returns>
    public string GetID(LeaderboardType leaderboard)
    {
        return m_leaderboardIDs[(int)leaderboard];
    }

    /// <summary>
    /// Gets the leaderboard with the specified ID.
    /// </summary>
    /// <param name="leaderboardID">The leaderboard ID.</param>
    /// <returns>The leaderboard</returns>
    public LeaderboardType GetLeaderboard(string leaderboardID)
    {
        // Find the ID string in the array of leaderboard IDs
        for (int index = 0; index < m_leaderboardIDs.Length; ++index)
        {
            // If ID is found, return the leaderboard with the same index
            if (m_leaderboardIDs[index] == leaderboardID)
            {
                return (LeaderboardType)index;
            }
        }
        // ID not found - return an invalid value
        return LeaderboardType.SIZE;
    }

    #endregion // Public Interface

    #region Variables

    private string[] m_leaderboardIDs =
    {
        CRCPlayGamesConstants.leaderboard_score
    };

    #endregion // Variables
}
