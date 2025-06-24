using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public class ItemManager : ExtendedContentManager<ExtendedItem, Item, ItemManager>
    {
        public static void RefreshDynamicItemRarityOnAllExtendedLevels()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                InjectCustomItemsIntoLevelViaDynamicRarity(extendedLevel);
        }
        public static void InjectCustomItemsIntoLevelViaDynamicRarity(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            foreach (ExtendedItem extendedItem in PatchedContent.CustomExtendedItems.Where(i => i.Item.isScrap))
            {
                string debugString = string.Empty;
                int returnRarity = extendedItem.LevelMatchingProperties.GetDynamicRarity(extendedLevel);
                SpawnableItemWithRarity alreadyInjectedItem = extendedLevel.SelectableLevel.spawnableScrap.Where(s => s.spawnableItem == extendedItem).FirstOrDefault();

                if (alreadyInjectedItem != null)
                {
                    if (returnRarity > 0)
                    {
                        alreadyInjectedItem.rarity = returnRarity;
                        debugString = "Updated Rarity Of: " + extendedItem.Item.itemName + " To: " + returnRarity + " On Planet: " + extendedLevel.NumberlessPlanetName;
                    }
                    else
                    {
                        extendedLevel.SelectableLevel.spawnableScrap.Remove(alreadyInjectedItem);
                        debugString = "Removed " + extendedItem.Item.itemName + " From Planet: " + extendedLevel.NumberlessPlanetName;
                    }
                }
                else
                {
                    SpawnableItemWithRarity newSpawnableItem = new SpawnableItemWithRarity();
                    newSpawnableItem.spawnableItem = extendedItem.Item;
                    newSpawnableItem.rarity = returnRarity;
                    extendedLevel.SelectableLevel.spawnableScrap.Add(newSpawnableItem);
                    debugString = "Added " + extendedItem.Item.itemName + " To Planet: " + extendedLevel.NumberlessPlanetName + " With A Rarity Of: " + returnRarity;
                }
                if (debugResults == true)
                    DebugHelper.Log(debugString, DebugType.Developer);
            }
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedItem extendedItem)
        {
            if (extendedItem.Item.spawnPrefab == null)
                return (false, "SpawnPrefab Was Null");
            else
                return (true, string.Empty);
        }

        internal static int GetAverageScrapValue(ExtendedItem extendedItem)
        {
            return (Mathf.RoundToInt(Mathf.Lerp(extendedItem.Item.minValue, extendedItem.Item.maxValue, 0.5f)));
        }
    }
}
