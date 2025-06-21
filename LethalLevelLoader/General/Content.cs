using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace LethalLevelLoader
{
    public static class PatchedContent
    {
        public static ExtendedMod VanillaMod { get; internal set; }

        public static List<string> AllLevelSceneNames { get; internal set; } = new List<string>();

        public static List<ExtendedMod> ExtendedMods { get; internal set; } = new List<ExtendedMod>();
        internal static Dictionary<string, ExtendedContent> UniqueIdentifiersDictionary = new Dictionary<string, ExtendedContent>();

        public static List<ExtendedLevel> ExtendedLevels => LevelManager.ExtendedContents;
        public static List<ExtendedLevel> VanillaExtendedLevels => GetContentOfType(ExtendedLevels, ContentType.Vanilla);
        public static List<ExtendedLevel> CustomExtendedLevels => GetContentOfType(ExtendedLevels, ContentType.Custom);
        internal static Dictionary<SelectableLevel, ExtendedLevel> ExtendedLevelDictionary = new Dictionary<SelectableLevel, ExtendedLevel>();

        [Obsolete("Use PatchedContent.SelectableLevels instead.")] // probably used by no mod, but this is public so we should be careful
        public static List<SelectableLevel> SeletectableLevels => SeletectableLevels;
        public static List<SelectableLevel> SelectableLevels => ExtendedLevels.Select(l => l.SelectableLevel).ToList();
        public static List<SelectableLevel> MoonsCatalogue => OriginalContent.MoonsCatalogue.Concat(CustomExtendedLevels.Select(l => l.SelectableLevel)).ToList(); //Weird but only cleaning up what this originally was doing so don't wanna poke it too much

        public static List<ExtendedDungeonFlow> ExtendedDungeonFlows => DungeonManager.ExtendedContents;
        public static List<ExtendedDungeonFlow> VanillaExtendedDungeonFlows => GetContentOfType(ExtendedDungeonFlows, ContentType.Vanilla);
        public static List<ExtendedDungeonFlow> CustomExtendedDungeonFlows => GetContentOfType(ExtendedDungeonFlows, ContentType.Custom);
        internal static Dictionary<DungeonFlow, ExtendedDungeonFlow> ExtendedDungeonFlowDictionary = new Dictionary<DungeonFlow, ExtendedDungeonFlow>();

        public static List<ExtendedItem> ExtendedItems => ItemManager.ExtendedContents;
        public static List<ExtendedItem> VanillaExtendedItems => GetContentOfType(ExtendedItems, ContentType.Vanilla);
        public static List<ExtendedItem> CustomExtendedItems => GetContentOfType(ExtendedItems, ContentType.Custom);
        internal static Dictionary<Item, ExtendedItem> ExtendedItemDictionary = new Dictionary<Item, ExtendedItem>();

        public static List<ExtendedEnemyType> ExtendedEnemyTypes => EnemyManager.ExtendedContents;
        public static List<ExtendedEnemyType> VanillaExtendedEnemyTypes => GetContentOfType(ExtendedEnemyTypes, ContentType.Vanilla);
        public static List<ExtendedEnemyType> CustomExtendedEnemyTypes => GetContentOfType(ExtendedEnemyTypes, ContentType.Custom);
        internal static Dictionary<EnemyType, ExtendedEnemyType> ExtendedEnemyTypeDictionary = new Dictionary<EnemyType, ExtendedEnemyType>();

        public static List<ExtendedWeatherEffect> ExtendedWeatherEffects => WeatherManager.ExtendedContents;
        public static List<ExtendedWeatherEffect> VanillaExtendedWeatherEffects => GetContentOfType(ExtendedWeatherEffects, ContentType.Vanilla);
        public static List<ExtendedWeatherEffect> CustomExtendedWeatherEffects => GetContentOfType(ExtendedWeatherEffects, ContentType.Custom);

        public static List<ExtendedBuyableVehicle> ExtendedBuyableVehicles => VehiclesManager.ExtendedContents;
        public static List<ExtendedBuyableVehicle> VanillaExtendedBuyableVehicles => GetContentOfType(ExtendedBuyableVehicles, ContentType.Vanilla);
        public static List<ExtendedBuyableVehicle> CustomExtendedBuyableVehicles => GetContentOfType(ExtendedBuyableVehicles, ContentType.Custom);
        internal static Dictionary<BuyableVehicle, ExtendedBuyableVehicle> ExtendedBuyableVehicleDictionary = new Dictionary<BuyableVehicle, ExtendedBuyableVehicle>();

        public static List<ExtendedUnlockableItem> ExtendedUnlockableItems => UnlockableItemManager.ExtendedContents;
        public static List<ExtendedUnlockableItem> CustomExtendedUnlockableItems => GetContentOfType(ExtendedUnlockableItems, ContentType.Custom);
        public static List<ExtendedUnlockableItem> VanillaExtendedUnlockableItems => GetContentOfType(ExtendedUnlockableItems, ContentType.Vanilla);
        internal static Dictionary<UnlockableItem, ExtendedUnlockableItem> ExtendedUnlockableItemDictionary = new Dictionary<UnlockableItem, ExtendedUnlockableItem>();

        public static List<AudioMixer> AudioMixers { get; internal set; } = new List<AudioMixer>();
        public static List<AudioMixerGroup> AudioMixerGroups { get; internal set; } = new List<AudioMixerGroup>();
        public static List<AudioMixerSnapshot> AudioMixerSnapshots { get; internal set; } = new List<AudioMixerSnapshot>();

        //Items

        public static List<Item> Items { get; internal set; } = new List<Item>();

        //Enemies

        public static List<EnemyType> Enemies { get; internal set; } = new List<EnemyType>();


        internal static List<ExtendedContentManager> ExtendedContentManagers = new List<ExtendedContentManager>();

        public static M GetExtendedManager<E,C,M>() where M : ExtendedContentManager, IExtendedManager<E,C,M> where E : ExtendedContent<E,C,M>
        {
            foreach (ExtendedContentManager manager in ExtendedContentManagers)
                if (manager is ExtendedContentManager<E, C, M> castManager)
                    return (castManager as M);
            return (null);
        }


        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            extendedDungeonFlow.ConvertObsoleteValues();
            if (string.IsNullOrEmpty(extendedDungeonFlow.name))
            {
                DebugHelper.LogWarning("Tried to register ExtendedDungeonFlow with missing name! Setting to DungeonFlow name for safety!", DebugType.Developer);
                extendedDungeonFlow.name = extendedDungeonFlow.DungeonFlow.name;
            }
            LethalBundleManager.RegisterNewExtendedContent(extendedDungeonFlow, null);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            LethalBundleManager.RegisterNewExtendedContent(extendedLevel, null);
        }

        public static void RegisterExtendedMod(ExtendedMod extendedMod)
        {
            DebugHelper.Log("Registering ExtendedMod: " + extendedMod.ModName + " Manually.", DebugType.IAmBatby);
            LethalBundleManager.RegisterExtendedMod(extendedMod, null);
        }

        internal static void SortExtendedMods()
        {
            ExtendedMods = new List<ExtendedMod>(ExtendedMods.OrderBy(o => o.ModName));

            foreach (ExtendedMod extendedMod in ExtendedMods)
                extendedMod.SortRegisteredContent();
        }

        internal static void PopulateContentDictionaries()
        {
            foreach (ExtendedLevel extendedLevel in ExtendedLevels)
            {
                TryAdd(ExtendedLevelDictionary, extendedLevel.SelectableLevel, extendedLevel);
                TryAddUUID(extendedLevel);
            }
            foreach (ExtendedDungeonFlow extendedDungeonFlow in ExtendedDungeonFlows)
            {
                TryAdd(ExtendedDungeonFlowDictionary, extendedDungeonFlow.DungeonFlow, extendedDungeonFlow);
                TryAddUUID(extendedDungeonFlow);
            }
            foreach (ExtendedItem extendedItem in ExtendedItems)
            {
                TryAdd(ExtendedItemDictionary, extendedItem.Item, extendedItem);
                TryAddUUID(extendedItem);
            }
            foreach (ExtendedEnemyType extendedEnemyType in ExtendedEnemyTypes)
            {
                TryAdd(ExtendedEnemyTypeDictionary, extendedEnemyType.EnemyType, extendedEnemyType);
                TryAddUUID(extendedEnemyType);
            }
            foreach (ExtendedBuyableVehicle extendedBuyableVehicle in ExtendedBuyableVehicles)
            {
                TryAdd(ExtendedBuyableVehicleDictionary, extendedBuyableVehicle.BuyableVehicle, extendedBuyableVehicle);
                TryAddUUID(extendedBuyableVehicle);
            }
            foreach (ExtendedUnlockableItem extendedUnlockableItem in ExtendedUnlockableItems)
            {
                TryAdd(ExtendedUnlockableItemDictionary, extendedUnlockableItem.UnlockableItem, extendedUnlockableItem);
                TryAddUUID(extendedUnlockableItem);
            }
        }

        internal static void TryAddUUID(ExtendedContent extendedContent)
        {
            TryAdd(UniqueIdentifiersDictionary, extendedContent.UniqueIdentificationName, extendedContent);
        }

        internal static bool TryAdd<T1,T2>(Dictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return (true);
            }
            else
            {
                DebugHelper.LogError("Could not add " + key.ToString() + " to dictionary.", DebugType.Developer);
                return (false);
            }
        }

        public static List<T> GetContentOfType<T>(List<T> list, ContentType type) where T : ExtendedContent
        {
            return (list.Where(c => c.ContentType == type).ToList());
        }

        public static bool TryGetExtendedContent(SelectableLevel selectableLevel, out ExtendedLevel extendedLevel)
        {
            return (ExtendedLevelDictionary.TryGetValue(selectableLevel, out extendedLevel));
        }

        public static bool TryGetExtendedContent(DungeonFlow dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow)
        {
            return (ExtendedDungeonFlowDictionary.TryGetValue(dungeonFlow, out extendedDungeonFlow));
        }

        public static bool TryGetExtendedContent(Item item, out ExtendedItem extendedItem)
        {
            return (ExtendedItemDictionary.TryGetValue(item, out extendedItem));
        }

        public static bool TryGetExtendedContent(EnemyType enemyType, out ExtendedEnemyType extendedEnemyType)
        {
            return (ExtendedEnemyTypeDictionary.TryGetValue(enemyType, out extendedEnemyType));
        }

        public static bool TryGetExtendedContent(BuyableVehicle buyableVehicle, out ExtendedBuyableVehicle extendedBuyableVehicle)
        {
            return (ExtendedBuyableVehicleDictionary.TryGetValue(buyableVehicle, out extendedBuyableVehicle));
        }

        public static bool TryGetExtendedContent(UnlockableItem unlockableItem, out ExtendedUnlockableItem extendedUnlockableItem)
        {
            return (ExtendedUnlockableItemDictionary.TryGetValue(unlockableItem, out extendedUnlockableItem));
        }

        public static bool TryGetExtendedContent<T>(string uniqueIdentifierName, out T extendedContent) where T : ExtendedContent
        {
            extendedContent = null;
            if (UniqueIdentifiersDictionary.TryGetValue(uniqueIdentifierName, out ExtendedContent result))
                extendedContent = result as T;
            return (extendedContent != null);
        }
    }

    public static class OriginalContent
    {
        public static StartOfRound StartOfRound => Patches.StartOfRound;
        public static RoundManager RoundManager => Patches.RoundManager;
        public static Terminal Terminal => Patches.Terminal;
        public static TimeOfDay TimeOfDay => Patches.TimeOfDay;
        
        //Levels
        public static List<SelectableLevel> SelectableLevels { get; internal set; } = new List<SelectableLevel>();
        public static List<SelectableLevel> MoonsCatalogue { get; internal set; } = new List<SelectableLevel>();

        //Dungeons
        public static List<DungeonFlow> DungeonFlows { get; internal set; } = new List<DungeonFlow>();

        //Items
        public static List<Item> Items { get; internal set; } = new List<Item>();
        public static List<ItemGroup> ItemGroups { get; internal set; } = new List<ItemGroup>();

        //Unlockable Items
        public static List<UnlockableItem> UnlockableItems { get; internal set; } = new List<UnlockableItem>();

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
