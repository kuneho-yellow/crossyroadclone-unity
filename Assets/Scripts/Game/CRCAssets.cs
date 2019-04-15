/******************************************************************************
*  @file       CRCAssets.cs
*  @brief      In-game implementation of Soomla Store's IStoreAssets
*  @author     Ron
*  @date       October 16, 2015
*      
*  @par [explanation]
*		> 
******************************************************************************/

#region Namespaces

using System.Collections.Generic;
using Soomla.Store;

#endregion // Namespaces

/// <summary>
/// This class defines our game's economy, which includes virtual goods, virtual currencies
/// and currency packs, virtual categories
/// </summary>
public class CRCAssets : IStoreAssets
{
    #region Public Interface
    
    /// <summary>
    /// see parent.
    /// </summary>
    public int GetVersion()
    {
        return 0;
    }

    /// <summary>
    /// see parent.
    /// </summary>
    public VirtualCurrency[] GetCurrencies()
    {
        return new VirtualCurrency[] { CoinCurrency };
    }

    /// <summary>
    /// see parent.
    /// </summary>
    public VirtualGood[] GetGoods()
    {
        return m_virtualGoodsArray;
    }

    /// <summary>
    /// Adds to the character goods list.
    /// </summary>
    /// <param name="vg">The character virtual good to add.</param>
    public void AddToCharacterGoodsList(VirtualGood vg)
    {
        // Add to character goods list as well as to virtual goods list
        m_charCategoryGoodsList.Add(vg.ItemId);
        m_virtualGoodsList.Add(vg);
    }

    /// <summary>
    /// Creates a final "Characters" category and virtual goods array.
    /// This should be called after all characters have been added to the list of
    ///     characters, and just before Store.Initialize is called.
    /// </summary>
    public void FinalizeCharacterGoods()
    {
        CharactersCategory = new VirtualCategory("Characters", m_charCategoryGoodsList);
        m_virtualGoodsArray = m_virtualGoodsList.ToArray();
    }

    /// <summary>
    /// see parent.
    /// </summary>
    public VirtualCurrencyPack[] GetCurrencyPacks()
    {
        return new VirtualCurrencyPack[] { ThousandCoinPack };
    }

    /// <summary>
    /// see parent.
    /// </summary>
    public VirtualCategory[] GetCategories()
    {
        return new VirtualCategory[] { GeneralCategory, CharactersCategory };
    }

    /// <summary>
    /// Gets the characters category from StoreInfo.
    /// Note: This is only valid after Soomla Store has been initialized.
    /// </summary>
    /// <returns>The characters category</returns>
    public static VirtualCategory GetCharactersCategory()
    {
        return StoreInfo.Categories[1]; // Use index of CharactersCategory in the GetCategories() method
    }

    #endregion // Public Interface

    #region Private Variables

    // Temporary collection to populate with virtual goods
    // Transfer to virtual goods array once all goods have been added
    private List<VirtualGood>   m_virtualGoodsList      = new List<VirtualGood> { GachaGood, NoAdsLTVG };
    // Final collection of all virtual goods
    private VirtualGood[]       m_virtualGoodsArray     = null;
    // List of character good item IDs to add to the Characters category
    private List<string>        m_charCategoryGoodsList = new List<string>();

    #endregion // Private Variables

    #region Static Final Members
    
    public const string COIN_CURRENCY_ITEM_ID           = "currency_coin";

    public const string GACHA_ITEM_ID                   = "gacha_good";
    public const int    GACHA_PRICE                     = 100;

    public const string THOUSAND_COIN_PACK_PRODUCT_ID   = "1000_coins";
    
    public const string NO_ADS_LIFETIME_PRODUCT_ID      = "no_ads";

    /* Google Test Product IDs
    *   Use the following four reserved product IDs to simulate static IAP billing responses
    *   This way, billing requests can be made without the need to first upload an apk to Google
    *            Product ID                 Google Play Response
    *       android.test.purchased          Successful purchase
    *       android.test.canceled           Canceled purchase
    *       android.test.refunded           Refunded purchase
    *       android.test.item_unavailable   Item not listed in application's product list
    */
    public const string PURCHASED_TEST_ID       = "android.test.purchased";
    public const string CANCELED_TEST_ID        = "android.test.canceled";
    public const string REFUNDED_TEST_ID        = "android.test.refunded";
    public const string UNAVAILABLE_TEST_ID     = "android.test.item_unavailable";

    #endregion // Static Final Members

    #region Virtual Currencies

    public static VirtualCurrency CoinCurrency = new VirtualCurrency(
        "Coins",                                        // name
        "",                                             // description
        COIN_CURRENCY_ITEM_ID                           // item id
    );

    #endregion // Virtual Currencies

    #region Virtual Currency Packs
    
    public static VirtualCurrencyPack ThousandCoinPack = new VirtualCurrencyPack(
        "1000 Coins",                                   // name
        "1000 Coins",                                   // description
        THOUSAND_COIN_PACK_PRODUCT_ID,                  // item id
        1000,                                           // number of currencies in the pack
        COIN_CURRENCY_ITEM_ID,                          // the currency associated with this pack
        new PurchaseWithMarket(THOUSAND_COIN_PACK_PRODUCT_ID, 0.99)
    );

    #endregion // Virtual Currency Packs

    #region Virtual Goods

    public static VirtualGood GachaGood = new SingleUseVG(
        "Gacha",                                                            // name
        "Get a random character from the list obtainable from gacha",       // description
        GACHA_ITEM_ID,                                                      // item id
        new PurchaseWithVirtualItem(COIN_CURRENCY_ITEM_ID, GACHA_PRICE));   // the way this virtual good is purchased
    
    #endregion // Virtual Goods
    
    #region Virtual Categories
    
    public static VirtualCategory GeneralCategory = new VirtualCategory(
        "General", new List<string>(new string[] { GACHA_ITEM_ID,
                                                   THOUSAND_COIN_PACK_PRODUCT_ID,
                                                   NO_ADS_LIFETIME_PRODUCT_ID })
    );

    public static VirtualCategory CharactersCategory = null;

    #endregion // Virtual Categories

    #region LifeTime Virtual Goods

    public static VirtualGood[] CharactersLTVGArray = new EquippableVG[(int)CharacterType.SIZE];

    public static VirtualGood NoAdsLTVG = new LifetimeVG(
        "No Ads",                                                       // name
        "No More Ads!",                                                 // description
        "no_ads",                                                       // item id
        new PurchaseWithMarket(NO_ADS_LIFETIME_PRODUCT_ID, 0.99));	    // the way this virtual good is purchased

    #endregion // LifeTime Virtual Goods
}
