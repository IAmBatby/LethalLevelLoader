using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedUnlockableItem", menuName = "Lethal Level Loader/Extended Content/ExtendedUnlockableItem", order = 21)]
    public class ExtendedUnlockableItem : ExtendedContent<ExtendedUnlockableItem, UnlockableItem, UnlockableItemManager>
    {
        public override RestorationPeriod RestorationPeriod => RestorationPeriod.Lobby;
        public override UnlockableItem Content => UnlockableItem;
        [field: Header("General Settings")]

        [field: SerializeField] public UnlockableItem UnlockableItem { get; set; }

        [field: SerializeField] public int ItemCost { get; set; }

        [field: Space(5)]
        [field: Header("Terminal Store & Info Override Settings")]

        [field: TextArea(2, 20)]
        [field: SerializeField] public string OverrideInfoNodeDescription { get; set; } = string.Empty;
        [field: TextArea(2, 20)]
        [field: SerializeField] public string OverrideBuyNodeDescription { get; set; } = string.Empty;
        [field: TextArea(2, 20)]
        [field: SerializeField] public string OverrideBuyConfirmNodeDescription { get; set; } = string.Empty;

        public int UnlockableItemID => GameID;

        public UnlockableType UnlockableType
        {
            get
            {
                if (UnlockableItem.unlockableType == 0) return (UnlockableType.Suit);
                else if (UnlockableItem.unlockableType == 1) return (UnlockableType.Furniture);
                else if (UnlockableItem.unlockableType < 0) return (UnlockableType.Invalid);
                return (UnlockableType.Unknown);
            }
        }

        public GameObject Prefab => UnlockableItem.prefabObject;
        public AutoParentToShip AutoParentToShip { get; private set; }
        public PlaceableShipObject PlaceableShipObject { get; private set; }

        public TerminalKeyword BuyKeyword { get; internal set; }
        public TerminalNode BuyNode { get; internal set; }
        public TerminalNode BuyConfirmNode { get; internal set; }
        public TerminalNode BuyInfoNode { get; internal set; }

        internal override void Initialize()
        {
            if (Prefab != null)
            {
                AutoParentToShip = Prefab.GetComponent<AutoParentToShip>();
                PlaceableShipObject = Prefab.GetComponentInChildren<PlaceableShipObject>();
            }
        }

        protected override void OnGameIDChanged()
        {
            if (AutoParentToShip != null) AutoParentToShip.unlockableID = GameID;
            if (PlaceableShipObject != null) PlaceableShipObject.unlockableID = GameID;
            if (BuyNode != null) BuyNode.shipUnlockableID = GameID;
            if (BuyConfirmNode != null) BuyConfirmNode.shipUnlockableID = GameID;
        }

        internal static ExtendedUnlockableItem Create(UnlockableItem newUnlockableItem)
        {
            ExtendedUnlockableItem extendedUnlockableItem = ScriptableObject.CreateInstance<ExtendedUnlockableItem>();
            extendedUnlockableItem.UnlockableItem = newUnlockableItem;
            extendedUnlockableItem.name = newUnlockableItem.unlockableName.SkipToLetters().RemoveWhitespace() + "ExtendedUnlockableItem";
            return (extendedUnlockableItem);
        }

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
        internal override List<GameObject> GetNetworkPrefabsForRegistration()
        {
            if (UnlockableItem.unlockableType == 1 && UnlockableItem.prefabObject != null)
                return (new List<GameObject>() { UnlockableItem.prefabObject});
            else
                return (NoNetworkPrefabs);
        }
    }

    public enum UnlockableType
    {
        Invalid,
        Suit,
        Furniture,
        Unknown,
    }
}