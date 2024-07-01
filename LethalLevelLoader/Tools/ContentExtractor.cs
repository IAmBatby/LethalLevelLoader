using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    public class ContentExtractor
    {
        internal static void TryScrapeVanillaItems(StartOfRound startOfRound)
        {
            //This is a little obtuse but had some weird issues with this, will rework later.
            List<ItemGroup> extractedItemGroups = new List<ItemGroup>(Resources.FindObjectsOfTypeAll<ItemGroup>());
            foreach (Item item in startOfRound.allItemsList.itemsList)
            {
                if (item.spawnPrefab != null)
                {
                    TryAddReference(OriginalContent.Items, item);
                    foreach (ItemGroup itemGroup in item.spawnPositionTypes)
                    {
                        if (extractedItemGroups.Contains(itemGroup))
                        {
                            OriginalContent.ItemGroups.Add(itemGroup);
                            extractedItemGroups.Remove(itemGroup);
                        }
                    }
                }
            }
            OriginalContent.ItemGroups = OriginalContent.ItemGroups.Distinct().ToList();

        }
        internal static void TryScrapeVanillaContent(StartOfRound startOfRound, RoundManager roundManager)
        {
            if (Plugin.IsSetupComplete == false)
            {
                if (startOfRound != null)
                {
                    foreach (IndoorMapType indoorFlowType in roundManager.dungeonFlowTypes)
                        TryAddReference(OriginalContent.DungeonFlows, indoorFlowType.dungeonFlow);

                    foreach (SelectableLevel selectableLevel in startOfRound.levels)
                        ExtractSelectableLevelReferences(selectableLevel);

                    foreach (IndoorMapType indoorFlowType in roundManager.dungeonFlowTypes)
                        ExtractDungeonFlowReferences(indoorFlowType.dungeonFlow);
                }
                if (TerminalManager.Terminal.currentNode != null)
                    TryAddReference(OriginalContent.TerminalNodes, TerminalManager.Terminal.currentNode);

                foreach (TerminalNode terminalNode in TerminalManager.Terminal.terminalNodes.terminalNodes)
                    TryAddReference(OriginalContent.TerminalNodes, terminalNode);


                foreach (TerminalNode terminalNode in TerminalManager.Terminal.terminalNodes.specialNodes)
                    TryAddReference(OriginalContent.TerminalNodes, terminalNode);

                foreach (TerminalNode terminalNode in TerminalManager.Terminal.enemyFiles)
                    TryAddReference(OriginalContent.TerminalNodes, terminalNode);


                foreach (TerminalNode terminalNode in TerminalManager.Terminal.logEntryFiles)
                    TryAddReference(OriginalContent.TerminalNodes, terminalNode);


                foreach (TerminalNode terminalNode in TerminalManager.Terminal.ShipDecorSelection)
                    TryAddReference(OriginalContent.TerminalNodes, terminalNode);
                foreach (TerminalKeyword terminalKeyword in TerminalManager.Terminal.terminalNodes.allKeywords)
                {
                    TryAddReference(OriginalContent.TerminalKeywords, terminalKeyword);
                    if (terminalKeyword.compatibleNouns != null)
                        foreach (CompatibleNoun compatibleNoun in terminalKeyword.compatibleNouns)
                            if (compatibleNoun.result != null)
                                TryAddReference(OriginalContent.TerminalNodes, compatibleNoun.result);
                    if (terminalKeyword.specialKeywordResult != null)
                        TryAddReference(OriginalContent.TerminalNodes, terminalKeyword.specialKeywordResult);
                }
                foreach (TerminalNode terminalNode in new List<TerminalNode>(OriginalContent.TerminalNodes))
                    if (terminalNode.terminalOptions != null)
                        foreach (CompatibleNoun compatibleNoun in terminalNode.terminalOptions)
                            if (compatibleNoun.result != null)
                                TryAddReference(OriginalContent.TerminalNodes, compatibleNoun.result);

                ExtractMemoryLoadedAudioMixerGroups();

                //BAD BAD BAD
                foreach (ReverbPreset reverbPreset in Resources.FindObjectsOfTypeAll<ReverbPreset>())
                    TryAddReference(OriginalContent.ReverbPresets, reverbPreset);

                OriginalContent.SelectableLevels = new List<SelectableLevel>(startOfRound.levels.ToList());
                OriginalContent.MoonsCatalogue = new List<SelectableLevel>(TerminalManager.Terminal.moonsCatalogueList.ToList());

            }
            //DebugHelper.DebugScrapedVanillaContent();
        }

        internal static void TryScrapeCustomContent()
        {
            /*
            foreach (EnemyType enemyType in Resources.FindObjectsOfTypeAll(typeof(EnemyType)))
                if (!OriginalContent.Enemies.Contains(enemyType))
                    PatchedContent.Enemies.Add(enemyType);

            foreach (Item item in Resources.FindObjectsOfTypeAll(typeof(Item)))
                if (!OriginalContent.Items.Contains(item))
                    PatchedContent.Items.Add(item);*/
        }

        internal static void ObtainSpecialItemReferences()
        {
            foreach (GrabbableObject sceneGrabbableObject in Patches.StartOfRound.shipAnimator.gameObject.GetComponentsInChildren<GrabbableObject>())
                if (sceneGrabbableObject.itemProperties != null && !OriginalContent.Items.Contains(sceneGrabbableObject.itemProperties))
                    if (sceneGrabbableObject.itemProperties.spawnPrefab != null)
                        OriginalContent.Items.Add(sceneGrabbableObject.itemProperties);

            foreach (EnemyType enemyType in OriginalContent.Enemies)
            {
                if (enemyType.name == "Nutcracker_0")
                {
                    NutcrackerEnemyAI nutcrackerEnemy = enemyType.enemyPrefab.GetComponent<NutcrackerEnemyAI>();
                    if (nutcrackerEnemy != null)
                    {
                        OriginalContent.Items.Add(nutcrackerEnemy.gunPrefab.GetComponent<GrabbableObject>().itemProperties);
                        OriginalContent.Items.Add(nutcrackerEnemy.shotgunShellPrefab.GetComponent<GrabbableObject>().itemProperties);
                    }
                }
                else if (enemyType.name == "Butler_0")
                {
                    ButlerEnemyAI butlerEnemy = enemyType.enemyPrefab.GetComponent<ButlerEnemyAI>();
                    if (butlerEnemy != null)
                        OriginalContent.Items.Add(butlerEnemy.knifePrefab.GetComponent<GrabbableObject>().itemProperties);
                }
                else if (enemyType.name == "RedLocustBees")
                {
                    RedLocustBees beesEnemy = enemyType.enemyPrefab.GetComponent<RedLocustBees>();
                    if (beesEnemy != null)
                        OriginalContent.Items.Add(beesEnemy.hivePrefab.GetComponent<GrabbableObject>().itemProperties); 
                }
            }
        }

        internal static void ExtractMemoryLoadedAudioMixerGroups()
        {

            foreach (AudioMixer audioMixer in Resources.FindObjectsOfTypeAll(typeof(AudioMixer)))
            {
                if (!OriginalContent.AudioMixers.Contains(audioMixer))
                    TryAddReference(PatchedContent.AudioMixers, audioMixer);
            }

            foreach (AudioMixerGroup audioMixerGroup in Resources.FindObjectsOfTypeAll(typeof(AudioMixerGroup)))
            {
                if (OriginalContent.AudioMixers.Contains(audioMixerGroup.audioMixer))
                    TryAddReference(OriginalContent.AudioMixerGroups, audioMixerGroup);
                else
                    TryAddReference(PatchedContent.AudioMixerGroups, audioMixerGroup);
            }

            foreach (AudioMixerSnapshot audioMixerSnapshot in Resources.FindObjectsOfTypeAll(typeof(AudioMixerSnapshot)))
            {
                if (OriginalContent.AudioMixers.Contains(audioMixerSnapshot.audioMixer))
                    TryAddReference(OriginalContent.AudioMixerSnapshots, audioMixerSnapshot);
                else
                    TryAddReference(PatchedContent.AudioMixerSnapshots, audioMixerSnapshot);
            }
        }

        internal static void ExtractSelectableLevelReferences(SelectableLevel selectableLevel)
        {
            foreach (SpawnableEnemyWithRarity enemyWithRarity in selectableLevel.Enemies)
                TryAddReference(OriginalContent.Enemies, enemyWithRarity.enemyType);

            foreach (SpawnableEnemyWithRarity enemyWithRarity in selectableLevel.OutsideEnemies)
                TryAddReference(OriginalContent.Enemies, enemyWithRarity.enemyType);

            foreach (SpawnableEnemyWithRarity enemyWithRarity in selectableLevel.DaytimeEnemies)
                TryAddReference(OriginalContent.Enemies, enemyWithRarity.enemyType);

            foreach (SpawnableMapObject spawnableMapObject in selectableLevel.spawnableMapObjects)
                TryAddReference(OriginalContent.SpawnableMapObjects, spawnableMapObject.prefabToSpawn);

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObject in selectableLevel.spawnableOutsideObjects)
                TryAddReference(OriginalContent.SpawnableOutsideObjects, spawnableOutsideObject.spawnableObject);

            TryAddReference(OriginalContent.LevelAmbienceLibraries, selectableLevel.levelAmbienceClips);
        }

        internal static void ExtractDungeonFlowReferences(DungeonFlow dungeonFlow)
        {
            /*foreach (Tile tile in dungeonFlow.GetTiles())
                foreach (RandomScrapSpawn randomScrapSpawn in tile.gameObject.GetComponentsInChildren<RandomScrapSpawn>())
                    TryAddReference(OriginalContent.ItemGroups, randomScrapSpawn.spawnableItems);*/
        }

        internal static void TryAddReference<T>(List<T> referenceList, T reference) where T : UnityEngine.Object
        {
            if (!referenceList.Contains(reference))
                referenceList.Add(reference);
        }
    }
}
