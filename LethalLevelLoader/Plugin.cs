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
    [BepInDependency(LethalModDataLib.PluginInfo.PLUGIN_GUID)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "1.2.0.0";

        internal static Plugin Instance;

        internal static AssetBundle MainAssets;
        internal static readonly Harmony Harmony = new Harmony(ModGUID);

        internal static BepInEx.Logging.ManualLogSource logger;

        public delegate void OnSetupComplete();
        public static event OnSetupComplete onSetupComplete;
        public static bool IsSetupComplete { get; private set; } = false;

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
            Harmony.PatchAll(typeof(SafetyPatches));
            NetworkScenePatcher.Patch();

            NetcodePatch();

            //AssetBundleLoader.LoadBundles();
            //AssetBundleLoader.Instance.pluginInstace = this;

            GameObject test = new GameObject("LethalLevelLoader AssetBundleLoader");
            test.AddComponent<AssetBundleLoader>().LoadBundles();
            test.hideFlags = HideFlags.HideAndDontSave;
            AssetBundleLoader.onBundlesFinishedLoading += AssetBundleLoader.LoadContentInBundles;

            foreach (GameObject gameObject in UnityEngine.Object.FindObjectsOfType<GameObject>(true))
                DebugHelper.Log("GameObject Found: " + gameObject.name);

            foreach (MonoBehaviour monoBehaviour in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true))
                DebugHelper.Log("MonoBheaviour Found: " + monoBehaviour.gameObject.name + " - " + monoBehaviour.GetType().Name);

            //UnityEngine.Object.FindFirstObjectByType<GameObject>()
        }

        internal static void CompleteSetup()
        {
            DebugHelper.Log("LethalLevelLoader Has Finished Initializing.");
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
                DebugHelper.Log("NetcodePatcher did a big fucksie wuckise!");
            }
        }
    }
}