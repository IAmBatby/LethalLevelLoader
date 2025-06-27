using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "DungeonMatchingProperties", menuName = "Lethal Level Loader/Utility/DungeonMatchingProperties", order = 13)]
    public class DungeonMatchingProperties : MatchingProperties
    {
        [Space(5)] public List<StringWithRarity> dungeonTags = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> dungeonNames = new List<StringWithRarity>();

        //Obsolete.
        public static new DungeonMatchingProperties Create(ExtendedContent extendedContent) => Create<DungeonMatchingProperties>(extendedContent);
        public int GetDynamicRarity(ExtendedDungeonFlow extendedDungeonFlow)
        {
            int returnRarity = 0;

            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedTags(extendedDungeonFlow.ContentTags, dungeonNames), extendedDungeonFlow.name, "Content Tags");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedDungeonFlow.AuthorName, authorNames), extendedDungeonFlow.name, "Author Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedStrings(extendedDungeonFlow.ExtendedMod.ModNameAliases, modNames), extendedDungeonFlow.name, "Mod Name Name");
            UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedDungeonFlow.DungeonFlow.name, dungeonNames), extendedDungeonFlow.name, "Dungeon Name");

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
