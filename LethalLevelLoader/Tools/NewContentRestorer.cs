using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace LethalLevelLoader.Tools
{
    public static class NewContentRestorer
    {
        public static ItemRestore ItemRestore { get; private set; } = new ItemRestore();
        public static EnemyRestore EnemyRestore { get; private set; } = new EnemyRestore();
        public static SpawnableMapObjectRestore SpawnableMapObjectRestore { get; private set; } = new SpawnableMapObjectRestore();
        public static SpawnableOutsideObjectRestore SpawnableOutsideObjectRestore { get; private set; } = new SpawnableOutsideObjectRestore();
        public static UnityContentRestore<LevelAmbienceLibrary> LevelAmbienceRestore { get; private set; } = new UnityContentRestore<LevelAmbienceLibrary>();
        public static ItemGroupRestore ItemGroupRestore { get; private set; } = new ItemGroupRestore();

        public static AudioMixerGroupRestore AudioMixerGroupRestore { get; private set; } = new AudioMixerGroupRestore();
        public static ReverbPresetRestore ReverbPresetRestore { get; private set; } = new ReverbPresetRestore();

        internal static List<ContentRestore> ContentRestores = new List<Tools.ContentRestore>()
        {
            ItemRestore, EnemyRestore, SpawnableMapObjectRestore, SpawnableOutsideObjectRestore, LevelAmbienceRestore,
            ItemGroupRestore, AudioMixerGroupRestore, ReverbPresetRestore
        };

        internal static void RestoreVanillaLevelAssetReferences(ExtendedLevel extendedLevel)
        {
            SelectableLevel selectableLevel = extendedLevel.SelectableLevel;

            foreach (SpawnableItemWithRarity spawnableItem in new List<SpawnableItemWithRarity>(selectableLevel.spawnableScrap))
                if (spawnableItem.spawnableItem == null)
                    selectableLevel.spawnableScrap.Remove(spawnableItem);

            ItemRestore.TryRestoreContents(selectableLevel.spawnableScrap, OriginalContent.Items);

            EnemyRestore.TryRestoreContents(selectableLevel.Enemies, OriginalContent.Enemies);
            EnemyRestore.TryRestoreContents(selectableLevel.DaytimeEnemies, OriginalContent.Enemies);
            EnemyRestore.TryRestoreContents(selectableLevel.OutsideEnemies, OriginalContent.Enemies);

            SpawnableMapObjectRestore.TryRestoreContents(selectableLevel.spawnableMapObjects, OriginalContent.SpawnableMapObjects);

            SpawnableOutsideObjectRestore.TryRestoreContents(selectableLevel.spawnableOutsideObjects, OriginalContent.SpawnableOutsideObjects);

            selectableLevel.levelAmbienceClips = LevelAmbienceRestore.TryRestoreContent(selectableLevel.levelAmbienceClips, OriginalContent.LevelAmbienceLibraries);
        }

        internal static void RestoreVanillaInteriorAssetReferences(ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (Tile tile in extendedDungeonFlow.DungeonFlow.GetTiles())
                RestoreVanillaTileAssetReferences(tile);
        }

        internal static void RestoreVanillaTileAssetReferences(Tile tile)
        {
            foreach (RandomScrapSpawn spawn in tile.GetComponentsInChildren<RandomScrapSpawn>())
                spawn.spawnableItems = ItemGroupRestore.TryRestoreContent(spawn.spawnableItems, OriginalContent.ItemGroups);

            foreach (RandomMapObject spawn in tile.GetComponentsInChildren<RandomMapObject>())
                SpawnableMapObjectRestore.TryRestoreContents(spawn, OriginalContent.SpawnableMapObjects);
        }

        internal static void RestoreVanillaParentAudioAssetReferences(GameObject parent)
        {
            AudioSource[] allAudioSources = parent.GetComponentsInChildren<AudioSource>(true);
            AudioReverbTrigger[] allReverbTriggers = parent.GetComponentsInChildren<AudioReverbTrigger>(true);

            AudioMixerGroupRestore.TryRestoreContents(allAudioSources, OriginalContent.AudioMixerGroups, destroyOnRestore: false);
            ReverbPresetRestore.TryRestoreContents(allReverbTriggers, OriginalContent.ReverbPresets);
        }

        internal static void FlushAll()
        {
            foreach (ContentRestore contentRestore in ContentRestores)
                contentRestore.Flush();
        }
    }
}
