/******************************************************************************
*  @file       MicrophoneInput.cs
*  @brief      Computes the loudness of the microphone input
*  @author     Lori
*  @date       August 6, 2015
*      
*  @par [explanation]
*		> http://www.kaappine.fi/tutorials/using-microphone-input-in-unity3d/
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class MicrophoneInput : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Starts the mic input.
	/// </summary>
	public void StartMicInput()
	{
		m_audioClip = Microphone.Start(null, true, m_audioSeconds, AudioSettings.outputSampleRate);
		m_isMicOn = true;
	}

	/// <summary>
	/// Stops the mic input.
	/// </summary>
	public void StopMicInput()
	{
		Microphone.End(null);
		m_isMicOn = false;
		m_audioClip = null;
	}

	/// <summary>
	/// Gets the loudness.
	/// </summary>
	/// <returns>The loudness.</returns>
	public float GetLoudness()
	{
		return m_loudness;
	}

	#endregion // Public Interface

	#region Serialized Variables
	
	[SerializeField] private		float		m_sensitivity		= 100f;
	[SerializeField] private		float		m_dataThreshold		= 0.001f;
	[SerializeField] private		float		m_loudness			= 0f;
	[SerializeField] private		int			m_audioSeconds		= 1;

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
		if (m_isMicOn)
		{
			m_loudness = GetAveragedVolume() * m_sensitivity;
		}
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour

	#region Audio Clip

	private		AudioClip		m_audioClip		= null;
	private		bool			m_isMicOn		= false;

	/// <summary>
	/// Gets the averaged volume.
	/// </summary>
	/// <returns>The averaged volume.</returns>
	private float GetAveragedVolume()
	{
		if (m_audioClip == null)
		{
			return 0f;
		}

		int count = 256;
		int validCount = count;
		float[] data = new float[count];
		m_audioClip.GetData(data, 0);
		float sum = 0f;
		for (uint i = 0; i < count; ++i)
		{
			float absData = Mathf.Abs(data[i]);
			if (absData >= m_dataThreshold)
			{
				sum += Mathf.Abs(data[i]);
			}
			else
			{
				validCount--;
			}
		}
		if (sum == 0f)
		{
			return 0f;
		}
		return sum / validCount;
	}

	#endregion Audio Clip
}
