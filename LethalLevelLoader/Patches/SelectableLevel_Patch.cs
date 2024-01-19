using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public class SelectableLevel_Patch
    {
        public static List<ExtendedLevel> allLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> vanillaLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> customLevelsList = new List<ExtendedLevel>();

        internal static List<SelectableLevel> prePatchedLevelsList = new List<SelectableLevel>();
        internal static List<SelectableLevel> patchedLevelsList = new List<SelectableLevel>();
        internal static List<SelectableLevel> prePatchedMoonsCatalogueList = new List<SelectableLevel>();
        internal static List<SelectableLevel> patchedMoonsCatalogueList = new List<SelectableLevel>();

        internal static string injectionSceneName = "InitSceneLaunchOptions";

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static void StartOfRound_Start()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                CreatePatchedLevelsList();
                CreatePatchedMoonsCatalogueList();
                Terminal_Patch.CreateMoonsFilterTerminalAssets();
                Terminal_Patch.CreateVanillaExtendedLevelGroups();
                Terminal_Patch.CreateCustomExtendedLevelGroups();

                foreach (ExtendedLevel customLevel in customLevelsList)
                    AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                    AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                LethalLevelLoaderPlugin.hasVanillaBeenPatched = true;
            }

            PatchVanillaLevelLists();
        }

        internal static void AddSelectableLevel(ExtendedLevel extendedLevel)
        {
            if (extendedLevel.levelType == ContentType.Custom)
                customLevelsList.Add(extendedLevel);
            else
                vanillaLevelsList.Add(extendedLevel);

            allLevelsList.Add(extendedLevel);
        }

        internal static void CreatePatchedLevelsList()
        {
            prePatchedLevelsList = new List<SelectableLevel>(StartOfRound.Instance.levels.ToList());
            patchedLevelsList = new List<SelectableLevel>(prePatchedLevelsList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedLevelsList.Add(extendedLevel.selectableLevel);
        }

        internal static void CreatePatchedMoonsCatalogueList()
        {
            prePatchedMoonsCatalogueList = new List<SelectableLevel>(Terminal_Patch.Terminal.moonsCatalogueList);
            patchedMoonsCatalogueList = new List<SelectableLevel>(prePatchedMoonsCatalogueList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedMoonsCatalogueList.Add(extendedLevel.selectableLevel);
        }

        internal static void PatchVanillaLevelLists()
        {
            Terminal terminal = GameObject.FindAnyObjectByType<Terminal>();
            StartOfRound startOfRound = StartOfRound.Instance;

            startOfRound.levels = patchedLevelsList.ToArray();
            terminal.moonsCatalogueList = patchedMoonsCatalogueList.ToArray();
        }

        public static bool TryGetExtendedLevel(SelectableLevel selectableLevel, out ExtendedLevel returnExtendedLevel, ContentType levelType = ContentType.Any)
        {
            returnExtendedLevel = null;
            List<ExtendedLevel> extendedLevelsList = new List<ExtendedLevel>();

            switch (levelType)
            {
                case ContentType.Vanilla:
                    extendedLevelsList = vanillaLevelsList;
                    break;
                case ContentType.Custom:
                    extendedLevelsList = customLevelsList;
                    break;
                case ContentType.Any:
                    extendedLevelsList = allLevelsList;
                    break;
            }

            foreach (ExtendedLevel extendedLevel in extendedLevelsList)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel != null);
        }

        public static ExtendedLevel GetExtendedLevel(SelectableLevel selectableLevel)
        {
            ExtendedLevel returnExtendedLevel = null;

            foreach (ExtendedLevel extendedLevel in allLevelsList)
                if (extendedLevel.selectableLevel == selectableLevel)
                    returnExtendedLevel = extendedLevel;

            return (returnExtendedLevel);
        }
    }
}