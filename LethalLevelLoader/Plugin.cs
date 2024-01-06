using BepInEx;
using HarmonyLib;
using System.Security.Permissions;
using UnityEngine;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalLevelLoader
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string ModGUID = "imabatby.lethallevelloader";
        public const string ModName = "LethalLevelLoader";
        public const string ModVersion = "0.0.1";

        public static AssetBundle MainAssets;
        private static readonly Harmony Harmony = new Harmony(ModGUID);

        public static BepInEx.Logging.ManualLogSource logger;


        private void Awake() {
            logger = Logger;

            Logger.LogInfo($"LethalLevelLoader loaded!!");

            Harmony.PatchAll(typeof(DebugHelper));
            Harmony.PatchAll(typeof(DebugOrderOfExecution));
            Harmony.PatchAll(typeof(ContentExtractor));
            Harmony.PatchAll(typeof(DungeonLoader));
            Harmony.PatchAll(typeof(SelectableLevel_Patch));
            Harmony.PatchAll(typeof(Terminal_Patch));
            Harmony.PatchAll(typeof(LevelLoader));
            Harmony.PatchAll(typeof(AssetBundleLoader));

            AssetBundleLoader.FindBundles();
        }
    }
}