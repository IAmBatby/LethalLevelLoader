using System;
using System.Collections.Generic;
using System.Text;
using LethalLevelLoader.Components.MatchingProperties.PropertyMatchers;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "DungeonMatchingProperties", menuName = "Lethal Level Loader/Utility/DungeonMatchingProperties", order = 13)]
    public class DungeonMatchingProperties : MatchingProperties
    {
        [Space(5)] public List<StringWithRarity> dungeonTags = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> dungeonNames = new List<StringWithRarity>();

        public static new DungeonMatchingProperties Create(ExtendedContent extendedContent)
        {
            DungeonMatchingProperties dungeonMatchingProperties = ScriptableObject.CreateInstance<DungeonMatchingProperties>();
            dungeonMatchingProperties.name = extendedContent.name + "DungeonMatchingProperties";
            return (dungeonMatchingProperties);
        }

        public int GetDynamicRarity(ExtendedDungeonFlow extendedDungeonFlow)
        {
            PropertyMatcher propertyMatcher = GetPropertyMatcher();

            int returnRarity = baseRarity;

            foreach (int value in propertyMatcher.GetRaritiesViaMatchingNormalizedTags(extendedDungeonFlow.ContentTags, dungeonNames))
                propertyMatcher.UpdateRarity(ref returnRarity, value, extendedDungeonFlow.name, "Content Tags");

            foreach (int value in propertyMatcher.GetRaritiesViaMatchingNormalizedString(extendedDungeonFlow.AuthorName, authorNames))
                propertyMatcher.UpdateRarity(ref returnRarity, value, extendedDungeonFlow.name, "Author Name");

            foreach (int value in propertyMatcher.GetRaritiesViaMatchingNormalizedStrings(extendedDungeonFlow.ExtendedMod.ModNameAliases, modNames))
                propertyMatcher.UpdateRarity(ref returnRarity, value, extendedDungeonFlow.name, "Mod Name Name");

            foreach (int value in propertyMatcher.GetRaritiesViaMatchingNormalizedString(extendedDungeonFlow.DungeonFlow.name, dungeonNames))
                propertyMatcher.UpdateRarity(ref returnRarity, value, extendedDungeonFlow.name, "Dungeon Name");

            return (returnRarity);
        }

        public void ApplyValues(List<StringWithRarity> newModNames = null, List<StringWithRarity> newAuthorNames = null, List<StringWithRarity> newDungeonTags = null, List<StringWithRarity> newDungeonNames = null)
        {
            if (newModNames != null && newModNames.Count != 0)
                modNames = new List<StringWithRarity>(newModNames);
            if (newAuthorNames != null && newAuthorNames.Count != 0)
                authorNames = new List<StringWithRarity>(newAuthorNames);
            if (newDungeonTags != null && newDungeonTags.Count != 0)
                dungeonTags = new List<StringWithRarity>(newDungeonTags);
            if (newDungeonNames != null && newDungeonNames.Count != 0)
                dungeonNames = new List<StringWithRarity>(newDungeonNames);
        }
    }
}
