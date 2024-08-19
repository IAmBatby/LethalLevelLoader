using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DunGen;
using HarmonyLib;
using LethalLevelLoader.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Application;

namespace LethalLevelLoader
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LethalModDataLib.PluginInfo.PLUGIN_GUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "1.3.10";

        internal static Plugin Instance;

        internal static AssetBundle MainAssets;
        internal static readonly Harmony Harmony = new Harmony(ModGUID);

        internal static BepInEx.Logging.ManualLogSource logger;

        public static event Action onBeforeSetup;
        public static event Action onSetupComplete;
        public static bool IsSetupComplete { get; private set; } = false;

        internal static GameObject networkManagerPrefab;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            logger = Logger;

            Logger.LogInfo($"LethalLevelLoader loaded!!");

            //We do this here to try and assure this doesn't accidently catch anything from any AssetBundles
            LevelLoader.vanillaWaterShader = Shader.Find("Shader Graphs/WaterShaderHDRP");
            if (LevelLoader.vanillaWaterShader == null)
                DebugHelper.LogError("Could Not Find Water Shader", DebugType.User);

            Harmony.PatchAll(typeof(LethalLevelLoaderNetworkManager));
            Harmony.PatchAll(typeof(DungeonLoader));

            Harmony.PatchAll(typeof(Patches));
            Harmony.PatchAll(typeof(EventPatches));
            Harmony.PatchAll(typeof(SafetyPatches));

            TrySoftPatch(LethalLib.Plugin.ModGUID, typeof(LethalLibPatches));
			
            NetworkScenePatcher.Patch();
			Patches.InitMonoModHooks();

            NetcodePatch();

            GameObject assetBundleLoaderObject = new GameObject("LethalLevelLoader AssetBundleLoader");
            assetBundleLoaderObject.AddComponent<AssetBundleLoader>().LoadBundles();
            if (Application.isEditor)
                DontDestroyOnLoad(assetBundleLoaderObject);
            else
                assetBundleLoaderObject.hideFlags = HideFlags.HideAndDontSave;
            AssetBundleLoader.onBundlesFinishedLoading += AssetBundleLoader.LoadContentInBundles;

            ConfigLoader.BindGeneralConfigs();
        }

        internal static void OnBeforeSetupInvoke()
        {
            onBeforeSetup?.Invoke();
        }

        internal static void CompleteSetup()
        {
            DebugHelper.Log("LethalLevelLoader Has Finished Initializing.", DebugType.User);
            Plugin.IsSetupComplete = true;
            onSetupComplete?.Invoke();
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
                DebugHelper.LogError("NetcodePatcher Failed! This Is Very Bad.", DebugType.Developer);
            }
        }

        internal static void TrySoftPatch(string pluginName, Type type)
        {
            if (Chainloader.PluginInfos.ContainsKey(pluginName))
            {
                Harmony.CreateClassProcessor(type, true).Patch();
                DebugHelper.Log(pluginName + "found, enabling compatability patches.", DebugType.User);
            }

        }
    }
}