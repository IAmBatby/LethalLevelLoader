using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static LethalLevelLoader.SaveManager;

namespace LethalLevelLoader
{
    internal static class SaveManager
    {
        public static LLLSaveFile currentSaveFile;
        public static List<Item> defaultCachedItemsList = new List<Item>(); 

        internal static void InitializeSave()
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;

            if (GameNetworkManager.Instance.currentSaveFileName.Contains("LC"))
                currentSaveFile = new LLLSaveFile(GameNetworkManager.Instance.currentSaveFileName.Replace("LC", "LLL"));
            else
                currentSaveFile = new LLLSaveFile("LLLSaveFile");
            currentSaveFile.Load();
            DebugHelper.Log("Initialized LLL Save File, Current Level Was: " + currentSaveFile.CurrentLevelName + ", Current Vanilla Save Is: " + GameNetworkManager.Instance.currentSaveFileName, DebugType.User);

            if (ES3.KeyExists("CurrentPlanetID", GameNetworkManager.Instance.currentSaveFileName))
                DebugHelper.Log("Vanilla CurrentSaveFileName Has Saved Current Planet ID: " + ES3.Load<int>("CurrentPlanetID", GameNetworkManager.Instance.currentSaveFileName), DebugType.Developer);
            else
            {
                currentSaveFile.customItemDictionary = new Dictionary<string, string>();
                currentSaveFile.allItemsList = new List<string>();
                currentSaveFile.itemSaveDataList = new List<AllItemsListItemData>();
                currentSaveFile.itemSaveData = new Dictionary<int, AllItemsListItemData>();
                currentSaveFile.CurrentLevelName = string.Empty;
            }

            /*foreach (string item in currentSaveFile.allItemsList)
                DebugHelper.Log("Saved AllItemsList: " + item);

            foreach (KeyValuePair<string, string> extendedItemSaveInfo in currentSaveFile.customItemDictionary)
                DebugHelper.Log("Save Item Info: " + extendedItemSaveInfo.Key + " from " + extendedItemSaveInfo.Value);*/

            foreach (AllItemsListItemData itemSaveData in currentSaveFile.itemSaveDataList)
                DebugHelper.Log("Item Save Data: " + itemSaveData.itemName + ", " + itemSaveData.itemDisplayName + ", " + itemSaveData.modName + ", " + itemSaveData.allItemsListIndex, DebugType.Developer);


            //ValidateSaveData();
        }

        internal static ProcessedData ValidateSaveData()
        {
            List<int> savedAllItemsListIndices = null;
            if (ES3.KeyExists("shipGrabbableItemIDs", GameNetworkManager.Instance.currentSaveFileName))
                savedAllItemsListIndices = ES3.Load<int[]>("shipGrabbableItemIDs", GameNetworkManager.Instance.currentSaveFileName).ToList();

            List<AllItemsListItemData> savedItems = new List<AllItemsListItemData>();
            List<AllItemsListItemData> liveItems = GetItemSaveData();

            List<int> validIndicies = new List<int>();
            Dictionary<int,int> recoveredIndicies = new Dictionary<int,int>();
            List<int> invalidIndicies = new List<int>();

            if (savedAllItemsListIndices != null)
                foreach (int itemIndex in savedAllItemsListIndices)
                    savedItems.Add(currentSaveFile.itemSaveData[itemIndex]);

            foreach (AllItemsListItemData savedItem in savedItems)
            {
                if (savedItem.allItemsListIndex < liveItems.Count && savedItem.allItemsListIndex > -1 && liveItems[savedItem.allItemsListIndex].itemName == savedItem.itemName)
                    validIndicies.Add(savedItem.allItemsListIndex);
                else
                {
                    int recoveredItemIndex = -1;
                    foreach (AllItemsListItemData liveItem in liveItems)
                    {
                        int compareCount = 0;
                        if (savedItem.itemName == liveItem.itemName)
                            compareCount++;
                        if (savedItem.itemDisplayName == liveItem.itemDisplayName)
                            compareCount++;
                        if (savedItem.modName == liveItem.modName)
                            compareCount++;
                        if (compareCount >= 2)
                            recoveredItemIndex = liveItems.IndexOf(liveItem);
                    }
                    if (recoveredItemIndex != -1)
                        recoveredIndicies.Add(savedItem.allItemsListIndex, recoveredItemIndex);
                    else
                        invalidIndicies.Add(savedItem.allItemsListIndex);
                }
            }

            return (new ProcessedData(validIndicies, recoveredIndicies, invalidIndicies));
        }

        internal static List<SavedShipItemData> ProcessSavedItemIndicies(ProcessedData processedData)
        {
            return (ProcessSavedItemIndicies(processedData.validIndicies, processedData.recoveredIndicies, processedData.invalidIndicies));
        }

        internal static List<SavedShipItemData> ProcessSavedItemIndicies(List<int> validIndices, Dictionary<int, int> recoveredIndicies, List<int> invalidIndicies)
        {
            AllItemsList patchedItemsList = Patches.StartOfRound.allItemsList;
            List<SavedShipItemData> validatedSavedShipItemDataList = new List<SavedShipItemData>();
            Dictionary<int, SavedShipItemData> constructedSavedShipItemDataDict = GetConstructedSavedShipItemData();

            DebugHelper.Log("Processed Saved Items In: " + GameNetworkManager.Instance.currentSaveFileName, DebugType.User);
            if (validIndices.Count > 0)
            {
                string validItemsLog = "Valid Saved Items: ";
                foreach (int validIndex in validIndices)
                    validItemsLog += patchedItemsList.itemsList[validIndex].itemName + ", ";
                DebugHelper.Log(validItemsLog, DebugType.User);
            }
            if (recoveredIndicies.Count > 0)
            {
                string recoveredItemsLog = "Recovered Saved Items: ";
                foreach (KeyValuePair<int, int> recoveredIndexPair in recoveredIndicies)
                    recoveredItemsLog += patchedItemsList.itemsList[recoveredIndexPair.Value].itemName + ", ";
                DebugHelper.LogWarning(recoveredItemsLog, DebugType.User);
            }
            if (invalidIndicies.Count > 0)
            {
                string invalidItemsLog = "Corrupted Saved Items: ";
                foreach (int invalidIndex in invalidIndicies)
                   invalidItemsLog += "Invalid ID: (" + invalidIndex + ")" + ", ";
                DebugHelper.LogError(invalidItemsLog, DebugType.User);
            }

            foreach (SavedShipItemData savedShipItemData in constructedSavedShipItemDataDict.Values)
                DebugHelper.Log("Constructed SavedShipItemData: " + savedShipItemData.itemAllItemsListIndex + " | " + savedShipItemData.itemPosition + " | " + savedShipItemData.itemScrapValue + " | " + savedShipItemData.itemAdditionalSavedData, DebugType.User);


            foreach (int validIndex in validIndices)
                validatedSavedShipItemDataList.Add(constructedSavedShipItemDataDict[validIndex]);

            foreach (KeyValuePair<int, int> recoveredIndex in  recoveredIndicies)
            {
                constructedSavedShipItemDataDict[recoveredIndex.Key].itemAllItemsListIndex = recoveredIndex.Value;
                validatedSavedShipItemDataList.Add(constructedSavedShipItemDataDict[recoveredIndex.Key]);
            }

            validatedSavedShipItemDataList = validatedSavedShipItemDataList.OrderBy(s => s.itemAllItemsListIndex).ToList();

            foreach (SavedShipItemData savedShipItemData in validatedSavedShipItemDataList)
                DebugHelper.Log("Validated SavedShipItemData: " + savedShipItemData.itemAllItemsListIndex + " | " + savedShipItemData.itemPosition + " | " + savedShipItemData.itemScrapValue + " | " + savedShipItemData.itemAdditionalSavedData, DebugType.User);

            //OverrideCurrentSaveFileItemData(validatedSavedShipItemDataList);
            return (validatedSavedShipItemDataList);
        }

        internal static void RefreshSaveItemInfo()
        {
            /*
            currentSaveFile.customItemDictionary = new Dictionary<string, string>();

            currentSaveFile.allItemsList = new List<string>();
            foreach (Item item in Patches.StartOfRound.allItemsList.itemsList)
                currentSaveFile.allItemsList.Add(item.name);

            currentSaveFile.itemSaveDataList = GetItemSaveData();
            currentSaveFile.itemSaveData.Clear();
            foreach (AllItemsListItemData itemSaveData in GetItemSaveData())
                if (!currentSaveFile.itemSaveData.ContainsKey(itemSaveData.allItemsListIndex))
                    currentSaveFile.itemSaveData.Add(itemSaveData.allItemsListIndex, itemSaveData);

            currentSaveFile.customItemDictionary.Clear();
            foreach (ExtendedItem extendedItem in PatchedContent.ExtendedItems)
                if (!currentSaveFile.customItemDictionary.ContainsKey(extendedItem.ModName + "_" + extendedItem.name))
                    currentSaveFile.customItemDictionary.Add(extendedItem.ModName + "_" + extendedItem.name, extendedItem.ModName);


            currentSaveFile.Save();*/
        }

        internal static void SaveCurrentSelectableLevel(SelectableLevel selectableLevel)
        {
            if (LethalLevelLoaderNetworkManager.networkManager.IsServer == false)
                return;
            currentSaveFile.CurrentLevelName = selectableLevel.name;
            currentSaveFile.Save();
            
        }

        internal static List<AllItemsListItemData> GetItemSaveData()
        {
            List<AllItemsListItemData > items = new List<AllItemsListItemData>();
            List<Item> itemsList = new List<Item>();
            int counter = 0;
            foreach (Item item in Patches.StartOfRound.allItemsList.itemsList)
            {
                foreach (ExtendedItem extendedItem in PatchedContent.ExtendedItems)
                    if (extendedItem.Item == item && !itemsList.Contains(item))
                    {
                        itemsList.Add(item);
                        items.Add(new AllItemsListItemData(item.name, item.itemName, extendedItem.ModName, counter));
                        break;
                    }
                counter++;
            }

            return (items);
        }

        internal static void LoadShipGrabbableItems()
        {
            List<int> shipGrabbableItemIDs = new List<int>();
            List<Vector3> shipGrabbableItemPos = new List<Vector3>();
            List<int> shipScrapValues = new List<int>();
            List<int> shipItemSaveData = new List<int>();

            foreach (SavedShipItemData savedShipItemData in ProcessSavedItemIndicies(ValidateSaveData()))
            {
                shipGrabbableItemIDs.Add(savedShipItemData.itemAllItemsListIndex);
                if (savedShipItemData.itemPosition != new Vector3(-1, -1, -1))
                    shipGrabbableItemPos.Add(savedShipItemData.itemPosition);
                if (savedShipItemData.itemScrapValue != -1)
                    shipScrapValues.Add(savedShipItemData.itemScrapValue);
                if (savedShipItemData.itemAdditionalSavedData != -1)
                    shipItemSaveData.Add(savedShipItemData.itemAdditionalSavedData);
            }
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
                if (savedShipItemData.itemPosition != new Vector3(-1,-1,-1))
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

        internal static Dictionary<int, SavedShipItemData> GetConstructedSavedShipItemData()
        {
            Dictionary<int, SavedShipItemData> returnDict = new Dictionary<int, SavedShipItemData>();

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

            for (int i = 0; i < shipGrabbableItemIDs.Count; i++)
            {
                int newGrabbableItemID = shipGrabbableItemIDs[i];
                Vector3 newGrabbableItemPos = new Vector3(-1, -1, -1);
                int newShipScrapValue = -1;
                int newShipItemSaveData = -1;

                if (shipGrabbableItemPos.Count > i)
                    newGrabbableItemPos = shipGrabbableItemPos[i];
                if (shipScrapValues.Count > i)
                    newShipScrapValue = shipScrapValues[i];
                if (shipItemSaveData.Count > i)
                    newShipItemSaveData = shipItemSaveData[i];

                returnDict.Add(newGrabbableItemID, new SavedShipItemData(newGrabbableItemID, newGrabbableItemPos, newShipScrapValue, newShipItemSaveData));
            }

            if (returnDict.Count == 0)
                DebugHelper.LogWarning("GetConstructedSavedShipItemData() Returning Empty Dict!", DebugType.User);

            return (returnDict);
        }
    }

    public class SavedShipItemData
    {
        public int itemAllItemsListIndex;
        public Vector3 itemPosition;
        public int itemScrapValue;
        public int itemAdditionalSavedData;

        public SavedShipItemData(int newItemAllItemsListIndex, Vector3 newItemPosition, int newItemScrapValue, int newItemAdditionalSavedData)
        {
            itemAllItemsListIndex = newItemAllItemsListIndex;
            itemPosition = newItemPosition;
            itemScrapValue = newItemScrapValue;
            itemAdditionalSavedData = newItemAdditionalSavedData;
        }
    }

        public struct ProcessedData
        {
            public List<int> validIndicies;
            public Dictionary<int, int> recoveredIndicies;
            public List<int> invalidIndicies;

            public ProcessedData(List<int> newValidIndicies, Dictionary<int,int> newRecoveredIndicies,  List<int> newInvalidIndicies)
            {
                validIndicies = newValidIndicies;
                recoveredIndicies = newRecoveredIndicies;
                invalidIndicies = newInvalidIndicies;
            }
        }
}
