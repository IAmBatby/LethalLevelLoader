using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace LethalLevelLoader
{
    public class EnemyManager : ExtendedContentManager<ExtendedEnemyType, EnemyType>
    {
        protected override List<EnemyType> GetVanillaContent() => OriginalContent.Enemies;

        protected override ExtendedEnemyType ExtendVanillaContent(EnemyType content)
        {
            ExtendedEnemyType newExtendedEnemyType = ExtendedEnemyType.Create(content);
            ScanNodeProperties enemyScanNode = newExtendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
            if (enemyScanNode == null)
                DebugHelper.LogError(content.name + " Missing ScanNode!", DebugType.User);
            else
                newExtendedEnemyType.EnemyDisplayName = enemyScanNode.headerText;
            //Setting ID
            //Terminal stuff
            return (newExtendedEnemyType);
        }

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);

            List<ExtendedEnemyType> enemies = new List<ExtendedEnemyType>(PatchedContent.ExtendedEnemyTypes);
            for (int i = 0; i < enemies.Count; i++)
                enemies[i].SetGameID(i);

            QuickMenuManager quickMenuManager = UnityEngine.Object.FindAnyObjectByType<QuickMenuManager>();
            if (quickMenuManager == null) return;
            SelectableLevel test = quickMenuManager.testAllEnemiesLevel;
            List<EnemyType> existingEnemies = test.Enemies.Concat(test.OutsideEnemies).Concat(test.DaytimeEnemies).Select(s => s.enemyType).Distinct().ToList();
            foreach (ExtendedEnemyType enemy in enemies)
            {
                if (existingEnemies.Contains(enemy.EnemyType)) continue;
                SpawnableEnemyWithRarity spawnableEnemyWithRarity = Utilities.Create(enemy.EnemyType, 300);
                test.Enemies.Add(spawnableEnemyWithRarity);
                test.OutsideEnemies.Add(spawnableEnemyWithRarity);
                test.DaytimeEnemies.Add(spawnableEnemyWithRarity);
            }

            foreach (ExtendedEnemyType enemy in enemies)
            {
                if (!Terminal.enemyFiles.Contains(enemy.InfoNode))
                    Terminal.enemyFiles.Add(enemy.InfoNode);
                if (enemy is ITerminalEntry terminalEntry)
                    terminalEntry.TryRegister();
            }
        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

        public static void RefreshDynamicEnemyTypeRarityOnAllExtendedLevels()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                InjectCustomEnemyTypesIntoLevelViaDynamicRarity(extendedLevel);
        }

        public static void InjectCustomEnemyTypesIntoLevelViaDynamicRarity(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.CustomExtendedEnemyTypes)
            {
                string debugString = string.Empty;
                SpawnableEnemyWithRarity alreadyInjectedInsideEnemy = null;
                SpawnableEnemyWithRarity alreadyInjectedOutsideEnemy = null;
                SpawnableEnemyWithRarity alreadyInjectedDaytimeEnemy = null;

                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.SelectableLevel.Enemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedInsideEnemy = spawnableEnemyWithRarity;
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.SelectableLevel.OutsideEnemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedOutsideEnemy = spawnableEnemyWithRarity;
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.SelectableLevel.DaytimeEnemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedDaytimeEnemy = spawnableEnemyWithRarity;


                int insideLevelRarity = extendedEnemyType.InsideLevelMatchingProperties.GetDynamicRarity(extendedLevel);
                //int insideDungeonRarity = extendedEnemyType.insideDungeonMatchingProperties.GetDynamicRarity(extendedLevel);
                int outsideLevelRarity = extendedEnemyType.OutsideLevelMatchingProperties.GetDynamicRarity(extendedLevel);
                int daytimeLevelRarity = extendedEnemyType.DaytimeLevelMatchingProperties.GetDynamicRarity(extendedLevel);

                if (outsideLevelRarity > 0)
                    DebugHelper.Log("Custom ExtendedEnemyType: " + extendedEnemyType.EnemyDisplayName + " Has: " + outsideLevelRarity + " OutsideLevelRarity On Moon: " + extendedLevel.NumberlessPlanetName, DebugType.Developer);
                if (daytimeLevelRarity> 0)
                    DebugHelper.Log("Custom ExtendedEnemyType: " + extendedEnemyType.EnemyDisplayName + " Has: " + daytimeLevelRarity + " DaytimeLevelRarity On Moon: " + extendedLevel.NumberlessPlanetName, DebugType.Developer);

                if (TryInjectEnemyIntoPool(extendedLevel.SelectableLevel.Enemies, extendedEnemyType, insideLevelRarity, out SpawnableEnemyWithRarity spawnableInsideEnemy) == false)
                    extendedLevel.SelectableLevel.Enemies.Remove(spawnableInsideEnemy);
                if (TryInjectEnemyIntoPool(extendedLevel.SelectableLevel.OutsideEnemies, extendedEnemyType, outsideLevelRarity, out SpawnableEnemyWithRarity spawnableOutsideEnemy) == false)
                    extendedLevel.SelectableLevel.OutsideEnemies.Remove(spawnableOutsideEnemy);
                if (TryInjectEnemyIntoPool(extendedLevel.SelectableLevel.DaytimeEnemies, extendedEnemyType, daytimeLevelRarity, out SpawnableEnemyWithRarity spawnableDaytimeEnemy) == false)
                    extendedLevel.SelectableLevel.DaytimeEnemies.Remove(spawnableDaytimeEnemy);
            }
        }

        internal static bool TryInjectEnemyIntoPool(List<SpawnableEnemyWithRarity> enemyPool, ExtendedEnemyType extendedEnemy, int newRarity, out SpawnableEnemyWithRarity spawnableEnemyWithRarity)
        {
            spawnableEnemyWithRarity = null;
            foreach (SpawnableEnemyWithRarity currentSpawnableEnemyWithRarity in enemyPool)
                if (currentSpawnableEnemyWithRarity.enemyType == extendedEnemy.EnemyType)
                    spawnableEnemyWithRarity = currentSpawnableEnemyWithRarity;

            if (spawnableEnemyWithRarity != null)
            {
                if (newRarity > 0)
                    spawnableEnemyWithRarity.rarity = newRarity;    
            }
            else
            {
                SpawnableEnemyWithRarity newEnemy = Utilities.Create(extendedEnemy.EnemyType, newRarity);
                spawnableEnemyWithRarity = newEnemy;
                enemyPool.Add(newEnemy);
            }

            return (spawnableEnemyWithRarity.rarity > 0);
        }

        protected override (bool result, string log) ValidateExtendedContent(ExtendedEnemyType extendedEnemyType)
        {
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

        protected override void PopulateContentTerminalData(ExtendedEnemyType content)
        {
            TerminalNode infoNode = null;
            TerminalKeyword infoKeyword = null;
            VideoClip videoClip = content.InfoNodeVideoClip;
            if (Terminal.enemyFiles.Count > content.GameID)
            {
                infoNode = Terminal.enemyFiles[content.GameID];
                videoClip = infoNode.displayVideo;
                if (Keywords.Info.compatibleNouns.TryGet(infoNode, out TerminalKeyword noun))
                    infoKeyword = noun;
            }
            else
            {
                infoKeyword = Terminal.CreateKeyword(content.name + "BestiaryKeyword", content.EnemyDisplayName.ToLower(), Keywords.Info);
                infoNode = Terminal.CreateNode(content.name + "BestiaryNode", content.InfoNodeDescription);
                infoNode.creatureName = content.EnemyDisplayName;
                infoNode.playSyncedClip = 2;
                infoNode.displayVideo = content.InfoNodeVideoClip;
                infoNode.loadImageSlowly = content.InfoNodeVideoClip != null;
            }
            content.NounKeyword = infoKeyword;
            content.InfoNode = infoNode;
            content.InfoNodeVideoClip = videoClip;
        }
    }

    struct EnemyData
    {
        public EnemyAI enemyAI;
        public GameObject gamePrefab;
        public GameObject networkPrefab;
    }
}
