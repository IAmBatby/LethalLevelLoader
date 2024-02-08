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
        public const string ModVersion = "1.1.0.2";

        public static Plugin Instance;

        public static AssetBundle MainAssets;
        internal static readonly Harmony Harmony = new Harmony(ModGUID);

        internal static BepInEx.Logging.ManualLogSource logger;

        public static bool hasVanillaBeenPatched;

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