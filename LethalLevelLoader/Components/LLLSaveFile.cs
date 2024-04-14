using LethalModDataLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class LLLSaveFile : ModDataContainer
    {
        public string CurrentLevelName { get; internal set; } = string.Empty;

        public Dictionary<string, string> customItemDictionary = new Dictionary<string, string>();

        public List<string> allItemsList = new List<string>();

        public List<AllItemsListItemData> itemSaveDataList = new List<AllItemsListItemData>();
        public Dictionary<int, AllItemsListItemData> itemSaveData = new Dictionary<int, AllItemsListItemData>();

        public LLLSaveFile(string name)
        {
            OptionalPrefixSuffix = name;
        }
    }

    public struct AllItemsListItemData
    {
        public string itemName;
        public string itemDisplayName;
        public string modName;
        public int allItemsListIndex;

        public AllItemsListItemData(string newItemName, string newItemDisplayName, string newModName, int newAllItemsListIndex)
        {
            itemName = newItemName;
            itemDisplayName = newItemDisplayName;
            modName = newModName;
            allItemsListIndex = newAllItemsListIndex;
        }
    }
}
