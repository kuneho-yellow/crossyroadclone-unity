/******************************************************************************
*  @file       SoundInfo.cs
*  @brief      Holds data for organizing sound resources
*  @author     Ron, Lori
*  @date       October 17, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SoundInfo
{
	#region Sound Identifiers

	private const string AUDIO_PREFAB_ROOT_PATH = "Audio/Prefabs/";
	private const string SFX_PREFAB_PREFIX = "SFX-";
	private const string BGM_PREFAB_PREFIX = "BGM-";

    // Enum of all sound effects that would be used in the game
    // Also the names of the SFX prefabs
    public enum SFXID
    {
        UIButtonPress,
		UIButtonRelease,
        PauseCountdown,
        CharacterSelect,
        VideoAdsStrip,
        GiftStrip,
        GachaStrip,
        GiftOpen,
        WinCoins,
        GachaJingle,
        GachaOpen,
        NewCharacter,
        OldCharacter,
        TopScore,
        Screenshot,

        CharacterMove,
        ChickCheep1,
        ChickCheep2,
        ChickCheep3,
        ChickCry,
        ChickenCluck1,
        ChickenCluck2,
        ChickenCluck3,
        ChickenCry1,
        ChickenCry2,
        CowMoo1,
        CowMoo2,
        CowMoo3,
        CowMoo4,
        CowCry,
        PuppyBark1,
        PuppyBark2,
        PuppyBark3,
        PuppyCry,
        CatMeow1,
        CatMeow2,
        CatMeow3,
        CatMeow4,
        CatCry,
        PigeonCoo1,
        PigeonCoo2,
        PigeonCoo3,
        PigeonCoo4,
        PigeonCry,
        EagleScreech,
        WaterSplash,
        RiverRush,
        CarHit,
        CarPassing1,
        CarPassing2,
        CarPassing3,
        CarHorn1,
        CarHorn2,
        CarHorn3,
        CarHorn4,
        CarHorn5,
        TrainBell,
        TrainPassing,
        TrainHorn,
        PoliceSiren,
        CoinPickup1,
        CoinPickup2,
        CoinPickup3,
        WaterLilyStep,
        WaterLogStep1,
        WaterLogStep2,
        Reach50Score,
        Reach100Coins,

        SIZE
	}

    // Enum of all background music that would be used in the game
    // Also the names of the BGM prefabs
    public enum BGMID
	{
		// Empty
		
		SIZE
	}

	// Types of sounds
	public enum SoundType
	{
		ONE_SHOT,	// Plays once, then is destroyed
		REGULAR,	// Plays once
		BGM			// Plays and loops
	}

	/// <summary>
	/// Gets the SFX prefab path.
	/// </summary>
	/// <returns>The SFX prefab path.</returns>
	/// <param name="sfxID">ID of sound effect.</param>
	public string GetSoundPrefabPath(SFXID sfxID)
	{
		if (sfxID == SFXID.SIZE)
		{
			Debug.Log("Specified item is not an SFX");
			return null;
		}
		return AUDIO_PREFAB_ROOT_PATH + SFX_PREFAB_PREFIX + sfxID.ToString();
	}
	
	/// <summary>
	/// Gets the BGM prefab path.
	/// </summary>
	/// <returns>The BGM prefab path.</returns>
	/// <param name="bgmID">ID of background music.</param>
	public string GetSoundPrefabPath(BGMID bgmID)
	{
		if (bgmID == BGMID.SIZE)
		{
			Debug.Log("Specified item is not a BGM");
			return null;
		}
		return AUDIO_PREFAB_ROOT_PATH + BGM_PREFAB_PREFIX + bgmID.ToString();
	}
	
	#endregion // Sound Identifiers
}
