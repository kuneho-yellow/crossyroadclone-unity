/******************************************************************************
*  @file       GameData.cs
*  @brief      Holds static game values
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> Holds game settings and variables that are not part of Soomla Store
******************************************************************************/

// TODO: Special things to do when saving arrays/strings

public struct GameData
{
	public  int     TopScore;
    public  bool    IsMuted;
    public  long    NextGiftTime;
    public  int     GiftCount;
    public  int     OrientationPref;
    public  bool    UnlockedAchiev_CollectChars00;
    public  bool    UnlockedAchiev_CollectChars01;
    public  bool    UnlockedAchiev_CollectChars02;
    public  bool    UnlockedAchiev_PlayGacha00;
    public  bool    UnlockedAchiev_TrainTracks00;
    public  bool    UnlockedAchiev_StraightLine00;

    public void Initialize ()
	{
        TopScore        = 0;
        IsMuted         = false;
        NextGiftTime    = 0;
        GiftCount       = 0;
        OrientationPref = 0;
        UnlockedAchiev_CollectChars00   = false;
        UnlockedAchiev_CollectChars01   = false;
        UnlockedAchiev_CollectChars02   = false;
        UnlockedAchiev_PlayGacha00      = false;
        UnlockedAchiev_TrainTracks00    = false;
        UnlockedAchiev_StraightLine00   = false;
    }
}
