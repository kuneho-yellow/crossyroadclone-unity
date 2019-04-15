/******************************************************************************
*  @file       UIManager.cs
*  @brief      Handles all UI activity in the game
*  @author     Ron
*  @date       September 24, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

#endregion // Namespaces

public class UIManager : UIManagerBase
{
	#region Public Interface

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	public override bool Initialize()
	{
        m_isInitialized = true;

        return base.Initialize();
    }

    #endregion // Public Interface

    #region Serialized Variables

    #endregion // Serialized Variables

    #region Variables

    #endregion // Variables

    #region UI Button Sounds

    public void UIButtonPressHandler(object sender, System.EventArgs e)
    {
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.UIButtonPress);
    }
    public void UIButtonReleaseHandler(object sender, System.EventArgs e)
    {
        Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.UIButtonRelease);
    }

    #endregion // UI Button Sounds

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        if (!m_isInitialized)
        {
            return;
        }
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion // MonoBehaviour
}
