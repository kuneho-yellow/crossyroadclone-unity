/******************************************************************************
*  @file       TitleSceneMaster.cs
*  @brief      Handles the lifetime of a single scene
*  @author     Ron
*  @date       October 10, 2015
*      
*  @par [explanation]
*		> Loads and unloads resources for one scene
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class TitleSceneMaster : SceneMasterBase
{
	#region Public Interface

	public override bool Load()
	{
        m_titleUI.Initialize(true);
        DontDestroyOnLoad(m_titleUI.gameObject);
        
        // Set initialized flag
        m_isInitialized = true;

        return true;
	}

	public override bool Unload()
	{
        // Clear initialized flag
        m_isInitialized = false;

		return true;
	}

	public override void StartScene()
	{
        // Scroll in title and show title BG
        m_titleUI.ShowTitle();
        m_titleUI.StartTitleEnter();
        m_titleUI.ShowBG(false);
        
        // Switch to GAME scene
        Locator.GetMain().NotifySwitchScene(SceneInfo.SceneEnum.GAME);
    }

	#endregion // Public Interface

	#region Serialized Variables

    [SerializeField] private TitleUI m_titleUI  = null;

	#endregion // Serialized Variables

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
