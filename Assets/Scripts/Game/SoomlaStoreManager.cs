/******************************************************************************
*  @file       SoomlaStoreManager.cs
*  @brief      Handles Soomla Store activities
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using UnityEngine;
using System;
using System.Collections.Generic;
using Soomla;
using Soomla.Store;

#endregion // Namespaces

public class CharPurchaseEventArgs : EventArgs
{
    public string CharacterID { get; set; }
    public CharPurchaseEventArgs(string characterID)
    {
        CharacterID = characterID;
    }
}

public class SoomlaStoreManager
{
    #region Public Interface

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(CharacterResource charResource,
                           Action<string> charPurchaseSucceededDelegate,
                           Action<string> charPurchaseCancelledDelegate,
                           Action restoreTransactionsCompletedDelegate,
                           Action restoreTransactionsFailedDelegate,
                           Action<int> coinBalanceChangedDelegate,
                           Action<int> gachaBalanceChangedDelegate)
    {
        // Initialize only once
        if (m_isInitialized) 
        {
            return;
        }
        
        // Set up character data in Soomla's StoreAssets
        CRCAssets.CharactersLTVGArray = new EquippableVG[charResource.CharacterCount];
        m_charItemIDDictionary.Clear();
        // For testing: Index used for assigning test product IDs
        //int testIDIndex = 0;
        for (int index = 0; index < charResource.CharacterCount; ++index)
        {
            CharacterType character = (CharacterType)index;
            CharacterResource.CharacterStruct charData = charResource.GetCharacterStruct(character);

            // Check if character is buyable
            PurchaseType purchaseType = new PurchaseWithMarket(charData.ItemID, charData.Price);
            /*
            // For testing: Use test product IDs for the first four purchasable characters
            PurchaseType purchaseType = null;
            string productID = charData.ItemID;
            if (charData.IsBuyable)
            {
                if (testIDIndex == 0)       productID = CRCAssets.PURCHASED_TEST_ID;
                else if (testIDIndex == 1)  productID = CRCAssets.CANCELED_TEST_ID;
                else if (testIDIndex == 2)  productID = CRCAssets.REFUNDED_TEST_ID;
                else if (testIDIndex == 3)  productID = CRCAssets.UNAVAILABLE_TEST_ID;
                testIDIndex++;
            }
            purchaseType = new PurchaseWithMarket(productID, charData.Price);
            //purchaseType = new PurchaseWithMarket(CRCAssets.PURCHASED_TEST_ID, charData.Price);
            */

            // Create equippable virtual good instance for each character
            CRCAssets.CharactersLTVGArray[index] = new EquippableVG(
                EquippableVG.EquippingModel.CATEGORY,
                charData.Name,
                "",
                charData.ItemID,
                purchaseType);
            
            // Add the item ID and character type to a dictionary
            if (!m_charItemIDDictionary.ContainsKey(charData.ItemID))
            {
                m_charItemIDDictionary.Add(charData.ItemID, character);
            }
        }

        // Create an instance of the in-game implementation of IStoreAssets
        CRCAssets crcAssets = new CRCAssets();
        // Update the list of character goods
        for (int index = 0; index < charResource.CharacterCount; ++index)
        {
            crcAssets.AddToCharacterGoodsList(CRCAssets.CharactersLTVGArray[index]);
        }
        // Finalize character goods collections before initializing Soomla Store
        crcAssets.FinalizeCharacterGoods();

        // Save delegates
        m_charPurchaseSucceededDelegate             = charPurchaseSucceededDelegate;
        m_charPurchaseCancelledDelegate             = charPurchaseCancelledDelegate;
        m_restoreTransactionsCompletedDelegate      = restoreTransactionsCompletedDelegate;
        m_restoreTransactionsFailedDelegate         = restoreTransactionsFailedDelegate;
        m_coinBalanceChangedDelegate                = coinBalanceChangedDelegate;
        m_gachaBalanceChangedDelegate               = gachaBalanceChangedDelegate;

        // Subscribe to Store events
        StoreEvents.OnSoomlaStoreInitialized        += OnSoomlaStoreInitialized;
        StoreEvents.OnCurrencyBalanceChanged        += OnCurrencyBalanceChanged;
        StoreEvents.OnMarketPurchaseStarted         += OnMarketPurchaseStarted;
        StoreEvents.OnMarketPurchase                += OnMarketPurchase;
        StoreEvents.OnMarketPurchaseCancelled       += OnMarketPurchaseCancelled;
        StoreEvents.OnMarketRefund                  += OnMarketRefund;
        StoreEvents.OnRestoreTransactionsStarted    += OnRestoreTransactionsStarted;
        StoreEvents.OnRestoreTransactionsFinished   += OnRestoreTransactionsFinished;
        StoreEvents.OnUnexpectedStoreError          += OnUnexpectedStoreError;

        // Initialize rewards
        m_firstLaunchReward = new VirtualItemReward(
            "first-launch",
            "Give coins at first launch",
            CRCAssets.COIN_CURRENCY_ITEM_ID,
            FIRST_LAUNCH_REWARD_AMOUNT);

        // Initialize the store with the in-game implementation of IStoreAssets
        // Note: This must be done after setting up values in the store assets class
        SoomlaStore.Initialize(crcAssets);
    }

    /// <summary>
    /// Adds to the player's coin balance.
    /// </summary>
    /// <param name="amount">The amount of coins to add.</param>
    public void AddToCoinBalance(int amount)
    {
        StoreInventory.GiveItem(CRCAssets.COIN_CURRENCY_ITEM_ID, amount);
    }

    /// <summary>
    /// [FOR TESTING PURPOSES ONLY] Sets the coin balance.
    /// </summary>
    /// <param name="amount">The amount to set the balance to.</param>
    public void SetCoinBalance(int amount)
    {
        int curBalance = GetCoinBalance();
        if (curBalance > amount)
        {
            StoreInventory.TakeItem(CRCAssets.COIN_CURRENCY_ITEM_ID, curBalance - amount);
        }
        else if (curBalance < amount)
        {
            StoreInventory.GiveItem(CRCAssets.COIN_CURRENCY_ITEM_ID, amount - curBalance);
        }
    }

    /// <summary>
    /// Gives the player a reward for watching a video ad.
    /// </summary>
    public void GiveVideoAdReward()
    {
        StoreInventory.GiveItem(CRCAssets.COIN_CURRENCY_ITEM_ID, VIDEO_AD_REWARD_AMOUNT);
    }

    /// <summary>
    /// Gets the amount of coins awarded for watching a video ad.
    /// </summary>
    public int GetVideoAdRewardAmount()
    {
        return VIDEO_AD_REWARD_AMOUNT;
    }

    /// <summary>
    /// Buys a gacha round.
    /// </summary>
    /// <returns>True if successful.</returns>
    public bool BuyGachaRound()
    {
        // Check if balance is enough
        if (CanAffordGacha())
        {
            StoreInventory.BuyItem(CRCAssets.GACHA_ITEM_ID);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Determines whether player can afford a gacha round.
    /// </summary>
    public bool CanAffordGacha()
    {
        return StoreInventory.CanAfford(CRCAssets.GACHA_ITEM_ID);
    }

    /// <summary>
    /// Gets the price of a gacha round.
    /// </summary>
    public int GetGachaPrice()
    {
        return CRCAssets.GACHA_PRICE;
    }

    /// <summary>
    /// Buys the character.
    /// </summary>
    /// <param name="character">The character to buy.</param>
    public void BuyCharacter(CharacterType character)
    {
        StoreInventory.BuyItem(CRCAssets.CharactersLTVGArray[(int)character].ItemId);
    }

    /// <summary>
    /// Restores the player's transactions.
    /// </summary>
    public void RestoreTransactions()
    {
        if (!SoomlaStore.TransactionsAlreadyRestored())
        {
            SoomlaStore.RestoreTransactions();
        }
    }

    /// <summary>
    /// Gets the character with the specified item ID.
    /// </summary>
    public CharacterType GetCharacterWithID(string itemID)
    {
        return m_charItemIDDictionary[itemID];
    }

    /// <summary>
    /// Gives the player the specified character.
    /// </summary>
    public void GiveCharacter(CharacterType character)
    {
        string firstCharID = CRCAssets.CharactersLTVGArray[(int)character].ItemId;
        // Make sure character is not already owned
        if (StoreInventory.GetItemBalance(firstCharID) == 0)
        {
            StoreInventory.GiveItem(firstCharID, 1);
        }
    }

    /// <summary>
    /// Equips the specified character.
    /// </summary>
    /// <param name="character">The character to equip.</param>
    public void EquipCharacter(CharacterType character)
    {
        StoreInventory.EquipVirtualGood(CRCAssets.CharactersLTVGArray[(int)character].ItemId);
        // When a character is equipped, it is considered "used"
        SoomlaDataSystem dataSystem = (SoomlaDataSystem)Locator.GetDataSystem();
        dataSystem.SetCharacterUsed(character, true);
    }

    /// <summary>
    /// Gets the currently equipped character.
    /// </summary>
    /// <returns>The equipped character</returns>
    public CharacterType GetEquippedCharacter()
    {
        EquippableVG evg = StoreInventory.GetEquippedVirtualGood(CRCAssets.GetCharactersCategory());
        if (evg != null)
        {
            return m_charItemIDDictionary[evg.ItemId];
        }
        // Give first character and equip
        CharacterType firstCharacter = CharacterType.Chicken;
        if (!IsOwned(firstCharacter))
        {
            GiveCharacter(firstCharacter);
        }
        EquipCharacter(firstCharacter);
        return firstCharacter;
    }

    /// <summary>
    /// Determines whether the specified character is owned by the player.
    /// </summary>
    public bool IsOwned(CharacterType character)
    {
        // Character is owned if its item balance in the inventory is 1 (non-zero)
        string charItemID = CRCAssets.CharactersLTVGArray[(int)character].ItemId;
        return StoreInventory.GetItemBalance(charItemID) > 0;
    }

    /// <summary>
    /// Gets the coin balance.
    /// </summary>
    /// <returns>Coins in balance</returns>
    public int GetCoinBalance()
    {
        return StoreInventory.GetItemBalance(StoreInfo.Currencies[0].ItemId);
    }

    /// <summary>
    /// Gets the number of times the player has played gacha.
    /// </summary>
    public int GetGachaPlayCount()
    {
        return StoreInventory.GetItemBalance(CRCAssets.GACHA_ITEM_ID);
    }

    /// <summary>
    /// Deletes this instance.
    /// </summary>
    public void Delete()
    {
        // Unsubscribe from Store events
        foreach (var d in StoreEvents.OnSoomlaStoreInitialized.GetInvocationList())
        {
            StoreEvents.OnSoomlaStoreInitialized -= d as StoreEvents.Action;
        }
        foreach (var d in StoreEvents.OnCurrencyBalanceChanged.GetInvocationList())
        {
            StoreEvents.OnCurrencyBalanceChanged -= d as System.Action<VirtualCurrency, int, int>;
        }
        foreach (var d in StoreEvents.OnMarketPurchaseStarted.GetInvocationList())
        {
            StoreEvents.OnMarketPurchaseStarted -= d as System.Action<PurchasableVirtualItem>;
        }
        foreach (var d in StoreEvents.OnMarketPurchase.GetInvocationList())
        {
            StoreEvents.OnMarketPurchase -= d as System.Action<PurchasableVirtualItem, string, Dictionary<string, string>>;
        }
        foreach (var d in StoreEvents.OnMarketPurchaseCancelled.GetInvocationList())
        {
            StoreEvents.OnMarketPurchaseCancelled -= d as System.Action<PurchasableVirtualItem>;
        }
        foreach (var d in StoreEvents.OnMarketRefund.GetInvocationList())
        {
            StoreEvents.OnMarketRefund -= d as System.Action<PurchasableVirtualItem>;
        }
        foreach (var d in StoreEvents.OnRestoreTransactionsStarted.GetInvocationList())
        {
            StoreEvents.OnRestoreTransactionsStarted -= d as StoreEvents.Action;
        }
        foreach (var d in StoreEvents.OnRestoreTransactionsFinished.GetInvocationList())
        {
            StoreEvents.OnRestoreTransactionsFinished -= d as System.Action<bool>;
        }
        foreach (var d in StoreEvents.OnUnexpectedStoreError.GetInvocationList())
        {
            StoreEvents.OnUnexpectedStoreError -= d as System.Action<int>;
        }
    }

    /// <summary>
    /// Gets whether Soomla Store is initialized.
    /// </summary>
    public bool IsInitialized
    {
        get { return m_isInitialized; }
    }

#endregion // Public Interface

#region Variables

    private bool m_isInitialized   = false;

    private Dictionary<string, CharacterType> m_charItemIDDictionary = new Dictionary<string, CharacterType>((int)CharacterType.SIZE);

#endregion // Variables

#region Game Data

    /// <summary>
    /// Resets game data.
    /// </summary>
    public void ResetGameData()
    {
        // Take the first launch reward (so it can be given again)
        m_firstLaunchReward.Take();
        // Reset coin balance to 0
        SetCoinBalance(0);
        // Unequip equipped character
        EquippableVG vg = StoreInventory.GetEquippedVirtualGood(CRCAssets.GetCharactersCategory());
        if (vg != null)
        {
            StoreInventory.UnEquipVirtualGood(vg.ItemId);
        }
        // Remove all characters from inventory
        for (int index = 0; index<CRCAssets.CharactersLTVGArray.Length; ++index)
        {
            VirtualGood charVG = CRCAssets.CharactersLTVGArray[index];
            if (charVG != null)
            {
                charVG.ResetBalance(0);
            }
        }
        // Reset other data
        SoomlaDataSystem dataSystem = (SoomlaDataSystem)Locator.GetDataSystem();
        dataSystem.ResetGameData();
        // Reset all characters to unused
        for (int index = 0; index<CRCAssets.CharactersLTVGArray.Length; ++index)
        {
            dataSystem.SetCharacterUsed((CharacterType)index, false);
        }
    }

#endregion // Game Data

#region Reward

    private Reward      m_firstLaunchReward         = null;

    private const int   FIRST_LAUNCH_REWARD_AMOUNT  = 50;
    private const int   VIDEO_AD_REWARD_AMOUNT      = 20;

#endregion // Reward
    
#region Soomla Store Delegates

    private Action<string>  m_charPurchaseSucceededDelegate         = null;
    private Action<string>  m_charPurchaseCancelledDelegate         = null;
    private Action          m_restoreTransactionsCompletedDelegate  = null;
    private Action          m_restoreTransactionsFailedDelegate     = null;
    private Action<int>     m_coinBalanceChangedDelegate            = null;
    private Action<int>     m_gachaBalanceChangedDelegate           = null;

    /// <summary>
    /// Called when Soomla Store is initialized.
    /// </summary>
    private void OnSoomlaStoreInitialized()
    {
        // Check if first launch
        if (!m_firstLaunchReward.Owned)
        {
            // Award a starting amount of coins
            m_firstLaunchReward.Give();
            // Give player the first character and equip it
            GiveCharacter(0);
            EquipCharacter(0);
        }

        // Set initialized flag
        m_isInitialized = true;
    }

    /// <summary>
    /// Called when the currency balance changes.
    /// </summary>
    /// <param name="virtualCurrency">The currency whose balance was changed.</param>
    /// <param name="balance">The balance of the currency after the change.</param>
    /// <param name="amountAdded">The amount that was added to the currency balance.
    ///     (in case the amount was reduced, this will be a negative value)</param>
    private void OnCurrencyBalanceChanged(VirtualCurrency virtualCurrency, int balance, int amountAdded)
    {
        if (virtualCurrency.ItemId == CRCAssets.COIN_CURRENCY_ITEM_ID)
        {
            m_coinBalanceChangedDelegate.Invoke(balance);
        }
    }

    /// <summary>
    /// Called when the balance of a specific virtual good has changed.
    /// </summary>
    /// <param name="good">The virtual good whose balance was changed.</param>
    /// <param name="balance">The balance of the good after the change.</param>
    /// <param name="amountAdded">The amount that was added to the good balance.
    ///     (in case the amount was reduced, this will be a negative value)</param>
    private void OnGoodBalanceChanged(VirtualGood good, int balance, int amountAdded)
    {
        if (good.ItemId == CRCAssets.GACHA_ITEM_ID)
        {
            m_gachaBalanceChangedDelegate.Invoke(balance);
        }
    }

    /// <summary>
    /// Called when market purchase begins.
    /// </summary>
    /// <param name="pvi">The PurchasableVirtualItem whose purchase operation has just started.</param>
    private void OnMarketPurchaseStarted(PurchasableVirtualItem pvi)
    {
        Debug.Log("Market purchase of item " + pvi.Name + " started");
    }

    /// <summary>
    /// Called when a market item is purchased successfully.
    /// </summary>
    /// <param name="pvi">The PurchasableVirtualItem that was just purchased.</param>
    /// <param name="payload">A text that you can give when you initiate the
    ///     purchase operation and you want to receive back upon completion.</param>
    /// <param name="extra">Contains platform specific information about the market purchase
    ///     Android: The "extra" dictionary will contain:
    ///         'token', 'orderId', 'originalJson', 'signature', 'userId'
    ///     iOS: The "extra" dictionary will contain:
    ///         'receiptUrl', 'transactionIdentifier', 'receiptBase64', 'transactionDate',
    ///         'originalTransactionDate', 'originalTransactionIdentifier'
    /// .</param>
    private void OnMarketPurchase(PurchasableVirtualItem pvi, string payload,
                                  Dictionary<string, string> extra)
    {
        Debug.Log("Market purchase of item " + pvi.Name + " completed successfully");

        m_charPurchaseSucceededDelegate.Invoke(pvi.ItemId);
    }

    /// <summary>
    /// Called when market purchase is cancelled.
    /// </summary>
    /// <param name="pvi">The PurchasableVirtualItem whose purchase operation was cancelled.</param>
    private void OnMarketPurchaseCancelled(PurchasableVirtualItem pvi)
    {
        Debug.Log("Market purchase of item " + pvi.Name + " cancelled");

        m_charPurchaseCancelledDelegate.Invoke(pvi.ItemId);
    }

    /// <summary>
    /// Called when market refund operation is completed.
    /// </summary>
    /// <param name="pvi">The PurchasableVirtualItem that was refunded.</param>
    private void OnMarketRefund(PurchasableVirtualItem pvi)
    {
        Debug.Log("Market item " + pvi.Name + " refunded");
    }

    /// <summary>
    /// Called when restore transactions operation has started.
    /// </summary>
    private void OnRestoreTransactionsStarted()
    {
        Debug.Log("Restore transaction started");
    }

    /// <summary>
    /// Called when restore transactions operation has completed.
    /// </summary>
    /// <param name="success">if set to <c>true</c> operation was successful.</param>
    private void OnRestoreTransactionsFinished(bool success)
    {
        if (success)
        {
            m_restoreTransactionsCompletedDelegate.Invoke();
        }
        else
        {
            m_restoreTransactionsFailedDelegate.Invoke();
        }
    }

    /// <summary>
    /// Called when when an unexpected store error occurs.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    private void OnUnexpectedStoreError(int errorCode)
    {
        SoomlaUtils.LogError("CRCSoomlaManager", "error with code: " + errorCode);
    }

#endregion // Soomla Store Delegates
}
