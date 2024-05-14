using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public static class Validators
    {
        public static bool ValidateExtendedContent(ExtendedContent extendedContent)
        {
            (bool, string) result = (false, string.Empty);

            if (extendedContent is ExtendedLevel extendedLevel)
                result = ValidateExtendedContent(extendedLevel);
            else if (extendedContent is ExtendedDungeonFlow extendedDungeonFlow)
                result = ValidateExtendedContent(extendedDungeonFlow);
            else if (extendedContent is ExtendedItem extendedItem)
                result = ValidateExtendedContent(extendedItem);
            else if (extendedContent is ExtendedEnemyType extendedEnemyType)
                result = ValidateExtendedContent(extendedEnemyType);
            else if (extendedContent is ExtendedFootstepSurface extendedFootstepSurface)
                result = ValidateExtendedContent(extendedFootstepSurface);

            if (result.Item1 == false)
                DebugHelper.Log(result.Item2, DebugType.Developer);

            return (result.Item1);
        }
        
        public static (bool result, string log) ValidateExtendedContent(ExtendedItem extendedItem)
        {
            if (extendedItem == null)
                return (false, "ExtendedItem Was Null");
            else if (extendedItem.Item == null)
                return (false, "Item Was Null");
            else if (extendedItem.Item.spawnPrefab == null)
                return (false, "SpawnPrefab Was Null");
            else
                return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedLevel extendedLevel)
        {
            if (extendedLevel == null)
                return ((false, "ExtendedLevel Was Null"));
            else if (extendedLevel.SelectableLevel == null)
                return ((false, "SelectableLevel Was Null"));
            else if (string.IsNullOrEmpty(extendedLevel.SelectableLevel.sceneName))
                return ((false, "SelectableLevel SceneName Was Null Or Empty"));
            else if (extendedLevel.SelectableLevel.planetPrefab == null)
                return ((false, "SelectableLevel PlanetPrefab Was Null"));
            else if (extendedLevel.SelectableLevel.planetPrefab.GetComponent<Animator>() == null)
                return ((false, "SelectableLevel PlanetPrefab Animator Was Null"));
            else if (extendedLevel.SelectableLevel.planetPrefab.GetComponent<Animator>().runtimeAnimatorController == null)
                return ((false, "SelectableLevel PlanetPrefab Animator AnimatorController Was Null"));
            else
                return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedDungeonFlow extendedDungeonFlow)
        {
            return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedEnemyType extendedEnemyType)
        {
            if (extendedEnemyType == null)
                return ((false, "ExtendedEnemyType Was Null"));
            if (extendedEnemyType.EnemyType == null)
                return ((false, "EnemyType Was Null"));
            if (extendedEnemyType.EnemyType.enemyPrefab == null)
                return ((false, "EnemyPrefab Was Null"));
            if (extendedEnemyType.EnemyType.enemyPrefab.GetComponent<NetworkObject>() == false)
                return ((false, "EnemyPrefab Did Not Contain A NetworkObject"));
            EnemyAI enemyAI = extendedEnemyType.EnemyType.enemyPrefab.GetComponent<EnemyAI>();
            if (enemyAI == null)
                enemyAI = extendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<EnemyAI>();
            if (enemyAI == null)
                return ((false, "EnemyPrefab Did Not Contain A Component Deriving From EnemyAI"));
            if (enemyAI.enemyType == null)
                return ((false, "EnemyAI.enemyType Was Null"));
            if (enemyAI.enemyType != extendedEnemyType.EnemyType)
                return ((false, "EnemyAI.enemyType Did Not Match ExtendedEnemyType.EnemyType"));

            return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedWeatherEffect extendedWeatherEffect)
        {
            return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedFootstepSurface extendedFootstepSurface)
        {
            return (true, string.Empty);
        }

        public static (bool result, string log) ValidateExtendedContent(ExtendedStoryLog extendedStoryLog)
        {
            if (string.IsNullOrEmpty(extendedStoryLog.sceneName))
                return (false, "StoryLog SceneName Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.terminalKeywordNoun))
                return (false, "StoryLog TerminalKeywordNoun Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.storyLogTitle))
                return (false, "StoryLog Title Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.storyLogDescription))
                return (false, "StoryLog Description Was Null Or Empty");

            return (true, string.Empty);
        }
    }
}
