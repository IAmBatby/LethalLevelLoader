using LethalModDataLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class LLLSaveFile : ModDataContainer
    {
        public string CurrentLevelName { get; internal set; } = string.Empty;

        public int parityStepsTaken;
        public Dictionary<int, AllItemsListItemData> itemSaveData = new Dictionary<int, AllItemsListItemData>();

        public LLLSaveFile()
        {
            //OptionalPrefixSuffix = name;
        }

        public void Reset()
        {
            CurrentLevelName = string.Empty;
            parityStepsTaken = 0;
            itemSaveData = new Dictionary<int, AllItemsListItemData>();
        }
    }

    public struct AllItemsListItemData
    {
        public string itemObjectName;
        public string itemName;
        public string modName;
        public string modAuthor;
        public int allItemsListIndex;
        public int modItemsListIndex;
        public int itemNameDuplicateIndex;
        public bool isScrap;
        public bool saveItemVariable;

        public AllItemsListItemData(string newItemObjectName, string newItemName, string newModName, string newModAuthor, int newAllItemsListIndex, int newModItemsListIndex, int newItemNameDuplicateIndex, bool newIsScrap, bool newSaveItemVariable)
        {
            itemObjectName = newItemObjectName;
            itemName = newItemName;
            modName = newModName;
            modAuthor = newModAuthor;
            allItemsListIndex = newAllItemsListIndex;
            modItemsListIndex = newModItemsListIndex;
            itemNameDuplicateIndex = newItemNameDuplicateIndex;
            isScrap = newIsScrap;
            saveItemVariable = newSaveItemVariable;
        }
    }
}
