using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedItem", menuName = "Lethal Level Loader/Extended Content/ExtendedItem", order = 23)]
    public class ExtendedItem : ExtendedContent<ExtendedItem, Item, ItemManager>, ITerminalInfoEntry, ITerminalPurchasableEntry
    {
        public override Item Content { get => Item; protected set => Item = value; }

        public int PurchasePrice { get; private set; }

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

        public TerminalKeyword NounKeyword { get; internal set; }
        public TerminalNode PurchasePromptNode { get; internal set; }
        public TerminalNode PurchaseConfirmNode { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }

        TerminalKeyword ITerminalPurchasableEntry.RegistryKeyword => TerminalManager.Keyword_Buy;
        TerminalKeyword ITerminalInfoEntry.RegistryKeyword => TerminalManager.Keyword_Info;
        public List<CompatibleNoun> GetRegistrations() => new() { (this as ITerminalInfoEntry).GetPair(), (this as ITerminalPurchasableEntry).GetPair() };

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

        protected override void OnGameIDChanged()
        {
            if (PurchasePromptNode != null) PurchasePromptNode.buyItemIndex = GameID;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.buyItemIndex = GameID;
        }

        public void SetPurchasePrice(int newPrice)
        {
            PurchasePrice = newPrice;
            Item.creditsWorth = newPrice;
            if (PurchasePromptNode != null) PurchasePromptNode.itemCost = newPrice;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.itemCost = newPrice;
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
