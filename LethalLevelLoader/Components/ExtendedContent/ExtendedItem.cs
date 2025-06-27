using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedItem", menuName = "Lethal Level Loader/Extended Content/ExtendedItem", order = 23)]
    public class ExtendedItem : ExtendedContent<ExtendedItem, Item, ItemManager>
    {
        public override Item Content { get => Item; protected set => Item = value; }

        [field: Header("General Settings")]
        [field: SerializeField] public Item Item { get; set; }
        [field: SerializeField] public string PluralisedItemName { get; set; } = string.Empty;
        [field: SerializeField] public bool IsBuyableItem { get; set; }

        [field: Space(5), Header("Dynamic Injection Matching Settings")]
        [field: SerializeField] public LevelMatchingProperties LevelMatchingProperties { get; set; }
        [field: SerializeField] public DungeonMatchingProperties DungeonMatchingProperties { get; set; }

        [field: Space(5), Header("Terminal Store & Info Override Settings")]
        [field: SerializeField] public string OverrideInfoNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] public string OverrideBuyNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] public string OverrideBuyConfirmNodeDescription { get; set; } = string.Empty;

        public TerminalKeyword BuyKeyword { get; internal set; }
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

        //Might be obsolete
        public static ExtendedItem Create(Item newItem, ExtendedMod extendedMod, ContentType contentType) => Create(newItem);
        public static ExtendedItem Create(Item newItem)
        {
            ExtendedItem extendedItem = Create<ExtendedItem, Item, ItemManager>(newItem.itemName.SkipToLetters().RemoveWhitespace() + "ExtendedItem", newItem);
            extendedItem.TryCreateMatchingProperties();
            return (extendedItem);
        }

        internal override void Initialize()
        {
            TryCreateMatchingProperties();
        }

        internal override void TryCreateMatchingProperties()
        {
            LevelMatchingProperties = MatchingProperties.TryCreate(LevelMatchingProperties, this);
            DungeonMatchingProperties = MatchingProperties.TryCreate(DungeonMatchingProperties, this);
        }

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
        internal override List<GameObject> GetNetworkPrefabsForRegistration() => Item.spawnPrefab.GetComponentsInChildren<NetworkObject>().Select(n => n.gameObject).ToList();
    }
}
