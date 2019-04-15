/******************************************************************************
*  @file       MapEnums.cs
*  @brief      
*  @author     Lori
*  @date       September 7, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Lane Type

public enum LaneType
{
	Grass,
	Road,
	Railroad,
	River,
	SIZE
}

#endregion // Lane Type

#region Lane Resource Type

public enum LaneResourceType
{
	Grass_Dark,
	Grass_Light,
	Road_Single,
	Road_Bottom,
	Road_Middle,
	Road_Top,
	Railroad,
	River,
	SIZE
}

#endregion // Lane Resource Type

#region Lane Set Type

public enum LaneSetType
{
	REGULAR_START,
	Grass,
	Road,
	Railroad,
	River,
	REGULAR_END,
	SPECIAL_START,
	Beginning,
	Rest,
	Tutorial,
	SPECIAL_END,
}

#endregion // Lane Set Type

#region MapItems

public enum MapItemType
{
    Tree_Tall,
    Tree_Medium,
    Tree_Short,
    Tree_Trunk,
    Rock,
    LilyPad,
    Log_1,
    Log_2,
    Log_3,
    SIZE
}

#endregion // MapItems

#region Vehicles

public enum VehicleType
{
    OrangeCar,
    PurpleCar,
    GreenCar,
    BlueMiniCar,
    TaxiCab,
    RedTruck,
    BlueTruck,
    GreenTruck,
    Train,
    PoliceCar,
    RaceCar,
    GasTruck,
    SIZE
}

#endregion // Vehicles

#region LaneDirection

public enum LaneDirection
{
    LEFT = 0,
    RIGHT,
    NONE
}

#endregion // LaneDirection