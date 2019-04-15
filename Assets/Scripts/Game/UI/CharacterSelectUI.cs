/******************************************************************************
*  @file       CharacterSelectUI.cs
*  @brief      Handles the character select UI
*  @author     Ron
*  @date       October 19, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System.Collections.Generic;

#endregion // Namespaces

public class CharacterSelectUI : MonoBehaviour
{
	#region Public Interface

    /// <summary>
    /// Initializes this instance. 
    /// </summary>
    public void Initialize(GameSceneMaster sceneMaster,
                           NewCharWinAnimator newCharWinAnim,
                           Vector3 initialCharacterRotation,
                           CharacterResource characterResource,
                           System.EventHandler<System.EventArgs> playCharacterDelegate,
                           System.EventHandler<System.EventArgs> buyCharacterDelegate,
                           System.EventHandler<System.EventArgs> charSelectShareDelegate,
                           System.Action<int> ownedCharCountChangedDelegate,
                           System.EventHandler<System.EventArgs> pressSoundDelegate,
                           System.EventHandler<System.EventArgs> releaseSoundDelegate)
    {
        m_newCharWinAnim = newCharWinAnim;

        // Set the initial rotation for characters in character select
        m_initialRotation = initialCharacterRotation;

        // Initialize buttons
        m_playCharacterBtn.Initialize(playCharacterDelegate, UIButton.TriggerType.ON_RELEASE);
        m_buyCharacterBtn.Initialize(buyCharacterDelegate, UIButton.TriggerType.ON_RELEASE);
        m_charSelectShareBtn.Initialize(charSelectShareDelegate, UIButton.TriggerType.ON_RELEASE);
        // Set button sounds
        m_playCharacterBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_buyCharacterBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);
        m_charSelectShareBtn.AddSoundDelegates(pressSoundDelegate, releaseSoundDelegate);

        // Store delegate for when number of owned characters changes
        m_ownedCharCountChangedDelegate = ownedCharCountChangedDelegate;

        // Initialize text
        m_characterNameText.Initialize();
        m_characterPriceText.Initialize();
        m_characterCountText.Initialize();

        // Initialize character list
        m_characterResource = characterResource;
        InitializeCharacterScrollPanel(sceneMaster);

        UpdateCharacterCountText();

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Scrolls the character select list to the character with the specified index.
    /// </summary>
    /// <param name="index">The index of the character to scroll to.</param>
    /// <param name="smoothScroll">if set to <c>true</c> scroll from the starting position
    /// to the specified item. Else, move scroll position directly to specified item.</param>
    /// <param name="smoothScrollSpeed">The scroll speed when smoothScroll is set to true.</param>
    public void ScrollToIndex(int index, bool smoothScroll, float smoothScrollSpeed = 0.0f)
    {
        m_characterSelectPanel.ScrollToIndex(index, smoothScroll, smoothScrollSpeed);
        UpdateFocusedCharacter();
    }

    /// <summary>
    /// Sets the character text.
    /// </summary>
    /// <param name="text">The text.</param>
    public void SetCharacterText(string text)
    {
        m_characterPriceText.SetText(text);
    }

    /// <summary>
    /// Called when player gets a new character from gacha.
    /// </summary>
    /// <param name="character">The new character.</param>
    public void OnGetNewGachaCharacter(CharacterType character)
    {
        SetOwned(character);
        // Increment owned character count
        m_ownedCharCount++;
        m_ownedCharCountChangedDelegate.Invoke(m_ownedCharCount);
    }

    /// <summary>
    /// Called when the focused character is bought.
    /// </summary>
    public void OnBuyFocusedCharacter()
    {
        if (m_focusedItem == null)
        {
            return;
        }
        m_focusedItem.SetOwned();
        m_focusedItem.PlayNewCharAnim();
        // Disable scrolling while new char anim is playing
        m_characterSelectPanel.DisableScrolling();

        UpdateFocusAreaFields();
        // Increment owned character count
        m_ownedCharCount++;
        m_ownedCharCountChangedDelegate.Invoke(m_ownedCharCount);
        // Update character count display
        UpdateCharacterCountText();
    }

    /// <summary>
    /// Sets the specified character in character select as "used" by the player.
    /// </summary>
    /// <param name="character">The character.</param>
    public void SetUsed(CharacterType character)
    {
        CharacterSelectItem item = m_charSelectItemList[(int)character];
        if (item != null && !item.IsUsed)
        {
            item.SetUsed();
            // Increment used character count
            m_usedCharCount++;
        }
    }

    /// <summary>
    /// Sets the specified character in character select as owned by the player.
    /// </summary>
    /// <param name="character">The newly-bought character.</param>
    public void SetOwned(CharacterType character)
    {
        m_charSelectItemList[(int)character].SetOwned();
    }

    /// <summary>
    /// Determines whether a new (unused) character is available.
    /// </summary>
    public bool IsNewCharacterAvailable()
    {
        // If the player has used less characters than s/he owns,
        //  then there is at least one that has not yet been used
        return m_usedCharCount < m_ownedCharCount;
    }

    /// <summary>
    /// Shows the character select UI.
    /// </summary>
    public void Show()
    {
        // Update positions
        m_playCharacterBtn.UpdateScreenPosition();
        m_buyCharacterBtn.UpdateScreenPosition();
        m_charSelectShareBtn.UpdateScreenPosition();

        // Enable scrolling
        m_characterSelectPanel.EnableScrolling();

        // Reset rotation and position of characters in character select
        foreach (CharacterSelectItem csi in m_charSelectItemList)
        {
            csi.Reset();
        }

        // Enable character select UI, then update focus
        m_characterSelectRoot.SetActive(true);
        UpdateFocusedCharacter();
    }

    /// <summary>
    /// Hides the character select UI.
    /// </summary>
    public void Hide()
    {
        m_newCharWinAnim.StopAnim();
        RemoveFocus(false);
        m_characterSelectRoot.SetActive(false);
    }

    /// <summary>
    /// Pauses this instance.
    /// </summary>
    public void Pause()
    {
        if (m_isPaused)
        {
            return;
        }

        m_isPaused = true;
    }

    /// <summary>
    /// Unpauses this instance.
    /// </summary>
    public void Unpause()
    {
        if (!m_isPaused)
        {
            return;
        }

        m_isPaused = false;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        // Hide buttons and locked sprites
        m_playCharacterBtn.gameObject.SetActive(false);
        m_buyCharacterBtn.gameObject.SetActive(false);
        m_charSelectShareBtn.gameObject.SetActive(false);
        m_lockedCharacterRoot.gameObject.SetActive(false);
        m_shareLockedSprite.gameObject.SetActive(false);
        // Clear text
        m_characterNameText.SetText("");
        m_characterPriceText.SetText("");
        // Remove focus on focused character (if any)
        RemoveFocus(false);
        // Reset new char win animation
        m_newCharWinAnim.Reset();
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        foreach (CharacterSelectItem charSelectItem in m_charSelectItemList)
        {
            charSelectItem.Delete();
        }
        m_charSelectItemList.Clear();
        m_characterSelectPanel.Delete();
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets the currently focused character in the character list.
    /// </summary>
    public CharacterSelectItem FocusedItem
    {
        get { return m_focusedItem; }
    }

    /// <summary>
    /// Gets whether this instance is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

    /// <summary>
    /// Gets whether this instance is paused.
    /// </summary>
    public bool IsPaused
    {
        get { return m_isPaused; }
    }

    #endregion // Public Interface

    #region Serialized Variables
    
    [SerializeField] private GameObject     m_characterSelectRoot   = null;

    [SerializeField] private UIButton       m_playCharacterBtn      = null;
    [SerializeField] private UIButton       m_buyCharacterBtn       = null;
    [SerializeField] private Transform      m_lockedCharacterRoot   = null;
    [SerializeField] private UIButton       m_charSelectShareBtn    = null;
    [SerializeField] private SpriteRenderer m_shareLockedSprite     = null;
    [SerializeField] private UIText         m_characterPriceText    = null;
    [SerializeField] private UIText         m_characterNameText     = null;
    [SerializeField] private UIText         m_characterCountText    = null;
    [SerializeField] private UIScrollPanel  m_characterSelectPanel  = null;
    
    [Tooltip("Rotation speed of characters in character select that are already owned by the player")]
    [SerializeField] private float          m_ownedCharRotSpeed     = 40.0f;
    [Tooltip("Time for focused characters in character select to go into focus (enlarge, center on screen)")]
    [SerializeField] private float          m_focusCharAnimTime     = 0.1f;
    [Tooltip("Normal scale of characters in character select")]
    [SerializeField] private float          m_normalCharScale       = 0.3f;
    [Tooltip("Scale of characters in character select scale when focused on")]
    [SerializeField] private float          m_focusCharScale        = 0.6f;

    [Tooltip("Position of focused characters")]
    [SerializeField] private Vector3        m_focusCharPosition     = new Vector3(0.0f, -3.7f, -1.0f);
    [Tooltip("Local z position of normal (unfocused) characters")]
    [SerializeField] private float          m_normalCharLocalPosZ   = 2.0f;

    [Tooltip("Color of character name text for owned characters")]
    [SerializeField] private Color          m_ownedCharNameColor    = Color.white;
    [Tooltip("Color of character name text for unowned characters")]
    [SerializeField] private Color          m_unownedCharNameColor  = Color.gray;

    [Tooltip("Invisible animating object whose \"hopping\" animation is copied by new/unused characters")]
    [SerializeField] private Animator       m_newCharAnimReference  = null;

    [SerializeField] private string         m_scrollItemLayer           = "UI";
    [SerializeField] private string         m_scrollItemSortingLayer    = "UI";
    [SerializeField] private int            m_scrollItemSortingOrder    = 1;

    #endregion // Serialized Variables

    #region Variables
    
    private bool    m_isInitialized     = false;
    private bool    m_isPaused          = false;

    private NewCharWinAnimator m_newCharWinAnim = null;

    private System.Action<int> m_ownedCharCountChangedDelegate = null;

    #endregion // Variables

    #region Character Scroll Panel

    private List<CharacterSelectItem>   m_charSelectItemList    = new List<CharacterSelectItem>();
    private CharacterSelectItem         m_focusedItem           = null;
    private CharacterResource           m_characterResource     = null;
    private int                         m_ownedCharCount        = 0;
    private int                         m_usedCharCount         = 0;

    // Initial rotation of characters in character select (in euler angles)
    private Vector3                     m_initialRotation       = Vector3.zero;

    /// <summary>
    /// Initializes the character scroll panel.
    /// </summary>
    private void InitializeCharacterScrollPanel(GameSceneMaster sceneMaster)
    {
        // Initialize scroll panel
        m_characterSelectPanel.Initialize(false, false);
        // Populate character list
        GameObject[] prefabs = m_characterResource.GetCharacterPrefabs();
        for (int index = 0; index < prefabs.Length; ++index)
        {
            // Instantiate character
            Character character = m_characterResource.CreateCharacter((CharacterType)index);
            // Create scroll item
            UIScrollItem newItem = m_characterSelectPanel.CreateScrollItem();
            // Child character under scroll item
            character.transform.parent = newItem.transform;
            // Position character "behind" the scroll item
            character.transform.localPosition = new Vector3(0.0f, 0.0f, m_normalCharLocalPosZ);
            // Set initial rotation
            character.transform.eulerAngles = m_initialRotation;
            // Set normal scale
            character.transform.localScale = Vector3.one * m_normalCharScale;
            // Add scroll item to scroll panel
            m_characterSelectPanel.AddScrollItem(newItem);

            // Set the character layer and sorting layer to UI
            character.ModelRenderer.gameObject.layer = LayerMask.NameToLayer(m_scrollItemLayer);
            character.ModelRenderer.sortingLayerName = m_scrollItemSortingLayer;
            character.ModelRenderer.sortingOrder = m_scrollItemSortingOrder;

            // Attach a CharacterSelectItem component to allow handling from CharacterSelectUI
            CharacterSelectItem charSelectItem = newItem.gameObject.AddComponentNoDupe<CharacterSelectItem>();
            bool isUsed = sceneMaster.IsUsed((CharacterType)index);
            bool isOwned = sceneMaster.IsOwned((CharacterType)index);
            charSelectItem.Initialize(this, newItem, character, isUsed, isOwned,
                                      m_ownedCharRotSpeed, m_normalCharScale, m_focusCharScale,
                                      m_focusCharAnimTime, m_focusCharPosition, m_newCharWinAnim,
                                      m_newCharAnimReference);
            if (!m_charSelectItemList.Contains(charSelectItem))
            {
                m_charSelectItemList.Add(charSelectItem);
            }
            // Update the number of used characters (those that have been equipped by the player at least once)
            if (isUsed)
            {
                m_usedCharCount++;
            }
            // Update the number of owned characters
            if (isOwned)
            {
                m_ownedCharCount++;
            }
        }
    }

    /// <summary>
    /// Updates the character count text.
    /// </summary>
    private void UpdateCharacterCountText()
    {
        // Update character count text
        int collectionCount = m_ownedCharCount;
        int totalCharacters = m_characterSelectPanel.ItemCount;
        m_characterCountText.SetText(collectionCount.ToString() + "/" + totalCharacters.ToString());
    }

    #endregion // Character Scroll Panel

    #region Focus Area

    /// <summary>
    /// Detects and sets focus on the character currently in the center of the screen.
    /// </summary>
    private void UpdateFocusedCharacter()
    {
        // If character select is hidden, do not do any focusing
        if (!m_characterSelectRoot.activeInHierarchy)
        {
            return;
        }
        Vector3 focusAreaCenter = m_characterSelectPanel.ScrollBounds.center;
        UIScrollItem nearestScrollItem = m_characterSelectPanel.GetItemNearestToPosition(focusAreaCenter);
        if (nearestScrollItem == null)
        {
            return;
        }

        // Scroll item should be within slot-size distance of the focus area
        // If character panel is scrolled out too far from the focus area,
        //  do not focus on any character
        if (Mathf.Abs(nearestScrollItem.transform.position.x - focusAreaCenter.x) > m_characterSelectPanel.SlotSize.x)
        {
            // If there was a focused item, remove focus
            if (m_focusedItem != null)
            {
                m_focusedItem.SetFocus(false);
                m_focusedItem = null;
                UpdateFocusAreaFields();
            }
        }
        else
        {
            // If new focused item, unfocus the old one and focus on the new one
            CharacterSelectItem nearestItem = nearestScrollItem.GetComponent<CharacterSelectItem>();
            if (m_focusedItem != nearestItem)
            {
                RemoveFocus(true);
                nearestItem.SetFocus(true);
                m_focusedItem = nearestItem;
                UpdateFocusAreaFields();

                // Play the character select sound
                Locator.GetSoundManager().PlayOneShot(SoundInfo.SFXID.CharacterSelect);
            }
        }
    }

    /// <summary>
    /// Updates the UI fields for data about the focused character (e.g. name, price).
    /// </summary>
    private void UpdateFocusAreaFields()
    {
        if (m_focusedItem == null)
        {
            return;
        }
        // Get character type of focused item
        int scrollItemIndex = m_focusedItem.ScrollItem.Index;
        // Get character struct
        CharacterResource.CharacterStruct charStruct =
            m_characterResource.GetCharacterStruct((CharacterType)(scrollItemIndex));
        m_characterNameText.SetText(charStruct.Name);
        // Show/hide buttons or text depending on whether character is owned and/or buyable
        if (m_focusedItem.IsOwned)
        {
            // Show play character and share buttons
            m_playCharacterBtn.gameObject.SetActive(true);
            m_charSelectShareBtn.gameObject.SetActive(true);
            // Hide the buy button and locked sprites
            m_buyCharacterBtn.gameObject.SetActive(false);
            m_characterPriceText.SetText("");
            m_lockedCharacterRoot.gameObject.SetActive(false);
            m_shareLockedSprite.gameObject.SetActive(false);
            // Set color of character name text
            m_characterNameText.SetColor(m_ownedCharNameColor);
        }
        else
        {
            // Hide play button
            m_playCharacterBtn.gameObject.SetActive(false);
            // Hide share button and show locked sprite
            m_charSelectShareBtn.gameObject.SetActive(false);
            m_shareLockedSprite.gameObject.SetActive(true);
            // Set color of character name text
            m_characterNameText.SetColor(m_unownedCharNameColor);

            // Show/hide buy button and locked sprite depending on whether character is buyable
            m_buyCharacterBtn.gameObject.SetActive(charStruct.IsBuyable);
            m_lockedCharacterRoot.gameObject.SetActive(!charStruct.IsBuyable);

            // If buyable, show character price. Else, leave price text empty.
            // TODO: Format price with the proper currency
            m_characterPriceText.SetText(charStruct.IsBuyable ? 
                                         "$" + charStruct.Price.ToString("0.00") :
                                         "");
            
        }
    }

    /// <summary>
    /// Removes focus on the focused item, if there is one.
    /// </summary>
    private void RemoveFocus(bool lerpToNewState)
    {
        if (m_focusedItem != null)
        {
            m_focusedItem.SetFocus(false, lerpToNewState);
            m_focusedItem = null;
        }
    }

    #endregion // Focus Area

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    private void Awake()
	{

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
		if (!m_isInitialized)
        {
            return;
        }
        
        // Update the focused character only when the panel is scrolling
        if (m_characterSelectPanel.IsScrolling)
        {
            UpdateFocusedCharacter();
        }

        // If new char anim is finished, re-enable scrolling
        if (m_characterSelectRoot.activeInHierarchy &&
            m_newCharWinAnim.IsCharAnimFinished)
        {
            m_newCharWinAnim.StopCharAnim();
            m_characterSelectPanel.EnableScrolling();
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
