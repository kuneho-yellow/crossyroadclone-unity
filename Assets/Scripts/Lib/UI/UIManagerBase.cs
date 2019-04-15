/******************************************************************************
*  @file       UIManagerBase.cs
*  @brief      Abstract base class for UI manager classes
*  @author     Ron
*  @date       October 18, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;

#endregion // Namespaces

public class UIManagerBase : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual bool Initialize()
    {
        // Create and initialize shared UI
        InitializeSharedUI();

        return true;
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    /// <summary>
    /// Sets the fader overlay to block or allow input.
    /// </summary>
    public void SetBlockInput(bool blockInput = true)
    {
        m_uiFader.SetBlockInput(blockInput);
    }

    /// <summary>
    /// Starts the fade in (for scene transitions).
    /// </summary>
    public void StartFadeIn(bool startFadedOut = false)
    {
        m_uiFader.FadeIn(startFadedOut);
    }

    /// <summary>
    /// Starts the fade out (for scene transitions).
    /// </summary>
    public void StartFadeOut(bool startFadedIn = false)
    {
        m_uiFader.FadeOut(startFadedIn);
    }

    /// <summary>
    /// Determines whether the fader state is FADED_OUT.
    /// </summary>
    public bool IsFadedOut()
    {
        return m_uiFader.FaderState == UIFader.FadeAnimationState.FADED_OUT;
    }

    /// <summary>
    /// Gets the state of the fader animation.
    /// </summary>
    public UIFader.FadeAnimationState FaderAnimState
    {
        get { return (m_uiFader != null) ? m_uiFader.FaderState : UIFader.FadeAnimationState.NONE; }
    }

    /// <summary>
    /// Gets whether the screen orientation changed in the last frame.
    /// Note that this returns true only on the *frame* the screen orientation changes.
    /// </summary>
    public bool HasScreenOrientationChanged
    {
        get { return m_hasScreenOrientationChanged; }
    }

    /// <summary>
    /// Gets the UI camera.
    /// </summary>
    public UICamera UICamera
    {
        get
        {
            if (Locator.GetSceneMaster() != null)
            {
                return Locator.GetSceneMaster().UICamera;
            }
            return null;
        }
    }

    #endregion // Public Interface

    #region Variables

    protected bool  m_isInitialized     = false;

    #endregion // Variables

    #region Screen Orientation Check

    // Used to detect changes in screen orientation
    private bool    m_isPrevLandscape              = false;
    // Whether screen orientation changed in the last frame
    private bool    m_hasScreenOrientationChanged  = false;

    /// <summary>
    /// Detects changes in screen orientation.
    /// </summary>
    private void CheckScreenOrientation()
    {
        // Check if screen orientation changes
        bool isCurLandscape = Screen.width > Screen.height;
        if (m_isPrevLandscape != isCurLandscape)
        {
            // Set the screen orientation changed flag
            m_hasScreenOrientationChanged = true;
            // Update screen orientation check value
            m_isPrevLandscape = isCurLandscape;
        }
        else
        {
            // If no change, reset the screen orientation changed flag
            m_hasScreenOrientationChanged = false;
        }
    }

    #endregion // Screen Orientation Check

    #region Shared UI

    private UIFader m_uiFader = null;

    private const string UI_FADER_PREFAB_PATH = "Prefabs/UI/UIFader";

    /// <summary>
    /// Creates and initializes shared UI objects.
    /// </summary>
    private void InitializeSharedUI()
    {
        if (m_uiFader == null)
        {
            m_uiFader = CreateSharedUIFromPrefab<UIFader>(UI_FADER_PREFAB_PATH);
            m_uiFader.transform.parent = this.transform;
        }
        if (!m_uiFader.IsInitialized)
        {
            m_uiFader.Initialize();
        }
    }

    /// <summary>
    /// Creates a shared UI object from prefab.
    /// </summary>
    /// <returns>The shared UI class.</returns>
    /// <param name="prefabPath">Prefab path.</param>
    /// <typeparam name="T">Shared UI class type.</typeparam>
    private T CreateSharedUIFromPrefab<T>(string prefabPath) where T : Component
    {
        GameObject resource = Resources.Load(prefabPath, typeof(GameObject)) as GameObject;
        if (resource != null)
        {
            GameObject obj = GameObject.Instantiate(resource);
            return obj.AddComponentNoDupe<T>();
        }
        Debug.LogError("No prefab found at " + prefabPath);
        return null;
    }

    #endregion // Shared UI

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected virtual void Awake()
    {

    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    protected virtual void Start()
    {

    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    protected virtual void Update()
    {
        // Screen orientation is only relevant on mobile platforms
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        CheckScreenOrientation();
#endif
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected virtual void OnDestroy()
    {

    }

    #endregion // MonoBehaviour
}
