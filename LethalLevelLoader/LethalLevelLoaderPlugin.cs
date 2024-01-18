using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    internal class LethalLevelLoaderPlugin : BaseUnityPlugin
    {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "1.0.8";

        public static LethalLevelLoaderPlugin Instance;

        public static AssetBundle MainAssets;
        private static readonly Harmony Harmony = new Harmony(ModGUID);

        internal static BepInEx.Logging.ManualLogSource logger;

        public static bool hasVanillaBeenPatched;

        private ConfigEntry<string> terminalMoonsPreviewInfoSetting;

        internal static GameObject networkManagerPrefab;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            logger = Logger;

            Logger.LogInfo($"LethalLevelLoader loaded!!");

            Harmony.PatchAll(typeof(AssetBundleLoader));
            Harmony.PatchAll(typeof(ContentExtractor));

            Harmony.PatchAll(typeof(SelectableLevel_Patch));
            Harmony.PatchAll(typeof(NetworkManager_Patch));
            Harmony.PatchAll(typeof(Terminal_Patch));

            Harmony.PatchAll(typeof(DungeonLoader));
            Harmony.PatchAll(typeof(LevelLoader));

            Harmony.PatchAll(typeof(DebugHelper));
            Harmony.PatchAll(typeof(DebugOrderOfExecution));

            NetworkScenePatcher.Patch();

            terminalMoonsPreviewInfoSetting = Config.Bind("General", "Terminal >Moons PreviewInfo Setting", "Weather", new ConfigDescription("What LethalLevelLoader displays next to each moon in the >moons Terminal listing. " + "\n" + "Valid LethalLevelLoader Overhaul Options: Weather, Price, Difficulty, None " + "\n" + "Valid LethalLevelLoader Compatability Options: Vanilla, Override "));

            if (terminalMoonsPreviewInfoSetting.Value == "Weather")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Weather;
            else if (terminalMoonsPreviewInfoSetting.Value == "Price")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Price;
            else if (terminalMoonsPreviewInfoSetting.Value == "Difficulty")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Difficulty;
            else if (terminalMoonsPreviewInfoSetting.Value == "None")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Empty;
            else if (terminalMoonsPreviewInfoSetting.Value == "Vanilla")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Vanilla;
            else if (terminalMoonsPreviewInfoSetting.Value == "Override")
                LethalLevelLoaderSettings.levelPreviewInfoType = LevelPreviewInfoType.Override;
            else
                Debug.LogError("LethalLevelLoader: TerminalMoonsPreviewInfoSetting Set To Invalid Value");

            networkManagerPrefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab("networkManagerPrefab");
            networkManagerPrefab.AddComponent<LethalLevelLoaderNetworkBehaviour>();

            //AssetBundleLoader.FindBundles();

            //scaleDownVanillaDungeonFlowRarityIfCustomDungeonFlowHasChance = Config.Bind("General", "Lower Vanilla Dungeon Spawn Rate If Custom Dungeon Can Spawn", 1.0f, new ConfigDescription("If a Custom Dungeon can spawn on a level, Any Vanilla Dungeons that also can spawn on the level will have their rarity scaled down based on this float (0f = No Rarity, 1f = Unchanged Rarity", new AcceptableValueRange<float>(0.0f, 1.0f)));
        }

        internal void Log(string log)
        {
            Logger.LogInfo(log);
        }
    }
}