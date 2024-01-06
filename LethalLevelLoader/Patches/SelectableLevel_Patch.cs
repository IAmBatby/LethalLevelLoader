using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader
{
    public class SelectableLevel_Patch
    {
        public static List<ExtendedLevel> allLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> vanillaLevelsList = new List<ExtendedLevel>();
        public static List<ExtendedLevel> customLevelsList = new List<ExtendedLevel>();

        public static string injectionSceneName = "InitSceneLaunchOptions";

        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        public static void RoundManager_Start(RoundManager __instance)
        {
            PatchVanillaLevelLists();

            foreach (ExtendedLevel customLevel in customLevelsList)
                AssetBundleLoader.RestoreVanillaLevelAssetReferences(customLevel);

            foreach (ExtendedDungeonFlow customDungeonFlow in DungeonFlow_Patch.customDungeonFlowsList)
                AssetBundleLoader.RestoreVanillaDungeonAssetReferences(customDungeonFlow);
        }

        public static void AddSelectableLevel(ExtendedLevel extendedLevel)
        {
            DebugHelper.Log("Adding Selectable Level: " + extendedLevel.NumberlessPlanetName);
            if (extendedLevel.levelType == ContentType.Custom)
                customLevelsList.Add(extendedLevel);
            else
                vanillaLevelsList.Add(extendedLevel);

            allLevelsList.Add(extendedLevel);
        }

        public static void PatchVanillaLevelLists()
        {
            DebugHelper.Log("Patching Vanilla Level List!");

            Terminal terminal = GameObject.FindAnyObjectByType<Terminal>();
            StartOfRound startOfRound = StartOfRound.Instance;

            List<SelectableLevel> allSelectableLevels = new List<SelectableLevel>();

            foreach (ExtendedLevel extendedLevel in allLevelsList)
                allSelectableLevels.Add(extendedLevel.selectableLevel);

            startOfRound.levels = allSelectableLevels.ToArray();
            terminal.moonsCatalogueList = allSelectableLevels.ToArray();

            DebugHelper.Log("StartOfRound Levels List Length Is: " + startOfRound.levels.Length);
            DebugHelper.Log("Terminal Levels List Length Is: " + terminal.moonsCatalogueList.Length);
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