/******************************************************************************
*  @file       EncryptionSystemBase.cs
*  @brief      Abstract base class for encryption system classes
*  @author     
*  @date       July 21, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public abstract class EncryptionSystemBase
{
	#region Public Interface

	public abstract byte[] Encrypt(byte[] data, byte[] key);
	public abstract byte[] Decrypt(byte[] data, byte[] key);

	#endregion // Public Interface
}
