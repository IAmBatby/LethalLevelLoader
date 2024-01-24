using BepInEx;
using BepInEx.Configuration;
using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.SceneManagement;

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
                ModSettings.levelPreviewInfoType = PreviewInfoType.Weather;
            else if (terminalMoonsPreviewInfoSetting.Value == "Price")
                ModSettings.levelPreviewInfoType = PreviewInfoType.Price;
            else if (terminalMoonsPreviewInfoSetting.Value == "Difficulty")
                ModSettings.levelPreviewInfoType = PreviewInfoType.Difficulty;
            else if (terminalMoonsPreviewInfoSetting.Value == "None")
                ModSettings.levelPreviewInfoType = PreviewInfoType.None;
            else if (terminalMoonsPreviewInfoSetting.Value == "Vanilla")
                ModSettings.levelPreviewInfoType = PreviewInfoType.Vanilla;
            else if (terminalMoonsPreviewInfoSetting.Value == "Override")
                ModSettings.levelPreviewInfoType = PreviewInfoType.Override;
            else
                Debug.LogError("LethalLevelLoader: TerminalMoonsPreviewInfoSetting Set To Invalid Value");

            Harmony.PatchAll(typeof(InitalizeGame_Patch));
            //AssetBundleLoader.FindBundles();

            //scaleDownVanillaDungeonFlowRarityIfCustomDungeonFlowHasChance = Config.Bind("General", "Lower Vanilla Dungeon Spawn Rate If Custom Dungeon Can Spawn", 1.0f, new ConfigDescription("If a Custom Dungeon can spawn on a level, Any Vanilla Dungeons that also can spawn on the level will have their rarity scaled down based on this float (0f = No Rarity, 1f = Unchanged Rarity", new AcceptableValueRange<float>(0.0f, 1.0f)));
        }

        internal void Log(string log)
        {
            Logger.LogInfo(log);
        }
    }

    internal static class InitalizeGame_Patch
    {
        static List<string> sceneNamesList = new List<string>()
        {
            "Level1Experimentation",
            "Level2Assurance",
            "Level3Vow",
            "Level4March",
            "Level5Rend",
            "Level6Dine",
            "Level7Offense",
            "Level8Titan",
            "InitScene"
        };
        static int sceneLoadedCounter = 0;
        static bool startedSceneLoadingChain = false;

        [HarmonyPriority(340)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static bool Awake_Prefix()
        {
            if (startedSceneLoadingChain == false)
            {
                startedSceneLoadingChain = true;
                SceneManager.sceneLoaded += UnloadScene;
                LoadScene(sceneNamesList[sceneLoadedCounter]);
                return (false);
            }
            else
            {
                SceneManager.sceneLoaded -= UnloadScene;
                if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
                    AssetBundleLoader.FindBundles();
                return (true);
            }
        }

        internal static void LoadScene(string sceneName)
        {
            Debug.Log("Loading Scene: " + sceneName + ", SceneLoadedCounter Is At: " + sceneLoadedCounter);
            sceneLoadedCounter++;
            AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        internal static void UnloadScene(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Unloading Scene: " + scene.name + ", SceneLoadedCounter Is At: " + sceneLoadedCounter);
            foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (RuntimeDungeon childObject in rootObject.GetComponentsInChildren<RuntimeDungeon>())
                    if (childObject.Generator.DungeonFlow != null)
                    {
                        Debug.Log("Found DungeonFlow: " + childObject.Generator.DungeonFlow.name + " On " + scene.name);
                        AssetBundleLoader.CreateVanillaExtendedDungeonFlow(childObject.Generator.DungeonFlow);
                    }
                    foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (Transform childObject in rootObject.GetComponentsInChildren<Transform>())
                    //Debug.Log(scene.name + " : " + childObject.name);
            SceneManager.UnloadSceneAsync(scene.name);

            if (sceneLoadedCounter == sceneNamesList.Count)
                SceneManager.sceneLoaded -= UnloadScene;
            else
                LoadScene(sceneNamesList[sceneLoadedCounter]);
        }
    }
}