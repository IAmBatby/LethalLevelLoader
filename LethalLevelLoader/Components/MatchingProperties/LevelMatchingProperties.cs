using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "LevelMatchingProperties", menuName = "Lethal Level Loader/Utility/LevelMatchingProperties", order = 12)]
    public class LevelMatchingProperties : MatchingProperties
    {
        [Space(5)] public List<StringWithRarity> levelTags = new List<StringWithRarity>();
        [Space(5)] public List<Vector2WithRarity> currentRoutePrice = new List<Vector2WithRarity>();
        [Space(5)] public List<StringWithRarity> currentWeather = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> planetNames = new List<StringWithRarity>();

        public static new LevelMatchingProperties Create(ExtendedContent extendedContent)
        {
            LevelMatchingProperties levelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            levelMatchingProperties.name = extendedContent.name + "LevelMatchingProperties";
            return (levelMatchingProperties);
        }

        public int GetDynamicRarity(ExtendedLevel extendedLevel)
        {
            int returnRarity = 0;

            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedTags(extendedLevel.ContentTags, levelTags), extendedLevel.name, "Content Tags");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.AuthorName, authorNames), extendedLevel.name, "Author Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedStrings(extendedLevel.ExtendedMod.ModNameAliases, modNames), extendedLevel.name, "Mod Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingWithinRanges(extendedLevel.RoutePrice, currentRoutePrice), extendedLevel.name, "Route Price");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.NumberlessPlanetName, planetNames), extendedLevel.name, "Planet Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.SelectableLevel.currentWeather.ToString(), currentWeather), extendedLevel.name, "Current Weather");


            return (returnRarity);
        }

        public void ApplyValues(List<StringWithRarity>? newModNames = null, List<StringWithRarity>? newAuthorNames = null, List<StringWithRarity>? newLevelTags = null, List<Vector2WithRarity>? newRoutePrices = null, List<StringWithRarity>? newCurrentWeathers = null, List<StringWithRarity>? newPlanetNames = null)
        {
            if (newModNames != null && newModNames.Count != 0)
                modNames = new List<StringWithRarity>(newModNames);
            if (newAuthorNames != null && newAuthorNames.Count != 0)
                authorNames = new List<StringWithRarity>(newAuthorNames);
            if (newLevelTags != null && newLevelTags.Count != 0)
                levelTags = new List<StringWithRarity>(newLevelTags);
            if (newRoutePrices != null && newRoutePrices.Count != 0)
                currentRoutePrice = new List<Vector2WithRarity>(newRoutePrices);
            if (newCurrentWeathers != null && newCurrentWeathers.Count != 0)
                currentWeather = new List<StringWithRarity>(newCurrentWeathers);
            if (newPlanetNames != null && newPlanetNames.Count != 0)
                planetNames = new List<StringWithRarity>(newPlanetNames);
        }
    }
}
