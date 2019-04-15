/******************************************************************************
*  @file       DateTimeExtensions.cs
*  @brief      Extensions for System.DateTime
*  @author     Ron
*  @date       September 22, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using System;

#endregion // Namespaces

public static class DateTimeExtensions
{
	#region Public Interface

	/// <summary>
	/// Gets a timestamp string of the specified date-time value, formatted
    /// from highest- to lowest- order bits (years first, milliseconds last).
	/// </summary>
	public static string GetTimestamp(this DateTime value)
	{
        return value.ToString("yyyyMMddHHmmssfff");
	}

	#endregion // Public Interface
}
