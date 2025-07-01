using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LethalLevelLoader.Tools
{
    internal static class ContentRestorer
    {
        internal static List<Object> objectsToDestroy = new List<Object>();

        internal static void RestoreVanillaDungeonAssetReferences(ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (extendedDungeonFlow == null)
            {
                DebugHelper.LogError("Tried To Restore Null Vanilla ExtendedDungeonFlow! Returning!", DebugType.User);
                return;
            }
            if (extendedDungeonFlow.DungeonFlow == null)
            {
                DebugHelper.LogError("Tried To Restore Null Vanilla ExtendedDungeonFlow " + extendedDungeonFlow.DungeonName +  " But DungeonFlow Was Null! Returning!", DebugType.User);
                return;
            }

            foreach (Tile tile in extendedDungeonFlow.DungeonFlow.GetTiles())
            {
                foreach (RandomScrapSpawn randomScrapSpawn in tile.gameObject.GetComponentsInChildren<RandomScrapSpawn>())
                    foreach (ItemGroup vanillaItemGroup in OriginalContent.ItemGroups)
                        if (vanillaItemGroup != null)
                            if (randomScrapSpawn.spawnableItems != null && randomScrapSpawn.spawnableItems.name != null)
                                if (vanillaItemGroup.name != null && randomScrapSpawn.spawnableItems.name == vanillaItemGroup.name)
                                    randomScrapSpawn.spawnableItems = RestoreAsset(randomScrapSpawn.spawnableItems, vanillaItemGroup, destroyOnReplace: false);
            }
            foreach (RandomMapObject randomMapObject in extendedDungeonFlow.DungeonFlow.GetRandomMapObjects())
            {
                foreach (GameObject spawnablePrefab in new List<GameObject>(randomMapObject.spawnablePrefabs))
                    foreach (GameObject vanillaPrefab in OriginalContent.SpawnableMapObjects)
                        if (vanillaPrefab != null && spawnablePrefab != null && spawnablePrefab.name != null &&  vanillaPrefab.name != null && spawnablePrefab.name == vanillaPrefab.name)
                            randomMapObject.spawnablePrefabs[randomMapObject.spawnablePrefabs.IndexOf(spawnablePrefab)] = RestoreAsset(randomMapObject.spawnablePrefabs[randomMapObject.spawnablePrefabs.IndexOf(spawnablePrefab)], vanillaPrefab, destroyOnReplace: false);
            }
            foreach (Tile tile in extendedDungeonFlow.DungeonFlow.GetTiles())
                RestoreAudioAssetReferencesInParent(tile.gameObject);
        }

        internal static void RestoreVanillaLevelAssetReferences(ExtendedLevel extendedLevel)
        {
            foreach (SpawnableItemWithRarity spawnableItem in new List<SpawnableItemWithRarity>(extendedLevel.SelectableLevel.spawnableScrap))
            {
                if (spawnableItem.spawnableItem == null)
                    extendedLevel.SelectableLevel.spawnableScrap.Remove(spawnableItem);
                else
                    foreach (Item vanillaItem in OriginalContent.Items)
                        if (spawnableItem.spawnableItem.name == vanillaItem.name)
                            spawnableItem.spawnableItem = RestoreAsset(spawnableItem.spawnableItem, vanillaItem);
            }

            foreach (EnemyType vanillaEnemyType in OriginalContent.Enemies)
                foreach (SpawnableEnemyWithRarity enemyRarityPair in extendedLevel.SelectableLevel.Enemies.Concat(extendedLevel.SelectableLevel.DaytimeEnemies).Concat(extendedLevel.SelectableLevel.OutsideEnemies))
                    if (enemyRarityPair.enemyType != null && !string.IsNullOrEmpty(enemyRarityPair.enemyType.name) && enemyRarityPair.enemyType.name == vanillaEnemyType.name)
                        enemyRarityPair.enemyType = RestoreAsset(enemyRarityPair.enemyType, vanillaEnemyType);

            foreach (SpawnableMapObject spawnableMapObject in extendedLevel.SelectableLevel.spawnableMapObjects)
                foreach (GameObject vanillaSpawnableMapObject in OriginalContent.SpawnableMapObjects)
                    if (spawnableMapObject.prefabToSpawn != null && spawnableMapObject.prefabToSpawn.name == vanillaSpawnableMapObject.name)
                        spawnableMapObject.prefabToSpawn = RestoreAsset(spawnableMapObject.prefabToSpawn, vanillaSpawnableMapObject);

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObject in extendedLevel.SelectableLevel.spawnableOutsideObjects)
                foreach (SpawnableOutsideObject vanillaSpawnableOutsideObject in OriginalContent.SpawnableOutsideObjects)
                    if (spawnableOutsideObject.spawnableObject != null && spawnableOutsideObject.spawnableObject.name == vanillaSpawnableOutsideObject.name)
                        spawnableOutsideObject.spawnableObject = RestoreAsset(spawnableOutsideObject.spawnableObject, vanillaSpawnableOutsideObject);

            foreach (LevelAmbienceLibrary vanillaAmbienceLibrary in OriginalContent.LevelAmbienceLibraries)
                if (extendedLevel.SelectableLevel.levelAmbienceClips != null && extendedLevel.SelectableLevel.levelAmbienceClips.name == vanillaAmbienceLibrary.name)
                    extendedLevel.SelectableLevel.levelAmbienceClips = RestoreAsset(extendedLevel.SelectableLevel.levelAmbienceClips, vanillaAmbienceLibrary);
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
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioSource.gameObject.name + " Has Missing AudioMixerGroup", DebugType.Developer);
                }
                else
                    TryRestoreAudioSource(audioSource);
            }

            foreach (AudioReverbTrigger audioReverbTrigger in parent.GetComponentsInChildren<AudioReverbTrigger>(includeInactive: true))
            {
                if (audioReverbTrigger.reverbPreset == null)
                    DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing ReverbPreset", DebugType.Developer);
                else
                {
                    foreach (ReverbPreset reverbPreset in OriginalContent.ReverbPresets)
                        if (reverbPreset != null && reverbPreset.name != null && audioReverbTrigger.reverbPreset.name == reverbPreset.name)
                        {
                            DebugHelper.Log("Restoring ReverbPreset: " + audioReverbTrigger.reverbPreset.name + " In AudioReverbTrigger: " + audioReverbTrigger.gameObject.name, DebugType.Developer);
                            audioReverbTrigger.reverbPreset = RestoreAsset(audioReverbTrigger.reverbPreset, reverbPreset, debugAction: false);
                        }
                }
                foreach (switchToAudio audioChange in audioReverbTrigger.audioChanges)
                {
                    if (audioChange.audio == null)
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing AudioChange AudioSource", DebugType.Developer);
                    else
                        TryRestoreAudioSource(audioChange.audio);
                    if (audioChange.changeToClip == null)
                        DebugHelper.LogWarning("Audio Restoration Warning: " + audioReverbTrigger.gameObject.name + " Has Missing AudioChange AudioClip", DebugType.Developer);
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
                //if (audioSource.clip != null)
                    //DebugHelper.Log("Restoring Audio Assets On AudioSource: " + audioSource.gameObject.name + ", AudioSource contained AudioClip: " + audioSource.clip.name);
                //else
                    //DebugHelper.Log("Restoring Audio Assets On AudioSource: " + audioSource.gameObject.name);
                audioSource.outputAudioMixerGroup = restoredMixerGroup;
            }
        }

        internal static void DestroyRestoredAssets(bool debugAction = false)
        {
            foreach (Object objectToDestroy in objectsToDestroy)
            {
                if (debugAction == true)
                    DebugHelper.Log("Destroying: " + objectToDestroy.name, DebugType.Developer);
                UnityEngine.Object.DestroyImmediate(objectToDestroy);
            }
            objectsToDestroy.Clear();
        }

        internal static void TryRestoreWaterShader(Material customMaterial)
        {
            if (customMaterial == null || customMaterial.shader == null || string.IsNullOrEmpty(customMaterial.shader.name))
                return;

            if (customMaterial.shader == LevelLoader.vanillaWaterShader)
                return;

            if (customMaterial.shader.name == LevelLoader.vanillaWaterShader.name)
            {
                customMaterial.shader = LevelLoader.vanillaWaterShader;
                customMaterial.DisableKeyword("_BLENDMODE_ALPHA");
                customMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                customMaterial.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
                customMaterial.EnableKeyword("_DISABLE_SSR_TRANSPARENT");
            }
        }


        internal static T RestoreAsset<T>(UnityEngine.Object currentAsset, T newAsset, bool debugAction = false, bool destroyOnReplace = true)
        {
            if (currentAsset != null && newAsset != null)
            {
                if (debugAction == true && currentAsset.name != null)
                    DebugHelper.Log("Restoring " + currentAsset.GetType().ToString() + ": Old Asset Name: " + currentAsset.name + " , New Asset Name: ", DebugType.Developer);

                if (destroyOnReplace == true)
                    if (!objectsToDestroy.Contains(currentAsset))
                        objectsToDestroy.Add(currentAsset);
            }
            else
                DebugHelper.LogWarning("Asset Restoration Failed, Null Reference Found!", DebugType.Developer);
            return (newAsset);
        }

    }
}
