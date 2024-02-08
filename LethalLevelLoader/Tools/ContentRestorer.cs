using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace LethalLevelLoader.Tools
{
    internal static class ContentRestorer
    {
        internal static void RestoreVanillaDungeonAssetReferences(ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (extendedDungeonFlow == null)
            {
                DebugHelper.LogError("Tried To Restore Null Vanilla ExtendedDungeonFlow! Returning!");
                return;
            }
            if (extendedDungeonFlow.dungeonFlow == null)
            {
                DebugHelper.LogError("Tried To Restore Null Vanilla ExtendedDungeonFlow " + extendedDungeonFlow.dungeonDisplayName +  " But DungeonFlow Was Null! Returning!");
                return;
            }

            foreach (Tile tile in extendedDungeonFlow.dungeonFlow.GetTiles())
            {
                foreach (RandomScrapSpawn randomScrapSpawn in tile.gameObject.GetComponentsInChildren<RandomScrapSpawn>())
                    foreach (ItemGroup vanillaItemGroup in OriginalContent.ItemGroups)
                        if (vanillaItemGroup != null)
                            if (randomScrapSpawn.spawnableItems != null && randomScrapSpawn.spawnableItems.name != null)
                                if (vanillaItemGroup.name != null && randomScrapSpawn.spawnableItems.name == vanillaItemGroup.name)
                                    randomScrapSpawn.spawnableItems = RestoreAsset(randomScrapSpawn.spawnableItems, vanillaItemGroup, destroyOnReplace: false);
            }
            foreach (RandomMapObject randomMapObject in extendedDungeonFlow.dungeonFlow.GetRandomMapObjects())
            {
                foreach (GameObject spawnablePrefab in new List<GameObject>(randomMapObject.spawnablePrefabs))
                    foreach (GameObject vanillaPrefab in OriginalContent.SpawnableMapObjects)
                        if (vanillaPrefab != null && spawnablePrefab != null && spawnablePrefab.name != null &&  vanillaPrefab.name != null && spawnablePrefab.name == vanillaPrefab.name)
                            randomMapObject.spawnablePrefabs[randomMapObject.spawnablePrefabs.IndexOf(spawnablePrefab)] = RestoreAsset(randomMapObject.spawnablePrefabs[randomMapObject.spawnablePrefabs.IndexOf(spawnablePrefab)], vanillaPrefab, destroyOnReplace: false);
            }
            foreach (Tile tile in extendedDungeonFlow.dungeonFlow.GetTiles())
                RestoreAudioAssetReferencesInParent(tile.gameObject);
        }

        internal static void RestoreVanillaLevelAssetReferences(ExtendedLevel extendedLevel)
        {
            foreach (SpawnableItemWithRarity spawnableItem in extendedLevel.selectableLevel.spawnableScrap)
                foreach (Item vanillaItem in OriginalContent.Items)
                    if (spawnableItem.spawnableItem.itemName == vanillaItem.itemName)
                        spawnableItem.spawnableItem = RestoreAsset(spawnableItem.spawnableItem, vanillaItem, debugAction: true);

            foreach (EnemyType vanillaEnemyType in OriginalContent.Enemies)
                foreach (SpawnableEnemyWithRarity enemyRarityPair in extendedLevel.selectableLevel.Enemies.Concat(extendedLevel.selectableLevel.DaytimeEnemies).Concat(extendedLevel.selectableLevel.OutsideEnemies))
                    if (enemyRarityPair.enemyType != null && enemyRarityPair.enemyType.enemyName == vanillaEnemyType.enemyName)
                        enemyRarityPair.enemyType = RestoreAsset(enemyRarityPair.enemyType, vanillaEnemyType, debugAction: true);

            foreach (SpawnableMapObject spawnableMapObject in extendedLevel.selectableLevel.spawnableMapObjects)
                foreach (GameObject vanillaSpawnableMapObject in OriginalContent.SpawnableMapObjects)
                    if (spawnableMapObject.prefabToSpawn != null && spawnableMapObject.prefabToSpawn.name == vanillaSpawnableMapObject.name)
                        spawnableMapObject.prefabToSpawn = RestoreAsset(spawnableMapObject.prefabToSpawn, vanillaSpawnableMapObject, debugAction: true);

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObject in extendedLevel.selectableLevel.spawnableOutsideObjects)
                foreach (SpawnableOutsideObject vanillaSpawnableOutsideObject in OriginalContent.SpawnableOutsideObjects)
                    if (spawnableOutsideObject.spawnableObject != null && spawnableOutsideObject.spawnableObject.name == vanillaSpawnableOutsideObject.name)
                        spawnableOutsideObject.spawnableObject = RestoreAsset(spawnableOutsideObject.spawnableObject, vanillaSpawnableOutsideObject, debugAction: true);

            foreach (LevelAmbienceLibrary vanillaAmbienceLibrary in OriginalContent.LevelAmbienceLibraries)
                if (extendedLevel.selectableLevel.levelAmbienceClips != null && extendedLevel.selectableLevel.levelAmbienceClips.name == vanillaAmbienceLibrary.name)
                    extendedLevel.selectableLevel.levelAmbienceClips = RestoreAsset(extendedLevel.selectableLevel.levelAmbienceClips, vanillaAmbienceLibrary, debugAction: true);
        }

        internal static void RestoreAudioAssetReferencesInParent(GameObject parent)
        {
            //DebugHelper.Log("Validating & Restoring AudioSources");
            foreach (AudioSource audioSource in parent.GetComponentsInChildren<AudioSource>(includeInactive: true))
            {
                //DebugHelper.Log("Trying To Find And Restore Audio Assets In: " + audioSource.gameObject.name);
                if (audioSource.outputAudioMixerGroup == null)
                {
                    if (audioSource.gameObject.name != null)
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioSource.gameObject.name + " Has Missing AudioMixerGroup");
                }
                else
                    TryRestoreAudioSource(audioSource);
            }

            foreach (AudioReverbTrigger audioReverbTrigger in parent.GetComponentsInChildren<AudioReverbTrigger>(includeInactive: true))
            {
                if (audioReverbTrigger.reverbPreset == null)
                    DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing ReverbPreset");
                else
                {
                    foreach (ReverbPreset reverbPreset in OriginalContent.ReverbPresets)
                        if (reverbPreset.name != null && audioReverbTrigger.reverbPreset.name == reverbPreset.name)
                        {
                            DebugHelper.Log("Restoring ReverbPreset: " + audioReverbTrigger.reverbPreset.name + " In AudioReverbTrigger: " + audioReverbTrigger.gameObject.name);
                            audioReverbTrigger.reverbPreset = RestoreAsset(audioReverbTrigger.reverbPreset, reverbPreset, debugAction: false);
                        }
                }
                foreach (switchToAudio audioChange in audioReverbTrigger.audioChanges)
                {
                    if (audioChange.audio == null)
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing AudioChange AudioSource");
                    else
                        TryRestoreAudioSource(audioChange.audio);
                    if (audioChange.changeToClip == null)
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing AudioChange AudioClip");
                }
            }
        }

        internal static void TryRestoreAudioSource(AudioSource audioSource)
        {
            if (audioSource.outputAudioMixerGroup == null) return;

            AudioMixerGroup targetMixerGroup = audioSource.outputAudioMixerGroup;
            AudioMixer targetMixer = audioSource.outputAudioMixerGroup.audioMixer;

            AudioMixerGroup restoredMixerGroup = null;
            AudioMixer restoredMixer = null;

            foreach (AudioMixer vanillaMixer in OriginalContent.AudioMixers)
                if (targetMixer.name == vanillaMixer.name)
                    restoredMixer = RestoreAsset(targetMixer, vanillaMixer, destroyOnReplace: false);

            foreach (AudioMixerGroup vanillaMixerGroup in OriginalContent.AudioMixerGroups)
                if (targetMixerGroup.name == vanillaMixerGroup.name)
                    restoredMixerGroup = RestoreAsset(targetMixerGroup, vanillaMixerGroup, destroyOnReplace: false);

            if (restoredMixerGroup != null && restoredMixer != null)
            {
                if (audioSource.clip != null)
                    DebugHelper.Log("Restoring Audio Assets On AudioSource: " + audioSource.gameObject.name + ", AudioSource contained AudioClip: " + audioSource.clip.name);
                else
                    DebugHelper.Log("Restoring Audio Assets On AudioSource: " + audioSource.gameObject.name);
                audioSource.outputAudioMixerGroup = restoredMixerGroup;
            }
        }


        internal static T RestoreAsset<T>(UnityEngine.Object currentAsset, T newAsset, bool debugAction = false, bool destroyOnReplace = true)
        {
            if (currentAsset != null && newAsset != null)
            {
                //if (debugAction == true && currentAsset.name != null)
                    //DebugHelper.Log("Restoring " + currentAsset.GetType().ToString() + ": Old Asset Name: " + currentAsset.name + " , New Asset Name: " + newAsset);

                    if (destroyOnReplace == true)
                        UnityEngine.Object.DestroyImmediate(currentAsset);
            }
            else
                DebugHelper.LogWarning("Asset Restoration Failed, Null Reference Found!");
            return (newAsset);
        }

    }
}
