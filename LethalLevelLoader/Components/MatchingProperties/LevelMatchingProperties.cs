using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/Level Matching Properties")]
    public class LevelMatchingProperties : MatchingProperties
    {
        [Space(5)] public List<StringWithRarity> levelTags = new List<StringWithRarity>();
        [Space(5)] public List<Vector2WithRarity> currentRoutePrice = new List<Vector2WithRarity>();
        [Space(5)] public List<StringWithRarity> currentWeather = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> planetNames = new List<StringWithRarity>();

        public int GetDynamicRarity(ExtendedLevel extendedLevel)
        {
            int returnRarity = 0;

            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedTags(extendedLevel.ContentTags, levelTags)))
                DebugHelper.Log("Raised Rarity Due To Matching Level Tags!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.AuthorName, authorNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Author Name!");
            foreach (string modNameAlias in extendedLevel.ExtendedMod.ModNameAliases)
                if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(modNameAlias, modNames)))
                    DebugHelper.Log("Raised Rarity Due To Matching Mod Name!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.NumberlessPlanetName, planetNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Planet Name!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingWithinRanges(extendedLevel.RoutePrice, currentRoutePrice)))
                DebugHelper.Log("Raised Rarity Due To Matching Route Price!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.selectableLevel.currentWeather.ToString(), currentWeather)))
                DebugHelper.Log("Raised Rarity Due To Matching Current Weather!");

            return (returnRarity);
        }
    }
}
