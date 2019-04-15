/******************************************************************************
*  @file       Locator.cs
*  @brief      Service locator class
*  @author     Ron
*  @date       October 17, 2015
*      
*  @par [explanation]
*		> Provides a global point of access to various services
******************************************************************************/

public static class Locator
{
	#region Public Interface

	/// <summary>
	/// Initialize the service locator.
	/// </summary>
	public static void Initialize()
	{
		// Initialize service references to "null" services
	}

	// Locators
    public static Main GetMain()
    {
        return m_main;
    }
	public static SceneMasterBase GetSceneMaster()
	{
		return m_sceneMaster;
	}
	public static UIManagerBase GetUIManager()
	{
		return m_uiManager;
	}
	public static SoundManagerBase GetSoundManager()
	{
		return m_soundManager;
	}
	public static DataSystemBase GetDataSystem()
	{
		return m_dataSystem;
	}
    public static PlayServicesSystem GetPlayServicesSystem()
    {
        return m_playServicesSystem;
    }
    public static NotificationSystem GetNotifSystem()
    {
        return m_notifSystem;
    }
	public static GameManager GetGameManager()
	{
		return m_gameManager;
	}

	// Providers
    public static void ProvideMain(Main main)
    {
        m_main = main;
    }
	public static void ProvideSceneMaster(SceneMasterBase sceneMaster)
	{
		m_sceneMaster = sceneMaster;
	}
	public static void ProvideUIManager(UIManagerBase uiManager)
	{
		m_uiManager = uiManager;
	}
	public static void ProvideSoundManager(SoundManagerBase soundManager)
	{
		m_soundManager = soundManager;
	}
	public static void ProvideDataSystem(DataSystemBase dataSystem)
	{
		m_dataSystem = dataSystem;
	}
    public static void ProvidePlayServicesSystem(PlayServicesSystem playServicesSystem)
    {
        m_playServicesSystem = playServicesSystem;
    }
    public static void ProvideNotifSystem(NotificationSystem notifSystem)
    {
        m_notifSystem = notifSystem;
    }
    public static void ProvideGameManager(GameManager gameManager)
	{
		m_gameManager = gameManager;
	}
	
    #endregion // Public Interface

    #region References

    private static Main                 m_main                  = null;
    private static SceneMasterBase	    m_sceneMaster 	        = null;
	private static UIManagerBase	    m_uiManager		        = null;
	private static SoundManagerBase	    m_soundManager	        = null;
	private static DataSystemBase	    m_dataSystem	        = null;
    private static PlayServicesSystem   m_playServicesSystem    = null;
    private static NotificationSystem   m_notifSystem           = null;
    private static GameManager		    m_gameManager	        = null;

	#endregion // References
}
