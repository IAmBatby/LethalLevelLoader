﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalLevelLoader
{
    public class MatchingProperties<T> : ScriptableObject where T : ExtendedContent
    {
        [Space(5)] public List<StringWithRarity> modNames = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> authorNames = new List<StringWithRarity>();

        public static MatchingProperties<T> Create(ExtendedContent extendedContent)
        {
            MatchingProperties<T> matchingProperties = ScriptableObject.CreateInstance<MatchingProperties<T>>();
            matchingProperties.name = extendedContent.name + "MatchingProperties";
            return (matchingProperties);
        }

        internal virtual int GetDynamicRarity(T content)
        {
            int result = 0;
            UpdateRarity(ref result, GetHighestRarityViaMatchingNormalizedString(content.AuthorName, authorNames), content.name, "Author Name");
            UpdateRarity(ref result, GetHighestRarityViaMatchingNormalizedStrings(content.ExtendedMod.ModNameAliases, modNames), content.name, "Mod Name Name");
            return result;
        }

        internal static bool UpdateRarity(ref int currentValue, int newValue, string debugActionObject = null, string debugActionReason = null)
        {
            if (newValue > currentValue)
            {
                if (!string.IsNullOrEmpty(debugActionReason))
                {
                    if (!string.IsNullOrEmpty(debugActionObject))
                        DebugHelper.Log("Raised Rarity Of: " + debugActionObject + " From (" + currentValue + ") To (" + newValue + ") Due To Matching " + debugActionReason, DebugType.Developer);
                    else
                        DebugHelper.Log("Raised Rarity From (" + currentValue + ") To (" + newValue + ") Due To Matching " + debugActionReason, DebugType.Developer);
                }
                currentValue = newValue;
                return (true);
            }
            return (false);
        }

        internal static int GetHighestRarityViaMatchingWithinRanges(int comparingValue, List<Vector2WithRarity> matchingVectors)
        {
            int returnInt = 0;
            foreach (Vector2WithRarity vectorWithRarity in matchingVectors)
                if (vectorWithRarity.Rarity >= returnInt)
                    if ((comparingValue >= vectorWithRarity.Min) && (comparingValue <= vectorWithRarity.Max))
                        returnInt = vectorWithRarity.Rarity;
            return (returnInt);
        }

        internal static int GetHighestRarityViaMatchingNormalizedString(string comparingString, List<StringWithRarity> matchingStrings)
        {
            return (GetHighestRarityViaMatchingNormalizedStrings(new List<string>() { comparingString }, matchingStrings));
        }

        internal static int GetHighestRarityViaMatchingNormalizedTags(List<ContentTag> comparingTags, List<StringWithRarity> matchingStrings)
        {
            List<string> contentTagStrings = comparingTags.Select(t => t.contentTagName).ToList();
            return GetHighestRarityViaMatchingNormalizedStrings(contentTagStrings, matchingStrings);
        }

        internal static int GetHighestRarityViaMatchingNormalizedStrings(List<string> comparingStrings, List<StringWithRarity> matchingStrings)
        {
            int returnInt = 0;
            foreach (StringWithRarity stringWithRarity in matchingStrings)
                foreach (string comparingString in new List<string>(comparingStrings))
                    if (stringWithRarity.Rarity >= returnInt)
                        if (stringWithRarity.Name.Sanitized().Contains(comparingString.Sanitized()) || comparingString.Sanitized().Contains(stringWithRarity.Name.Sanitized()))
                            returnInt = stringWithRarity.Rarity;
            return (returnInt);
        }
    }
}
