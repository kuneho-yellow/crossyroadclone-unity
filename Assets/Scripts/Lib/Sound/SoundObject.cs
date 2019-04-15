/******************************************************************************
*  @file       SoundObject.cs
*  @brief      Object handle for sound playback
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class SoundObject : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Starts sound playback.
	/// </summary>
	/// <param name="fromStart">If set to <c>true</c> rewind sound before playing.</param>
	public void Play(bool fromStart = true)
	{
		if (fromStart)
		{
			Seek(0.0f);
		}
		m_audioSource.Play();
		m_isPaused = false;
	}

	/// <summary>
	/// Plays this sound starting at the specified playback position.
	/// </summary>
	/// <param name="playbackPosition">Playback position to start playing at.</param>
	/// <param name="isNormalizedTime">If set to <c>true</c> given playback position is
	/// 								normalized relative to audio clip length.</param>
	public void Play(float playbackPosition, bool isNormalizedTime = false)
	{
		Seek(playbackPosition, isNormalizedTime);
		Play(false);
	}

	/// <summary>
	/// Pauses sound playback.
	/// </summary>
	/// <param name="pauseAll">If set to <c>true</c> was paused from SoundManager's PauseAllSounds.</param>
	public void Pause(bool pauseAll = false)
	{
		m_audioSource.Pause();
		if (!pauseAll)
		{
			m_isPaused = true;
		}
	}

	/// <summary>
	/// Unpauses sound playback.
	/// </summary>
	/// <param name="unpauseAll">If set to <c>true</c> was unpaused from SoundManager's UnpauseAllSounds.</param>
	public void Unpause(bool unpauseAll = false)
	{
		m_audioSource.UnPause();
		if (!unpauseAll)
		{
			m_isPaused = false;
		}
	}

	/// <summary>
	/// Stops sound playback.
	/// </summary>
	public void Stop()
	{
		m_audioSource.Stop();
		m_isPaused = true;
	}

	/// <summary>
	/// Move playback time to the specified position in the audio clip.
	/// </summary>
	/// <param name="playbackPosition">Playback position to seek to.</param>
	/// <param name="isNormalizedTime">If set to <c>true</c> given playback position is
	/// 								normalized relative to audio clip length.</param>
	public void Seek(float playbackPosition, bool isNormalizedTime = false)
	{
		float playbackTime = isNormalizedTime ? (playbackPosition * m_audioSource.clip.length) : playbackPosition;
		m_audioSource.time = playbackTime;
		m_playbackTime = playbackTime;
	}

	/// <summary>
	/// Mutes this sound - sets volume to 0.
	/// </summary>
	/// <param name="muteAll">If set to <c>true</c> was muted from SoundManager's MuteAllSounds.</param>
	public void Mute(bool muteAll = false)
	{
		m_audioSource.mute = true;
		if (!muteAll)
		{
			m_isMuted = true;
		}
	}

	/// <summary>
	/// Unmutes this sound - restores volume to that before muting.
	/// </summary>
	/// <param name="unmuteAll">If set to <c>true</c> was unmuted from SoundManager's UnmuteAllSounds.</param>
	public void Unmute(bool unmuteAll = false)
	{
		m_audioSource.mute = false;
		if (!unmuteAll)
		{
			m_isMuted = false;
		}
	}

	/// <summary>
	/// Sets the volume (0 to 1).
	/// </summary>
	/// <param name="volume">Volume.</param>
	public void SetVolume(float volume)
	{
		m_audioSource.volume = Mathf.Clamp(volume, 0.0f, 1.0f);
	}

    /// <summary>
    /// Gets the volume (0 to 1).
    /// </summary>
    public float GetVolume()
    {
        return m_audioSource.volume;
    }

	/// <summary>
	/// Sets the pitch.
	/// </summary>
	/// <param name="pitch">Pitch.</param>
	public void SetPitch(float pitch)
	{
		m_audioSource.pitch = pitch;
	}

    /// <summary>
    /// Gets the pitch.
    /// </summary>
    public float GetPitch()
    {
        return m_audioSource.pitch;
    }

    /// <summary>
    /// Sets the spatial blend.
    /// </summary>
    /// <param name="spatialBlend">The spatial blend.</param>
    public void SetSpatialBlend(float spatialBlend)
    {
        m_audioSource.spatialBlend = spatialBlend;
    }

    /// <summary>
    /// Gets the spatial blend.
    /// </summary>
    public float GetSpatialBlend()
    {
        return m_audioSource.spatialBlend;
    }

	/// <summary>
	/// Sets this sound object to persist even on scene changes.
	/// </summary>
	/// <param name="isPersistent">If set to <c>true</c> is persistent.</param>
	public void SetPersistent(bool isPersistent = true)
	{
		m_isPersistent = isPersistent;
	}

	/// <summary>
	/// Deletes this sound object.
	/// </summary>
	/// <param name="calledBySoundManager">If set to <c>true</c> delete was called by a sound manager.</param>
	public void Delete(bool calledBySoundManager = false)
	{
		// If deleted by anything other than a sound manager, inform sound manager of this object's deletion
		if (!calledBySoundManager)
		{
			m_soundManager.NotifySoundObjectDeletion(this);
		}
		// Destroy this object
		GameObject.Destroy(this.gameObject);
	}

	/// <summary>
	/// Determines whether sound playback has finished (assuming normal playback speed, pitch = 1.0f).
	/// Returns false for looped sounds.
	/// </summary>
	/// <returns><c>true</c> if playback has finished; otherwise, <c>false</c>.</returns>
	public bool HasFinishedPlayback()
	{
		return !m_audioSource.loop && m_playbackTime > m_audioSource.clip.length;
	}

	/// <summary>
	/// Gets whether this sound is currently playing.
	/// </summary>
	public bool IsPlaying
	{
		get { return m_audioSource.isPlaying; }
	}

	/// <summary>
	/// Gets whether this sound is paused (not via PauseAllSounds).
	/// </summary>
	public bool IsPaused
	{
		get { return m_isPaused; }
	}

	/// <summary>
	/// Gets whether this sound is muted (not via MuteAllSounds).
	/// </summary>
	public bool IsMuted
	{
		get { return m_isMuted; }
	}

	/// <summary>
	/// Gets this sound's type.
	/// </summary>
	public SoundInfo.SoundType GetSoundType
	{
		get { return m_soundType; }
	}

	/// <summary>
	/// Gets whether this sound persists even on scene changes.
	/// </summary>
	public bool IsPersistent
	{
		get { return m_isPersistent; }
	}

    #endregion // Public Interface

    #region Initialization

    /// <summary>
    /// This initialization method should be called by sound manager classes only.
    /// </summary>
    /// <param name="soundManager">Sound manager handling this sound object.</param>
    /// <param name="soundType">Type of sound this object contains.</param>
    /// <param name="startMuted">Whether sound should start muted.</param>
    public bool Initialize(SoundManagerBase soundManager, SoundInfo.SoundType soundType, bool startMuted)
	{
		if (soundManager == null)
		{
			return false;
		}
		m_soundManager = soundManager;
		m_soundType = soundType;

		// Get this object's AudioSource component (either attached to itself or to its children)
		m_audioSource = this.GetComponentInChildren<AudioSource>();
		// One-shot sounds do not loop
		if (m_soundType == SoundInfo.SoundType.ONE_SHOT)
		{
			m_audioSource.loop = false;
		}
		// If initialized by SoundManager as muted, consider this muted via MuteAll
		if (startMuted)
		{
			Mute(true);
		}

		return true;
	}

	#endregion // Initialization

	#region Audio

	// Reference to the sound system handling this sound object
	private SoundManagerBase      	m_soundManager 	= null;
	private AudioSource 			m_audioSource 	= null;
	private SoundInfo.SoundType 	m_soundType 	= SoundInfo.SoundType.REGULAR;

	// Whether this sound persists even on scene changes
	private bool 	m_isPersistent 	= false;
	// Tracks when sound is paused via Pause (to distinguish from pausing via PauseAllSounds)
	private bool	m_isPaused		= false;
	// Tracks when sound is muted via Mute (to distinguish from muting via MuteAllSounds)
	private bool	m_isMuted		= false;
	// Time since sound started playing
	//	Note: AudioSource.time is not precise, so there is need for a separate time tracker
	private float 	m_playbackTime 	= 0.0f;

	/// <summary>
	/// Updates the playback time.
	/// </summary>
	private void UpdatePlaybackTime()
	{
		if (!HasFinishedPlayback())
		{
			m_playbackTime += Time.deltaTime;
		}
	}

	/// <summary>
	/// Tracks playback of one-shot sound and deletes it when playback is done.
	/// </summary>
	private void UpdateOneShot()
	{
		// One-shot sound effects are destroyed when they finish playback
		if (m_soundType == SoundInfo.SoundType.ONE_SHOT && HasFinishedPlayback())
		{
			Delete();
		}
	}

	#endregion // Audio

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
		if (m_isPaused)
		{
			return;
		}

		UpdatePlaybackTime();
		UpdateOneShot();
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
