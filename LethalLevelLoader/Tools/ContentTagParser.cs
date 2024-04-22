using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine.Windows;
using System.Linq;

namespace LethalLevelLoader
{
    internal static class ContentTagParser
    {
        internal static Dictionary<string, List<string>> importedItemContentTagDictionary = new Dictionary<string, List<string>>();
        internal static Dictionary<string, List<string>> importedLevelContentTagDictionary = new Dictionary<string, List<string>>();
        internal static Dictionary<string, List<string>> importedEnemyContentTagDictionary = new Dictionary<string, List<string>>();

        internal static void ApplyVanillaContentTags()
        {
            ApplyImportedItemContentTags();
            ApplyImportedSelectableLevelContentTags();
            ApplyImportedEnemyTypeContentTags();
        }

        internal static void ImportVanillaContentTags()
        {
            ParseContentFile("Items", importedItemContentTagDictionary, 5);
            ParseContentFile("SelectableLevels", importedLevelContentTagDictionary, 3);
            ParseContentFile("Enemies", importedEnemyContentTagDictionary, 3);
        }

        internal static void ParseContentFile(string fileName, Dictionary<string, List<string>> importedContentTagDict, int startingLine)
        {
            DebugHelper.Log("Parsing Contents Of Content CSV Located At: " + fileName, DebugType.Developer);
            int lineCount = 0;
            string line;
            try
            {
                StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("LethalLevelLoader.VanillaContentTags." + fileName + ".csv"));
                line = sr.ReadLine();
                lineCount++;
                while (line != null)
                {
                    //write the line to console window
                    if (lineCount > startingLine)
                    {
                        (string, List<string>) parsedContent = ParseLine(line);
                        importedContentTagDict.Add(parsedContent.Item1, parsedContent.Item2);
                        DebugParsedLine(parsedContent);
                    }
                    //Read the next line
                    line = sr.ReadLine();
                    lineCount++;
                }
                //close the file
                sr.Close();
                if (lineCount > startingLine)
                {
                    (string, List<string>) parsedContent = ParseLine(line);
                    importedContentTagDict.Add(parsedContent.Item1, parsedContent.Item2);
                    DebugParsedLine(parsedContent);
                }
            }
            catch
            {

            }
        }

        internal static void ApplyImportedItemContentTags()
        {
            int counter = 0;
            List<int> appliedIndexes = new List<int>();
            foreach (KeyValuePair<string, List<string>> importedItemData in importedItemContentTagDictionary)
            {
                foreach (ExtendedItem extendedItem in PatchedContent.VanillaMod.ExtendedItems)
                {
                    if (extendedItem.Item.name.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower() || extendedItem.Item.itemName.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower())
                    {
                        DebugHelper.Log("Applying CSV Tags For Imported Item #" + (counter + 1) + " / " + (importedItemContentTagDictionary.Count - 1) + ": " + importedItemData.Key + " To ExtendedItem: " + extendedItem.Item.itemName + "(" + extendedItem.Item.name + ")", DebugType.Developer);
                        extendedItem.ContentTags = ContentTagManager.CreateNewContentTags(importedItemData.Value.Concat(new List<string>() { "Vanilla" }).ToList());
                        appliedIndexes.Add(counter);
                        break;
                    }
                }            
                counter++;
            }

            for (int i = 0; i < importedItemContentTagDictionary.Count; i++)
            {
                if (!appliedIndexes.Contains(i) && importedItemContentTagDictionary.Keys.ToList()[i] != string.Empty)
                    DebugHelper.LogWarning("Could Not Apply CSV Tags For Imported Item: " + importedItemContentTagDictionary.Keys.ToList()[i], DebugType.Developer);
            }
        }

        internal static void ApplyImportedSelectableLevelContentTags()
        {
            int counter = 0;
            List<int> appliedIndexes = new List<int>();
            foreach (KeyValuePair<string, List<string>> importedItemData in importedLevelContentTagDictionary)
            {
                foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaMod.ExtendedLevels)
                {
                    if (extendedLevel.SelectableLevel.name.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower() || extendedLevel.NumberlessPlanetName.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower())
                    {
                        DebugHelper.Log("Applying CSV Tags For Imported Level #" + (counter + 1) + " / " + (importedLevelContentTagDictionary.Count - 1) + ": " + importedItemData.Key + " To SelectableLevel: " + extendedLevel.SelectableLevel.PlanetName + "(" + extendedLevel.SelectableLevel.name + ")", DebugType.Developer);
                        extendedLevel.ContentTags = ContentTagManager.CreateNewContentTags(importedItemData.Value.Concat(new List<string>() { "Vanilla" }).ToList());
                        appliedIndexes.Add(counter);
                        break;
                    }
                }
                counter++;
            }

            for (int i = 0; i < importedLevelContentTagDictionary.Count; i++)
            {
                if (!appliedIndexes.Contains(i) && importedLevelContentTagDictionary.Keys.ToList()[i] != string.Empty)
                    DebugHelper.LogWarning("Could Not Apply CSV Tags For Imported SelectableLevel: " + importedLevelContentTagDictionary.Keys.ToList()[i], DebugType.Developer);
            }
        }

        internal static void ApplyImportedEnemyTypeContentTags()
        {
            int counter = 0;
            List<int> appliedIndexes = new List<int>();
            foreach (KeyValuePair<string, List<string>> importedItemData in importedEnemyContentTagDictionary)
            {
                foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.VanillaMod.ExtendedEnemyTypes)
                {
                    if (extendedEnemyType.EnemyType.name.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower() || extendedEnemyType.EnemyType.enemyName.RemoveWhitespace().StripSpecialCharacters().ToLower() == importedItemData.Key.RemoveWhitespace().StripSpecialCharacters().ToLower())
                    {
                        DebugHelper.Log("Applying CSV Tags For Imported Enemy #" + (counter + 1) + " / " + (importedEnemyContentTagDictionary.Count - 1) + ": " + importedItemData.Key + " To EnemyType: " + extendedEnemyType.EnemyType.enemyName + "(" + extendedEnemyType.EnemyType.name + ")", DebugType.Developer);
                        extendedEnemyType.ContentTags = ContentTagManager.CreateNewContentTags(importedItemData.Value.Concat(new List<string>() { "Vanilla" }).ToList());
                        appliedIndexes.Add(counter);
                        break;
                    }
                }
                counter++;
            }

            /*for (int i = 0; i < importedEnemyContentTagDictionary.Count; i++)
            {
                if (!appliedIndexes.Contains(i) && importedEnemyContentTagDictionary.Keys.ToList()[i] != string.Empty)
                    DebugHelper.LogWarning("Could Not Apply CSV Tags For Imported EnemyType: " + importedEnemyContentTagDictionary.Keys.ToList()[i]);
            }*/
        }

        internal static (string, List<string>) ParseLine(string line)
        {
            string parsedLine = string.Empty;
            string contentName = string.Empty;
            List<string> contentTags = new List<string>();
            if (!string.IsNullOrEmpty(line))
                if (line.Contains(","))
                {
                    contentName = line.Replace(line.Substring(line.IndexOf(",")), string.Empty);
                    parsedLine = line.Substring(line.IndexOf(",") + 1);
                    parsedLine = parsedLine.SkipToLetters();
                    string newContentTag = string.Empty;
                    while (parsedLine.Contains(","))
                    {
                        newContentTag = parsedLine.Replace(parsedLine.Substring(parsedLine.IndexOf(",")), string.Empty).SkipToLetters();
                        contentTags.Add(newContentTag);
                        if (parsedLine.Length > 1)
                            parsedLine = parsedLine.Substring(parsedLine.IndexOf(",") + 1);
                        else
                            parsedLine = string.Empty;
                    }
                }
            for (int i = 0; i < contentTags.Count; i++)
                contentTags[i] = new string(contentTags[i].ToCharArray().Where(c => Char.IsLetter(c)).ToArray());
            List<string> parsedContentTags = new List<string>();
            for (int i = 0; i < contentTags.Count; i++)
                if (!string.IsNullOrEmpty(contentTags[i]))
                    parsedContentTags.Add(contentTags[i]);

            return ((contentName, parsedContentTags));
        }

        internal static void DebugParsedLine((string, List<string>) parsedLine)
        {
            DebugParsedLine(parsedLine.Item1, parsedLine.Item2);
        }

        internal static void DebugParsedLine(string contentName, List<string> contentTags)
        {
            if (!string.IsNullOrEmpty(contentName) && contentTags.Count > 0)
            {
                string debugString = "ContentName: " + contentName + " | Content Tags: ";
                string otherDebugString = string.Empty;
                foreach (string tag in contentTags)
                    otherDebugString += ", " + tag;
                DebugHelper.Log(debugString + otherDebugString.SkipToLetters(), DebugType.Developer);
            }
        }

    }
}
