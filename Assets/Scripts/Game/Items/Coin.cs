/******************************************************************************
*  @file       Coin.cs
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

public class Coin : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initialize this instance
    /// </summary>
    public void Initialize()
    {
        m_collider.enabled = false;
    }

    /// <summary>
    /// Activate this instance
    /// </summary>
    public void Activate()
    {
        m_collider.enabled = true;
        m_idleDelayTimer = 0f;
        m_idleDelayDur = Random.Range(m_idleDelayMin, m_idleDelayMax);
        m_animator.Play(m_emptyAnim);
        if (m_idleDelayDur == 0f)
        {
            m_animator.Play(m_idleAnim);
        }
        m_coinAnim.SetOnCoinGetDelegate(OnGetAnimEnd);
    }

    /// <summary>
    /// Deactivate this instance
    /// </summary>
    public void Deactivate()
    {
        m_collider.enabled = false;
        transform.parent = m_coinRoot;
        transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;
        m_animator.speed = 0f;
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;
        m_animator.speed = 1f;
    }

    /// <summary>
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

    public delegate void OnCoinGet();

    /// <summary>
    /// Sets the on coin get delegate
    /// </summary>
    /// <param name="onCoinGet"></param>
    public void SetOnCoinGetDelegate(OnCoinGet onCoinGet)
    {
        m_onCoinGet = onCoinGet;
    }
    
    /// <summary>
    /// Sets the coin root
    /// </summary>
    /// <param name="coinRoot"></param>
    public void SetCoinRoot(Transform coinRoot)
    {
        m_coinRoot = coinRoot;
    }

    #endregion // Public Interface

    #region Serialized Variables

    [SerializeField]private     CoinAnim        m_coinAnim      = null;
    [SerializeField]private     Collider        m_collider      = null;
    [SerializeField]private     Animator        m_animator      = null;
    [SerializeField]private     string          m_emptyAnim     = "CoinEmpty";
    [SerializeField]private     string          m_idleAnim      = "CoinJump";
    [SerializeField]private     string          m_getAnim       = "CoinGet";
    [SerializeField]private     float           m_idleDelayMin  = 0f;
    [SerializeField]private     float           m_idleDelayMax  = 20f;

    #endregion // Serialized Variables

    #region Variables

    private     bool            m_isPaused      = false;
    private     Transform       m_coinRoot      = null;
    private     OnCoinGet       m_onCoinGet     = null;
    private     float           m_idleDelayTimer= 0f;
    private     float           m_idleDelayDur  = 0f;

    #endregion // Variables

    #region MonoBehaviour

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
	{
		if (m_isPaused)
        {
            return;
        }

        if (m_idleDelayDur > 0f)
        {
            m_idleDelayTimer += Time.deltaTime;
            if (m_idleDelayTimer >= m_idleDelayDur)
            {
                m_animator.Play(m_idleAnim);
                m_idleDelayDur = 0f;
            }
        }
	}

    /// <summary>
    /// Raises the trigger enter event
    /// </summary>
    /// <param name="col"></param>
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            m_animator.Play(m_getAnim);
            m_collider.enabled = false;
            m_idleDelayDur = 0f;
        }
    }

    #endregion // MonoBehaviour

    #region Animation Events

    /// <summary>
    /// Called when GetAnim ends
    /// </summary>
    private void OnGetAnimEnd()
    {
        if (m_onCoinGet != null)
        {
            m_onCoinGet();
        }
    }

    #endregion // Animation Events
}
