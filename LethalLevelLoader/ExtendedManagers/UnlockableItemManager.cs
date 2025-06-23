using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class UnlockableItemManager : ExtendedContentManager<ExtendedUnlockableItem, UnlockableItem, UnlockableItemManager>
    {
        internal static void PatchVanillaUnlockableItemLists()
        {
            Patches.StartOfRound.unlockablesList.unlockables = [.. PatchedContent.ExtendedUnlockableItems.Select(u => u.UnlockableItem)];
        }

        internal static void SetUnlockableItemIDs()
        {
            int unlockableID = 0;
            foreach (ExtendedUnlockableItem vanillaUnlockableItem in PatchedContent.VanillaExtendedUnlockableItems)
                vanillaUnlockableItem.UnlockableItemID = unlockableID++;

            foreach (ExtendedUnlockableItem customUnlockableItem in PatchedContent.CustomExtendedUnlockableItems)
                customUnlockableItem.UnlockableItemID = unlockableID++;

            foreach (ExtendedUnlockableItem extendedUnlockableItem in PatchedContent.ExtendedUnlockableItems)
            {
                if (extendedUnlockableItem.UnlockableItemID != 1) continue;
                if (extendedUnlockableItem.UnlockableItem.prefabObject == null) continue;
                if (extendedUnlockableItem.UnlockableItem.alreadyUnlocked) continue;

                AutoParentToShip autoParentToShip = extendedUnlockableItem.UnlockableItem.prefabObject.GetComponent<AutoParentToShip>();
                autoParentToShip.unlockableID = extendedUnlockableItem.UnlockableItemID;

                PlaceableShipObject placeableShipObject = extendedUnlockableItem.UnlockableItem.prefabObject.GetComponentInChildren<PlaceableShipObject>();
                if (placeableShipObject != null)
                {
                    placeableShipObject.parentObject = autoParentToShip;
                    placeableShipObject.unlockableID = extendedUnlockableItem.UnlockableItemID;
                }
            }
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedUnlockableItem extendedUnlockableItem)
        {
            if (extendedUnlockableItem.UnlockableItem.unlockableType == 1 && !extendedUnlockableItem.UnlockableItem.alreadyUnlocked)
            {
                if (extendedUnlockableItem.UnlockableItem.prefabObject == null)
                    return (false, "Unlockable Item Prefab Was Null Or Empty");
                else if (!extendedUnlockableItem.UnlockableItem.prefabObject.TryGetComponent(out NetworkObject _))
                    return (false, "Unlockable Item Prefab Is Missing NetworkObject Component");
                else if (!extendedUnlockableItem.UnlockableItem.prefabObject.TryGetComponent(out AutoParentToShip _))
                    return (false, "Unlockable Item Prefab Is Missing AutoParentToShip Component");
            }
            else if (extendedUnlockableItem.UnlockableItem.unlockableType == 0 && extendedUnlockableItem.UnlockableItem.suitMaterial == null)
                return (false, "Unlockable Suit Is Missing Suit Material");

            return (true, string.Empty);
        }
    }
}