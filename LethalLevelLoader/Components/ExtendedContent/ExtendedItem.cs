using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedItem")]
    public class ExtendedItem : ExtendedContent
    {
        public Item Item;
        [SerializeField] internal string pluralisedItemName = string.Empty;
        [SerializeField] internal bool isBuyableItem;

        public TerminalNode BuyNode { get; internal set; }
        public TerminalNode BuyConfirmNode { get; internal set; }
        public TerminalNode BuyInfoNode { get; internal set; }

        public int CreditsWorth
        {
            get
            {
                if (BuyNode != null && BuyConfirmNode != null)
                {
                    BuyNode.itemCost = Item.creditsWorth;
                    BuyConfirmNode.itemCost = Item.creditsWorth;
                }
                else
                    Debug.LogWarning("BuyNode And/Or BuyConfirm Node Missing!");
                return (Item.creditsWorth);
            }
            set
            {
                if (value >= 0)
                {
                    if (BuyNode != null && BuyConfirmNode != null)
                    {
                        BuyNode.itemCost = value;
                        BuyConfirmNode.itemCost = value;
                    }
                    else
                        Debug.LogWarning("BuyNode And/Or BuyConfirm Node Missing!");
                    Item.creditsWorth = value;
                }
            }
        }

        [Space(10)]
        [Header("Dynamic Item Injections Settings")]
        [SerializeField] internal LevelMatchingProperties levelMatchingProperties;
        [SerializeField] internal DungeonMatchingProperties dungeonMatchingProperties;

        [Header("Terminal Override Settings")]
        [SerializeField][TextArea(2, 20)] public string overrideInfoNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] public string overrideBuyNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] public string overrideBuyConfirmNodeDescription = string.Empty;

        public static ExtendedItem Create(Item newItem, ExtendedMod extendedMod, ContentType contentType)
        {
            ExtendedItem extendedItem = ScriptableObject.CreateInstance<ExtendedItem>();
            extendedItem.Item = newItem;
            extendedItem.name = newItem.itemName.SkipToLetters().RemoveWhitespace() + "ExtendedItem";
            extendedItem.ContentType = contentType;
            extendedMod.RegisterExtendedContent(extendedItem);

            extendedItem.TryCreateMatchingProperties();

            return (extendedItem);
        }

        public void Initialize()
        {
            DebugHelper.Log("Initializing Custom Item: " + Item.itemName + ". Is Buyable: " + isBuyableItem + ". Is Scrap: " + Item.isScrap);

            TryCreateMatchingProperties();

            Patches.StartOfRound.allItemsList.itemsList.Add(Item);
            if (isBuyableItem)
                TerminalManager.CreateItemTerminalData(this);
        }

        internal override void TryCreateMatchingProperties()
        {
            if (levelMatchingProperties == null)
                levelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            if (dungeonMatchingProperties == null)
                dungeonMatchingProperties = ScriptableObject.CreateInstance<DungeonMatchingProperties>();
        }

        public void SetLevelMatchingProperties(LevelMatchingProperties newLevelMatchingProperties)
        {
            if (Plugin.Instance != null)
                Debug.LogError("SetLevelMatchingProperties() Should Only Be Used In Editor!");
            levelMatchingProperties = newLevelMatchingProperties;
        }
    }
}
