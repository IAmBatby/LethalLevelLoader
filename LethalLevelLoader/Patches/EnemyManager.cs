using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public class EnemyManager
    {
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
                SpawnableEnemyWithRarity newSpawnableEnemy = new SpawnableEnemyWithRarity();
                newSpawnableEnemy.enemyType = extendedEnemy.EnemyType;
                newSpawnableEnemy.rarity = newRarity;
                spawnableEnemyWithRarity = newSpawnableEnemy;
                enemyPool.Add(newSpawnableEnemy);
            }


            if (spawnableEnemyWithRarity.rarity == 0)
                return (false);
            else
                return (true);
        }

        internal static void UpdateEnemyIDs()
        {
            
            /*foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.VanillaExtendedEnemyTypes)
            {

            }*/

            List<ExtendedEnemyType> vanillaEnemyTypes = PatchedContent.VanillaExtendedEnemyTypes;
            List<ExtendedEnemyType> customEnemyTypes = PatchedContent.CustomExtendedEnemyTypes;
            int highestVanillaEnemyScanNodeCreatureID = -1;

            foreach (ExtendedEnemyType extendedEnemyType in vanillaEnemyTypes)
                if (extendedEnemyType.EnemyID > highestVanillaEnemyScanNodeCreatureID)
                    highestVanillaEnemyScanNodeCreatureID = extendedEnemyType.EnemyID;


            int counter = 1; //we want this to be 1
            foreach (ExtendedEnemyType extendedEnemyType in customEnemyTypes)
            {
                ScanNodeProperties enemyScanNode = extendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                if (enemyScanNode != null)
                {
                    extendedEnemyType.ScanNodeProperties = enemyScanNode;
                    extendedEnemyType.ScanNodeProperties.creatureScanID = (highestVanillaEnemyScanNodeCreatureID + counter);
                    extendedEnemyType.EnemyID = (highestVanillaEnemyScanNodeCreatureID + counter);
                    DebugHelper.Log("Setting Custom EnemyType: " + extendedEnemyType.EnemyType.enemyName + " ID To: " + (highestVanillaEnemyScanNodeCreatureID + counter), DebugType.Developer);
                }
                counter++;
            }
        }

        internal static void AddCustomEnemyTypesToTestAllEnemiesLevel()
        {
            QuickMenuManager quickMenuManager = UnityEngine.Object.FindAnyObjectByType<QuickMenuManager>();

            if (quickMenuManager != null)
            {
                foreach (ExtendedEnemyType customEnemyType in PatchedContent.CustomExtendedEnemyTypes)
                {
                    SpawnableEnemyWithRarity spawnableEnemyWithRarity = new SpawnableEnemyWithRarity();
                    spawnableEnemyWithRarity.enemyType = customEnemyType.EnemyType;
                    spawnableEnemyWithRarity.rarity = 300;
                    quickMenuManager.testAllEnemiesLevel.Enemies.Add(spawnableEnemyWithRarity);
                    quickMenuManager.testAllEnemiesLevel.OutsideEnemies.Add(spawnableEnemyWithRarity);
                    quickMenuManager.testAllEnemiesLevel.DaytimeEnemies.Add(spawnableEnemyWithRarity);
                }
            }
        }
    }

    struct EnemyData
    {
        public EnemyAI enemyAI;
        public GameObject gamePrefab;
        public GameObject networkPrefab;
    }
}
