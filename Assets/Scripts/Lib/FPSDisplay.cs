/******************************************************************************
*  @file       FPSDisplay.cs
*  @brief      Displays the game's framerate
*  @author     Ron
*  @date       September 24, 2015
*      
*  @par [explanation]
*		> From http://wiki.unity3d.com/index.php/FramesPerSecond by Dave Hampson
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

public class FPSDisplay : MonoBehaviour
{
    #region Public Interface

    /// <summary>
    /// Enables/disables FPS display.
    /// </summary>
    public void Enable(bool isEnable = true)
    {
        m_isEnabled = isEnable;
    }
    
    /// <summary>
    /// Gets the duration of the last frame in milliseconds.
    /// </summary>
    public float Msec
    {
        get { return m_frameTime; }
    }

    /// <summary>
    /// Gets the framerate in frames per second.
    /// </summary>
    public float FPS
    {
        get { return m_fps; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Intervals at which the frame rate display is updated")]
    [SerializeField] private float m_updateInterval = 0.5f;

    #endregion // Serialized Variables

    #region Variables

    private bool    m_isEnabled     = false;
    private float   m_frameTime     = 0.0f;
    private float   m_fps           = 0.0f;
    private float   m_deltaTime     = 0.0f;
    private float   m_lowFPSValue   = 10.0f;
    private Color   m_normalColor   = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    private Color   m_lowFPSColor   = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    private float   m_timeSinceLastUpdate = 0.0f;
    
    #endregion // Variables

    #region MonoBehaviour
    
    /// <summary>
    /// Updates this instance.
    /// </summary>
    private void Update()
    {
        m_deltaTime += (Time.deltaTime - m_deltaTime) * 0.1f;

        // Calculate frame time and FPS every fixed interval
        m_timeSinceLastUpdate += Time.deltaTime;
        if (m_timeSinceLastUpdate > m_updateInterval)
        {
            // Update msec and FPS values
            m_frameTime = m_deltaTime * 1000.0f;
            m_fps = 1.0f / m_deltaTime;
            // Reset timer
            m_timeSinceLastUpdate = 0.0f;
        }
    }

    /// <summary>
    /// Raises the GUI event.
    /// </summary>
    private void OnGUI()
    {
        if (!m_isEnabled)
        {
            return;
        }

        int w = Screen.width;
        int h = Screen.height / 25;
        
        GUIStyle style = new GUIStyle();
        
        Rect rect = new Rect(0, 0, w, h);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h;
        style.normal.textColor = (m_fps > m_lowFPSValue) ? m_normalColor : m_lowFPSColor;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", m_frameTime, m_fps);
        GUI.Label(rect, text, style);
    }

    #endregion // MonoBehaviour
}