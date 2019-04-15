/******************************************************************************
*  @file       DebugUtils.cs
*  @brief      Holds various utility methods for use in debugging
*  @author     Ron
*  @date       September 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

#endregion // Namespaces

public static class DebugUtils
{
    #region Public Interface

    /// <summary>
    /// Gets the name of the object that the touch with the specified index hits (touches).
    /// </summary>
    /// <param name="touchIndex">The index of the touch to be checked.</param>
    /// <returns>The name of the object being touched, if applicable. Else, empty string.</returns>
    public static string GetTouchHitName(int touchIndex = 0)
    {
        var activeTouches = TouchScript.TouchManager.Instance.ActiveTouches;
        if (activeTouches.Count == 0 || touchIndex < 0 || touchIndex >= activeTouches.Count)
        {
            return "";
        }

        TouchScript.ITouch touch = activeTouches[0];
        if (touch.Hit != null && touch.Hit.Transform != null)
        {
            return touch.Hit.Transform.name;
        }
        return "";
    }

	#endregion // Public Interface
}
