/******************************************************************************
*  @file       Main.cs
*  @brief      Handles first-run initialization and scene transitions
*  @author     Ron
*  @date       October 15, 2015
*      
*  @par [explanation]
*       > There can be only one instance of Main in the scene
*       > The first instance registers itself to the service locator, through
*           which it could be accessed by other classes
*       > When a new Main instance is created, it first checks if another
*           already exists, and if so, destroys itself
*		> Functions:
*           - Initializes systems that need initializing upon loading of the app
*		    - In scene transitions, loads resources for the next scene,
*			    unloads resources from previous scene, and optionally shows a
*			    loading screen and/or fades the scene in/out during this period
******************************************************************************/

//#define SHOW_STATE_LOGS

#region Namespaces

using UnityEngine;
using System;
using System.Collections;

#endregion // Namespaces

public class Main : MonoBehaviour
{
	#region Public Interface

	/// <summary>
	/// Notifies Main of request to load the specified scene.
	/// </summary>
	/// <returns><c>true</c>, if scene switch request is granted, <c>false</c> otherwise.</returns>
	/// <param name="nextScene">Scene to switch to.</param>
	/// <param name="fadeOut">Whether scene should fade out before switching to the next scene</param>
	public bool NotifySwitchScene(SceneInfo.SceneEnum nextScene, bool fadeOut = true)
	{
		if (nextScene == SceneInfo.SceneEnum.SIZE)
		{
			Debug.LogWarning("Specified item is not a scene");
			return false;
		}

		// If already loading a new scene, do not load another scene
		if (m_curScene != m_nextScene)
		{
			return false;
		}

        m_nextScene = nextScene;
		m_enableFadeAnim = fadeOut;

		// Start scene loading
		m_initState = InitState.LOAD_SCENE;

		return true;
	}

	/// <summary>
	/// Sets the scene master.
	/// </summary>
	/// <param name="sceneMaster">Scene master.</param>
	public void SetSceneMaster(SceneMasterBase sceneMaster)
	{
		m_sceneMaster = sceneMaster;
	}

	/// <summary>
	/// Gets the scene enum of the current scene.
	/// </summary>
	public SceneInfo.SceneEnum GetCurSceneEnum
	{
		get { return m_curScene; }
	}
    
    /// <summary>
	/// Gets the scene enum of the previous scene.
	/// </summary>
	public SceneInfo.SceneEnum GetPrevSceneEnum
    {
        get { return m_prevScene; }
    }

    /// <summary>
    /// Gets whether game started from the Main scene.
    /// </summary>
    public bool IsGameStartedFromMain
    {
        get { return m_isGameStartedFromMain; }
    }

    /// <summary>
    /// Gets whether the scene is initialized and game is ready to run.
    /// </summary>
    public bool IsSceneInitialized
	{
		get { return m_initState == InitState.IDLE; }
	}

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("The scene master handling the current scene")]
    [SerializeField] private SceneMasterBase    m_sceneMaster           = null;
    [Tooltip("Denote active touch points with sprites")]
    [SerializeField] private bool               m_enableTouchDebugger   = false;
    [Tooltip("Play fade in/out animation when switching scenes")]
    [SerializeField] private bool               m_enableFadeAnim        = false;
    [Tooltip("Enable support for saving game progress on the cloud")]
    [SerializeField] private bool               m_enableSavedGames      = false;
    [Tooltip("Initialize and enable the push notification service")]
    [SerializeField] private bool               m_enablePushNotifs      = false;
    [Tooltip("Display the game's frame rate")]
    [SerializeField] private bool               m_showFPS               = false;
    
    #endregion // Serialized Variables

    #region Events
    
    /// <summary>
    /// Occurs when the scene has finished initialization.
    /// </summary>
    public event EventHandler<EventArgs> SceneInitialized
    {
        add { sceneInitializedInvoker += value; }
        remove { sceneInitializedInvoker -= value; }
    }

    // From TouchScript.Gesture: Needed to overcome iOS AOT limitations
    private EventHandler<EventArgs> sceneInitializedInvoker;

    #endregion // Events

    #region InitState

    // Note: Systems will be initialized in the order they are listed in this enum
    private enum InitState
	{
		NONE = 0,

		INIT_SHARED_SYSTEMS,
		WAIT_INIT_SHARED_SYSTEMS,

		INIT_LOCATOR,			// Should be after shared system initialization and before scene loading

		LOAD_SCENE,				// When switching scenes, set m_nextScene, then switch state to LOAD_SCENE
		WAIT_LOAD_SCENE,
		UNLOAD_CUR_SCENE,
		WAIT_UNLOAD_CUR_SCENE,
		SWITCH_SCENE,
		LOAD_SCENE_MASTER,
		WAIT_LOAD_SCENE_MASTER,

		DONE,
		IDLE,
	}
	private InitState m_initState = InitState.NONE;

    #endregion // InitState

    #region Variables
    
	// Each scene has a Main script which is not destroyed on scene switch,
	//	meaning multiple instances of Main will exist (at least for a while,
	//	until the second instance destroys itself).
	// 	This flag marks the instance that will remain in the game.
	private bool m_isActiveMain	= false;

    // Class that calculates and displays the game's framerate
    private FPSDisplay m_fpsDisplay = null;

    #endregion // Variables

    #region Common Systems

    private UIManagerBase       m_uiManager             = null;
    private SoundManagerBase    m_soundManager          = null;
    private DataSystemBase      m_dataSystem            = null;
    private PlayServicesSystem  m_playServicesSystem    = null;
    private NotificationSystem  m_notifSystem           = null;

    private const string        UI_MANAGER_OBJ_NAME     = "UIManager";
    private const string        SOUND_MANAGER_OBJ_NAME  = "SoundManager";

    #endregion // Common Systems

    #region Initialization

    // Max progress that async operation reaches when scene activation is deferred,
    //	i.e. allowSceneActivation = false
    private const float READY_TO_LOAD_PROGRESS = 0.9f;

    /// <summary>
    /// Updates the initialization process.
    /// </summary>
    private void UpdateInitState()
	{
		switch (m_initState)
		{
		case InitState.NONE:
            AdvanceInitState();
			break;
		
		case InitState.INIT_SHARED_SYSTEMS:
#if SHOW_STATE_LOGS
			Debug.Log("Initializing shared systems...");
#endif
            // Find or create containers for shared MonoBehaviour systems
            GameObject uiManagerObj = GameObject.Find(UI_MANAGER_OBJ_NAME);
            if (uiManagerObj == null)
            {
                uiManagerObj = new GameObject(UI_MANAGER_OBJ_NAME);
            }
            GameObject soundManagerObj = GameObject.Find(SOUND_MANAGER_OBJ_NAME);
            if (soundManagerObj == null)
            {
                soundManagerObj = new GameObject(SOUND_MANAGER_OBJ_NAME);
            }

            // Create and store references to shared systems
            m_uiManager = uiManagerObj.AddComponentNoDupe<UIManager>();
            m_soundManager = soundManagerObj.AddComponentNoDupe<SoundManager>();
            m_dataSystem = new SoomlaDataSystem();
            m_notifSystem = new NotificationSystem();
            m_playServicesSystem = new PlayServicesSystem();

            // Shared systems are not destroyed on scene switches
            DontDestroyOnLoad(uiManagerObj);
            DontDestroyOnLoad(soundManagerObj);

			// Initialize shared systems
			m_uiManager.Initialize();
			m_soundManager.Initialize();
			m_dataSystem.Initialize();
            m_playServicesSystem.Initialize(m_enableSavedGames);
            m_notifSystem.Initialize(m_enablePushNotifs);

            // Add FPSDisplay component
            m_fpsDisplay = this.gameObject.AddComponentNoDupe<FPSDisplay>();
            
			AdvanceInitState();
			break;
		case InitState.WAIT_INIT_SHARED_SYSTEMS:
#if SHOW_STATE_LOGS
			Debug.Log("Waiting for shared system initialization to finish...");
#endif
			// Wait for shared systems to finish initialization
			if (m_uiManager.IsInitialized &&
			    m_soundManager.IsInitialized &&
			    m_dataSystem.IsInitialized &&
                m_playServicesSystem.IsInitialized &&
                m_notifSystem.IsInitialized)
			{
				AdvanceInitState();
			}
			break;
		case InitState.INIT_LOCATOR:
#if SHOW_STATE_LOGS
			Debug.Log("Initializing Service Locator...");
#endif
			Locator.ProvideUIManager(m_uiManager);
			Locator.ProvideSoundManager(m_soundManager);
			Locator.ProvideDataSystem(m_dataSystem);
            Locator.ProvidePlayServicesSystem(m_playServicesSystem);
            Locator.ProvideNotifSystem(m_notifSystem);
            
			// If game is loaded from the Main scene (index 0)
			if (Application.loadedLevel == 0)
			{
				AdvanceInitState();
			}
			// If game is loaded from a different scene
			else
			{
				// No need to load a different scene
				// Just load the SceneMaster
				m_initState = InitState.LOAD_SCENE_MASTER;
			}
			break;

		// The following states are cycled through whenever scenes are switched
		case InitState.LOAD_SCENE:
#if SHOW_STATE_LOGS
			Debug.Log("Loading scene...");
#endif
			// Block input while next scene is loading
			m_uiManager.SetBlockInput();

			StartLoading(m_sceneInfo.GetSceneNameOf(m_nextScene));

			AdvanceInitState();
			break;
		case InitState.WAIT_LOAD_SCENE:
			// Wait for scene to finish loading in the background
#if SHOW_STATE_LOGS
			Debug.Log("Waiting for scene to load in the background...");
#endif
			if (m_async != null && m_async.progress >= READY_TO_LOAD_PROGRESS)
			{
				// Start fade out
				if (m_enableFadeAnim)
				{
                    // There is nothing to fade out if starting from Main scene
                    if (Application.loadedLevel != 0)
                    {
                        m_uiManager.StartFadeOut(true);
                    }
				}

				AdvanceInitState();
			}
			break;
		case InitState.UNLOAD_CUR_SCENE:
#if SHOW_STATE_LOGS
			Debug.Log("Unloading current scene...");
#endif
			// If starting from Main scene, there will be nothing to unload
			if (Application.loadedLevel == 0 || m_sceneMaster.Unload())
			{
				AdvanceInitState();
			}
			break;
		case InitState.WAIT_UNLOAD_CUR_SCENE:
#if SHOW_STATE_LOGS
			Debug.Log("Waiting for current scene to finish unloading...");
#endif
			// If starting from Main scene, there will be nothing to unload
			if (Application.loadedLevel == 0 || !m_sceneMaster.IsInitialized)
			{
				// If scene fading is enabled, wait for scene to fade out first
				if (!m_enableFadeAnim ||
				    (m_enableFadeAnim && m_uiManager.IsFadedOut()))
				{
					// Clean up non-persistent sounds
					m_soundManager.DeleteAllSoundObjects(false);
                    
					AdvanceInitState();
				}
			}
			break;
		case InitState.SWITCH_SCENE:
			// Load the next scene
#if SHOW_STATE_LOGS
			Debug.Log("Switching scene...");
#endif
			ActivateScene();
			// Initialization will continue in OnLevelWasLoaded
			break;

		case InitState.LOAD_SCENE_MASTER:
#if SHOW_STATE_LOGS
			Debug.Log("Loading scene master in scene " + Application.loadedLevelName);
#endif
			if (m_sceneMaster.Load())
			{
				// Provide SceneMaster to the service locator
				Locator.ProvideSceneMaster(m_sceneMaster);

				AdvanceInitState();
			}
			break;
		case InitState.WAIT_LOAD_SCENE_MASTER:
#if SHOW_STATE_LOGS
			Debug.Log("Waiting for scene master to load...");
#endif
			if (m_sceneMaster.IsInitialized)
			{
				// Start fade in
				if (m_enableFadeAnim)
				{
					m_uiManager.StartFadeIn();
				}

                // Fire the SceneInitialized event
                if (sceneInitializedInvoker != null)
                {
                    sceneInitializedInvoker(this, new System.EventArgs());
                }

				AdvanceInitState();
			}
			break;
		
		case InitState.DONE:
#if SHOW_STATE_LOGS
			if (BuildInfo.IsDebugMode)
			{
				Debug.Log("Main initialization complete");
			}
#endif
			// Switch to IDLE state
			// If the SceneMaster switches the scene, this state change will be overridden
			AdvanceInitState();
            
            // Keep track of previous scene
            m_prevScene = m_curScene;
			// Update scene enum for the current scene
			m_curScene = m_nextScene;
			// Start the scene - pass control over to the active scene master
			m_sceneMaster.StartScene();
			break;
		case InitState.IDLE:
			break;
		}
	}
    
	/// <summary>
	/// Advances initialization to the next phase.
	/// </summary>
	private void AdvanceInitState()
	{
		// Move on to the next initialization phase
		if (m_initState != InitState.IDLE)
		{
			m_initState = (InitState)((int)(m_initState + 1));
		}
	}
	
	#endregion // Initialization

	#region Scene Management
	
	private AsyncOperation 	m_async		= null;
	private	SceneInfo		m_sceneInfo	= new SceneInfo();

	private SceneInfo.SceneEnum m_nextScene = (SceneInfo.SceneEnum)0;
	private SceneInfo.SceneEnum m_curScene  = (SceneInfo.SceneEnum)0;
    private SceneInfo.SceneEnum m_prevScene = (SceneInfo.SceneEnum)0;

    private bool m_isGameStartedFromMain = false;

    private const string SCENE_MASTER_OBJ_NAME = "SceneMaster";
	
	/// <summary>
	/// Starts loading the next scene.
	/// </summary>
	private void StartLoading(string levelName)
	{
		StartCoroutine(Load(levelName));
	}

	/// <summary>
	/// Loads the specified level asynchronously.
	/// </summary>
	/// <param name="levelName">Name of level to load.</param>
	private IEnumerator Load(string levelName)
	{
#if SHOW_STATE_LOGS
		if (BuildInfo.IsDebugMode)
		{
			Debug.Log("Loading " + levelName + " scene...");
		}
#endif
		m_async = Application.LoadLevelAsync(levelName);
		m_async.allowSceneActivation = false;
		yield return m_async;
	}

	/// <summary>
	/// Allows the next scene to activate once ready.
	/// </summary>
	private void ActivateScene()
	{
		m_async.allowSceneActivation = true;
	}
	
	/// <summary>
	/// Finds the SceneMaster in the new scene.
	/// </summary>
	private SceneMasterBase FindSceneMaster()
	{
		GameObject obj = GameObject.Find(SCENE_MASTER_OBJ_NAME);
		if (obj == null)
		{
			Debug.LogError("No SceneMaster object found!");
		}
		SceneMasterBase sceneMaster = obj.GetComponent<SceneMasterBase>();
		if (sceneMaster == null)
		{
			Debug.LogError("No SceneMaster component found!");
		}
		return sceneMaster;
	}

	#endregion // Scene Management

	#region Touch Debugger

	/// <summary>
	/// Updates whether the touch debugger should be active or not.
	/// </summary>
	private void UpdateTouchDebugger()
	{
		if (BuildInfo.IsDebugMode)
		{
			if (m_sceneMaster != null && m_sceneMaster.TouchDebugger != null)
			{
				m_sceneMaster.TouchDebugger.gameObject.SetActive(m_enableTouchDebugger);
			}
		}
	}

	#endregion // Touch Debugger

	#region MonoBehaviour

	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
        // Initialize the service locator
        Locator.Initialize();

        if (Locator.GetMain() == null)
        {
            // Provide this instance of Main to the service locator
            Locator.ProvideMain(this);
            // Main is always present in the game
            DontDestroyOnLoad(this.gameObject);
            // Mark this as the only instance of Main allowed in the game
            m_isActiveMain = true;

            // Check if game started from the Main scene
            int loadedLevel = Application.loadedLevel;
            if (loadedLevel == 0)
            {
                m_isGameStartedFromMain = true;
            }
            // If not started from Main, initialize scene enum values to the current scene
            else
            {
                // Scene enums are just the scene index minus one 
                SceneInfo.SceneEnum curScene = (SceneInfo.SceneEnum)loadedLevel - 1;
                m_prevScene = curScene;
                m_curScene = curScene;
            }
        }
        else
        {
            // Pass the reference to this scene's SceneMaster to the existing Main instance
            Locator.GetMain().SetSceneMaster(m_sceneMaster);
            // There can be only one instance of Main in the game
            Destroy(this.gameObject);
        }
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
		UpdateInitState();

		UpdateTouchDebugger();

        if (IsSceneInitialized)
        {
            // Update FPSDisplay
            m_fpsDisplay.Enable(m_showFPS);
        }
	}

	/// <summary>
	/// Raises the level loaded event.
	/// </summary>
	/// <param name="loadedLevelIndex">Index of the loaded level in the Build Settings scene list.</param>
	private void OnLevelWasLoaded(int loadedLevelIndex)
	{
		// Only the active Main instance should handle this event
		if (m_isActiveMain)
		{
#if SHOW_STATE_LOGS
			if (BuildInfo.IsDebugMode)
			{
				Debug.Log("Level loaded!");
			}
#endif

			// TODO: Improve? Currently searches scene master by name
			SetSceneMaster(FindSceneMaster());

			// Proceed with initialization
			AdvanceInitState();
		}
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy()
	{

	}

	#endregion // MonoBehaviour
}
