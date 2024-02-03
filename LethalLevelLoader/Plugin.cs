using BepInEx;
using BepInEx.Configuration;
using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    internal class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "1.1.0";

        public static Plugin Instance;

        public static AssetBundle MainAssets;
        internal static readonly Harmony Harmony = new Harmony(ModGUID);

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

            Harmony.PatchAll(typeof(LethalLevelLoaderNetworkManager));
            Harmony.PatchAll(typeof(DungeonLoader));

            Harmony.PatchAll(typeof(Patches));
            Harmony.PatchAll(typeof(EventPatches));
            NetworkScenePatcher.Patch();

            NetcodePatch();

            terminalMoonsPreviewInfoSetting = Config.Bind("General", "Terminal >Moons PreviewInfo Setting", "Weather", new ConfigDescription("What LethalLevelLoader displays next to each moon in the >moons Terminal listing. " + "\n" + "Valid LethalLevelLoader Overhaul Options: Weather, Price, Difficulty, None " + "\n" + "Valid LethalLevelLoader Compatability Options: Vanilla, Override "));

            if (terminalMoonsPreviewInfoSetting.Value == "Weather")
                Settings.levelPreviewInfoType = PreviewInfoType.Weather;
            else if (terminalMoonsPreviewInfoSetting.Value == "Price")
                Settings.levelPreviewInfoType = PreviewInfoType.Price;
            else if (terminalMoonsPreviewInfoSetting.Value == "Difficulty")
                Settings.levelPreviewInfoType = PreviewInfoType.Difficulty;
            else if (terminalMoonsPreviewInfoSetting.Value == "None")
                Settings.levelPreviewInfoType = PreviewInfoType.None;
            else if (terminalMoonsPreviewInfoSetting.Value == "Vanilla")
                Settings.levelPreviewInfoType = PreviewInfoType.Vanilla;
            else if (terminalMoonsPreviewInfoSetting.Value == "Override")
                Settings.levelPreviewInfoType = PreviewInfoType.Override;
            else
                Debug.LogError("LethalLevelLoader: TerminalMoonsPreviewInfoSetting Set To Invalid Value");

            //AssetBundleLoader.FindBundles();

            //scaleDownVanillaDungeonFlowRarityIfCustomDungeonFlowHasChance = Config.Bind("General", "Lower Vanilla Dungeon Spawn Rate If Custom Dungeon Can Spawn", 1.0f, new ConfigDescription("If a Custom Dungeon can spawn on a level, Any Vanilla Dungeons that also can spawn on the level will have their rarity scaled down based on this float (0f = No Rarity, 1f = Unchanged Rarity", new AcceptableValueRange<float>(0.0f, 1.0f)));
        }

        private void NetcodePatch()
        {
            try
            {
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                        if (attributes.Length > 0)
                        {
                            method.Invoke(null, null);
                        }
                    }
                }
            }
            catch
            {
                DebugHelper.Log("NetcodePatcher did a big fucksie wuckise!");
            }
        }
    }
}