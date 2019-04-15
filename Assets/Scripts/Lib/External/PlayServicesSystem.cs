/******************************************************************************
*  @file       PlayServicesSystem.cs
*  @brief      Handles integration with Google Play Games Services
*  @author     Ron
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> The Play Games Plugin for Unity is used for integration
*       > This script exposes various methods from the plugin's interface
******************************************************************************/

#region Namespaces

using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

#endregion // Namespaces

public class PlayServicesSystem
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="enableSavedGames">if set to <c>true</c> support saving game progress on the cloud.</param>
    public void Initialize(bool enableSavedGames)
    {
        PlayGamesClientConfiguration.Builder config = new PlayGamesClientConfiguration.Builder();
        if (enableSavedGames)
        {
            config.EnableSavedGames();
        }
        // Initialize Play Games
        PlayGamesPlatform.InitializeInstance(config.Build());
        PlayGamesPlatform.DebugLogEnabled = BuildInfo.IsDebugMode;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();

        // Create instances of achievmements and leaderboards assets
        m_achievements = new AchievementInfo();
        m_leaderboards = new LeaderboardInfo();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Starts Google sign-in.
    /// </summary>
    /// <param name="signInFinishedDelegate">The event handler for when sign-in finishes.</param>
    public void SignInToGoogle(System.Action<bool> signInFinishedDelegate)
    {
        Social.localUser.Authenticate(signInFinishedDelegate);
    }

    /// <summary>
    /// Starts Google sign-out.
    /// </summary>
    public void SignOutFromGoogle()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
    }

    /// <summary>
    /// Increments the specified achievement.
    /// </summary>
    /// <param name="steps">The amount to increment the achievement by.</param>
    /// <param name="achievement">The achievement to increment.</param>
    /// <param name="incrementAchievementFinishedDelegate">The event handler for when increment achievement finishes.</param>
    public void IncrementAchievement(int steps, AchievementType achievement, System.Action<bool> incrementAchievementFinishedDelegate)
    {
        if (achievement == AchievementType.SIZE)
        {
            return;
        }
        if (!IsSignedInToGoogle)
        {
            return;
        }
        PlayGamesPlatform.Instance.IncrementAchievement(m_achievements.GetID(achievement), steps, incrementAchievementFinishedDelegate);
    }

    /// <summary>
    /// Unlocks the specified achievement.
    /// </summary>
    /// <param name="achievement">The achievement to unlock.</param>
    /// <param name="unlockAchievementFinishedDelegate">The event handler for when unlock achievement finishes.</param>
    public void UnlockAchievement(AchievementType achievement, System.Action<bool> unlockAchievementFinishedDelegate)
    {
        if (achievement == AchievementType.SIZE)
        {
            return;
        }
        if (!IsSignedInToGoogle)
        {
            return;
        }
        // Unlock achievement using its achievement ID
        Social.ReportProgress(m_achievements.GetID(achievement), 100.0f, unlockAchievementFinishedDelegate);
    }

    /// <summary>
    /// Posts a score to the specified leaderboard.
    /// </summary>
    /// <param name="score">The score to post.</param>
    /// <param name="leaderboard">The leaderboard to post to.</param>
    /// <param name="reportScoreFinishedDelegate">The event handler for when report score finishes.</param>
    public void PostScoreToLeaderboard(int score, LeaderboardType leaderboard, System.Action<bool> reportScoreFinishedDelegate)
    {
        if (leaderboard == LeaderboardType.SIZE)
        {
            return;
        }
        if (!IsSignedInToGoogle)
        {
            return;
        }
        Social.ReportScore(score, m_leaderboards.GetID(leaderboard), reportScoreFinishedDelegate);
    }

    /// <summary>
    /// Shows the achievements UI.
    /// </summary>
    /// <returns>Whether achievements UI was shown</returns>
    public bool ShowAchievementsUI()
    {
        if (!IsSignedInToGoogle)
        {
            return false;
        }
        Social.ShowAchievementsUI();
        return true;
    }

    /// <summary>
    /// Shows the built-in UI for all leaderboards.
    /// </summary>
    /// <returns>Whether leaderboard UI was shown</returns>
    public bool ShowLeaderboardUI()
    {
        if (!IsSignedInToGoogle)
        {
            return false;
        }
        Social.ShowLeaderboardUI();
        return true;
    }

    /// <summary>
    /// Shows the UI for the specified leaderboard.
    /// </summary>
    /// <param name="leaderboard">The leaderboard.</param>
    /// <returns>Whether leaderboard UI was shown</returns>
    public bool ShowLeaderboardUI(LeaderboardType leaderboard)
    {
        if (leaderboard == LeaderboardType.SIZE)
        {
            return false;
        }
        if (!IsSignedInToGoogle)
        {
            return false;
        }
        PlayGamesPlatform.Instance.ShowLeaderboardUI(m_leaderboards.GetID(leaderboard));
        return true;
    }

    /// <summary>
    /// Gets whether player is signed in to Google.
    /// </summary>
    public bool IsSignedInToGoogle
    {
        get { return Social.localUser.authenticated; }
    }

    /// <summary>
    /// Gets whether Play Services is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    #endregion // Public Interface

    #region Variables

    private bool m_isInitialized = false;

    private AchievementInfo m_achievements = null;
    private LeaderboardInfo m_leaderboards = null;

    #endregion // Variables
}
