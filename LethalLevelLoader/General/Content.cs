using DunGen.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace LethalLevelLoader
{
    public static class PatchedContent
    {
        public static ExtendedMod VanillaMod { get; internal set; }

        public static List<string> AllLevelSceneNames { get; internal set; } = new List<string>();

        public static List<ExtendedMod> ExtendedMods { get; internal set; } = new List<ExtendedMod>();

        public static List<ExtendedLevel> ExtendedLevels { get; internal set; } = new List<ExtendedLevel>();

        public static List<ExtendedLevel> VanillaExtendedLevels
        {
            get
            {
                List<ExtendedLevel> list = new List<ExtendedLevel>();
                foreach (ExtendedLevel level in ExtendedLevels)
                    if (level.ContentType == ContentType.Vanilla)
                        list.Add(level);
                return (list);
            }
        }

        public static List<ExtendedLevel> CustomExtendedLevels
        {
            get
            {
                List<ExtendedLevel> list = new List<ExtendedLevel>();
                foreach (ExtendedLevel level in ExtendedLevels)
                    if (level.ContentType == ContentType.Custom)
                        list.Add(level);
                return (list);
            }
        }

        public static List<SelectableLevel> SeletectableLevels
        {
            get
            {
                List<SelectableLevel> list = new List<SelectableLevel>();
                foreach (ExtendedLevel level in ExtendedLevels)
                    list.Add(level.selectableLevel);
                return (list);
            }
       
        }

        public static List<SelectableLevel> MoonsCatalogue
        {
            get
            {
                List<SelectableLevel> list = new List<SelectableLevel>();
                foreach (SelectableLevel selectableLevel in OriginalContent.MoonsCatalogue)
                    list.Add(selectableLevel);
                foreach (ExtendedLevel level in ExtendedLevels)
                    if (level.ContentType == ContentType.Custom)
                        list.Add(level.selectableLevel);
                return (list);
            }
        }

        public static List<ExtendedDungeonFlow> ExtendedDungeonFlows { get; internal set; } = new List<ExtendedDungeonFlow>();

        public static List<ExtendedDungeonFlow> VanillaExtendedDungeonFlows
        {
            get
            {
                List<ExtendedDungeonFlow> list = new List<ExtendedDungeonFlow>();
                foreach (ExtendedDungeonFlow dungeon in ExtendedDungeonFlows)
                    if (dungeon.ContentType == ContentType.Vanilla)
                        list.Add(dungeon);
                return (list);
            }
        }

        public static List<ExtendedDungeonFlow> CustomExtendedDungeonFlows
        {
            get
            {
                List<ExtendedDungeonFlow> list = new List<ExtendedDungeonFlow>();
                foreach (ExtendedDungeonFlow dungeon in ExtendedDungeonFlows)
                    if (dungeon.ContentType == ContentType.Custom)
                        list.Add(dungeon);
                return (list);
            }
        }

        public static List<ExtendedWeatherEffect> ExtendedWeatherEffects { get; internal set; } = new List<ExtendedWeatherEffect>();

        public static List<ExtendedWeatherEffect> VanillaExtendedWeatherEffects
        {
            get
            {
                List<ExtendedWeatherEffect> list = new List<ExtendedWeatherEffect>();
                foreach (ExtendedWeatherEffect effect in ExtendedWeatherEffects)
                    if (effect.contentType == ContentType.Vanilla)
                        list.Add(effect);
                return (list);
            }
        }

        public static List<ExtendedWeatherEffect> CustomExtendedWeatherEffects
        {
            get
            {
                List<ExtendedWeatherEffect> list = new List<ExtendedWeatherEffect>();
                foreach (ExtendedWeatherEffect effect in ExtendedWeatherEffects)
                    if (effect.contentType == ContentType.Custom)
                        list.Add(effect);
                return (list);
            }
        }

        public static List<ExtendedItem> ExtendedItems { get; internal set; } = new List<ExtendedItem>();

        public static List<ExtendedItem> CustomExtendedItems
        {
            get
            {
                List<ExtendedItem> returnList = new List<ExtendedItem>();
                foreach (ExtendedItem item in ExtendedItems)
                    if (item.ContentType == ContentType.Custom)
                        returnList.Add(item);
                return (returnList);
            }
        }

        public static List<ExtendedEnemyType> ExtendedEnemyTypes { get; internal set; } = new List<ExtendedEnemyType>();

        public static List<ExtendedEnemyType> CustomExtendedEnemyTypes
        {
            get
            {
                List<ExtendedEnemyType> returnList = new List<ExtendedEnemyType>();
                foreach (ExtendedEnemyType extendedEnemyType in ExtendedEnemyTypes)
                    if (extendedEnemyType.ContentType == ContentType.Custom)
                        returnList.Add(extendedEnemyType);
                return (returnList);
            }
        }

        public static List<ExtendedEnemyType> VanillaExtendedEnemyTypes
        {
            get
            {
                List<ExtendedEnemyType> returnList = new List<ExtendedEnemyType>();
                foreach (ExtendedEnemyType extendedEnemyType in ExtendedEnemyTypes)
                    if (extendedEnemyType.ContentType == ContentType.Vanilla)
                        returnList.Add(extendedEnemyType);
                return (returnList);
            }
        }

        public static List<AudioMixer> AudioMixers { get; internal set; } = new List<AudioMixer>();

        public static List<AudioMixerGroup> AudioMixerGroups { get; internal set; } = new List<AudioMixerGroup>();

        public static List<AudioMixerSnapshot> AudioMixerSnapshots { get; internal set; } = new List<AudioMixerSnapshot>();


        //Items

        public static List<Item> Items { get; internal set; } = new List<Item>();

        //Enemies

        public static List<EnemyType> Enemies { get; internal set; } = new List<EnemyType>();


        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (string.IsNullOrEmpty(extendedDungeonFlow.name))
            {
                DebugHelper.LogWarning("Tried to register ExtendedDungeonFlow with missing name! Setting to DungeonFlow name for safety!");
                extendedDungeonFlow.name = extendedDungeonFlow.dungeonFlow.name;
            }
            AssetBundleLoader.RegisterNewExtendedContent(extendedDungeonFlow, extendedDungeonFlow.name);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            AssetBundleLoader.RegisterNewExtendedContent(extendedLevel, extendedLevel.name);
        }

        public static void RegisterExtendedMod(ExtendedMod extendedMod)
        {

        }

        internal static void SortExtendedMods()
        {
            ExtendedMods = new List<ExtendedMod>(ExtendedMods.OrderBy(o => o.ModName).ToList());

            foreach (ExtendedMod extendedMod in ExtendedMods)
            {
                extendedMod.SortRegisteredContent();
            }
        }
    }

    public static class OriginalContent
    {
        //Levels

        public static List<SelectableLevel> SelectableLevels { get; internal set; } = new List<SelectableLevel>();

        public static List<SelectableLevel> MoonsCatalogue { get; internal set; } = new List<SelectableLevel>();

        //Dungeons

        public static List<DungeonFlow> DungeonFlows { get; internal set; } = new List<DungeonFlow>();

        //Items

        public static List<Item> Items { get; internal set; } = new List<Item>();

        public static List<ItemGroup> ItemGroups { get; internal set; } = new List<ItemGroup>();

        //Enemies

        public static List<EnemyType> Enemies { get; internal set; } = new List<EnemyType>();

        //Spawnable Objects

        public static List<SpawnableOutsideObject> SpawnableOutsideObjects { get; internal set; } = new List<SpawnableOutsideObject>();

        public static List<GameObject> SpawnableMapObjects { get; internal set; } = new List<GameObject>();

        //Audio

        public static List<AudioMixer> AudioMixers { get; internal set; } = new List<AudioMixer>();

        public static List<AudioMixerGroup> AudioMixerGroups { get; internal set; } = new List<AudioMixerGroup>();

        public static List<AudioMixerSnapshot> AudioMixerSnapshots { get; internal set; } = new List<AudioMixerSnapshot>();

        public static List<LevelAmbienceLibrary> LevelAmbienceLibraries { get; internal set; } = new List<LevelAmbienceLibrary>();

        public static List<ReverbPreset> ReverbPresets { get; internal set; } = new List<ReverbPreset>();

        //Terminal

        public static List<TerminalKeyword> TerminalKeywords { get; internal set; } = new List<TerminalKeyword>();

        public static List<TerminalNode> TerminalNodes { get; internal set; } = new List<TerminalNode>();
    }
}
