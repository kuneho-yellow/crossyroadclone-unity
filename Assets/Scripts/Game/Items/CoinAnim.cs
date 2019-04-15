/******************************************************************************
*  @file       CoinAnim.cs
*  @brief      
*  @author     Lori
*  @date       January 1, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class CoinAnim : MonoBehaviour
{
	#region Public Interface

    public delegate void OnCoinGet();

    /// <summary>
    /// Sets the on coin get delegate
    /// </summary>
    /// <param name="onCoinGet"></param>
    public void SetOnCoinGetDelegate(OnCoinGet onCoinGet)
    {
        m_onCoinGet = onCoinGet;
    }

    #endregion // Public Interface

    #region Animation Events

    private     OnCoinGet       m_onCoinGet     = null;

    private void OnGetAnimEnd()
    {
        if (m_onCoinGet != null)
        {
            m_onCoinGet();
        }
    }

    #endregion // Animation Events
}
