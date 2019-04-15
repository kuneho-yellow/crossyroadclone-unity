/******************************************************************************
*  @file       UIScrollPanel.cs
*  @brief      UI class for a scroll panel element
*  @author     Ron
*  @date       October 4, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;

using TouchScript;

#endregion // Namespaces

public class UIScrollPanel : UIElement
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="includeInactiveScrollItems">if set to <c>true</c>
    ///     inactive scroll items will be added to the scroll item list.</param>
    /// <param name="includeInactiveScrollItemElements">if set to <c>true</c>
    ///     inactive elements in scroll items will be considered in bounds calculations.</param>
    public void Initialize(bool includeInactiveScrollItems, bool includeInactiveScrollItemElements)
    {
        if (m_isInitialized)
        {
            return;
        }

        m_includeInactiveScrollItemElements = includeInactiveScrollItemElements;

        // Make sure scroll panel has gesture components
        m_pressGesture = this.gameObject.AddComponentNoDupe<PressGesture>();
        m_releaseGesture = this.gameObject.AddComponentNoDupe<ReleaseGesture>();

        // Subscribe to input events
        m_pressGesture.Pressed += OnScrollPanelPress;
        m_releaseGesture.Released += OnScrollPanelRelease;

        // Load the scroll item resource
        m_scrollItemResource = Resources.Load<GameObject>(SCROLL_ITEM_PREFAB_PATH);

        // Initialize the list of scroll items
        UIScrollItem[] scrollItems = m_scrollRoot.GetComponentsInChildren<UIScrollItem>(includeInactiveScrollItems);
        if (scrollItems.Length > 0)
        {
            // Add items to the scroll panel
            AddScrollItems(scrollItems);
            // Auto-arrange scroll panel elements according to their position (if enabled)
            if (m_enableAutoArrange)
            {
                ArrangeScrollItems(ArrangeRule.BY_POSITION);
            }
            // Update scroll limits given the newly-added items
            UpdateScrollLimits();
        }
        else
        {
            // Check if user specified a slot size, and store it if so
            CalculateSlotSize();
        }

        // Set the initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Arranges items in the scroll panel.
    /// </summary>
    public void ArrangeScrollItems(ArrangeRule arrangeRule = ArrangeRule.BY_LIST_INDEX)
    {
        switch (arrangeRule)
        {
            case ArrangeRule.BY_LIST_INDEX: ArrangeByListIndex();   break;
            case ArrangeRule.BY_POSITION:   ArrangeByPosition();    break;
        }
    }

    /// <summary>
    /// Adds a scroll item to the scroll panel.
    /// </summary>
    /// <param name="item">Scroll item to add</param>
    /// <param name="insertIndex">Inserts new item at the specified index</param>
    /// <returns></returns>
    public bool AddScrollItem(UIScrollItem item, int insertIndex = -1)
    {
        if (m_scrollItemList.Contains(item))
        {
            return false;
        }
        // Initialize new item
        item.Initialize(this, m_includeInactiveScrollItemElements);
        // Place new item under scroll root in the hierarchy
        item.transform.parent = m_scrollRoot;
        // If scroll item list is empty
        if (m_scrollItemList.Count == 0)
        {
            // Add new item to the list
            m_scrollItemList.Add(item);
            // Calculate the size of scroll item slots (if not specified by user)
            // Note: This should be done before UpdateScrollLimits and ArrangeByListIndex
            CalculateSlotSize(true);
            // Update scroll limits
            UpdateScrollLimits();
            // Arrange scroll panel
            ArrangeByListIndex();
        }
        // If scroll item list is not empty
        else
        {
            // Get offset between items
            float itemOffset = GetSignedSlotSize();
            // If insert index is unspecified (or out of bounds), add new item to the end of the list
            if (insertIndex < 0 || insertIndex >= m_scrollItemList.Count)
            {
                // Place new item at the end of the scroll panel
                UIScrollItem lastItem = m_scrollItemList[m_scrollItemList.Count - 1];
                Vector3 pos = lastItem.transform.position;
                if (IsHorizontalScrolling) pos.x += itemOffset;
                else pos.y += itemOffset;
                item.transform.position = pos;

                // Add item to the list
                m_scrollItemList.Add(item);
            }
            // If insert index is specified, insert new item at the specified position
            else
            {
                // Place new item in the scroll panel
                item.transform.position = m_scrollItemList[insertIndex].transform.position;
                // Move scroll items back
                MoveItemsInScrollPanel(insertIndex, true, 1);

                // Insert item into the specified position in the list
                m_scrollItemList.Insert(insertIndex, item);
            }
        }
        // Update UIScrollItem collider size
        UpdateScrollItemBounds(item);
        return true;
    }

    /// <summary>
    /// Adds a list of scroll items to the scroll panel.
    /// </summary>
    /// <param name="items">List of scroll items to add</param>
    /// <param name="insertIndex">Inserts new items at the specified index</param>
    /// <returns>Returns true if all items were successfully added, else false</returns>
    public bool AddScrollItems(List<UIScrollItem> items, int insertIndex = -1)
    {
        bool allItemsAdded = true;
        for (int itemNo = 0; itemNo < items.Count; ++itemNo)
        {
            if (AddScrollItem(items[itemNo], insertIndex))
            {
                // Increment position to insert next item into
                if (insertIndex >= 0)
                {
                    insertIndex++;
                }
            }
            else
            {
                allItemsAdded = false;
            }
        }
        return allItemsAdded;
    }

    /// <summary>
    /// Adds an array of scroll items to the scroll panel.
    /// </summary>
    /// <param name="itemArray">Array of scroll items to add</param>
    /// <param name="insertIndex">Inserts new items at the specified index</param>
    /// <returns>Returns true if all items were successfully added, else false</returns>
    public bool AddScrollItems(UIScrollItem[] itemArray, int insertIndex = -1)
    {
        // Convert the array to a list
        List<UIScrollItem> scrollItemList = new List<UIScrollItem>(itemArray.Length);
        foreach (UIScrollItem item in itemArray)
        {
            scrollItemList.Add(item);
        }
        // Pass on to the method overload that takes a list
        return AddScrollItems(scrollItemList, insertIndex);
    }

    /// <summary>
    /// Removes the specified scroll item from the scroll panel.
    /// </summary>
    /// <param name="removeIndex">Item to remove</param>
    /// <param name="deleteOnRemove">Whether to delete the item as well</param>
    /// <returns>True if item was removed, else false</returns>
    public bool RemoveScrollItem(UIScrollItem item, bool deleteOnRemove = true)
    {
        if (!m_scrollItemList.Contains(item))
        {
            return false;
        }
        int removeIndex = m_scrollItemList.IndexOf(item);
        if (m_scrollItemList.Remove(item))
        {
            // If item was in the middle or at the start of the list, move the other items back
            if (removeIndex < m_scrollItemList.Count)
            {
                MoveItemsInScrollPanel(removeIndex, false, 1);
            }
            // If item was at the end of the list, no need to do anything

            // Delete item
            if (deleteOnRemove)
            {
                item.Delete(true);
            }

            UpdateClampToScrollArea();
            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the scroll item with the specified index from the scroll panel.
    /// </summary>
    /// <param name="itemIndex">Index of item to remove</param>
    /// <param name="deleteOnRemove">Whether to delete the item as well</param>
    /// <returns>True if item was removed, else false</returns>
    public bool RemoveScrollItem(int itemIndex, bool deleteOnRemove = true)
    {
        if (itemIndex < 0 || itemIndex >= m_scrollItemList.Count)
        {
            return false;
        }
        return RemoveScrollItem(m_scrollItemList[itemIndex], deleteOnRemove);
    }

    /// <summary>
    /// Removes a list of scroll items from the scroll panel.
    /// </summary>
    /// <param name="items">List of items to remove</param>
    /// <param name="deleteOnRemove">Whether to delete the items as well</param>
    /// <returns>True if all items were removed, else false</returns>
    public bool RemoveScrollItems(List<UIScrollItem> items, bool deleteOnRemove = true)
    {
        bool allItemsRemoved = true;
        for (int index = items.Count - 1; index >= 0; --index)
        {
            if (!RemoveScrollItem(items[index], deleteOnRemove))
            {
                allItemsRemoved = false;
            }
        }
        return allItemsRemoved;
    }

    /// <summary>
    /// Removes scroll items with the specified indices from the scroll panel.
    /// </summary>
    /// <param name="itemIndices">Indices of items to remove</param>
    /// <param name="deleteOnRemove">Whether to delete the items as well</param>
    /// <returns>True if all items were removed, else false</returns>
    public bool RemoveScrollItems(List<int> itemIndices, bool deleteOnRemove = true)
    {
        bool allItemsRemoved = true;
        for (int index = itemIndices.Count - 1; index >= 0; --index)
        {
            if (!RemoveScrollItem(itemIndices[index], deleteOnRemove))
            {
                allItemsRemoved = false;
            }
        }
        return allItemsRemoved;
    }

    /// <summary>
    /// Removes all scroll items from the scroll panel.
    /// </summary>
    /// <param name="deleteOnRemove">Whether to delete the items as well</param>
    /// <returns>True if all items were removed, else false</returns>
    public bool RemoveAllScrollItems(bool deleteOnRemove = true)
    {
        return RemoveScrollItems(m_scrollItemList, deleteOnRemove);
    }

    /// <summary>
    /// Creates an empty floating scroll item.
    /// After adding content to the item, it should be added to the scroll panel via AddScrollItem.
    /// </summary>
    /// <returns>The created scroll item.</returns>
    public UIScrollItem CreateScrollItem()
    {
        GameObject newItemObj = GameObject.Instantiate<GameObject>(m_scrollItemResource);
        UIScrollItem newItem = newItemObj.GetComponent<UIScrollItem>();
        UpdateScrollItemBounds(newItem);
        return newItem;
    }

    /// <summary>
    /// Notifies of a scroll item deletion.
    /// </summary>
    /// <param name="deleteItem">Item that is about to be deleted</param>
    public void NotifyDeleteScrollItem(UIScrollItem deleteItem)
    {
        RemoveScrollItem(deleteItem, false);
    }

    /// <summary>
    /// Scrolls to the scroll item with the specified index.
    /// </summary>
    /// <param name="index">The index of the item to scroll to.</param>
    /// <param name="smoothScroll">if set to <c>true</c> scroll from the starting position
    /// to the specified item. Else, move scroll position directly to specified item.</param>
    /// <param name="smoothScrollSpeed">The scroll speed when smoothScroll is set to true.</param>
    public void ScrollToIndex(int index, bool smoothScroll, float smoothScrollSpeed = DEFAULT_SMOOTH_SCROLL_SPEED)
    {
        if (index < 0 || index >= m_scrollItemList.Count)
        {
            return;
        }
        // If smoothScroll is true but speed is 0, no scrolling happens
        if (smoothScroll && smoothScrollSpeed == 0.0f)
        {
            return;
        }
        
        // Get the vector from the first scroll item to the first grid slot
        float firstItemPos = IsHorizontalScrolling ?
                                m_scrollItemList[0].transform.position.x :
                                m_scrollItemList[0].transform.position.y;
        float firstItemToFirstSlotVec = GetFirstGridSlotPos() - firstItemPos;
        // The scroll distance is equal to the slot size times the number of items to scroll through,
        //  plus the distance between the first item and the first grid slot.
        // The sign accounts for the variations in item offsets due to scroll orientation and order.
        float signedScrollDist = -1.0f * GetSignedSlotSize() * index + firstItemToFirstSlotVec;
        // Get the position to scroll to
        float scrollTargetPos = GetScrollRootPosInScrollDir() + signedScrollDist;
        if (!smoothScroll)
        {
            // Scroll to position immediately
            if (IsHorizontalScrolling)  m_scrollRoot.SetPosX(scrollTargetPos);
            else                        m_scrollRoot.SetPosY(scrollTargetPos);
        }
        else /* if (smoothScroll) */
        {
            // Set target scroll position and scroll speed, then switch to AUTO_SCROLL mode
            float scrollRootPos = GetScrollRootPosInScrollDir();
            m_autoScrollTargetPos = scrollTargetPos;
            m_scrollSpeed = smoothScrollSpeed * Mathf.Sign(m_autoScrollTargetPos - scrollRootPos);
            m_scrollingState = ScrollingState.AUTO_SCROLL;
        }
    }

    /// <summary>
    /// Gets the index of the specified item in the list of scroll items.
    /// </summary>
    /// <param name="scrollItem">Item to look for index of</param>
    /// <returns>Index of scroll item in list, or -1 if not in list</returns>
    public int GetItemIndex(UIScrollItem scrollItem)
    {
        if (m_scrollItemList.Contains(scrollItem))
        {
            return m_scrollItemList.IndexOf(scrollItem);
        }
        return -1;
    }

    /// <summary>
    /// Resets the scroll panel to its initial state.
    /// </summary>
    public override void Reset()
    {
        // TODO: What is the "initial state" for a scroll panel? Empty? Or only the starting scroll items?

        // The default starting position for scroll root is the origin, regardless of scroll type
        m_scrollRoot.transform.position = Vector3.zero;

        m_firstTouchPos = Vector2.zero;
        m_prevTouchPos = Vector2.zero;
        //m_prevWorldTouchDelta = Vector2.zero;
        m_curWorldTouchDelta = Vector2.zero;
        m_isTouchForScrolling = false;
    }

    /// <summary>
    /// Enables scrolling.
    /// </summary>
    public void EnableScrolling()
    {
        m_isScrollingEnabled = true;
    }

    /// <summary>
    /// Disables scrolling.
    /// </summary>
    public void DisableScrolling()
    {
        m_isScrollingEnabled = false;
    }

    /// <summary>
    /// Hides the scroll panel.
    /// </summary>
    public void Hide()
    {
        // TODO
    }

    /// <summary>
    /// Shows the scroll panel.
    /// </summary>
    public void Show()
    {
        // TODO
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        // Delete all scroll items
        for (int index = m_scrollItemList.Count - 1; index >= 0; --index)
        {
            m_scrollItemList[index].Delete(true);
        }
        m_scrollItemList.Clear();

        // Delete scroll panel
        GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets the number of scroll items in the panel.
    /// </summary>
    public int ItemCount
    {
        get { return m_scrollItemList.Count; }
    }

    /// <summary>
    /// Gets the bounds of the scrolling area.
    /// </summary>
    public Bounds ScrollBounds
    {
        get { return m_scrollBounds.bounds; }
    }

    /// <summary>
    /// Gets the scroll item at the extreme left of the scroll panel (if in horizontal scrolling).
    /// </summary>
    /// <returns>Leftmost scroll item</returns>
    public UIScrollItem GetLeftmostItem()
    {
        if (m_scrollItemList.Count == 0 || IsVerticalScrolling)
        {
            return null;
        }
        return !m_isReverseOrder ? m_scrollItemList[0] : m_scrollItemList[m_scrollItemList.Count - 1];
    }

    /// <summary>
    /// Gets the scroll item at the extreme right of the scroll panel (if in horizontal scrolling).
    /// </summary>
    /// <returns>Rightmost scroll item</returns>
    public UIScrollItem GetRightmostItem()
    {
        if (m_scrollItemList.Count == 0 || IsVerticalScrolling)
        {
            return null;
        }
        return !m_isReverseOrder ? m_scrollItemList[m_scrollItemList.Count - 1] : m_scrollItemList[0];
    }

    /// <summary>
    /// Gets the scroll item at the top of the scroll panel (if in vertical scrolling).
    /// </summary>
    /// <returns>Top scroll item</returns>
    public UIScrollItem GetTopItem()
    {
        if (m_scrollItemList.Count == 0 || IsHorizontalScrolling)
        {
            return null;
        }
        return !m_isReverseOrder ? m_scrollItemList[0] : m_scrollItemList[m_scrollItemList.Count - 1];
    }

    /// <summary>
    /// Gets the scroll item at the bottom of the scroll panel (if in vertical scrolling).
    /// </summary>
    /// <returns>Bottom scroll item</returns>
    public UIScrollItem GetBottomItem()
    {
        if (m_scrollItemList.Count == 0 || IsHorizontalScrolling)
        {
            return null;
        }
        return !m_isReverseOrder ? m_scrollItemList[m_scrollItemList.Count - 1] : m_scrollItemList[0];
    }

    /// <summary>
    /// Gets the scroll item with the specified scroll panel index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The scroll item.</returns>
    public UIScrollItem GetScrollItemWithIndex(int index)
    {
        if (index < 0 || index >= m_scrollItemList.Count)
        {
            return null;
        }
        return m_scrollItemList[index];
    }

    /// <summary>
    /// Gets the list of scroll items.
    /// </summary>
    public List<UIScrollItem> GetScrollItems()
    {
        return m_scrollItemList;
    }

    /// <summary>
    /// Gets the position of the first grid slot in the scroll panel.
    /// Returns the grid x position if horizontal scrolling, y position if vertical scrolling.
    /// </summary>
    public float GetFirstGridSlotPos()
    {
        float scrollAreaBound = 0.0f;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                scrollAreaBound = !m_isReverseOrder ? ScrollBounds.min.x : ScrollBounds.max.x;
                break;
            case ScrollOrientation.VERTICAL:
                scrollAreaBound = !m_isReverseOrder ? ScrollBounds.max.y : ScrollBounds.min.y;
                break;
        }
        return scrollAreaBound + 0.5f * GetSignedSlotSize(); ;
    }

    /// <summary>
    /// Gets the position of the last grid slot in the scroll panel.
    /// Returns the grid x position if horizontal scrolling, y position if vertical scrolling.
    /// </summary>
    public float GetLastGridSlotPos()
    {
        float firstGridSlotPos = GetFirstGridSlotPos();
        // Get the edge of the scroll area opposite to the first grid slot pos
        float oppositeEdge = 0.0f;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                oppositeEdge = !m_isReverseOrder ? ScrollBounds.max.x : ScrollBounds.min.x;
                break;
            case ScrollOrientation.VERTICAL:
                oppositeEdge = !m_isReverseOrder ? ScrollBounds.min.y : ScrollBounds.max.y;
                break;
        }
        // Get distance from first slot to opposite edge
        float firstSlotToEdgeDist = Mathf.Abs(firstGridSlotPos - oppositeEdge);
        // Get number of slots in between
        float slotCount = Mathf.Floor(firstSlotToEdgeDist / SlotSizeInScrollDir);
        // Return the position of the last grid slot
        return firstGridSlotPos + slotCount * GetSignedSlotSize();
    }

    /// <summary>
    /// Gets whether all scroll items in the scroll panel are within the scroll area.
    /// </summary>
    public bool AreAllInScrollArea()
    {
        if (m_scrollItemList.Count == 0)
        {
            return true;
        }
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                return GetLeftmostItem().transform.position.x >= m_leftScrollLimits &&
                       GetRightmostItem().transform.position.x <= m_rightScrollLimits;
            case ScrollOrientation.VERTICAL:
                return GetTopItem().transform.position.x <= m_upperScrollLimits &&
                       GetBottomItem().transform.position.x >= m_lowerScrollLimits;
        }
        return false;
    }

    /// <summary>
    /// Gets whether all scroll items can fit within the scroll area.
    /// </summary>
    public bool CanFitInScrollArea()
    {
        if (m_scrollItemList.Count == 0)
        {
            return true;
        }
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                return GetDistFromFirstToLastItem() <= m_rightScrollLimits - m_leftScrollLimits;
            case ScrollOrientation.VERTICAL:
                return GetDistFromFirstToLastItem() <= m_upperScrollLimits - m_lowerScrollLimits;
        }
        return false;
    }

    /// <summary>
    /// Gets the distance from the first to the last item in the scroll panel.
    /// </summary>
    public float GetDistFromFirstToLastItem()
    {
        if (m_scrollItemList.Count == 0)
        {
            return 0.0f;
        }
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                return GetRightmostItem().transform.position.x - GetLeftmostItem().transform.position.x;
            case ScrollOrientation.VERTICAL:
                return GetTopItem().transform.position.y - GetBottomItem().transform.position.y;
        }
        return 0.0f;
    }

    /// <summary>
    /// Gets whether scroll orientation is horizontal.
    /// </summary>
    public bool IsHorizontalScrolling
    {
        get { return m_scrollOrientation == ScrollOrientation.HORIZONTAL; }
    }

    /// <summary>
    /// Gets whether scroll orientation is vertical.
    /// </summary>
    public bool IsVerticalScrolling
    {
        get { return m_scrollOrientation == ScrollOrientation.VERTICAL; }
    }

    /// <summary>
    /// Gets the size of scroll item slots.
    /// </summary>
    public Vector2 SlotSize
    {
        get { return m_slotSize; }
    }

    /// <summary>
    /// Gets the size of scroll item slots along the scroll direction.
    /// </summary>
    public float SlotSizeInScrollDir
    {
        get { return IsHorizontalScrolling ? m_slotSize.x : m_slotSize.y; }
    }

    /// <summary>
    /// Gets whether the panel is currently scrolling.
    /// </summary>
    public bool IsScrolling
    {
        get { return m_scrollingState != ScrollingState.IDLE; }
    }

    #endregion // Public Interface

    #region Serialized Variables

    [Tooltip("Direction in which the scroll panel can be scrolled")]
    [SerializeField] private ScrollOrientation m_scrollOrientation = ScrollOrientation.HORIZONTAL;
    [Tooltip("Default ordering is left to right for horizontal scrolling and top to bottom for vertical scrolling\n" +
             "Set this to true to reverse the ordering (e.g. right to left, bottom to top)")]
    [SerializeField] private bool       m_isReverseOrder    = false;

    // TODO: Multi-row/multi-column panels
    //[SerializeField] private int        m_rowCount        = -1;
    //[SerializeField] private int        m_columnCount     = -1;

    [Tooltip("The root object that contains the scroll item elements")]
    [SerializeField] private Transform  m_scrollRoot        = null;
    [Tooltip("The collider used to represent the scrolling limits")]
    [SerializeField] private Collider2D m_scrollBounds      = null;

    [Header("Initialization")]
    [Tooltip("Automatically arrange all UIScrollItems found under the scroll root")]
    [SerializeField] private bool       m_enableAutoArrange         = true;
    [Tooltip("Manually set the distance between (centers of) items in the scroll panel\n" +
             "If left at zero, the scroll panel will use the size of the first element to determine spacing\n" +
             "Note: When getting distance between scroll items, use GetDistBetweenCenters() instead")]
    [SerializeField] private float      m_spaceBetweenItemCenters   = 0.0f;
    [Tooltip("Account for inactive elements in scroll items in bounds calculations")]
    [SerializeField] private bool       m_includeInactiveScrollItemElements = true;

    [Header("Post-Touch Scrolling Options")]
    [Tooltip("Enable scrolling past the scroll bounds")]
    [SerializeField] private bool       m_enableScrollingPastLimits     = true;
    [Tooltip("Factor by which the amount of scrolling is modified when scrolling past the scroll bounds.\n" +
             "(0 means no scrolling past limits, as if scrolling past limits is disabled\n" +
             "1 means normal scrolling, as if scrolling within scroll bounds)")]
    [SerializeField] private float      m_overLimitScrollingFactor      = 0.3f;
    [Tooltip("Speed at which the scroll panel scrolls back within the scroll bounds " +
             "when there is no active touch on the scroll panel (0 means no rebound)")]
    [SerializeField] private float      m_scrollReboundSpeed            = 2.0f;

    [Tooltip("Enable residual scrolling, i.e. scrolling inertia after touch is released")]
    [SerializeField] private bool       m_enableResidualScroll          = true;
    [Tooltip("Rate at which residual scrolling slows down")]
    [SerializeField] private float      m_residualScrollDeceleration    = 1.0f;
    [Tooltip("Maximum speed residual scrolling can reach (0 means no limit)")]
    [SerializeField] private float      m_residualScrollMaxSpeed        = 1.0f;

    [Tooltip("Enable grid-locking, i.e. moving scroll items to the nearest grid position\n" +
             "Assumes the initial positions (or positions after auto-arrange) to be the grid positions")]
    [SerializeField] private bool       m_enableGridLocking             = true;
    [Tooltip("Speed at which the panel scrolls to the grid positions\n" +
             "When residual scrolling is enabled, this is the speed at which " +
             "scrolling state goes from RESIDUAL_SCROLL to GRID_LOCK")]
    [SerializeField] private float      m_lockToGridScrollSpeed         = 0.0f;

    #endregion // Serialized Variables

    #region Constants

    private const float DEFAULT_SMOOTH_SCROLL_SPEED = 5.0f;

    #endregion // Constants

    #region Scroll Orientation

    public enum ScrollOrientation
    {
        HORIZONTAL,
        VERTICAL
    }

    #endregion // Scroll Orientation

    #region Delegates

    /// <summary>
    /// Raises the scroll panel press event.
    /// </summary>
    private void OnScrollPanelPress(object sender, System.EventArgs e)
    {
        // Process only one touch per scroll panel
        if (m_activeTouch != null)
        {
            return;
        }

        // Find and store a reference to the touch on this scroll panel
        m_activeTouch = m_pressGesture.ActiveTouches[0];
        // Store first touch position
        m_firstTouchPos = m_activeTouch.Position;
        // Initialize previous touch position to the current position
        m_prevTouchPos = m_activeTouch.Position;

        // Switch to TOUCH_SCROLL state
        m_scrollingState = ScrollingState.TOUCH_SCROLL;
    }

    /// <summary>
    /// Raises the scroll panel release event.
    /// </summary>
    private void OnScrollPanelRelease(object sender, System.EventArgs e)
    {
        // Clear active touch
        m_activeTouch = null;
        
        // Reset flag for determining touches for scrolling
        m_isTouchForScrolling = false;

        // Determine what scrolling state to enter
        // If scrolling past limits is enabled and scroll panel is outside scroll bounds,
        //  go to INIT_REBOUND_SCROLL state
        if (m_enableScrollingPastLimits && !IsWithinScrollBounds())
        {
            m_scrollingState = ScrollingState.INIT_REBOUND_SCROLL;
        }
        // If scroll speed is greater than the lock-to-grid speed,
        //  prioritize going to RESIDUAL_SCROLL state (if enabled)
        else if (Mathf.Abs(m_scrollSpeed) > m_lockToGridScrollSpeed)
        {
            // If residual scrolling is enabled, go to RESIDUAL_SCROLL state
            if (m_enableResidualScroll)
            {
                m_scrollingState = ScrollingState.RESIDUAL_SCROLL;
            }
            // Else, if grid-locking is enabled, go to INIT_GRID_LOCK state
            else if (m_enableGridLocking)
            {
                m_scrollingState = ScrollingState.INIT_GRID_LOCK;
            }
            // Else, stop scrolling (go to IDLE state)
            else
            {
                m_scrollingState = ScrollingState.IDLE;
            }
        }
        // If scroll speed is less than the lock-to-grid speed,
        //  prioritize going to GRID_LOCK state (if enabled)
        else /* if (Mathf.Abs(m_scrollSpeed) < m_lockToGridScrollSpeed) */
        {
            // If grid-locking is enabled, go to INIT_GRID_LOCK state
            if (m_enableGridLocking)
            {
                m_scrollingState = ScrollingState.INIT_GRID_LOCK;
            }
            // Else, if residual scrolling is enabled and scroll speed is non-zero,
            //  go to RESIDUAL_SCROLL state
            else if (m_enableResidualScroll && m_scrollSpeed != 0.0f)
            {
                m_scrollingState = ScrollingState.RESIDUAL_SCROLL;
            }
            // Else, stop scrolling (go to IDLE state)
            else
            {
                m_scrollingState = ScrollingState.IDLE;
            }
        }
    }

    #endregion // Delegates

    #region Scroll Panel Arrangement

    private Vector2 m_slotSize = Vector2.zero;

    private const float DEFAULT_SPACE_BETWEEN_ITEMS = 0.5f;

    public enum ArrangeRule
    {
        BY_LIST_INDEX,
        BY_POSITION
    }

    /// <summary>
    /// Arranges scroll list items according to their index in the list.
    /// </summary>
    private void ArrangeByListIndex()
    {
        if (m_scrollItemList.Count == 0)
        {
            return;
        }
        // Get offset between scroll items
        float itemOffset = GetSignedSlotSize();
        // Position items according to index in the scroll item list
        float posZ = this.transform.position.z - 1.0f;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                {
                    // Place first item near leftmost/rightmost edge of scroll area
                    float posX = itemOffset * 0.5f +
                            (!m_isReverseOrder ? ScrollBounds.min.x : ScrollBounds.max.x);
                    float posY = ScrollBounds.center.y;
                    foreach (UIScrollItem scrollItem in m_scrollItemList)
                    {
                        scrollItem.transform.position = new Vector3(posX, posY, posZ);
                        posX += itemOffset;
                    }
                    break;
                }
            case ScrollOrientation.VERTICAL:
                {
                    // Place first item near leftmost/rightmost edge of scroll area
                    float posX = ScrollBounds.center.x;
                    float posY = itemOffset * 0.5f +
                            (!m_isReverseOrder ? ScrollBounds.max.y : ScrollBounds.min.y);
                    foreach (UIScrollItem scrollItem in m_scrollItemList)
                    {
                        scrollItem.transform.position = new Vector3(posX, posY, posZ);
                        posY += itemOffset;
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// Calculates the size of item "slots" in the scroll panel.
    /// </summary>
    private void CalculateSlotSize(bool recalculate = false)
    {
        // Slot size is calculated only on initialization (unless recalculate is set to true)
        if (!recalculate && m_slotSize != Vector2.zero)
        {
            return;
        }
        // Check if user specified the space between items
        if (m_spaceBetweenItemCenters != 0.0f)
        {
            // Start with a square slot with side equal to the specified space
            m_slotSize = Vector2.one * m_spaceBetweenItemCenters;
            // Check if there is already an item in the scroll panel
            if (m_scrollItemList.Count > 0)
            {
                // Get the size of the first scroll item in the direction perpendicular
                //  to the scroll direction. Assume this is the size for all items.
                if (IsHorizontalScrolling) m_slotSize.y = m_scrollItemList[0].Height;
                else m_slotSize.x = m_scrollItemList[0].Width;
            }
        }
        // If user did not specify the space between items...
        else
        {
            // If there is an item in the scroll panel...
            if (m_scrollItemList.Count > 0)
            {
                // Get the size of the first scroll item, and add a default space.
                // Assume this is the size for all items.
                m_slotSize.x = m_scrollItemList[0].Width + DEFAULT_SPACE_BETWEEN_ITEMS;
                m_slotSize.y = m_scrollItemList[0].Height + DEFAULT_SPACE_BETWEEN_ITEMS;
            }
            // If scroll item list is empty, just return for now,
            //  and update again later when a scroll item is added
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// Gets the signed scroll panel slot size, taking into account scroll orientation and order.
    /// This is used for calculating offsets needed for positioning scroll items.
    /// </summary>
    private float GetSignedSlotSize()
    {
        float signedSlotSize = SlotSizeInScrollDir;
        if ((m_isReverseOrder && IsHorizontalScrolling) ||
            (!m_isReverseOrder && IsVerticalScrolling))
        {
            signedSlotSize *= -1.0f;
        }
        return signedSlotSize;
    }

    /// <summary>
    /// Arranges scroll list items according to their initial position in the scroll panel.
    /// </summary>
    private void ArrangeByPosition()
    {
        if (m_scrollItemList.Count == 0)
        {
            return;
        }
        // Reorder scroll item list according to item position
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                m_scrollItemList.Sort((item1, item2) =>
                    item1.transform.position.x.CompareTo(item2.transform.position.x));
                break;
            case ScrollOrientation.VERTICAL:
                m_scrollItemList.Sort((item1, item2) =>
                    item1.transform.position.y.CompareTo(item2.transform.position.y));
                break;
        }
        // Make sure items are evenly spaced
        ArrangeByListIndex();
    }

    #endregion // Scroll Panel Arrangement

    #region Scroll Items

    private GameObject          m_scrollItemResource    = null;
    private const string        SCROLL_ITEM_PREFAB_PATH = "Prefabs/UI/UIScrollItem";

    private List<UIScrollItem>  m_scrollItemList        = new List<UIScrollItem>();

    /// <summary>
    /// Moves items in the scroll panel.
    /// This moves items starting from itemIndex to the end of the item list
    ///     either forward or backward by the specified number of panel "slots".
    /// </summary>
    /// <param name="itemIndex">Move scroll items starting from this index until the last item in the list.</param>
    /// <param name="moveForward">if set to <c>true</c> move items forward in the scroll panel,
    ///     where "forward" means in the direction from item at index 0 to the last item.</param>
    /// <param name="moveSlotCount">The move amount in terms of number of item slots.</param>
    private void MoveItemsInScrollPanel(int itemIndex, bool moveForward, int moveSlotCount)
    {
        float itemOffset = GetSignedSlotSize() * moveSlotCount;
        if (!moveForward)
        {
            itemOffset *= -1.0f;
        }
        for (int index = itemIndex; index < m_scrollItemList.Count; ++index)
        {
            if (IsHorizontalScrolling)
            {
                float posX = m_scrollItemList[index].transform.position.x;
                m_scrollItemList[index].transform.SetPosX(posX + itemOffset);
            }
            else
            {
                float posY = m_scrollItemList[index].transform.position.y;
                m_scrollItemList[index].transform.SetPosY(posY + itemOffset);
            }
        }
    }

    /// <summary>
    /// Updates the scroll item's bounds.
    /// </summary>
    /// <param name="item">The scroll item.</param>
    private void UpdateScrollItemBounds(UIScrollItem item)
    {
        // If slot size has not yet been determined, return
        if (m_slotSize == Vector2.zero)
        {
            return;
        }
        float sizeX = m_slotSize.x;
        float sizeY = m_slotSize.y;
        // If user did not specify the slot size, subtract the default space from the size
        if (m_spaceBetweenItemCenters == 0.0f)
        {
            sizeX = Mathf.Clamp(sizeX - DEFAULT_SPACE_BETWEEN_ITEMS * 0.5f, 0.0f, Mathf.Infinity);
            sizeY = Mathf.Clamp(sizeY - DEFAULT_SPACE_BETWEEN_ITEMS * 0.5f, 0.0f, Mathf.Infinity);
        }
        item.SetColliderSize(sizeX, sizeY);
    }

    #endregion // Scroll Items

    #region Scrolling

    public enum ScrollingState
    {
        IDLE,                   // No scrolling
        TOUCH_SCROLL,           // Normal scrolling via touch
        RESIDUAL_SCROLL,        // Continuation of scrolling on touch release
        INIT_GRID_LOCK,         // Scrolling until items stop at grid positions
        INIT_REBOUND_SCROLL,    // Scrolling from outside to back within scroll bounds
        AUTO_SCROLL             // Scrolling from and to specified start and end positions
    }
    [SerializeField] private ScrollingState  m_scrollingState        = ScrollingState.IDLE;

    private bool            m_isScrollingEnabled    = true;

    private ITouch          m_activeTouch           = null;
    // Position of the active touch in the previous frame
    // Note: ITouch's PreviousPosition field does not update when touch is not moving
    //  (i.e. it only updates when PreviousPosition != Position)
    private Vector2         m_prevTouchPos          = Vector2.zero;

    // TouchScript gestures
    private PressGesture    m_pressGesture          = null;
    private ReleaseGesture  m_releaseGesture        = null;
    
    //private Vector2       m_curScreenTouchDelta   = Vector2.zero;
    //private Vector2       m_prevScreenTouchDelta  = Vector2.zero;

    private Vector2         m_curWorldTouchDelta    = Vector2.zero;
    //private Vector2         m_prevWorldTouchDelta   = Vector2.zero;
    
    // The amount to scroll this frame
    private float           m_scrollSpeed           = 0.0f;
    // The target scroll position when in AUTO_SCROLL mode
    private float           m_autoScrollTargetPos   = 0.0f;

    // Fields for determining if touch is intended for scrolling or for interacting with scroll item elements
    private Vector2         m_firstTouchPos         = Vector2.zero;
    // Set when touch moves beyond minimum distance threshold
    private bool            m_isTouchForScrolling   = false;
    // When touch moves this distance (in inches) away from initial touch position,
    //  consider it as touch intended for scrolling
    private const   float   SCROLLING_TOUCH_DIST_MIN_INCHES = 0.1f;
    // Some platforms, such as older devices and the Editor, do not support Screen.dpi
    // Use minimum distance in pixels for these platforms
    private const   float   SCROLLING_TOUCH_DIST_MIN_PIXELS = 5;
    
    /// <summary>
    /// Updates scrolling.
    /// </summary>
    private void UpdateScrollingState()
    {
        if (!m_isScrollingEnabled)
        {
            return;
        }
        
        switch (m_scrollingState)
        {
            case ScrollingState.IDLE:
                // No scrolling
                break;
            // State when there is an active touch
            case ScrollingState.TOUCH_SCROLL:
                CheckTouchForScrolling();
                UpdateScrollWithTouch();

                Scroll(m_scrollSpeed);

                break;
            // State when there is no active touch and residual scrolling is enabled
            // If scroll speed is less than lock-to-grid speed and 
            case ScrollingState.RESIDUAL_SCROLL:
                UpdateResidualScroll();
                
                Scroll(m_scrollSpeed);

                // Check if scrolling should go to GRID_LOCK state
                if (m_enableGridLocking)
                {
                    // If enabled and scroll speed goes under the lock-to-grid speed,
                    //  let grid-lock code handle scrolling
                    if (Mathf.Abs(m_scrollSpeed) < m_lockToGridScrollSpeed)
                    {
                        m_scrollingState = ScrollingState.INIT_GRID_LOCK;
                    }
                }
                else
                {
                    // If grid-locking is disabled, continue with residual scrolling
                    //  until scroll speed decays to 0, then switch to IDLE state
                    if (m_scrollSpeed == 0.0f)
                    {
                        m_scrollingState = ScrollingState.IDLE;
                    }
                }
                break;
            // State when there is no active touch and grid-locking is enabled
            // If scroll speed is above lock-to-grid speed and residual scrolling is enabled,
            //  state is RESIDUAL_SCROLL
            case ScrollingState.INIT_GRID_LOCK:
                InitializeGridLocking();
                break;
            case ScrollingState.INIT_REBOUND_SCROLL:
                InitializeReboundScrolling();
                break;
            case ScrollingState.AUTO_SCROLL:
                // If scroll speed is 0, no scrolling occurs
                if (m_scrollSpeed == 0.0f)
                {
                    m_scrollingState = ScrollingState.IDLE;
                    break;
                }
                
                // Check if scroll panel reaches target position
                float scrollRootPos = GetScrollRootPosInScrollDir();
                if (scrollRootPos == m_autoScrollTargetPos ||
                    (m_scrollSpeed > 0.0f && scrollRootPos > m_autoScrollTargetPos) ||
                    (m_scrollSpeed < 0.0f && scrollRootPos < m_autoScrollTargetPos))
                {
                    // Make sure scroll root stops at the actual target position
                    if (IsHorizontalScrolling)  m_scrollRoot.SetPosX(m_autoScrollTargetPos);
                    else                        m_scrollRoot.SetPosY(m_autoScrollTargetPos);
                    // Stop scrolling
                    m_scrollSpeed = 0.0f;
                    m_scrollingState = ScrollingState.IDLE;
                }
                else
                {
                    // While target has not yet been reached, scroll towards target
                    // Make sure panel does not scroll past the target position
                    float distToTarget = m_autoScrollTargetPos - scrollRootPos;
                    if (Mathf.Abs(m_scrollSpeed) < Mathf.Abs(distToTarget))
                    {
                        Scroll(m_scrollSpeed);
                    }
                    else
                    {
                        Scroll(distToTarget);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Initializes rebound scrolling.
    /// </summary>
    private void InitializeReboundScrolling()
    {
        // If rebound speed is 0, stop scrolling and go to IDLE state
        if (m_scrollReboundSpeed == 0.0f)
        {
            m_scrollingState = ScrollingState.IDLE;
            return;
        }

        // Get scroll root position
        float scrollRootPos = GetScrollRootPosInScrollDir();
        // The amount the scroll panel is over the scroll limit
        float overLimitAmount = 0.0f;
        // Reset autoscroll values
        m_autoScrollTargetPos = 0.0f;
        // Set autoscroll values depending on which scroll limits the panel is going over
        if (IsHorizontalScrolling)
        {
            if (CanFitInScrollArea())
            {
                float leftToLimit = GetLeftmostItem().transform.position.x - m_leftScrollLimits;
                float rightToLimit = GetRightmostItem().transform.position.x - m_rightScrollLimits;
                overLimitAmount = (Mathf.Abs(leftToLimit) <= Mathf.Abs(rightToLimit)) ? leftToLimit : rightToLimit;
            }
            else
            {
                float leftOverLimit = GetLeftmostItem().transform.position.x - m_leftScrollLimits;
                float rightOverLimit = GetRightmostItem().transform.position.x - m_rightScrollLimits;
                if (leftOverLimit > 0.0f)
                {
                    overLimitAmount = leftOverLimit;
                }
                else if (rightOverLimit < 0.0f)
                {
                    overLimitAmount = rightOverLimit;
                }
            }
        }
        else /* if (IsVerticalScrolling) */
        {
            if (CanFitInScrollArea())
            {
                float topToLimit = GetTopItem().transform.position.x - m_upperScrollLimits;
                float bottomToLimit = GetBottomItem().transform.position.x - m_lowerScrollLimits;
                overLimitAmount = (Mathf.Abs(topToLimit) <= Mathf.Abs(bottomToLimit)) ? topToLimit : bottomToLimit;
            }
            else
            {
                float topOverLimit = GetTopItem().transform.position.y - m_upperScrollLimits;
                float bottomOverLimit = GetBottomItem().transform.position.y - m_lowerScrollLimits;
                if (topOverLimit < 0.0f)
                {
                    overLimitAmount = topOverLimit;
                }
                else if (bottomOverLimit > 0.0f)
                {
                    overLimitAmount = bottomOverLimit;
                }
            }
        }

        if (overLimitAmount != 0.0f)
        {
            // Set target pos
            m_autoScrollTargetPos = scrollRootPos - overLimitAmount;
            // Set rebound scroll speed
            m_scrollSpeed = m_scrollReboundSpeed * Mathf.Sign(m_autoScrollTargetPos - scrollRootPos);
            
            // Start autoscroll
            m_scrollingState = ScrollingState.AUTO_SCROLL;
        }
        else
        {
            // If already within scroll limits, go to IDLE state
            m_scrollingState = ScrollingState.IDLE;
        }
    }

    /// <summary>
    /// Gets the scroll item nearest to the specified position.
    /// </summary>
    /// <param name="pos">The position.</param>
    /// <returns>The scroll item.</returns>
    public UIScrollItem GetItemNearestToPosition(Vector3 pos)
    {
        if (m_scrollItemList.Count == 0)
        {
            return null;
        }
        float checkPos = 0.0f;
        float firstItemPos = 0.0f;
        if (IsHorizontalScrolling)
        {
            checkPos = pos.x;
            firstItemPos = m_scrollItemList[0].transform.position.x;
            UIScrollItem leftItem = GetLeftmostItem();
            UIScrollItem rightItem = GetRightmostItem();
            if (checkPos < leftItem.transform.position.x)
            {
                return leftItem;
            }
            else if (checkPos > rightItem.transform.position.x)
            {
                return rightItem;
            }
        }
        else /* if (IsVerticalScrolling) */
        {
            checkPos = pos.y;
            firstItemPos = m_scrollItemList[0].transform.position.y;
            UIScrollItem topItem = GetTopItem();
            UIScrollItem bottomItem = GetBottomItem();
            if (checkPos > topItem.transform.position.y)
            {
                return topItem;
            }
            else if (checkPos < bottomItem.transform.position.y)
            {
                return bottomItem;
            }
        }
        float slotSize = SlotSizeInScrollDir;
        float checkPosToFirstItemDist = Mathf.Abs(checkPos - firstItemPos);
        int nearestSlotIndex = (int)Math.Round(checkPosToFirstItemDist / slotSize,
                                               MidpointRounding.AwayFromZero);
        if (nearestSlotIndex < 0 || nearestSlotIndex >= m_scrollItemList.Count)
        {
            return null;
        }
        return m_scrollItemList[nearestSlotIndex];
    }

    /// <summary>
    /// Initializes grid locking.
    /// </summary>
    private void InitializeGridLocking()
    {
        if (m_scrollItemList.Count == 0)
        {
            return;
        }
        
        // Get the position of the first scroll item
        float firstItemPos = IsHorizontalScrolling ?
                                m_scrollItemList[0].transform.position.x :
                                m_scrollItemList[0].transform.position.y;
        
        // Look for the grid position nearest to the first item
        float firstSlotPos = GetFirstGridSlotPos();
        float slotSize = SlotSizeInScrollDir;
        float firstSlotToFirstItemVec = firstItemPos - firstSlotPos;
        // Get the number of grid slots between the first item and the first slot
        float firstSlotToFirstItemSlotCount = Mathf.Round(firstSlotToFirstItemVec / slotSize);
        // Get the nearest slot position
        float nearestSlotPos = firstSlotPos + firstSlotToFirstItemSlotCount * slotSize;
        // Get vector from first item to the nearest slot
        float firstItemToNearestSlotVec = nearestSlotPos - firstItemPos;

        // Set autoscroll values
        float scrollRootPos = GetScrollRootPosInScrollDir();
        m_autoScrollTargetPos = scrollRootPos + firstItemToNearestSlotVec;
        m_scrollSpeed = m_lockToGridScrollSpeed * Mathf.Sign(m_autoScrollTargetPos - scrollRootPos);

        // Start autoscroll
        m_scrollingState = ScrollingState.AUTO_SCROLL;
    }

    /// <summary>
    /// Gets the distance from the first scroll item to the scroll root.
    /// </summary>
    private float GetRootToFirstItemDir()
    {
        if (m_scrollItemList.Count == 0)
        {
            return 0.0f;
        }
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                return m_scrollItemList[0].transform.localPosition.x;
            case ScrollOrientation.VERTICAL:
                return m_scrollItemList[0].transform.localPosition.y;
        }
        return 0.0f;
    }

    /// <summary>
    /// Checks if the touch is intended for scrolling or for interacting with scroll item elements.
    /// </summary>
    private void CheckTouchForScrolling()
    {
        if (m_activeTouch == null || m_isTouchForScrolling)
        {
            return;
        }
        
        // Vector from first touch position to current touch position
        Vector2 touchVec = m_activeTouch.Position - m_firstTouchPos;
        // Consider only the distance the touch has moved along the scrolling direction
        float touchDist = Mathf.Abs(IsHorizontalScrolling ? touchVec.x : touchVec.y);
        // When touch moves a certain distance from the start position,
        //  consider it as a touch intended for scrolling
        if ((Screen.dpi > 0 && (touchDist / Screen.dpi > SCROLLING_TOUCH_DIST_MIN_INCHES)) ||
            (Screen.dpi == 0 && touchDist > SCROLLING_TOUCH_DIST_MIN_PIXELS))
        {
            m_isTouchForScrolling = true;
            // Get UI camera
            Camera cam = Locator.GetUIManager().UICamera.Camera;
            // Reset UI elements under the selected scroll item (if one was selected)
            RaycastHit2D[] hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(m_firstTouchPos), Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                UIScrollItem touchedItem = hit.transform.GetComponent<UIScrollItem>();
                if (touchedItem != null)
                {
                    touchedItem.Reset();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Updates scrolling when a touch is on the scroll panel.
    /// </summary>
    private void UpdateScrollWithTouch()
    {
        if (m_activeTouch == null)
        {
            return;
        }

        //m_prevScreenTouchDelta = m_curScreenTouchDelta;
        //m_curScreenTouchDelta = m_activeTouch.Position - m_prevTouchPos;

        // Get UI camera
        Camera cam = Locator.GetUIManager().UICamera.Camera;
        // Get touch position in world coordinates
        Vector3 prevWorldTouchPos = cam.ScreenToWorldPoint(m_prevTouchPos);
        Vector3 curWorldTouchPos = cam.ScreenToWorldPoint(m_activeTouch.Position);
        // Store the current touch position for the next frame
        m_prevTouchPos = m_activeTouch.Position;
        // Save the previous value before updating the current value
        //m_prevWorldTouchDelta = m_curWorldTouchDelta;
        m_curWorldTouchDelta = curWorldTouchPos - prevWorldTouchPos;
        // Update scrolling speed
        m_scrollSpeed = IsHorizontalScrolling ? m_curWorldTouchDelta.x : m_curWorldTouchDelta.y;

        // If scrolled beyond scroll limits, modify scrolling speed by an over-limit factor
        if (m_enableScrollingPastLimits && !IsWithinScrollBounds())
        {
            m_scrollSpeed *= m_overLimitScrollingFactor;
        }
    }

    /// <summary>
    /// Updates residual scrolling.
    /// </summary>
    private void UpdateResidualScroll()
    {
        if (m_residualScrollDeceleration == 0.0f || m_scrollSpeed == 0.0f)
        {
            return;
        }
        float prevSign = Mathf.Sign(m_scrollSpeed);
        m_scrollSpeed = prevSign * (Mathf.Abs(m_scrollSpeed) - m_residualScrollDeceleration * Time.deltaTime);
        float newSign = Mathf.Sign(m_scrollSpeed);
        // Clamp scroll speed
        if (m_residualScrollMaxSpeed != 0.0f)
        {
            if (Mathf.Abs(m_scrollSpeed) > m_residualScrollMaxSpeed)
            {
                m_scrollSpeed = newSign * m_residualScrollMaxSpeed;
            }
        }
        // Decelerate to 0
        if (prevSign != newSign)
        {
            m_scrollSpeed = 0.0f;
        }
        // If scrolled beyond scroll limits, go to INIT_REBOUND_SCROLL state
        if (m_enableScrollingPastLimits && !IsWithinScrollBounds())
        {
            m_scrollingState = ScrollingState.INIT_REBOUND_SCROLL;
        }
    }

    /// <summary>
    /// Gets the scroll root position (dimension) along the scroll direction.
    /// </summary>
    /// <returns></returns>
    private float GetScrollRootPosInScrollDir()
    {
        return IsHorizontalScrolling ? m_scrollRoot.position.x : m_scrollRoot.position.y;
    }

    /// <summary>
    /// Scrolls the panel (i.e. moves the scroll root).
    /// </summary>
    /// <param name="scrollDistance">Distance to move</param>
    private void Scroll(float scrollDistance)
    {
        Vector3 newPos = m_scrollRoot.position;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:  newPos.x += scrollDistance;     break;
            case ScrollOrientation.VERTICAL:    newPos.y += scrollDistance;     break;
        }
        // If scrolling past limits is not allowed (or allowed, but over-limit factor zeroes the speed),
        //  clamp the scroll panel within scroll limits
        if (!m_enableScrollingPastLimits || m_overLimitScrollingFactor == 0.0f)
        {
            newPos = ClampToScrollArea(newPos);
        }
        m_scrollRoot.position = newPos;
    }

    #endregion // Scrolling

    #region Scroll Limits

    // Note: When you need to know how far the scroll root object can scroll,
    //  use the GetLeft/Right/Upper/LowerScrollRootLimits methods instead
    private float m_leftScrollLimits = 0.0f;
    private float m_rightScrollLimits = 0.0f;
    private float m_upperScrollLimits = 0.0f;
    private float m_lowerScrollLimits = 0.0f;

    /// <summary>
    /// Determines whether the scroll panel is within scroll bounds.
    /// </summary>
    private bool IsWithinScrollBounds()
    {
        if (m_scrollItemList.Count == 0)
        {
            return true;
        }
        
        if (IsHorizontalScrolling)
        {
            float leftItemPos = GetLeftmostItem().transform.position.x;
            float rightItemPos = GetRightmostItem().transform.position.x;
            if (CanFitInScrollArea())
            {
                return leftItemPos >= m_leftScrollLimits && rightItemPos <= m_rightScrollLimits;
            }
            else
            {
                return leftItemPos <= m_leftScrollLimits && rightItemPos >= m_rightScrollLimits;
            }
        }
        else /* if (IsVerticalScrolling) */
        {
            float topItemPos = GetTopItem().transform.position.y;
            float bottomItemPos = GetBottomItem().transform.position.y;
            if (CanFitInScrollArea())
            {
                return topItemPos <= m_upperScrollLimits && bottomItemPos >= m_lowerScrollLimits;
            }
            else
            {
                return topItemPos >= m_upperScrollLimits && bottomItemPos <= m_lowerScrollLimits;
            }
        }
    }

    /// <summary>
    /// Gets the furthest the scroll root object can scroll to the right (if in horizontal scrolling).
    /// </summary>
    private float GetLeftScrollRootLimits()
    {
        if (m_scrollItemList.Count > 0 && IsHorizontalScrolling)
        {
            return m_leftScrollLimits + Mathf.Abs(GetLeftmostItem().transform.localPosition.x);
        }
        return m_scrollRoot.position.x;
    }

    /// <summary>
    /// Gets the furthest the scroll root object can scroll to the left (if in horizontal scrolling).
    /// </summary>
    private float GetRightScrollRootLimits()
    {
        if (m_scrollItemList.Count > 0 && IsHorizontalScrolling)
        {
            return m_rightScrollLimits - Mathf.Abs(GetRightmostItem().transform.localPosition.x);
        }
        return m_scrollRoot.position.x;
    }

    /// <summary>
    /// Gets the furthest the scroll root object can scroll downward (if in vertical scrolling).
    /// </summary>
    private float GetUpperScrollRootLimits()
    {
        if (m_scrollItemList.Count > 0 && IsVerticalScrolling)
        {
            return m_upperScrollLimits - Mathf.Abs(GetTopItem().transform.localPosition.y);
        }
        return m_scrollRoot.position.y;
    }

    /// <summary>
    /// Gets the furthest the scroll root object can scroll upward (if in vertical scrolling).
    /// </summary>
    private float GetLowerScrollRootLimits()
    {
        if (m_scrollItemList.Count > 0 && IsVerticalScrolling)
        {
            return m_lowerScrollLimits + Mathf.Abs(GetBottomItem().transform.localPosition.y);
        }
        return m_scrollRoot.position.y;
    }

    /// <summary>
    /// Updates the scroll limits.
    /// </summary>
    private void UpdateScrollLimits()
    {
        if (m_scrollItemList.Count == 0)
        {
            return;
        }

        float limitDistFromEdge = 0.5f * SlotSizeInScrollDir;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                m_leftScrollLimits = ScrollBounds.min.x + limitDistFromEdge;
                m_rightScrollLimits = ScrollBounds.max.x - limitDistFromEdge;
                break;
            case ScrollOrientation.VERTICAL:
                m_upperScrollLimits = ScrollBounds.max.y - limitDistFromEdge;
                m_lowerScrollLimits = ScrollBounds.min.y + limitDistFromEdge;
                break;
        }
    }

    /// <summary>
    /// Clamps the scroll root to the scroll area.
    /// </summary>
    /// <param name="pos">The position to clamp.</param>
    /// <returns>The clamped position</returns>
    private Vector3 ClampToScrollArea(Vector3 pos)
    {
        Vector3 newPos = pos;
        switch (m_scrollOrientation)
        {
            case ScrollOrientation.HORIZONTAL:
                float leftLimits = GetLeftScrollRootLimits();
                float rightLimits = GetRightScrollRootLimits();
                bool clampToLeftEdge = newPos.x >= leftLimits;
                bool clampToRightEdge = newPos.x <= rightLimits;
                // If order is left-to-right, prioritize clamping to left edge
                if (!m_isReverseOrder)
                {
                    if (clampToLeftEdge)// || CanFitInScrollArea())
                    {
                        newPos.x = leftLimits;
                    }
                    else if (clampToRightEdge)
                    {
                        newPos.x = rightLimits;
                    }
                }
                // If order is right-to-left, prioritize clamping to right edge
                else
                {
                    if (clampToRightEdge)// || CanFitInScrollArea())
                    {
                        newPos.x = rightLimits;
                    }
                    else if (clampToLeftEdge)
                    {
                        newPos.x = leftLimits;
                    }
                }
                break;
            case ScrollOrientation.VERTICAL:
                float upperLimits = GetUpperScrollRootLimits();
                float lowerLimits = GetLowerScrollRootLimits();
                bool clampToUpperEdge = newPos.y <= upperLimits;
                bool clampToLowerEdge = newPos.y >= lowerLimits;
                // If order is top-to-bottom, prioritize clamping to upper edge
                if (!m_isReverseOrder)
                {
                    if (clampToUpperEdge)// || CanFitInScrollArea())
                    {
                        newPos.y = upperLimits;
                    }
                    else if (clampToLowerEdge)
                    {
                        newPos.y = lowerLimits;
                    }
                }
                // If order is bottom-to-top, prioritize clamping to lower edge
                if (!m_isReverseOrder)
                {
                    if (clampToLowerEdge)// || CanFitInScrollArea())
                    {
                        newPos.y = lowerLimits;
                    }
                    else if (clampToUpperEdge)
                    {
                        newPos.y = upperLimits;
                    }
                }
                break;
        }

        return newPos;
    }

    /// <summary>
    /// Clamps the scroll panel within the scroll area.
    /// </summary>
    private void UpdateClampToScrollArea()
    {
        // If scroll panel is empty, reset
        if (m_scrollItemList.Count == 0)
        {
            Reset();
            return;
        }
        m_scrollRoot.transform.position = ClampToScrollArea(m_scrollRoot.transform.position);
    }

    #endregion // Scroll Limits

    #region MonoBehaviour

    /// <summary>
    /// Awake this instance.
    /// </summary>
    protected override void Awake()
	{
		base.Awake();
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start()
	{
		base.Start();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	protected override void Update()
	{
		base.Update();
        
        if (!m_isInitialized)
        {
            return;
        }

        UpdateScrollingState();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    protected override void OnDestroy()
	{
		base.OnDestroy();
    }

	#endregion // MonoBehaviour
}
