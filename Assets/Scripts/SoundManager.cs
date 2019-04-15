/******************************************************************************
*  @file       SoundManager.cs
*  @brief      Handles all sounds in the game
*  @author     Ron
*  @date       October 17, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;

#endregion // Namespaces

public class SoundManager : SoundManagerBase
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public override bool Initialize()
    {
        if (base.Initialize())
        {
            m_areInGameSoundsPaused = false;
            m_inGameSounds.Clear();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Plays the specified sound once, and deletes the sound object after playback.
    /// </summary>
    public override void PlayOneShot(SoundInfo.SFXID soundID, Vector3 pos)
    {
        SoundObject oneShotSFX = base.PlayOneShotInternal(soundID, pos);
        oneShotSFX.transform.parent = this.transform;
        // All in-game sounds are in the second half of the SFXID enum
        if ((int)soundID >= (int)GetFirstInGameSFXID())
        {
            m_inGameSounds.Add(oneShotSFX);
        }
    }

    /// <summary>
    /// Plays the specified sound.
    /// </summary>
    public override SoundObject PlaySound(SoundInfo.SFXID soundID)
    {
        SoundObject sound = base.PlaySound(soundID);
        sound.transform.parent = this.transform;
        // All in-game sounds are in the second half of the SFXID enum
        if ((int)soundID >= (int)GetFirstInGameSFXID())
        {
            m_inGameSounds.Add(sound);
        }
        return sound;
    }

    /// <summary>
    /// Plays the specified BGM.
    /// </summary>
    public override SoundObject PlayBGM(SoundInfo.BGMID soundID, bool setPersistent = false)
    {
        SoundObject bgm = base.PlayBGM(soundID, setPersistent);
        bgm.transform.parent = this.transform;
        return bgm;
    }

    /// <summary>
    /// Pauses all sounds.
    /// </summary>
    public override void PauseAllSounds(bool includeBGM = true)
    {
        base.PauseAllSounds(includeBGM);
    }

    /// <summary>
    /// Unpauses all sounds.
    /// </summary>
    public override void UnpauseAllSounds()
    {
        base.UnpauseAllSounds();
    }

    /// <summary>
    /// Mutes all sounds.
    /// </summary>
    public override void MuteAllSounds(bool includeBGM = true)
    {
        base.MuteAllSounds(includeBGM);
    }

    /// <summary>
    /// Unmutes all sounds.
    /// </summary>
    public override void UnmuteAllSounds()
    {
        base.UnmuteAllSounds();
    }

    /// <summary>
    /// Notifies of a sound object's deletion via means external to SoundManager.
    /// </summary>
    /// <param name="soundObject">Sound object to delete.</param>
    public override void NotifySoundObjectDeletion(SoundObject soundObject)
    {
        base.NotifySoundObjectDeletion(soundObject);

        if (m_inGameSounds.Contains(soundObject))
        {
            m_inGameSounds.Remove(soundObject);
        }
    }

    /// <summary>
    /// Deletes all sound objects.
    /// </summary>
    /// <param name="includePersistent">If set to <c>true</c> delete persistent sounds as well.</param>
    public override void DeleteAllSoundObjects(bool includePersistent = true)
    {
        base.DeleteAllSoundObjects(includePersistent);

        m_inGameSounds.Clear();
    }

    #endregion // Public Interface

    #region Game-specific

    private List<SoundObject> m_inGameSounds = new List<SoundObject>();
    private bool m_areInGameSoundsPaused = false;

    private List<SoundInfo.SFXID> m_chickCheepSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.ChickCheep1,
        SoundInfo.SFXID.ChickCheep2,
        SoundInfo.SFXID.ChickCheep3,
    };
    private List<SoundInfo.SFXID> m_chickCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.ChickCry,
    };
    private List<SoundInfo.SFXID> m_chickenCluckSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.ChickenCluck1,
        SoundInfo.SFXID.ChickenCluck2,
        SoundInfo.SFXID.ChickenCluck3,
    };
    private List<SoundInfo.SFXID> m_chickenCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.ChickenCry1,
        SoundInfo.SFXID.ChickenCry2,
    };
    private List<SoundInfo.SFXID> m_cowMooSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CowMoo1,
        SoundInfo.SFXID.CowMoo2,
        SoundInfo.SFXID.CowMoo3,
        SoundInfo.SFXID.CowMoo4,
    };
    private List<SoundInfo.SFXID> m_cowCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CowCry,
    };
    private List<SoundInfo.SFXID> m_puppyBarkSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.PuppyBark1,
        SoundInfo.SFXID.PuppyBark2,
        SoundInfo.SFXID.PuppyBark3,
    };
    private List<SoundInfo.SFXID> m_puppyCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.PuppyCry,
    };
    private List<SoundInfo.SFXID> m_catMeowSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CatMeow1,
        SoundInfo.SFXID.CatMeow2,
        SoundInfo.SFXID.CatMeow3,
        SoundInfo.SFXID.CatMeow4,
    };
    private List<SoundInfo.SFXID> m_catCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CatCry,
    };
    private List<SoundInfo.SFXID> m_pigeonCooSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.PigeonCoo1,
        SoundInfo.SFXID.PigeonCoo2,
        SoundInfo.SFXID.PigeonCoo3,
        SoundInfo.SFXID.PigeonCoo4,
    };
    private List<SoundInfo.SFXID> m_pigeonCrySounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.PigeonCry,
    };
    private List<SoundInfo.SFXID> m_carPassingSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CarPassing1,
        SoundInfo.SFXID.CarPassing2,
        SoundInfo.SFXID.CarPassing3,
    };
    private List<SoundInfo.SFXID> m_carHornSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CarHorn1,
        SoundInfo.SFXID.CarHorn2,
        SoundInfo.SFXID.CarHorn3,
        SoundInfo.SFXID.CarHorn4,
        SoundInfo.SFXID.CarHorn5,
    };
    private List<SoundInfo.SFXID> m_coinPickupSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CoinPickup1,
        SoundInfo.SFXID.CoinPickup2,
    };
    private List<SoundInfo.SFXID> m_coinRewardSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.CoinPickup2,
        SoundInfo.SFXID.CoinPickup3,
    };
    private List<SoundInfo.SFXID> m_waterLogStepSounds = new List<SoundInfo.SFXID>
    {
        SoundInfo.SFXID.WaterLogStep1,
        SoundInfo.SFXID.WaterLogStep2,
    };

    /// <summary>
    /// Gets whether in-game sounds are paused.
    /// </summary>
    public bool AreInGameSoundsPaused
    {
        get { return m_areInGameSoundsPaused; }
    }

    /// <summary>
    /// Gets a random sound ID from the given list of sounds.
    /// </summary>
    /// <param name="soundList">The sound list.</param>
    /// <returns>A random SFXID from the list</returns>
    private SoundInfo.SFXID GetRandomSound(List<SoundInfo.SFXID> soundList)
    {
        return soundList[Random.Range(0, soundList.Count)];
    }

    /// <summary>
    /// Plays a random "call" sound associated with the specified character.
    /// </summary>
    /// <param name="character">The character associated with the sound.</param>
    /// <returns>The SoundObject handle for the character sound</returns>
    public SoundObject PlayCharacterCallSound(CharacterType character)
    {
        switch (character)
        {
            case CharacterType.Chick:
                return PlaySound(GetRandomSound(m_chickCheepSounds));
            case CharacterType.Chicken:
            case CharacterType.Giftee:
                return PlaySound(GetRandomSound(m_chickenCluckSounds));
            case CharacterType.Cow_BlackWhite:
            case CharacterType.Cow_Brown:
                return PlaySound(GetRandomSound(m_cowMooSounds));
            case CharacterType.Puppy_Brown:
            case CharacterType.Puppy_White:
                return PlaySound(GetRandomSound(m_puppyBarkSounds));
            case CharacterType.Cat_Calico:
            case CharacterType.Cat_Heterochromia:
            case CharacterType.Cat_Kutsushita:
            case CharacterType.Cat_Maneki:
                return PlaySound(GetRandomSound(m_catMeowSounds));
            case CharacterType.Pigeon:
                return PlaySound(GetRandomSound(m_pigeonCooSounds));
        }
        return null;
    }

    /// <summary>
    /// Plays a random "cry" sound associated with the specified character.
    /// </summary>
    /// <param name="character">The character associated with the sound.</param>
    public void PlayCharacterCrySound(CharacterType character)
    {
        switch (character)
        {
            case CharacterType.Chick:
                PlayOneShot(GetRandomSound(m_chickCrySounds));
                break;
            case CharacterType.Chicken:
            case CharacterType.Giftee:
                PlayOneShot(GetRandomSound(m_chickenCrySounds));
                break;
            case CharacterType.Cow_BlackWhite:
            case CharacterType.Cow_Brown:
                PlayOneShot(GetRandomSound(m_cowCrySounds));
                break;
            case CharacterType.Puppy_Brown:
            case CharacterType.Puppy_White:
                PlayOneShot(GetRandomSound(m_puppyCrySounds));
                break;
            case CharacterType.Cat_Calico:
            case CharacterType.Cat_Heterochromia:
            case CharacterType.Cat_Kutsushita:
            case CharacterType.Cat_Maneki:
                PlayOneShot(GetRandomSound(m_catCrySounds));
                break;
            case CharacterType.Pigeon:
                PlayOneShot(GetRandomSound(m_pigeonCrySounds));
                break;
        }
    }

    /// <summary>
    /// Plays a random car passing sound.
    /// </summary>
    public SoundObject PlayCarPassingSound()
    {
        return PlaySound(GetRandomSound(m_carPassingSounds));
    }

    /// <summary>
    /// Plays a random car passing sound.
    /// </summary>
    /// <param name="pos">The position where sound should play.</param>
    public void PlayCarHornSound(Vector3 pos)
    {
        PlayOneShot(GetRandomSound(m_carHornSounds), pos);
    }

    /// <summary>
    /// Plays a random coin pickup sound.
    /// </summary>
    public void PlayCoinPickupSound()
    {
        PlayOneShot(GetRandomSound(m_coinPickupSounds));
    }

    /// <summary>
    /// Plays a random coin reward sound.
    /// </summary>
    public void PlayCoinRewardSound()
    {
        PlayOneShot(GetRandomSound(m_coinRewardSounds));
    }

    /// <summary>
    /// Plays a random water log step sound.
    /// </summary>
    public void PlayWaterLogStepSound()
    {
        PlayOneShot(GetRandomSound(m_waterLogStepSounds));
    }

    /// <summary>
    /// Pauses all in-game sounds.
    /// </summary>
    public void PauseInGameSounds()
    {
        foreach (SoundObject soundObj in m_inGameSounds)
        {
            if (soundObj != null)
            {
                soundObj.Pause();
            }
        }
        m_areInGameSoundsPaused = true;
    }

    /// <summary>
    /// Unpauses all in-game sounds.
    /// </summary>
    public void UnpauseInGameSounds()
    {
        foreach (SoundObject soundObj in m_inGameSounds)
        {
            if (soundObj != null)
            {
                soundObj.Unpause();
            }
        }
        m_areInGameSoundsPaused = false;
    }

    /// <summary>
    /// Gets the first in-game sound in the SFXID enum.
    /// </summary>
    /// <returns></returns>
    private SoundInfo.SFXID GetFirstInGameSFXID()
    {
        return SoundInfo.SFXID.CharacterMove;
    }

    #region Game-specific: River sounds

    private SoundObject m_riverSound = null;

    private bool m_isRiverSoundAnimInitialized = false;

    private float m_riverInitialVolume = 0.0f;

    // River sound volume during normal play
    private float m_riverNormalVolume = 0.0f;
    // River sound volume when character is carried away by the river
    private float m_riverGameOverVolume = 0.0f;
    // Amount of time to get from normal volume to game over volume, or vice versa
    private float m_riverVolumeChangeTime = 0.5f;
    // Amount of time the river sound volume stays at game over volume before returning to normal volume
    private float m_riverGameOverVolumeTime = 0.0f;

    private float m_timeSinceRiverAnimStateChange = 0.0f;

    private enum RiverSoundAnimState
    {
        Normal,
        ToGameOver,
        GameOver,
        ToNormal
    }
    [SerializeField]
    private RiverSoundAnimState m_riverAnimState = RiverSoundAnimState.Normal;

    /// <summary>
    /// Updates the river sound animation.
    /// </summary>
    private void UpdateRiverSoundAnim()
    {
        if (m_riverSound == null)
        {
            return;
        }

        switch (m_riverAnimState)
        {
            case RiverSoundAnimState.Normal:
                // Do nothing
                break;
            case RiverSoundAnimState.ToGameOver:
                m_timeSinceRiverAnimStateChange += Time.deltaTime;
                m_riverSound.SetVolume(Mathf.Lerp(m_riverNormalVolume,
                                                  m_riverGameOverVolume,
                                                  m_timeSinceRiverAnimStateChange / m_riverVolumeChangeTime));
                if (m_timeSinceRiverAnimStateChange > m_riverVolumeChangeTime)
                {
                    m_timeSinceRiverAnimStateChange = 0.0f;
                    m_riverAnimState = RiverSoundAnimState.GameOver;
                }
                break;
            case RiverSoundAnimState.GameOver:
                m_timeSinceRiverAnimStateChange += Time.deltaTime;
                if (m_timeSinceRiverAnimStateChange > m_riverVolumeChangeTime)
                {
                    m_timeSinceRiverAnimStateChange = 0.0f;
                    m_riverAnimState = RiverSoundAnimState.ToNormal;
                }
                break;
            case RiverSoundAnimState.ToNormal:
                m_timeSinceRiverAnimStateChange += Time.deltaTime;
                m_riverSound.SetVolume(Mathf.Lerp(m_riverGameOverVolume,
                                                  m_riverNormalVolume,
                                                  m_timeSinceRiverAnimStateChange / m_riverGameOverVolumeTime));
                m_riverSound.SetSpatialBlend(Mathf.Lerp(0.0f, 1.0f, m_timeSinceRiverAnimStateChange / m_riverGameOverVolumeTime));
                if (m_timeSinceRiverAnimStateChange > m_riverGameOverVolumeTime)
                {
                    // Restore 3D sound volume
                    m_riverSound.SetVolume(m_riverInitialVolume);

                    m_timeSinceRiverAnimStateChange = 0.0f;
                    m_riverAnimState = RiverSoundAnimState.Normal;
                }
                break;
        }
    }

    /// <summary>
    /// Initializes the river sound animation, where the sound volume changes
    ///     when character is carried away by a river.
    /// </summary>
    /// <param name="normalVolume">Sound volume during normal play.</param>
    /// <param name="gameOverVolume">Sound volume when character is carried away by river.</param>
    /// <param name="volumeChangeTime">Time to get from normal volume to game over volume, and vice versa.</param>
    /// <param name="gameOverVolumeTime">Duration that river volume stays at the game over volume.</param>
    public void InitializeRiverSoundAnim(float normalVolume, float gameOverVolume,
                                         float volumeChangeTime, float gameOverVolumeTime)
    {
        m_riverNormalVolume = normalVolume;
        m_riverGameOverVolume = gameOverVolume;
        m_riverVolumeChangeTime = volumeChangeTime;
        m_riverGameOverVolumeTime = gameOverVolumeTime;

        // Create river sound object, but do not play yet
        m_riverSound = PlaySound(SoundInfo.SFXID.RiverRush);
        m_riverSound.Stop();
        // Get initial volume
        m_riverInitialVolume = m_riverSound.GetVolume();

        // Set the initialized flag
        m_isRiverSoundAnimInitialized = true;
    }

    /// <summary>
    /// Plays the river sound.
    /// </summary>
    /// <param name="soundPos">The position for the river sound object.</param>
    /// <param name="resetToNormalVolume">if set to <c>true</c> reset sound volume to the normal value.</param>
    public void PlayRiverSound(Vector3 soundPos, bool resetToNormalVolume = false)
    {
        m_riverSound.Play();
        m_riverSound.transform.position = soundPos;
        // Play as 3D sound
        m_riverSound.SetSpatialBlend(1.0f);

        if (resetToNormalVolume)
        {
            m_riverSound.SetVolume(m_riverInitialVolume);
        }
    }

    /// <summary>
    /// Stops the river sound playback.
    /// </summary>
    public void StopRiverSound()
    {
        if (m_riverSound != null)
        {
            m_riverSound.Stop();
        }
    }

    /// <summary>
    /// Plays the river game over sound (when character is carried away by river).
    /// </summary>
    public void PlayRiverGameOverSound()
    {
        if (m_riverSound == null)
        {
            return;
        }
        // Change river sound to 2D
        m_riverSound.SetSpatialBlend(0.0f);
        // Start river sound anim
        m_timeSinceRiverAnimStateChange = 0.0f;
        m_riverAnimState = RiverSoundAnimState.ToGameOver;
    }

    #endregion Game-specific: River sounds

    #endregion Game-specific

    #region Serialized Variables

    #endregion // Serialized Variables

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
    {

    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    private void Start()
    {

    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        if (!m_isInitialized)
        {
            return;
        }

        if (m_isRiverSoundAnimInitialized)
        {
            UpdateRiverSoundAnim();
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {

    }

    #endregion // MonoBehaviour
}
