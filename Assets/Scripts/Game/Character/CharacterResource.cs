/******************************************************************************
*  @file       CharacterResource.cs
*  @brief      Holds data for playable characters
*  @author     Ron
*  @date       October 8, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

#endregion // Namespaces

public class CharacterResource
{
	#region Public Interface

    /// <summary>
    /// Loads character data and resources.
    /// </summary>
    /// <param name="characterDataJSONPath"></param>
    public void InitializeData(string characterDataJSONPath)
    {
        // Read character data from JSON
        TextAsset dataText = Resources.Load(characterDataJSONPath) as TextAsset;
        JSONNode node = JSON.Parse(dataText.text);
        int characterCount = (int)CharacterType.SIZE;
        m_characterArray = new CharacterStruct[characterCount];
        for (int charNo = 0; charNo < characterCount; ++charNo)
        {
            // Read character properties
            JSONNode charNode = node["characters"][charNo];
            m_characterArray[charNo].Name       = charNode["name"].Value;
            m_characterArray[charNo].PrefabPath = CHARACTER_FOLDER_PATH + charNode["prefabPath"].Value;
            m_characterArray[charNo].ItemID     = charNode["itemID"].Value;
            m_characterArray[charNo].MapType    = charNode["mapType"].AsInt;
            m_characterArray[charNo].IsInGacha  = charNode["isInGacha"].AsBool;
            m_characterArray[charNo].IsBuyable  = charNode["isBuyable"].AsBool;
            m_characterArray[charNo].Price      = charNode["price"].AsFloat;
            m_characterArray[charNo].Type       = (CharacterType)charNo;
        }
        
        // Create list of characters obtainable via gacha
        for (int index = 0; index < characterCount; ++index)
        {
            if (m_characterArray[index].IsInGacha)
            {
                m_gachaCharacterIndices.Add(index);
            }
        }

        // Load characters from prefabs
        m_characterPrefabs = new GameObject[characterCount];
        for (int charNo = 0; charNo < characterCount; ++charNo)
        {
            string prefabPath = m_characterArray[charNo].PrefabPath;
            m_characterPrefabs[charNo] = Resources.Load<GameObject>(prefabPath);
            if (m_characterPrefabs[charNo] == null)
            {
                Debug.LogWarning("Invalid path: No prefab found at " + prefabPath);
            }
        }
    }

    /// <summary>
    /// Gets the data struct of the specified character.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <returns></returns>
    public CharacterStruct GetCharacterStruct(CharacterType character)
    {
        if (character == CharacterType.SIZE)
        {
            Debug.LogError("Invalid character type. Returning first character.");
            return m_characterArray[0];
        }
        return m_characterArray[(int)character];
    }

    /// <summary>
    /// Gets the prefab for the specified character.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <returns></returns>
    public GameObject GetCharacterPrefab(CharacterType character)
    {
        if (character == CharacterType.SIZE)
        {
            Debug.LogWarning("Invalid character type");
            return null;
        }
        return m_characterPrefabs[(int)character];
    }

    /// <summary>
    /// Gets the array of character prefabs.
    /// </summary>
    public GameObject[] GetCharacterPrefabs()
    {
        return m_characterPrefabs;
    }

    /// <summary>
    /// Creates a character.
    /// </summary>
    /// <param name="character">The character.</param>
    /// <returns></returns>
    public Character CreateCharacter(CharacterType character)
    {
        if (character == CharacterType.SIZE)
        {
            Debug.LogWarning("Invalid character type");
            return null;
        }
        GameObject charObj = GameObject.Instantiate<GameObject>(m_characterPrefabs[(int)character]);
        return charObj.AddComponentNoDupe<Character>();
    }

    /// <summary>
    /// Gets the list of indices of characters obtainable via gacha.
    /// </summary>
    public List<int> GachaCharacterIndices
    {
        get { return m_gachaCharacterIndices; }
    }

    /// <summary>
    /// Gets the number of characters.
    /// </summary>
    public int CharacterCount
    {
        get { return (int)CharacterType.SIZE; }
    }

    #endregion // Public Interface

    #region Gacha Characters

    private List<int> m_gachaCharacterIndices = new List<int>();

    #endregion // Gacha Characters

    #region Character Structs

    private CharacterStruct[] m_characterArray = null;

    private const string CHARACTER_FOLDER_PATH = "Prefabs/Characters/";

    public struct CharacterStruct
    {
        public string Name;         // Character name
        public string PrefabPath;   // Path to the character's prefab
        public string ItemID;       // Character ID used in Soomla Store and in the Google market
        public int MapType;         // Type of map to be loaded along with the character
        public bool IsInGacha;      // Can be acquired from gacha (random chance)
        public bool IsBuyable;      // Can be acquired through IAP
        public float Price;         // IAP price (if buyable)
        public CharacterType Type;  // This character's ID in the CharacterType enum
        //public bool IsUnlockable;   // Can be acquired by doing something in-game
        //public int UnlockType;      // Unlock method (if unlockable)
        //public int CollectCount;    // Number of items to collect in-game to unlock
    }

    /*public enum UnlockType
    {
        NONE        = 0,
        COLLECT     = 1,    // Collect a certain number of a certain item
        STEP_COUNT  = 2,    // Make a certain number of steps
        SCORE       = 4,    // Get a certain minimum score
        OTHER       // Hardcoded in-game
    }
    // Example: Can be unlocked by meeting COLLECT and/or SCORE requirements
    // private char m_unlockFlags = (int)(UnlockType.COLLECT) |
    //                              (int)(UnlockType.SCORE);
    */

    #endregion // Character Structs
        
    #region Resources

    private GameObject[] m_characterPrefabs = null;

    #endregion // Resources
}
