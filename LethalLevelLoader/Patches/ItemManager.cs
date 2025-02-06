using UnityEngine;

namespace LethalLevelLoader
{
    public static class ItemManager
    {
        public static void RefreshDynamicItemRarityOnAllExtendedLevels()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                InjectCustomItemsIntoLevelViaDynamicRarity(extendedLevel);
        }
        public static void InjectCustomItemsIntoLevelViaDynamicRarity(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            foreach (ExtendedItem extendedItem in PatchedContent.CustomExtendedItems)
            {
                if (!extendedItem.Item.isScrap) continue;
                string debugString = string.Empty;
                SpawnableItemWithRarity alreadyInjectedItem = null;
                foreach (SpawnableItemWithRarity spawnableItem in extendedLevel.SelectableLevel.spawnableScrap)
                {
                    if (spawnableItem.spawnableItem != extendedItem) continue;

                    alreadyInjectedItem = spawnableItem;
                    break;
                }

                int returnRarity = 0;
                int levelRarity = extendedItem.LevelMatchingProperties.GetDynamicRarity(extendedLevel);
                //int dungeonRarity
                returnRarity = levelRarity;
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

        internal static void GetExtendedItemPriceData()
        {
            /*
            int highestPrice = 0;
            ExtendedItem highestExtendedItem = null;

            int lowestPrice = 999999;
            ExtendedItem lowestExtendedItem = null;

            List<int> allMaxValues = new List<int>();

            List<ExtendedItem> sortedItems = PatchedContent.ExtendedItems.OrderBy(o => GetAverageScrapValue(o)).ToList();

            foreach (ExtendedItem extendedItem in sortedItems)
            {
                int averageValue = GetAverageScrapValue(extendedItem);
                if (averageValue != 0)
                {
                    if (averageValue > highestPrice)
                    {
                        highestPrice = averageValue;
                        highestExtendedItem = extendedItem;
                    }
                    if (averageValue < lowestPrice)
                    {
                        lowestPrice = averageValue;
                        lowestExtendedItem = extendedItem;
                    }

                    allMaxValues.Add(averageValue);
                }

            }

            DebugHelper.Log("Highest MaxValue Item Was: " + highestExtendedItem.Item.itemName + " At: " + GetAverageScrapValue(highestExtendedItem));
            DebugHelper.Log("Lowest MaxValue Item Was: " + lowestExtendedItem.Item.itemName + " At: " + GetAverageScrapValue(lowestExtendedItem));
            DebugHelper.Log("Average MaxValue Was: " + (int)allMaxValues.Average());

            int highThreshold = Mathf.RoundToInt(Mathf.Lerp(lowestPrice, highestPrice, 0.6f));
            int lowThreshold = Mathf.RoundToInt(Mathf.Lerp(lowestPrice, highestPrice, 0.2f));
            DebugHelper.Log("Valuable Tag Range Bracket Would Be: " + highThreshold + " - " + highestPrice);
            DebugHelper.Log("Valueless Tag Range Bracket Would Be: " + lowestPrice + " - " + lowThreshold);

            string freeBracket = string.Empty;
            string lowBracket = string.Empty;
            string middleBracket = string.Empty;
            string highBracket = string.Empty;

            foreach (ExtendedItem extendedItem in sortedItems)
            {
                if (extendedItem.Item.minValue != 0 && extendedItem.Item.maxValue != 0)
                {
                    int adjustedLowAverageValue = GetAverageScrapValue(extendedItem);
                    int adjustedHighAverageValue = GetAverageScrapValue(extendedItem);
                    if (adjustedLowAverageValue != 0 && adjustedLowAverageValue < lowThreshold)
                        lowBracket += "\n" + extendedItem.Item.itemName + " | " + GetAverageScrapValue(extendedItem);
                    else if (adjustedHighAverageValue != 0 && adjustedHighAverageValue > lowThreshold && adjustedHighAverageValue < highThreshold)
                        middleBracket += "\n" + extendedItem.Item.itemName + " | " + GetAverageScrapValue(extendedItem);
                    else
                        highBracket += "\n" + extendedItem.Item.itemName + " | " + GetAverageScrapValue(extendedItem);
                }
                else
                    freeBracket += "\n" + extendedItem.Item.itemName + " | " + 0;
            }

            DebugHelper.Log("Items That Fall Into The Valueless Range Are: " + "\n" + freeBracket);
            DebugHelper.Log("Items That Fall Into The Low-Value Range Are: " + "\n" + lowBracket);
            DebugHelper.Log("Items That Fall Into The Average-Value Range Are: " + "\n" + middleBracket);
            DebugHelper.Log("Items That Fall Into The Valuable Range Are: " + "\n" + highBracket);
            */
        }

        public static void GetExtendedItemWeightData()
        {
            /*float highestWeight = 0;
            ExtendedItem heaviestExtendedItem = null;

            float lowestWeight = 999999;
            ExtendedItem lightestExtendedItem = null;

            List<float> allWeights = new List<float>();

            List<ExtendedItem> sortedItems = PatchedContent.ExtendedItems.OrderBy(o => o.Item.weight).ToList();

            foreach (ExtendedItem extendedItem in sortedItems)
            {;
                if (extendedItem.Item.weight != 0)
                {
                    if (extendedItem.Item.weight > highestWeight)
                    {
                        highestWeight = extendedItem.Item.weight;
                        heaviestExtendedItem = extendedItem;
                    }
                    if (extendedItem.Item.weight < lowestWeight)
                    {
                        lowestWeight = extendedItem.Item.weight;
                        lightestExtendedItem = extendedItem;
                    }

                    allWeights.Add(extendedItem.Item.weight);
                }

            }

            DebugHelper.Log("Heaviest Item Was: " + heaviestExtendedItem.Item.itemName + " At: " + heaviestExtendedItem.Item.weight);
            DebugHelper.Log("Lightest Item Was: " + lightestExtendedItem.Item.itemName + " At: " + lightestExtendedItem.Item.weight);
            DebugHelper.Log("Average Weight Was: " + allWeights.Average());

            float highThreshold = Mathf.Lerp(lowestWeight, highestWeight, 0.6f);
            float lowThreshold = Mathf.Lerp(lowestWeight, highestWeight, 0.15f);
            DebugHelper.Log("Heavy Tag Range Bracket Would Be: " + highThreshold + " - " + highestWeight);
            DebugHelper.Log("Light Tag Range Bracket Would Be: " + lowestWeight + " - " + lowThreshold);

            string freeBracket = string.Empty;
            string lowBracket = string.Empty;
            string middleBracket = string.Empty;
            string highBracket = string.Empty;

            foreach (ExtendedItem extendedItem in sortedItems)
            {
                if (extendedItem.Item.weight != 0)
                {
                    if (extendedItem.Item.weight != 0 && extendedItem.Item.weight < lowThreshold)
                        lowBracket += "\n" + extendedItem.Item.itemName + " | " + extendedItem.Item.weight;
                    else if (extendedItem.Item.weight != 0 && extendedItem.Item.weight > lowThreshold && extendedItem.Item.weight < highThreshold)
                        middleBracket += "\n" + extendedItem.Item.itemName + " | " + extendedItem.Item.weight;
                    else
                        highBracket += "\n" + extendedItem.Item.itemName + " | " + extendedItem.Item.weight;
                }
                else
                    freeBracket += "\n" + extendedItem.Item.itemName + " | " + 0;
            }

            DebugHelper.Log("Items That Fall Into The Weightless Range Are: " + "\n" + freeBracket);
            DebugHelper.Log("Items That Fall Into The Light-Weight Range Are: " + "\n" + lowBracket);
            DebugHelper.Log("Items That Fall Into The Average-Weight Range Are: " + "\n" + middleBracket);
            DebugHelper.Log("Items That Fall Into The Heavy-Weight Range Are: " + "\n" + highBracket);*/
        }

        internal static int GetAverageScrapValue(ExtendedItem extendedItem)
        {
            return (Mathf.RoundToInt(Mathf.Lerp(extendedItem.Item.minValue, extendedItem.Item.maxValue, 0.5f)));
        }
    }
}
