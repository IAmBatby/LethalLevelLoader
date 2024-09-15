﻿using BepInEx.Configuration;
using HarmonyLib;
using LethalLevelLoader.General;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public class ConfigHelper
    {
        //Turns a single string into a list of StringWithRarity's, For easy config setup
        //Example: string configString = "FirstPlanetName (Rarity: Int), SecondPlanetName (Rarity: Int)"

        public const string indexSeperator = ",";
        public const string keyPairSeperator = ":";
        public const string vectorSeperator = "-";
        public const string illegalCharacters = ".,?!@#$%^&*()_+-=';:'\"";

        public static List<StringWithRarity> ConvertToStringWithRarityList(string newInputString, Vector2 clampRarity)
        {
            List<StringWithRarity> returnList = new List<StringWithRarity>();

            List<string> stringList = SplitStringsByIndexSeperator(newInputString);

            foreach (string stringString in stringList)
            {
                (string,string) splitStringData = SplitStringByKeyPairSeperator(stringString);
                string levelName = splitStringData.Item1;
                int rarity = 0;
                if (int.TryParse(splitStringData.Item2, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    rarity = Math.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                returnList.Add(new StringWithRarity(levelName, rarity));
            }
            return (returnList);
        }

        public static List<Vector2WithRarity> ConvertToVector2WithRarityList(string newInputString, Vector2 clampRarity)
        {
            List<Vector2WithRarity> returnList = new List<Vector2WithRarity>();

            List<string> stringList = SplitStringsByIndexSeperator(newInputString);

            foreach (string stringString in stringList)
            {
                (string, string) splitStringData = SplitStringByKeyPairSeperator(stringString);
                (string,string) vector2Strings = SplitStringByVectorSeperator(splitStringData.Item1);

                float x = 0f;
                float y = 0f;
                int rarity = 0;
                if (float.TryParse(vector2Strings.Item1, out float xValue))
                    x = xValue;
                if (float.TryParse(vector2Strings.Item2, out float yValue))
                    y = yValue;
                if (int.TryParse(splitStringData.Item2, out int value))
                    rarity = value;

                if (clampRarity != Vector2.zero)
                    rarity = Math.Clamp(rarity, Mathf.RoundToInt(clampRarity.x), Mathf.RoundToInt(clampRarity.y));

                returnList.Add(new Vector2WithRarity(new Vector2(x,y), rarity));
            }
            return (returnList);
        }

        public static List<SpawnableEnemyWithRarity> ConvertToSpawnableEnemyWithRarityList(string newInputString, Vector2 clampRarity)
        {
            List<StringWithRarity> stringList = ConvertToStringWithRarityList(newInputString, clampRarity);
            List<SpawnableEnemyWithRarity> returnList = new List<SpawnableEnemyWithRarity>();

            foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.ExtendedEnemyTypes)
            {
                EnemyType enemyType = extendedEnemyType.EnemyType;
                foreach (StringWithRarity stringString in new List<StringWithRarity>(stringList))
                {
                    if (enemyType.enemyName.ToLower().Contains(stringString.Name.ToLower()))
                    {
                        SpawnableEnemyWithRarity newEnemy = new SpawnableEnemyWithRarity();
                        newEnemy.enemyType = enemyType;
                        newEnemy.rarity = stringString.Rarity;
                        returnList.Add(newEnemy);
                        stringList.Remove(stringString);
                    }
                }
            }

            //Incase the user put in the real name (eg. Bracken) instead of the internal name (Flowerman) we go through the scannode texts which has the more updated name.
            foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.ExtendedEnemyTypes)
            {
                EnemyType enemyType = extendedEnemyType.EnemyType;
                foreach (StringWithRarity stringString in new List<StringWithRarity>(stringList))
                {
                    if (enemyType.enemyPrefab != null)
                    {
                        ScanNodeProperties enemyScanNode = enemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                        if (enemyScanNode != null)
                            if (enemyScanNode.headerText.ToLower().Contains(stringString.Name.ToLower()) || stringString.Name.ToLower().Contains(enemyScanNode.headerText.ToLower()))
                            {
                                SpawnableEnemyWithRarity newEnemy = new SpawnableEnemyWithRarity();
                                newEnemy.enemyType = enemyType;
                                newEnemy.rarity = stringString.Rarity;
                                returnList.Add(newEnemy);
                                stringList.Remove(stringString);
                            }

                    }    
                }
            }

                    return (returnList);
        }

        public static List<SpawnableItemWithRarity> ConvertToSpawnableItemWithRarityList(string newInputString, Vector2 clampRarity)
        {
            List<StringWithRarity> stringList = ConvertToStringWithRarityList(newInputString, clampRarity);
            List<SpawnableItemWithRarity> returnList = new List<SpawnableItemWithRarity>();

            foreach (ExtendedItem extendedItem in PatchedContent.ExtendedItems)
            {
                Item item = extendedItem.Item;
                foreach (StringWithRarity stringString in new List<StringWithRarity>(stringList))
                {
                    if (SanitizeString(item.itemName).Contains(SanitizeString(stringString.Name)) || SanitizeString(stringString.Name).Contains(SanitizeString(item.itemName)))
                    {
                        DebugHelper.Log("Vanilla Item Name: " + SanitizeString(item.itemName) + " , Parsed Item Name: " + SanitizeString(stringString.Name), DebugType.Developer);
                        SpawnableItemWithRarity newItem = new SpawnableItemWithRarity();
                        newItem.spawnableItem = item;
                        newItem.rarity = stringString.Rarity;
                        returnList.Add(newItem);
                        stringList.Remove(stringString);
                    }
                }
            }

            return (returnList);
        }


        public static string SpawnableEnemiesWithRaritiesToString(List<SpawnableEnemyWithRarity> spawnableEnemiesList)
        {
            var stringBuilder = new StringBuilder();

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in spawnableEnemiesList)
            {
                stringBuilder.Append(spawnableEnemyWithRarity.enemyType.enemyName)
                    .Append(keyPairSeperator)
                    .AppendValue(spawnableEnemyWithRarity.rarity)
                    .Append(indexSeperator);
            }

            stringBuilder.TrimEnd(',');

            if (stringBuilder.Length == 0)
                return "Default Values Were Empty";

            return (stringBuilder.ToString());
        }

        public static string SpawnableItemsWithRaritiesToString(List<SpawnableItemWithRarity> spawnableItemsList)
        {
            var stringBuilder = new StringBuilder();

            foreach (SpawnableItemWithRarity spawnableItemWithRarity in spawnableItemsList)
            {
                stringBuilder.Append(spawnableItemWithRarity.spawnableItem.itemName)
                    .Append(keyPairSeperator)
                    .AppendValue(spawnableItemWithRarity.rarity)
                    .Append(indexSeperator);
            }

            stringBuilder.TrimEnd(',');

            if (stringBuilder.Length == 0)
                return "Default Values Were Empty";

            return (stringBuilder.ToString());
        }

        public static string StringWithRaritiesToString(List<StringWithRarity> names)
        {
            var stringBuilder = new StringBuilder();

            foreach (StringWithRarity name in names)
            {
                stringBuilder.Append(name.Name)
                    .Append(keyPairSeperator)
                    .AppendValue(name.Rarity)
                    .Append(indexSeperator);
            }

            stringBuilder.TrimEnd(',');

            if (stringBuilder.Length == 0)
                return "Default Values Were Empty";

            return (stringBuilder.ToString());
        }

        public static string Vector2WithRaritiesToString(List<Vector2WithRarity> values)
        {
            var stringBuilder = new StringBuilder();

            foreach (Vector2WithRarity vector2withRarity in values)
            {
                stringBuilder.AppendValue(vector2withRarity.Min)
                    .Append(vectorSeperator)
                    .AppendValue(vector2withRarity.Max)
                    .AppendValue(vector2withRarity.Rarity)
                    .Append(indexSeperator);
            }

            stringBuilder.TrimEnd(',');

            if (stringBuilder.Length == 0)
                return "Default Values Were Empty";

            return (stringBuilder.ToString());
        }

        public static List<string> SplitStringsByIndexSeperator(string newInputString)
        {
            List<string> stringList = new List<string>();

            string inputString = newInputString;

            while (inputString.Contains(indexSeperator))
            {
                string inputStringWithoutTextBeforeFirstComma = inputString.Substring(inputString.IndexOf(indexSeperator));
                stringList.Add(inputString.Replace(inputStringWithoutTextBeforeFirstComma, ""));
                if (inputStringWithoutTextBeforeFirstComma.Contains(indexSeperator))
                    inputString = inputStringWithoutTextBeforeFirstComma.Substring(inputStringWithoutTextBeforeFirstComma.IndexOf(indexSeperator) + 1);

            }
            stringList.Add(inputString);

            return (stringList);
        }

        public static (string, string) SplitStringByKeyPairSeperator(string inputString)
        {
            return (SplitStringByCharacter(inputString, keyPairSeperator));
        }

        public static (string, string) SplitStringByVectorSeperator(string inputString)
        {
            return (SplitStringByCharacter(inputString, vectorSeperator));
        }

        public static (string, string) SplitStringByCharacter(string newInputString, string splitValue)
        {
            if (!newInputString.Contains(splitValue))
                return ((newInputString, string.Empty));
            else
            {
                string firstValue = string.Empty;
                string secondValue = string.Empty;
                firstValue = newInputString.Replace(newInputString.Substring(newInputString.IndexOf(splitValue)), "");
                secondValue = newInputString.Substring(newInputString.IndexOf(splitValue) + 1);
                return ((firstValue, secondValue));
            }
        }

        [Obsolete("Use " + nameof(Extensions) + "." + nameof(Extensions.Sanitized))]
        public static string SanitizeString(string inputString)
        {
            return inputString.Sanitized();
        }
    }
}
