using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class LethalLevelLoaderPlugin : BaseUnityPlugin {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "1.0.0";

        public static LethalLevelLoaderPlugin Instance;

        public static AssetBundle MainAssets;
        private static readonly Harmony Harmony = new Harmony(ModGUID);

        public static BepInEx.Logging.ManualLogSource logger;

        public static bool hasVanillaBeenPatched;

        public ConfigEntry<bool> enableAllCustomDungeonsOnAllLevels;
        public ConfigEntry<int> overrideAllCustomMoonRarities;

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

            AssetBundleLoader.FindBundles();
        }
    }
}