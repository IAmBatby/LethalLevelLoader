using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;

namespace LethalLevelLoader
{
    internal static class SaveManager
    {
        public static LLLSaveFile currentSaveFile;
        public static bool parityCheck;

        internal static void InitializeSave()
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;

            currentSaveFile = new LLLSaveFile();
            currentSaveFile.Load();

            if (currentSaveFile.CurrentLevelName != null)
                DebugHelper.Log("Initialized LLL Save File, Current Level Was: " + currentSaveFile.CurrentLevelName + ", Current Vanilla Save Is: " + GameNetworkManager.Instance.currentSaveFileName, DebugType.User);
            else
                DebugHelper.Log("Initialized LLL Save File, Current Level Was: (Empty) " + ", Current Vanilla Save Is: " + GameNetworkManager.Instance.currentSaveFileName, DebugType.User);

            if (ES3.KeyExists("CurrentPlanetID", GameNetworkManager.Instance.currentSaveFileName))
                DebugHelper.Log("Vanilla CurrentSaveFileName Has Saved Current Planet ID: " + ES3.Load<int>("CurrentPlanetID", GameNetworkManager.Instance.currentSaveFileName), DebugType.Developer);

            // Compare saved "Steps Taken" statistic, to try to check whether the Vanilla and LethalLevelLoader saves are the same
            int originalStepsTaken = ES3.Load<int>("Stats_StepsTaken", GameNetworkManager.Instance.currentSaveFileName, 0);

            if (originalStepsTaken == currentSaveFile.parityStepsTaken)
                parityCheck = true;
            else
            {
                DebugHelper.Log("Vanilla Save File Mismatch, LLL Steps Taken: " + currentSaveFile.parityStepsTaken + ", Vanilla Steps Taken: " + originalStepsTaken, DebugType.Developer);

                currentSaveFile.Reset();
                currentSaveFile.parityStepsTaken = originalStepsTaken;

                parityCheck = false;
            }

            if (currentSaveFile.extendedLevelSaveData != null)
            {
                foreach (ExtendedLevelData extendedLevelData in currentSaveFile.extendedLevelSaveData)
                    LethalLevelLoaderNetworkManager.Instance.SetExtendedLevelValuesServerRpc(extendedLevelData);
            }
        }

        internal static void SaveGameValues()
        {
            currentSaveFile.itemSaveData = GetAllItemsListItemDataDict();
            currentSaveFile.parityStepsTaken = Patches.StartOfRound.gameStats.allStepsTaken;
            SaveAllLevels();
            currentSaveFile.Save();
        }

        internal static void SaveCurrentSelectableLevel(SelectableLevel selectableLevel)
        {
            /*
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;
            currentSaveFile.CurrentLevelName = selectableLevel.name;
            currentSaveFile.Save();
            */
        }

        internal static void SaveAllLevels()
        {
            currentSaveFile.extendedLevelSaveData.Clear();
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
            {
                DebugHelper.Log("Saving Level: " + extendedLevel.UniqueIdentificationName, DebugType.User);
                currentSaveFile.extendedLevelSaveData.Add(new ExtendedLevelData(extendedLevel));
            }

            DebugHelper.Log("Saved The Following", DebugType.User);
            foreach (ExtendedLevelData levelData in currentSaveFile.extendedLevelSaveData)
                DebugHelper.Log(levelData.UniqueIdentifier, DebugType.User);
        }

        internal static void LoadShipGrabbableItems()
        {
            if (!parityCheck)
                return;

            // TODO: Config option to disable this process preferably

            List<SavedShipItemData> loadedShipItemData = GetConstructedSavedShipItemData(currentSaveFile.itemSaveData);
            FixMismatchedSavedItemData(loadedShipItemData);
            OverrideCurrentSaveFileItemData(loadedShipItemData);
        }

        internal static void OverrideCurrentSaveFileItemData(List<SavedShipItemData> savedShipItemDatas)
        {
            List<int> shipGrabbableItemIDs = new List<int>();
            List<Vector3> shipGrabbableItemPos = new List<Vector3>();
            List<int> shipScrapValues = new List<int>();
            List<int> shipItemSaveData = new List<int>();

            string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;

            foreach (SavedShipItemData savedShipItemData in savedShipItemDatas)
            {
                shipGrabbableItemIDs.Add(savedShipItemData.itemAllItemsListIndex);
                shipGrabbableItemPos.Add(savedShipItemData.itemPosition);
                if (savedShipItemData.itemScrapValue != -1)
                    shipScrapValues.Add(savedShipItemData.itemScrapValue);
                if (savedShipItemData.itemAdditionalSavedData != -1)
                    shipItemSaveData.Add(savedShipItemData.itemAdditionalSavedData);
            }

            if (ES3.KeyExists("shipGrabbableItemIDs", currentSaveFileName))
                ES3.DeleteKey("shipGrabbableItemIDs", currentSaveFileName);
            if (ES3.KeyExists("shipGrabbableItemPos", currentSaveFileName))
                ES3.DeleteKey("shipGrabbableItemPos", currentSaveFileName);
            if (ES3.KeyExists("shipScrapValues", currentSaveFileName))
                ES3.DeleteKey("shipScrapValues", currentSaveFileName);
            if (ES3.KeyExists("shipItemSaveData", currentSaveFileName))
                ES3.DeleteKey("shipItemSaveData", currentSaveFileName);

            if (shipGrabbableItemIDs.Count > 0)
                ES3.Save<int[]>("shipGrabbableItemIDs", shipGrabbableItemIDs.ToArray(), currentSaveFileName);
            if (shipGrabbableItemPos.Count > 0)
                ES3.Save<Vector3[]>("shipGrabbableItemPos", shipGrabbableItemPos.ToArray(), currentSaveFileName);
            if (shipScrapValues.Count > 0)
                ES3.Save<int[]>("shipScrapValues", shipScrapValues.ToArray(), currentSaveFileName);
            if (shipItemSaveData.Count > 0)
                ES3.Save<int[]>("shipItemSaveData", shipItemSaveData.ToArray(), currentSaveFileName);
        }

        internal static void FixMismatchedSavedItemData(List<SavedShipItemData> savedShipItemDatas)
        {
            Dictionary<int, AllItemsListItemData> itemDataDict = GetAllItemsListItemDataDict();

            int firstMismatch = 0;

            foreach (SavedShipItemData savedShipItemData in savedShipItemDatas)
            {
                int allitemsListIndex = savedShipItemData.itemAllItemsListIndex;
                if (!itemDataDict.ContainsKey(savedShipItemData.itemAllItemsListIndex))
                    break;

                AllItemsListItemData itemData = savedShipItemData.itemAllItemsListData;
                AllItemsListItemData newItemData = itemDataDict[allitemsListIndex];

                if (newItemData.itemName != itemData.itemName)
                    break;

                firstMismatch++;
            }

            if (firstMismatch >= savedShipItemDatas.Count)
                return;

            for (int i = firstMismatch; i < savedShipItemDatas.Count; i++)
            {
                SavedShipItemData savedShipItemData = savedShipItemDatas[i];
                int oldIndex = savedShipItemData.itemAllItemsListIndex;

                int newIndex = FixAllItemsListIndex(savedShipItemData.itemAllItemsListData, itemDataDict);
                savedShipItemData.itemAllItemsListIndex = newIndex;

                if (itemDataDict.ContainsKey(newIndex))
                {
                    AllItemsListItemData newItemData = itemDataDict[newIndex];

                    if (oldIndex != newIndex)
                    {
                        DebugHelper.Log($"Fixing Item ┌ {savedShipItemData.itemAllItemsListData.modName} ┬ {savedShipItemData.itemAllItemsListData.itemName} ┬ {savedShipItemData.itemAllItemsListData.itemObjectName} ┬ #{oldIndex}", DebugType.User);
                        DebugHelper.Log($"     -----> └ {newItemData.modName                           } ┴ {newItemData.itemName                           } ┴ {newItemData.itemObjectName                           } ┴ #{newIndex}", DebugType.User);
                    }

                    savedShipItemData.itemAllItemsListData = newItemData;

                    if (!newItemData.isScrap && savedShipItemData.itemScrapValue >= 0)
                        savedShipItemData.itemScrapValue = -1;
                    else if (newItemData.isScrap && savedShipItemData.itemScrapValue == -1)
                        savedShipItemData.itemScrapValue = 0; // Could generate a fitting scrap value here if desired

                    if (!newItemData.saveItemVariable && savedShipItemData.itemAdditionalSavedData >= 0)
                        savedShipItemData.itemAdditionalSavedData = -1;
                    else if (newItemData.saveItemVariable && savedShipItemData.itemAdditionalSavedData == -1)
                        savedShipItemData.itemAdditionalSavedData = 0; // Might cause problems with some items but what are you gonna do
                }
                else
                {
                    DebugHelper.Log($"Removing Item: [ {savedShipItemData.itemAllItemsListData.modName} ][ {savedShipItemData.itemAllItemsListData.itemName} ][ {savedShipItemData.itemAllItemsListData.itemObjectName} ][ #{oldIndex}", DebugType.User);

                    savedShipItemData.itemScrapValue = -1;
                    savedShipItemData.itemAdditionalSavedData = -1;
                }
            }
        }

        internal static int FixAllItemsListIndex(AllItemsListItemData itemData, Dictionary<int, AllItemsListItemData> itemDataDict)
        {
            // Priorities (higher number = higher priority)

            // Variable definitions:
            //  itemObjectName         : The name of the item's asset file.
            //  itemName               : The name of the item (field set in the item's ScriptableObject).
            //  modName                : The name of the mod the item is from.
            //  allItemsListIndex      : The items list index of this item.
            //  modItemsListIndex      : The items list index of this item relative to the modName.
            //  itemNameDuplicateIndex : The duplicate index of the item's itemName relative to the modName, e.g. if a mod has two items named "Item A" this will be 1 for the second.

            // Formulas (matching variables):
            //  64 -> modAuthor                          (Only if priority is >= 4)
            //  32 -> modName                            (Only if priority is >= 4)
            //  16 -> itemName & itemNameDuplicateIndex  (Exclusive with below)
            //   8 -> itemName                           (Exclusive with above)
            //   4 -> itemObjectName                     (Minimum to not be removed)
            //   2 -> modItemsListIndex & modName        (Only if priority is < 4)
            //   1 -> allItemsListIndex                  (Only if priority is < 4)

            // Possible values:
            // 116 -> modAuthor & modName & itemName & itemNameDuplicateIndex & itemObjectName
            // 112 -> modAuthor & modName & itemName & itemNameDuplicateIndex
            // 108 -> modAuthor & modName & itemName & itemObjectName
            // 104 -> modAuthor & modName & itemName
            // 100 -> modAuthor & modName & itemObjectName
            //  84 -> modAuthor & itemName & itemNameDuplicateIndex & itemObjectName
            //  80 -> modAuthor & itemName & itemNameDuplicateIndex
            //  76 -> modAuthor & itemName & itemObjectName
            //  72 -> modAuthor & itemName
            //  68 -> modAuthor & itemObjectName
            //  52 -> modName & itemName & itemNameDuplicateIndex & itemObjectName
            //  48 -> modName & itemName & itemNameDuplicateIndex
            //  44 -> modName & itemName & itemObjectName 
            //  40 -> modName & itemName
            //  36 -> modName & itemObjectName
            //  20 -> itemName & itemNameDuplicateIndex & itemObjectName
            //  16 -> itemName & itemNameDuplicateIndex
            //  12 -> itemName & itemObjectName
            //   8 -> itemName
            //   4 -> itemObjectName
            // Low values (removed by default):
            //   3 -> modItemsListIndex & modName & allitemsListIndex
            //   2 -> modItemsListIndex & modName
            //   1 -> allItemsListIndex

            List<Item> allItemsList = Patches.StartOfRound.allItemsList.itemsList;

            int matchedPriority = 0;
            int matchedIndex = -1;

            for (int newIndex = 0; newIndex < allItemsList.Count; newIndex++)
            {
                if (!itemDataDict.ContainsKey(newIndex))
                    break;

                int currentPriority = 0;
                AllItemsListItemData newItemData = itemDataDict[newIndex];

                if (newItemData.itemName == itemData.itemName)
                    if (newItemData.itemNameDuplicateIndex == itemData.itemNameDuplicateIndex)
                        currentPriority += 16;
                    else
                        currentPriority += 8;

                if (newItemData.itemObjectName == itemData.itemObjectName)
                    currentPriority += 4;

                if (currentPriority >= 4)
                {
                    if (newItemData.modAuthor == itemData.modAuthor)
                        currentPriority += 64;

                    if (CompareModNames(newItemData.modName, itemData.modName))
                        currentPriority += 32;
                }
                else
                {
                    if (newItemData.modItemsListIndex == itemData.modItemsListIndex && CompareModNames(newItemData.modName, itemData.modName))
                        currentPriority += 2;

                    if (newItemData.allItemsListIndex == itemData.allItemsListIndex)
                        currentPriority += 1;
                }

                if (currentPriority > matchedPriority)
                {
                    matchedPriority = currentPriority;
                    matchedIndex = newIndex;

                    if (matchedPriority == 64 + 32 + 16 + 4) // Max value
                        return matchedIndex;
                }
            }

            // TODO: Config option to disable removing items that aren't name matched?  
            if (matchedPriority >= 4)
                return matchedIndex;
            else
                return int.MaxValue; // Use MaxValue since vanilla loading code checks upper bounds
        }

        internal static Dictionary<int, AllItemsListItemData> GetAllItemsListItemDataDict()
        {
            Dictionary<int, AllItemsListItemData> items = new Dictionary<int, AllItemsListItemData>();
            int counter = 0;
            foreach (Item item in Patches.StartOfRound.allItemsList.itemsList)
            {
                TryGetExtendedItemInfo(item, out string modName, out string modAuthor, out int modItemIndex);
                int itemNameDuplicateIndex = GetItemNameDuplicateIndex(item, modName);

                items.Add(counter, new AllItemsListItemData(item.name, item.itemName, modName, modAuthor, counter, modItemIndex, itemNameDuplicateIndex, item.isScrap, item.saveItemVariable));
                counter++;
            }

            return (items);
        }

        internal static bool TryGetExtendedItemInfo(Item item, out string modName, out string modAuthor, out int modItemIndex)
        {
            int lowestNameAliases = int.MaxValue;
            modName = "";
            modAuthor = "";
            modItemIndex = -1;

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
            {
                if (lowestNameAliases <= extendedMod.ModNameAliases.Count)
                    continue;

                int modCounter = 0;

                foreach (ExtendedItem extendedItem in extendedMod.ExtendedItems)
                {
                    if (extendedItem.Item == item)
                    {
                        modName = string.Join(';', extendedMod.ModNameAliases);
                        modAuthor = extendedMod.AuthorName;
                        modItemIndex = modCounter;

                        lowestNameAliases = extendedMod.ModNameAliases.Count;
                        break;
                    }
                    modCounter++;
                }
            }

            return modItemIndex > -1;
        }

        internal static int GetItemNameDuplicateIndex(Item item, string modName)
        {
            if (modName != "")
            {
                foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                    if (CompareModNames(extendedMod.ModName, modName))
                    {
                        int modCounter = 0;

                        foreach (ExtendedItem extendedItem in extendedMod.ExtendedItems)
                            if (extendedItem.Item == item)
                                break;
                            else if (extendedItem.Item.itemName == item.itemName)
                                modCounter++;

                        return modCounter;
                    }

                return 0;
            }
            else
            {
                int counter = 0;

                foreach (Item newItem in Patches.StartOfRound.allItemsList.itemsList)
                    if (newItem == item)
                        break;
                    else if (newItem.itemName == item.itemName && !TryGetExtendedItemInfo(item, out _, out _, out _))
                        counter++;

                return counter;
            }
        }

        internal static bool CompareModNames(string modNameA, string modNameB)
        {
            var modNamesA = modNameA.Split(';');
            var modNamesB = modNameB.Split(';');

            return modNamesA.Intersect(modNamesB).Any();
        }

        internal static List<AllItemsListItemData> GetAllItemsListItemDatas(List<int> itemIDs, Dictionary<int, AllItemsListItemData> itemDataDict)
        {
            List<AllItemsListItemData> result = new List<AllItemsListItemData>();

            foreach (int id in itemIDs)
            {
                if (itemDataDict.ContainsKey(id))
                    result.Add(itemDataDict[id]);
                else
                    // Don't know this item somehow? Add empty junk
                    result.Add(new AllItemsListItemData("", "", "", "", id, -1, 0, false, false));
            }

            return (result);
        }

        internal static List<SavedShipItemData> GetConstructedSavedShipItemData(Dictionary<int, AllItemsListItemData> itemDataDict)
        {
            List<SavedShipItemData> result = new List<SavedShipItemData>();

            string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;

            List<int> shipGrabbableItemIDs = null;
            List<Vector3> shipGrabbableItemPos = null;
            List<int> shipScrapValues = null;
            List<int> shipItemSaveData = null;

            if (ES3.KeyExists("shipGrabbableItemIDs", currentSaveFileName))
                shipGrabbableItemIDs = ES3.Load<int[]>("shipGrabbableItemIDs", currentSaveFileName).ToList();
            else
                shipGrabbableItemIDs = new List<int>();

            if (ES3.KeyExists("shipGrabbableItemPos", currentSaveFileName))
                shipGrabbableItemPos = ES3.Load<Vector3[]>("shipGrabbableItemPos", currentSaveFileName).ToList();
            else
                shipGrabbableItemPos = new List<Vector3>();

            if (ES3.KeyExists("shipScrapValues", currentSaveFileName))
                shipScrapValues = ES3.Load<int[]>("shipScrapValues", currentSaveFileName).ToList();
            else
                shipScrapValues = new List<int>();

            if (ES3.KeyExists("shipItemSaveData", currentSaveFileName))
                shipItemSaveData = ES3.Load<int[]>("shipItemSaveData", currentSaveFileName).ToList();
            else
                shipItemSaveData = new List<int>();

            List<AllItemsListItemData> shipGrabbableItemData = GetAllItemsListItemDatas(shipGrabbableItemIDs, itemDataDict);

            int scrapValueIndex = 0;
            int saveDataIndex = 0;

            for (int i = 0; i < shipGrabbableItemIDs.Count; i++)
            {
                int newGrabbableItemID = shipGrabbableItemIDs[i];
                Vector3 newGrabbableItemPos = Vector3.zero;
                int newShipScrapValue = -1;
                int newShipItemSaveData = -1;
                AllItemsListItemData newGrabbableItemData = shipGrabbableItemData[i];

                if (shipGrabbableItemPos.Count > i)
                    newGrabbableItemPos = shipGrabbableItemPos[i];

                if (newGrabbableItemData.isScrap)
                {
                    if (shipScrapValues.Count > scrapValueIndex)
                        newShipScrapValue = shipScrapValues[scrapValueIndex];
                    else
                        newShipScrapValue = 0;

                    scrapValueIndex++;
                }

                if (newGrabbableItemData.saveItemVariable)
                {
                    if (shipItemSaveData.Count > saveDataIndex)
                        newShipItemSaveData = shipItemSaveData[saveDataIndex];
                    else
                        newShipItemSaveData = 0;

                    saveDataIndex++;
                }

                result.Add(new SavedShipItemData(newGrabbableItemID, newGrabbableItemPos, newShipScrapValue, newShipItemSaveData, newGrabbableItemData));
            }

            return (result);
        }
    }

    public class SavedShipItemData
    {
        public int itemAllItemsListIndex;
        public Vector3 itemPosition;
        public int itemScrapValue;
        public int itemAdditionalSavedData;
        public AllItemsListItemData itemAllItemsListData;

        public SavedShipItemData(int newItemAllItemsListIndex, Vector3 newItemPosition, int newItemScrapValue, int newItemAdditionalSavedData, AllItemsListItemData newItemAllItemsListData)
        {
            itemAllItemsListIndex = newItemAllItemsListIndex;
            itemPosition = newItemPosition;
            itemScrapValue = newItemScrapValue;
            itemAdditionalSavedData = newItemAdditionalSavedData;
            itemAllItemsListData = newItemAllItemsListData;
        }
    }
}
