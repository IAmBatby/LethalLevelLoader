using IL;
using On;
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
        internal static void RefreshDynamicEnemyTypeRarityOnAllExtendedLevels()
        {
            foreach (ExtendedLevel extendedLevel in PatchedContent.ExtendedLevels)
                InjectCustomEnemyTypesIntoLevelViaDynamicRarity(extendedLevel);
        }

        internal static void InjectCustomEnemyTypesIntoLevelViaDynamicRarity(ExtendedLevel extendedLevel, bool debugResults = false)
        {
            foreach (ExtendedEnemyType extendedEnemyType in PatchedContent.CustomExtendedEnemyTypes)
            {
                string debugString = string.Empty;
                SpawnableEnemyWithRarity alreadyInjectedInsideEnemy = null;
                SpawnableEnemyWithRarity alreadyInjectedOutsideEnemy = null;
                SpawnableEnemyWithRarity alreadyInjectedDaytimeEnemy = null;

                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.selectableLevel.Enemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedInsideEnemy = spawnableEnemyWithRarity;
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.selectableLevel.OutsideEnemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedOutsideEnemy = spawnableEnemyWithRarity;
                foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in extendedLevel.selectableLevel.DaytimeEnemies)
                    if (spawnableEnemyWithRarity.enemyType == extendedEnemyType)
                        alreadyInjectedDaytimeEnemy = spawnableEnemyWithRarity;


                int insideLevelRarity = extendedEnemyType.insideLevelMatchingProperties.GetDynamicRarity(extendedLevel);
                //int insideDungeonRarity = extendedEnemyType.insideDungeonMatchingProperties.GetDynamicRarity(extendedLevel);
                int outsideLevelRarity = extendedEnemyType.outsideLevelMatchingProperties.GetDynamicRarity(extendedLevel);
                int daytimeLevelRarity = extendedEnemyType.daytimeLevelMatchingProperties.GetDynamicRarity(extendedLevel);

                if (TryInjectEnemyIntoPool(extendedLevel.selectableLevel.Enemies, extendedEnemyType, insideLevelRarity, out SpawnableEnemyWithRarity spawnableInsideEnemy) == false)
                    extendedLevel.selectableLevel.Enemies.Remove(spawnableInsideEnemy);
                if (TryInjectEnemyIntoPool(extendedLevel.selectableLevel.OutsideEnemies, extendedEnemyType, outsideLevelRarity, out SpawnableEnemyWithRarity spawnableOutsideEnemy) == false)
                    extendedLevel.selectableLevel.OutsideEnemies.Remove(spawnableOutsideEnemy);
                if (TryInjectEnemyIntoPool(extendedLevel.selectableLevel.DaytimeEnemies, extendedEnemyType, daytimeLevelRarity, out SpawnableEnemyWithRarity spawnableDaytimeEnemy) == false)
                    extendedLevel.selectableLevel.DaytimeEnemies.Remove(spawnableDaytimeEnemy);
            }
        }

        internal static bool TryInjectEnemyIntoPool(List<SpawnableEnemyWithRarity> enemyPool, ExtendedEnemyType extendedEnemy, int newRarity, out SpawnableEnemyWithRarity spawnableEnemyWithRarity)
        {
            spawnableEnemyWithRarity = null;
            foreach (SpawnableEnemyWithRarity currentSpawnableEnemyWithRarity in enemyPool)
                if (currentSpawnableEnemyWithRarity.enemyType == extendedEnemy)
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

            DebugHelper.Log("Highest Enemy ScanNode Creature ID Was: " + highestVanillaEnemyScanNodeCreatureID);

            int counter = 1; //we want this to be 1
            foreach (ExtendedEnemyType extendedEnemyType in customEnemyTypes)
            {
                ScanNodeProperties enemyScanNode = extendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                if (enemyScanNode != null)
                {
                    extendedEnemyType.ScanNodeProperties = enemyScanNode;
                    extendedEnemyType.ScanNodeProperties.creatureScanID = (highestVanillaEnemyScanNodeCreatureID + counter);
                    DebugHelper.Log("Setting Custom EnemyType: " + extendedEnemyType.EnemyType.enemyName + " ID To: " + (highestVanillaEnemyScanNodeCreatureID + counter));
                }
                counter++;
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
