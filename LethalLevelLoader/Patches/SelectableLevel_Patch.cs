using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalLevelLoader
{
    public class SelectableLevel_Patch
    {
        public static List<ExtendedLevel> allLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> vanillaLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> customLevelsList = new List<ExtendedLevel>();

        private static List<SelectableLevel> prePatchedLevelsList = new List<SelectableLevel>();
        private static List<SelectableLevel> patchedLevelsList = new List<SelectableLevel>();
        private static List<SelectableLevel> prePatchedMoonsCatalogueList = new List<SelectableLevel>();
        private static List<SelectableLevel> patchedMoonsCatalogueList = new List<SelectableLevel>();

        public static string injectionSceneName = "InitSceneLaunchOptions";

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static void StartOfRound_Start()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                CreatePatchedLevelsList();
                CreatePatchedMoonsCatalogueList();

                foreach (ExtendedLevel customLevel in customLevelsList)
                    AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

                foreach (ExtendedDungeonFlow customDungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                    AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);

                LethalLevelLoaderPlugin.hasVanillaBeenPatched = true;
            }

            PatchVanillaLevelLists();
        }

        public static void AddSelectableLevel(ExtendedLevel extendedLevel)
        {
            if (extendedLevel.levelType == ContentType.Custom)
                customLevelsList.Add(extendedLevel);
            else
                vanillaLevelsList.Add(extendedLevel);

            allLevelsList.Add(extendedLevel);
        }

        public static void CreatePatchedLevelsList()
        {
            prePatchedLevelsList = new List<SelectableLevel>(StartOfRound.Instance.levels.ToList());
            patchedLevelsList = new List<SelectableLevel>(prePatchedLevelsList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedLevelsList.Add(extendedLevel.selectableLevel);
        }

        public static void CreatePatchedMoonsCatalogueList()
        {
            prePatchedMoonsCatalogueList = new List<SelectableLevel>(Terminal_Patch.Terminal.moonsCatalogueList);
            patchedMoonsCatalogueList = new List<SelectableLevel>(prePatchedMoonsCatalogueList);

            foreach (ExtendedLevel extendedLevel in customLevelsList)
                patchedMoonsCatalogueList.Add(extendedLevel.selectableLevel);
        }

        public static void PatchVanillaLevelLists()
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
    }
}