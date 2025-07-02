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

        //Obsolete.
        public static new LevelMatchingProperties Create(ExtendedContent extendedContent) => Create<LevelMatchingProperties>(extendedContent);
        public int GetDynamicRarity(ExtendedLevel extendedLevel)
        {
            int returnRarity = 0;

            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedTags(extendedLevel.ContentTags, levelTags), extendedLevel.name, "Content Tags");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.AuthorName, authorNames), extendedLevel.name, "Author Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedStrings(extendedLevel.ExtendedMod.ModNameAliases, modNames), extendedLevel.name, "Mod Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingWithinRanges(extendedLevel.PurchasePrice, currentRoutePrice), extendedLevel.name, "Route Price");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.NumberlessPlanetName, planetNames), extendedLevel.name, "Planet Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedLevel.SelectableLevel.currentWeather.ToString(), currentWeather), extendedLevel.name, "Current Weather");

            return (returnRarity);
        }

        public void ApplyValues(List<StringWithRarity> newModNames = null, List<StringWithRarity> newAuthorNames = null, List<StringWithRarity> newLevelTags = null, List<Vector2WithRarity> newRoutePrices = null, List<StringWithRarity> newCurrentWeathers = null, List<StringWithRarity> newPlanetNames = null)
        {
            modNames = IsNullOrEmpty(newModNames) ? modNames : newModNames;
            authorNames = IsNullOrEmpty(newAuthorNames) ? authorNames : newAuthorNames;
            levelTags = IsNullOrEmpty(newLevelTags) ? levelTags : newLevelTags;
            currentRoutePrice = IsNullOrEmpty(newRoutePrices) ? currentRoutePrice : newRoutePrices;
            currentWeather = IsNullOrEmpty(newCurrentWeathers) ? currentWeather : newCurrentWeathers;
            planetNames = IsNullOrEmpty(newPlanetNames) ? planetNames : newPlanetNames;
        }

        private bool IsNullOrEmpty<T>(List<T> list) => !(list != null && list.Count > 0);
    }
}
