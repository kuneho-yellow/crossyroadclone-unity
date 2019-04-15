/******************************************************************************
*  @file       XORCipher.cs
*  @brief      Super simple XOR encryption
*  @author     Lori
*  @date       July 24, 2015
*      
*  @par [explanation]
*		> Does XOR encryption on byte data using byte key
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class XORCipher : EncryptionSystemBase
{
	#region Public Interface

	public override byte[] Encrypt(byte[] data, byte[] key)
	{
		byte[] encryptedData = new byte[data.Length];
		for (uint i = 0; i < data.Length; ++i)
		{
			if (i % 11 == 6)
			{
				encryptedData[i] = data[i];
			}
			else
			{
				encryptedData[i] = (byte)(data[i] ^ key[i % key.Length]);
			}
		}
		return encryptedData;
	}

	public override byte[] Decrypt(byte[] data, byte[] key)
	{
		// Symmetric
		return Encrypt(data, key);
	}
	
	#endregion // Public Interface
}
