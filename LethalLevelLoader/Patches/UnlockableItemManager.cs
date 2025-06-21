using System.Linq;

namespace LethalLevelLoader
{
    public static class UnlockableItemManager
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
                if (extendedUnlockableItem.UnlockableItem.unlockableType == 1)
                {
                    if (extendedUnlockableItem.UnlockableItem.prefabObject == null && extendedUnlockableItem.UnlockableItem.alreadyUnlocked)
                    {
                        continue;
                    }

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
        }
    }
}