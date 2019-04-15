/******************************************************************************
*  @file       SoomlaDataSystem.cs
*  @brief      Handles save-load and other data processing using Soomla Storage
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System;
using System.Collections.Generic;
using Soomla;

#endregion // Namespaces

public class SoomlaDataSystem : DataSystemBase
{
	#region Public Interface

	/// <summary>
	/// Initializes the data manager.
	/// </summary>
	public override bool Initialize()
	{
		m_gameData.Initialize();

        m_isInitialized = true;
		return true;
	}

    /// <summary>
    /// Initializes parts of the data system that require other systems to be initialized first.
    /// </summary>
    /// <param name="charResource">The character resource.</param>
    public void LateInitialize(CharacterResource charResource)
    {
        m_charResource = charResource;

        // Load game data
        Load();
        
        // Create list of new/unused characters
        foreach (CharacterType character in Enum.GetValues(typeof(CharacterType)))
        {
            // Skip the size member
            if (character == CharacterType.SIZE)
            {
                continue;
            }
            // If not yet used, add to the new character list
            if (!LoadCharacterUsed(character))
            {
                m_newCharList.Add(character);
            }
        }
    }

	/// <summary>
	/// Save game data.
	/// </summary>
	public override bool Save()
	{
        SaveTopScore(m_gameData.TopScore);
        SaveMuteState(m_gameData.IsMuted);
        SaveNextGiftTime(m_gameData.NextGiftTime);
        SaveGiftCount(m_gameData.GiftCount);
        SaveOrientationPref(m_gameData.OrientationPref);
        SaveAchievUnlocked_CollectChars00(m_gameData.UnlockedAchiev_CollectChars00);
        SaveAchievUnlocked_CollectChars01(m_gameData.UnlockedAchiev_CollectChars01);
        SaveAchievUnlocked_CollectChars02(m_gameData.UnlockedAchiev_CollectChars02);
        SaveAchievUnlocked_PlayGacha00(m_gameData.UnlockedAchiev_PlayGacha00);
        SaveAchievUnlocked_TrainTracks00(m_gameData.UnlockedAchiev_TrainTracks00);
        SaveAchievUnlocked_StraightLine00(m_gameData.UnlockedAchiev_StraightLine00);

        return true;
	}

	/// <summary>
	/// Load game data.
	/// </summary>
	public override bool Load()
	{
        m_gameData.TopScore         = LoadTopScore();
        m_gameData.IsMuted          = LoadMuteState();
        m_gameData.NextGiftTime     = LoadNextGiftTime();
        m_gameData.GiftCount        = LoadGiftCount();
        m_gameData.OrientationPref  = (int)LoadOrientationPref();
        m_gameData.UnlockedAchiev_CollectChars00    = LoadAchievUnlocked_CollectChars00();
        m_gameData.UnlockedAchiev_CollectChars01    = LoadAchievUnlocked_CollectChars01();
        m_gameData.UnlockedAchiev_CollectChars02    = LoadAchievUnlocked_CollectChars02();
        m_gameData.UnlockedAchiev_PlayGacha00       = LoadAchievUnlocked_PlayGacha00();
        m_gameData.UnlockedAchiev_TrainTracks00     = LoadAchievUnlocked_TrainTracks00();
        m_gameData.UnlockedAchiev_StraightLine00    = LoadAchievUnlocked_StraightLine00();

        return true;
	}

	/// <summary>
	/// Gets the game data.
	/// </summary>
	public GameData GameData
	{
		get { return m_gameData; }
	}

	/// <summary>
	/// Sets the game data.
	/// </summary>
	/// <param name="gameData">Game data.</param>
	public void SetGameData(GameData gameData)
	{
		m_gameData = gameData;
    }

    /// <summary>
    /// Gets whether screen orientation is locked.
    /// </summary>
    public bool IsRotationLocked
    {
        get { return GetOrientationPref() == ScreenOrientation.Portrait ||
                     GetOrientationPref() == ScreenOrientation.PortraitUpsideDown ||
                     GetOrientationPref() == ScreenOrientation.Landscape ||
                     GetOrientationPref() == ScreenOrientation.LandscapeRight; }
    }

    #endregion // Public Interface

    #region GameData

    private GameData            m_gameData      = new GameData();
    private CharacterResource   m_charResource  = null;
    // In-game save data
    private List<CharacterType> m_newCharList   = new List<CharacterType>();
    
    /// <summary>
    /// Gets the top score.
    /// </summary>
    public int GetTopScore()
    {
        return m_gameData.TopScore;
    }

    /// <summary>
    /// Sets the top score.
    /// </summary>
    public void SetTopScore(int topScore)
    {
        m_gameData.TopScore = topScore;
        SaveTopScore(topScore);
    }

    /// <summary>
    /// Gets the mute state.
    /// </summary>
    public bool GetMuteState()
    {
        return m_gameData.IsMuted;
    }

    /// <summary>
    /// Sets the mute state.
    /// </summary>
    public void SetMuteState(bool isMuted)
    {
        m_gameData.IsMuted = isMuted;
        SaveMuteState(isMuted);
    }

    /// <summary>
    /// Gets the next gift time.
    /// </summary>
    public long GetNextGiftTime()
    {
        return m_gameData.NextGiftTime;
    }

    /// <summary>
    /// Sets the next gift time.
    /// </summary>
    public void SetNextGiftTime(long nextGiftTime)
    {
        m_gameData.NextGiftTime = nextGiftTime;
        SaveNextGiftTime(nextGiftTime);
    }

    /// <summary>
    /// Gets the gift count.
    /// </summary>
    public int GetGiftCount()
    {
        return m_gameData.GiftCount;
    }

    /// <summary>
    /// Sets the gift count.
    /// </summary>
    public void SetGiftCount(int giftCount)
    {
        m_gameData.GiftCount = giftCount;
        SaveGiftCount(giftCount);
    }

    /// <summary>
    /// Gets the orientation preference.
    /// </summary>
    public ScreenOrientation GetOrientationPref()
    {
        return (ScreenOrientation)m_gameData.OrientationPref;
    }

    /// <summary>
    /// Sets the orientation preference.
    /// </summary>
    public void SetOrientationPref(ScreenOrientation orientationPref)
    {
        m_gameData.OrientationPref = (int)orientationPref;
        SaveOrientationPref((int)orientationPref);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_CollectChars00()
    {
        return m_gameData.UnlockedAchiev_CollectChars00;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_CollectChars00(bool unlocked)
    {
        m_gameData.UnlockedAchiev_CollectChars00 = unlocked;
        SaveAchievUnlocked_CollectChars00(unlocked);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_CollectChars01()
    {
        return m_gameData.UnlockedAchiev_CollectChars01;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_CollectChars01(bool unlocked)
    {
        m_gameData.UnlockedAchiev_CollectChars01 = unlocked;
        SaveAchievUnlocked_CollectChars01(unlocked);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_CollectChars02()
    {
        return m_gameData.UnlockedAchiev_CollectChars02;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_CollectChars02(bool unlocked)
    {
        m_gameData.UnlockedAchiev_CollectChars02 = unlocked;
        SaveAchievUnlocked_CollectChars02(unlocked);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_PlayGacha00()
    {
        return m_gameData.UnlockedAchiev_PlayGacha00;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_PlayGacha00(bool unlocked)
    {
        m_gameData.UnlockedAchiev_PlayGacha00 = unlocked;
        SaveAchievUnlocked_PlayGacha00(unlocked);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_TrainTracks00()
    {
        return m_gameData.UnlockedAchiev_TrainTracks00;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_TrainTracks00(bool unlocked)
    {
        m_gameData.UnlockedAchiev_TrainTracks00 = unlocked;
        SaveAchievUnlocked_TrainTracks00(unlocked);
    }

    /// <summary>
    /// Gets the lock state of the achievement.
    /// </summary>
    /// <returns>True if achievement is unlocked</returns>
    public bool GetAchievLockState_StraightLine00()
    {
        return m_gameData.UnlockedAchiev_StraightLine00;
    }

    /// <summary>
    /// Sets the lock state of the achievement.
    /// </summary>
    public void SetAchievLockState_StraightLine00(bool unlocked)
    {
        m_gameData.UnlockedAchiev_StraightLine00 = unlocked;
        SaveAchievUnlocked_StraightLine00(unlocked);
    }

    /// <summary>
    /// Gets whether the specified character has been used by the player.
    /// </summary>
    public bool GetCharacterUsed(CharacterType character)
    {
        return !m_newCharList.Contains(character);
    }

    /// <summary>
    /// Sets whether the character has been used by the player.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <param name="isUsed">if set to <c>true</c> [is used].</param>
    public void SetCharacterUsed(CharacterType character, bool isUsed = true)
    {
        if (isUsed && m_newCharList.Contains(character))
        {
            m_newCharList.Remove(character);
        }
        else if (!isUsed && !m_newCharList.Contains(character))
        {
            m_newCharList.Add(character);
        }
        SaveCharacterUsed(character, isUsed);
    }

    // Save data keys for database save file
    private const string TOP_SCORE_KEY                  = "TopScore";
    private const string MUTE_KEY                       = "Mute";
    private const string ORIENTATION_PREF_KEY           = "Orientation";
    private const string NEXT_GIFT_TIME_KEY             = "NextGiftTime";
    private const string GIFT_COUNT_KEY                 = "GiftCount";
    private const string ACHIEV_COLLECT_CHARS_00_KEY    = "Achiev_CollectChars00";
    private const string ACHIEV_COLLECT_CHARS_01_KEY    = "Achiev_CollectChars01";
    private const string ACHIEV_COLLECT_CHARS_02_KEY    = "Achiev_CollectChars02";
    private const string ACHIEV_PLAY_GACHA_00_KEY       = "Achiev_PlayGacha00";
    private const string ACHIEV_TRAIN_TRACKS_00_KEY     = "Achiev_TrainTracks00";
    private const string ACHIEV_STRAIGHT_LINE_00_KEY    = "Achiev_StraightLine00";
    private const string CHARACTER_USED_PREFIX          = "IsUsed_";

    /// <summary>
    /// Saves the top score.
    ///     Top score values:
    ///         int     Score
    /// </summary>
    private void SaveTopScore(int topScore)
    {
        KeyValueStorage.SetValue(TOP_SCORE_KEY, topScore.ToString());
    }

    /// <summary>
    /// Loads the top score.
    /// </summary>
    private int LoadTopScore()
    {
        string topScoreValue = KeyValueStorage.GetValue(TOP_SCORE_KEY);
        try { return int.Parse(topScoreValue); }
        catch { return 0; }
    }

    /// <summary>
    /// Saves the mute state.
    ///     Mute values:
    ///         1       Muted
    ///         0       Not muted
    /// </summary>
    private void SaveMuteState(bool muted)
    {
        KeyValueStorage.SetValue(MUTE_KEY, muted ? "1" : "0");
    }

    /// <summary>
    /// Loads the mute state.
    /// </summary>
    private bool LoadMuteState()
    {
        string muteValue = KeyValueStorage.GetValue(MUTE_KEY);
        return muteValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the orientation preference.
    ///     Orientation values (same as ScreenOrientation enum):
    ///         0       Unknown
    ///         1       Portrait
    ///         2       Portrait upside-down
    ///         3       Landscape left
    ///         4       Landscape right
    ///         5       Auto rotation
    /// </summary>
    private void SaveOrientationPref(int orientationIndex)
    {
        if (orientationIndex == (int)ScreenOrientation.Unknown)
        {
            return;
        }
        KeyValueStorage.SetValue(ORIENTATION_PREF_KEY, orientationIndex.ToString());
    }

    /// <summary>
    /// Loads the orientation preference.
    /// </summary>
    private ScreenOrientation LoadOrientationPref()
    {
        string orientationValue = KeyValueStorage.GetValue(ORIENTATION_PREF_KEY);
        try { return (ScreenOrientation)int.Parse(orientationValue); }
        catch { return ScreenOrientation.Unknown; }
    }

    /// <summary>
    /// Saves the next gift time.
    /// </summary>
    /// <param name="nextGiftTime">The next gift time.</param>
    private void SaveNextGiftTime(long nextGiftTime)
    {
        KeyValueStorage.SetValue(NEXT_GIFT_TIME_KEY, nextGiftTime.ToString());
    }

    /// <summary>
    /// Loads the next gift time.
    /// </summary>
    private long LoadNextGiftTime()
    {
        string nextGiftTimeValue = KeyValueStorage.GetValue(NEXT_GIFT_TIME_KEY);
        try { return long.Parse(nextGiftTimeValue); }
        catch { return 0; }
    }

    /// <summary>
    /// Saves the gift count.
    /// </summary>
    private void SaveGiftCount(int giftCount)
    {
        KeyValueStorage.SetValue(GIFT_COUNT_KEY, giftCount.ToString());
    }

    /// <summary>
    /// Loads the gift count.
    /// </summary>
    private int LoadGiftCount()
    {
        string giftCountValue = KeyValueStorage.GetValue(GIFT_COUNT_KEY);
        try { return int.Parse(giftCountValue); }
        catch { return 0; }
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_CollectChars00(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_COLLECT_CHARS_00_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_CollectChars00()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_COLLECT_CHARS_00_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_CollectChars01(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_COLLECT_CHARS_01_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_CollectChars01()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_COLLECT_CHARS_01_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_CollectChars02(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_COLLECT_CHARS_02_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_CollectChars02()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_COLLECT_CHARS_02_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_PlayGacha00(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_PLAY_GACHA_00_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_PlayGacha00()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_PLAY_GACHA_00_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_TrainTracks00(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_TRAIN_TRACKS_00_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_TrainTracks00()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_TRAIN_TRACKS_00_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves the lock state of the achievement.
    ///     Lock values:
    ///         1       Unlocked
    ///         0       Locked
    /// </summary>
    private void SaveAchievUnlocked_StraightLine00(bool unlocked)
    {
        KeyValueStorage.SetValue(ACHIEV_STRAIGHT_LINE_00_KEY, unlocked ? "1" : "0");
    }

    /// <summary>
    /// Loads the lock state of the achievement.
    /// </summary>
    private bool LoadAchievUnlocked_StraightLine00()
    {
        string unlockedValue = KeyValueStorage.GetValue(ACHIEV_STRAIGHT_LINE_00_KEY);
        return unlockedValue == "1" ? true : false;
    }

    /// <summary>
    /// Saves whether the specified character has been used by the player.
    ///     Character used values:
    ///         1       Used
    ///         0       Unused
    /// </summary>
    private void SaveCharacterUsed(CharacterType character, bool isUsed)
    {
        string characterUsedKey = CHARACTER_USED_PREFIX + m_charResource.GetCharacterStruct(character).ItemID;
        KeyValueStorage.SetValue(characterUsedKey, isUsed ? "1" : "0");
    }

    /// <summary>
    /// Loads whether the specified character has been used by the player.
    /// </summary>
    private bool LoadCharacterUsed(CharacterType character)
    {
        string characterUsedKey = CHARACTER_USED_PREFIX + m_charResource.GetCharacterStruct(character).ItemID;
        string isUsed = KeyValueStorage.GetValue(characterUsedKey);
        return isUsed == "1" ? true : false;
    }

    /// <summary>
    /// Resets game data.
    /// </summary>
    public void ResetGameData()
    {
        // Reset settings to default
        SaveTopScore(0);
        SaveMuteState(false);
        SaveOrientationPref((int)ScreenOrientation.AutoRotation);
        SaveNextGiftTime(0);
        SaveGiftCount(0);
    }

    #endregion // GameData
}
