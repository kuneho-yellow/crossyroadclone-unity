/******************************************************************************
*  @file       BuildInfo.cs
*  @brief      Holds build-specific information
*  @author     
*  @date       January 1, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

public static class BuildInfo
{
	#region Public Interface

	/// <summary>
	/// Sets whether game is in debug mode.
	/// </summary>
	public static void SetDebug(bool isDebugMode = true)
	{
		s_isDebugMode = isDebugMode;
	}

	/// <summary>
	/// Gets whether game is in debug mode.
	/// </summary>
	public static bool IsDebugMode
	{
		get { return s_isDebugMode; }
	}

	#endregion // Public Interface

	#region Variables

	private static bool s_isDebugMode = false;

	#endregion // Variables
}
