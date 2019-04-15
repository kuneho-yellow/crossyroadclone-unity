/******************************************************************************
*  @file       DataSystem.cs
*  @brief      Handles save-load and other data processing
*  @author     Lori
*  @date       July 24, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

#endregion // Namespaces

public class DataSystem : DataSystemBase
{
	#region Public Interface

	/// <summary>
	/// Initialize the data manager.
	/// Automatically loads saved data
	/// </summary>
	public override bool Initialize()
	{
		m_gameData = new GameData();
		m_gameData.Initialize();
		m_encryptionSystem = new XORCipher();

		Load();

		m_isInitialized = true;
		return true;
	}

	/// <summary>
	/// Save game data.
	/// </summary>
	public override bool Save()
	{
		SaveEncrypted();
		return true;
	}

	/// <summary>
	/// Load game data.
	/// </summary>
	public override bool Load()
	{
		LoadEncrypted();
		return true;
	}

	/// <summary>
	/// Gets the game data.
	/// </summary>
	/// <returns>The game data.</returns>
	public GameData GetGameData ()
	{
		return m_gameData;
	}

	/// <summary>
	/// Sets the game data.
	/// </summary>
	/// <param name="gameData">Game data.</param>
	public void SetGameData (GameData gameData)
	{
		m_gameData = gameData;
	}

	#endregion // Public Interface

	#region GameData

	private			GameData			m_gameData			= new GameData();
	private	const	string				FILE_NAME			= "com.moobunny.dwtdclone.pyonpyon.bin";
//	private	const	string				KEY					= "Berry-chanTpm:rdyrtVtodpdyp,pLptonrllrHpfpuLong-kun";
//	private	const	int					KEY					= 6111;	
	private	const	float				KEY					= 6.1117399038106f;	

	/// <summary>
	/// Gets the save directory
	/// </summary>
	/// <value>The save directory</value>
	private static string SaveDir
	{
		get
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				return Application.persistentDataPath + "/" + FILE_NAME;
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return Application.persistentDataPath + "/" + FILE_NAME; 
			}
			
			return Application.dataPath + "/" + FILE_NAME;
		}
	}

	// TODO: Generic type?

	/// <summary>
	/// Converts data to bytes.
	/// </summary>
	/// <returns>The data to bytes.</returns>
	/// <param name="data">Data.</param>
	private static byte[] ConvertDataToBytes (GameData data)
	{
		// Create a new byte array
		int size = Marshal.SizeOf(data);
		byte[] bytes = new byte[size];

		// Let the system do it's thing~
		// Copy the struct to a byte array
		System.IntPtr ptr = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(data, ptr, false);
		Marshal.Copy(ptr, bytes, 0, size);
		Marshal.FreeHGlobal(ptr);

		return bytes;
	}

	/// <summary>
	/// Converts bytes to data.
	/// </summary>
	/// <returns>The bytes to data.</returns>
	/// <param name="bytes">Bytes.</param>
	private static GameData ConvertBytesToData (byte[] bytes)
	{
		// Create a new data struct
		GameData data = new GameData();

		// Let the system do it's thing~
		// Copy the bytes to the struct
		int size = Marshal.SizeOf(typeof(GameData));
		System.IntPtr ptr = Marshal.AllocHGlobal(size);
		Marshal.Copy(bytes, 0, ptr, size);
		data = (GameData)Marshal.PtrToStructure(ptr, data.GetType());
		Marshal.FreeHGlobal(ptr);

		return data;
	}

	#endregion // GameData

	#region Encrypted Save-Load

	private		EncryptionSystemBase		m_encryptionSystem		= null;

	/// <summary>
	/// Saves the encrypted game data
	/// </summary>
	/// <returns><c>true</c>, if the encrypted game data was saved, <c>false</c> otherwise.</returns>
	private bool SaveEncrypted ()
	{
		// Encrypt the data first
		byte[] encryptedByteData = m_encryptionSystem.Encrypt(ConvertDataToBytes(m_gameData),
//		                                                      System.Text.Encoding.ASCII.GetBytes(KEY));
		                                                      System.BitConverter.GetBytes(KEY));	
		try
		{
			using (FileStream stream = File.Open(SaveDir, FileMode.Create))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(encryptedByteData);
					writer.Close();
				}
				stream.Close();
			}
			return true;
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
			return false;
		}
	}

	/// <summary>
	/// Loads the encrypted game data
	/// </summary>
	/// <returns><c>true</c>, if the encrypted game data was loaded, <c>false</c> otherwise.</returns>
	private bool LoadEncrypted ()
	{
		byte[] encryptedByteData = new byte[Marshal.SizeOf(m_gameData)];

		try
		{
			using (FileStream stream = File.Open(SaveDir, FileMode.Open))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					encryptedByteData = reader.ReadBytes(encryptedByteData.Length);
					reader.Close();
				}
				stream.Close();
			}

			// Decrypt the data
			m_gameData = ConvertBytesToData(m_encryptionSystem.Decrypt(encryptedByteData,
//			                                                           System.Text.Encoding.ASCII.GetBytes(KEY)));
			                                                           System.BitConverter.GetBytes(KEY)));
			return true;
		}
		catch
		{
			Debug.Log("Creating new save file at '" + SaveDir + "'");
			Save();
			return false;
		}
	}

	#endregion // Encrypted Save-Load
}
