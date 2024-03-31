using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/Dungeon Matching Properties")]
    public class DungeonMatchingProperties : MatchingProperties
    {
        [Space(5)] public List<StringWithRarity> dungeonTags = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> dungeonNames = new List<StringWithRarity>();

        public int GetDynamicRarity(ExtendedDungeonFlow extendedDungeonFlow)
        {
            int returnRarity = 0;

            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedTags(extendedDungeonFlow.ContentTags, dungeonNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Dungeon Tags!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedDungeonFlow.AuthorName, authorNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Author Name!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedDungeonFlow.ModName, modNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Mod Name!");
            if (UpdateRarity(ref returnRarity, GetHighestRarityViaMatchingNormalizedString(extendedDungeonFlow.dungeonFlow.name, dungeonNames)))
                DebugHelper.Log("Raised Rarity Due To Matching Dungeon Name!");

            return (returnRarity);
        }
    }
}
