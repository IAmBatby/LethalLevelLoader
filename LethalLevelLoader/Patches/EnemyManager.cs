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

            List<EnemyType> enemyTypes = PatchedContent.ExtendedEnemyTypes.Select(e => e.EnemyType).ToList();

            foreach (EnemyType enemyType in enemyTypes)
            {
                    DebugHelper.Log("Resource Find EnemyAI: " + enemyType.enemyPrefab.gameObject.name + " | " + enemyType.enemyPrefab.gameObject.GetInstanceID() + " | " + enemyType.enemyName);
            }

            foreach (GameObject networkPrefab in GameNetworkManager.Instance.GetComponent<NetworkManager>().NetworkConfig.Prefabs.m_Prefabs.Select(p => p.Prefab))
                if (networkPrefab != null && networkPrefab.GetComponent<EnemyAI>() != null)
                    if (networkPrefab.GetComponent<EnemyAI>().enemyType != null)
                        DebugHelper.Log("Enemy NetworkPrefab: " + networkPrefab.gameObject.name + " | " + networkPrefab.GetInstanceID() + " | " + networkPrefab.GetComponent<EnemyAI>().enemyType.enemyName);
        }
    }

    struct EnemyData
    {
        public EnemyAI enemyAI;
        public GameObject gamePrefab;
        public GameObject networkPrefab;
    }
}
