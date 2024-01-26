using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace LethalLevelLoader
{
    public static class PatchedContent
    {
        public static List<ExtendedLevel> ExtendedLevels { get; internal set; } = new List<ExtendedLevel>();

        public static List<ExtendedLevel> VanillaExtendedLevels
        {
            get
            {
                List<ExtendedLevel> list = new List<ExtendedLevel>();
                foreach (ExtendedLevel level in ExtendedLevels)
                    if (level.levelType == ContentType.Vanilla)
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
                    if (level.levelType == ContentType.Custom)
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
                    if (level.levelType == ContentType.Custom)
                        list.Add(level.selectableLevel);
                return (list);
            }
        }

        public static List<ExtendedDungeonFlow> ExtendedDungeonFlows { get; internal set; } = new List<ExtendedDungeonFlow>();

        public static List<string> AllExtendedLevelTags
        {
            get
            {
                List<string> allUniqueLevelTags = new List<string>();
                foreach (ExtendedLevel extendedLevel in ExtendedLevels)
                    foreach (string levelTag in extendedLevel.levelTags)
                        if (!allUniqueLevelTags.Contains(levelTag))
                            allUniqueLevelTags.Add(levelTag);
                return (allUniqueLevelTags);
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

        public static List<AudioMixerGroup> AudioMixerGroups { get; internal set; } = new List<AudioMixerGroup>();

        public static List<AudioMixer> AudioMixers { get; internal set; } = new List<AudioMixer>();

        public static List<LevelAmbienceLibrary> LevelAmbienceLibraries { get; internal set; } = new List<LevelAmbienceLibrary>();

        public static List<ReverbPreset> ReverbPresets { get; internal set; } = new List<ReverbPreset>();

        //Terminal

        public static List<TerminalKeyword> TerminalKeywords { get; internal set; } = new List<TerminalKeyword>();

        public static List<TerminalNode> TerminalNodes { get; internal set; } = new List<TerminalNode>();
    }
}
