using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedUnlockableItem", menuName = "Lethal Level Loader/Extended Content/ExtendedUnlockableItem", order = 21)]
    public class ExtendedUnlockableItem : ExtendedContent<ExtendedUnlockableItem, UnlockableItem, UnlockableItemManager>
    {
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

        public int UnlockableItemID { get; set; } = -1;

        public UnlockableType UnlockableType
        {
            get
            {
                if (UnlockableItemID == 0) return (UnlockableType.Suit);
                else if (UnlockableItemID == 1) return (UnlockableType.Unknown);
                else if (UnlockableItemID < 0) return (UnlockableType.Invalid);
                return (UnlockableType.Unknown);
            }
        }

        public TerminalNode BuyNode { get; internal set; }
        public TerminalNode BuyConfirmNode { get; internal set; }
        public TerminalNode BuyInfoNode { get; internal set; }

        internal override void Initialize()
        {
            TerminalManager.CreateUnlockableItemTerminalData(this);

            if (!Patches.StartOfRound.unlockablesList.unlockables.Contains(UnlockableItem))
                Patches.StartOfRound.unlockablesList.unlockables.Add(UnlockableItem);
        }

        internal static ExtendedUnlockableItem Create(UnlockableItem newUnlockableItem, ExtendedMod extendedMod, ContentType contentType)
        {
            ExtendedUnlockableItem extendedUnlockableItem = ScriptableObject.CreateInstance<ExtendedUnlockableItem>();
            extendedUnlockableItem.UnlockableItem = newUnlockableItem;
            extendedUnlockableItem.name = newUnlockableItem.unlockableName.SkipToLetters().RemoveWhitespace() + "ExtendedUnlockableItem";
            extendedUnlockableItem.ContentType = contentType;
            extendedMod.RegisterExtendedContent(extendedUnlockableItem);

            return (extendedUnlockableItem);
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