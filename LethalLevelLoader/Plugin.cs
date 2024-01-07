using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Security.Permissions;
using Unity.Netcode;
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

        public static bool hasVanillaBeenPatched;

        public ConfigEntry<bool> enableAllCustomDungeonsOnAllLevels;
        public ConfigEntry<int> overrideAllCustomMoonRarities;

        private void Awake() {
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

            //enableAllCustomDungeonsOnAllLevels = Config.Bind("General", "EnableCustomDungeonsOnAllMoons", false, new ConfigDescription("If enabled, All loaded custom dungeons will be added to the randomisation pools of every level. the rarity setting below controls their chances."));
           //overrideAllCustomMoonRarities = Config.Bind("General", "EnableCustomDungeonsOnAllMoonsRarityOverride", 100, new ConfigDescription("The rarity setting applied to all custom dungeons if the previous bool is enabled.", new AcceptableValueRange<int>(0, 300)));
        }
    }
}