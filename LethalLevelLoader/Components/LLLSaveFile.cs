using LethalModDataLib.Base;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public class LLLSaveFile : ModDataContainer
    {
        public string CurrentLevelName { get; internal set; } = string.Empty;

        public int parityStepsTaken;
        public Dictionary<int, AllItemsListItemData> itemSaveData = new Dictionary<int, AllItemsListItemData>();
        public List<ExtendedLevelData> extendedLevelSaveData = new List<ExtendedLevelData>();

        public LLLSaveFile()
        {
            //OptionalPrefixSuffix = name;
        }

        public void Reset()
        {
            CurrentLevelName = string.Empty;
            parityStepsTaken = 0;
            itemSaveData = new Dictionary<int, AllItemsListItemData>();
            extendedLevelSaveData = new List<ExtendedLevelData>();
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

    public struct ExtendedLevelData : INetworkSerializable
    {
        public string UniqueIdentifier => uniqueIdentifier;
        public string uniqueIdentifier = string.Empty;
        public bool isHidden;
        public bool isLocked;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref uniqueIdentifier);
            serializer.SerializeValue(ref isHidden);
            serializer.SerializeValue(ref isLocked);
        }

        public ExtendedLevelData(ExtendedLevel extendedLevel)
        {
            uniqueIdentifier = extendedLevel.UniqueIdentificationName;
            isHidden = extendedLevel.IsRouteHidden;
            isLocked = extendedLevel.IsRouteLocked;
        }

        public void ApplySavedValues(ExtendedLevel extendedLevel)
        {
            extendedLevel.IsRouteHidden = isHidden;
            extendedLevel.IsRouteLocked = isLocked;
        }
    }
}
