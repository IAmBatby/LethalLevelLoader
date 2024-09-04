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
        public static LevelAmbienceLibraryRestore LevelAmbienceRestore { get; private set; } = new LevelAmbienceLibraryRestore();
        public static ItemGroupRestore ItemGroupRestore { get; private set; } = new ItemGroupRestore();

        public static AudioMixerGroupRestore AudioMixerGroupRestore { get; private set; } = new AudioMixerGroupRestore();
        public static ReverbPresetRestore ReverbPresetRestore { get; private set; } = new ReverbPresetRestore();

        internal static List<ContentRestore> ContentRestores = new List<Tools.ContentRestore>()
        {
            ItemRestore, EnemyRestore, SpawnableMapObjectRestore, SpawnableOutsideObjectRestore, LevelAmbienceRestore,
            ItemGroupRestore, AudioMixerGroupRestore, ReverbPresetRestore
        };

        public static T TryRestoreContent<T>(T newContent)
        {
            foreach (ContentRestore contentRestore in ContentRestores)
                if (contentRestore is ContentRestore<T> castedRestore)
                    return (castedRestore.TryRestoreContent(newContent));
            return (newContent);
        }

        internal static void RestoreVanillaLevelAssetReferences(ExtendedLevel extendedLevel)
        {
            SelectableLevel selectableLevel = extendedLevel.SelectableLevel;

            foreach (SpawnableItemWithRarity spawnableItem in new List<SpawnableItemWithRarity>(selectableLevel.spawnableScrap))
                if (spawnableItem.spawnableItem == null)
                    selectableLevel.spawnableScrap.Remove(spawnableItem);

            ItemRestore.TryRestoreContents(selectableLevel.spawnableScrap);
            EnemyRestore.TryRestoreContents(selectableLevel.Enemies);
            EnemyRestore.TryRestoreContents(selectableLevel.DaytimeEnemies);
            EnemyRestore.TryRestoreContents(selectableLevel.OutsideEnemies);
            SpawnableMapObjectRestore.TryRestoreContents(selectableLevel.spawnableMapObjects);
            SpawnableOutsideObjectRestore.TryRestoreContents(selectableLevel.spawnableOutsideObjects);
            selectableLevel.levelAmbienceClips = LevelAmbienceRestore.TryRestoreContent(selectableLevel.levelAmbienceClips);
        }

        internal static void RestoreVanillaInteriorAssetReferences(ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (Tile tile in extendedDungeonFlow.DungeonFlow.GetTiles())
                RestoreVanillaTileAssetReferences(tile);
        }

        internal static void RestoreVanillaTileAssetReferences(Tile tile)
        {
            ItemGroupRestore.TryRestoreContents(tile.GetComponentsInChildren<RandomScrapSpawn>());
            SpawnableMapObjectRestore.TryRestoreContents(tile.GetComponentInChildren<RandomMapObject>());
        }

        internal static void RestoreVanillaParentAudioAssetReferences(GameObject parent)
        {
            AudioMixerGroupRestore.TryRestoreContents(parent.GetComponentsInChildren<AudioSource>(true), destroyOnRestore: false);
            ReverbPresetRestore.TryRestoreContents(parent.GetComponentsInChildren<AudioReverbTrigger>(true));
        }

        internal static void FlushAll()
        {
            foreach (ContentRestore contentRestore in ContentRestores)
                contentRestore.Flush();
        }
    }
}
