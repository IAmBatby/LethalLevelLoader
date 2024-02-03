using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static DunGen.Graph.DungeonFlow;
using Random = System.Random;

namespace LethalLevelLoader
{
    [System.Serializable]
    public class ExtendedDungeonFlowWithRarity
    {
        public ExtendedDungeonFlow extendedDungeonFlow;
        public int rarity;

        public ExtendedDungeonFlowWithRarity(ExtendedDungeonFlow newExtendedDungeonFlow, int newRarity) { extendedDungeonFlow = newExtendedDungeonFlow; rarity = newRarity; }

        public void UpdateRarity(int newRarity) { if (newRarity > rarity) rarity = newRarity; }
    }

    public class DungeonLoader
    {
        public delegate List<ExtendedDungeonFlowWithRarity> DungeonFlowsWithRarityDelegate(List<ExtendedDungeonFlowWithRarity> extendedDungeonFlowsWithRarities);
        public static event DungeonFlowsWithRarityDelegate onBeforeRandomDungeonFlowSelected;

        public delegate ExtendedDungeonFlow SelectedDungeonFlow(ExtendedDungeonFlow dungeonFlow);
        public static event SelectedDungeonFlow onSelectedRandomDungeonFlow;

        internal static void SelectDungeon()
        {
            RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow = null;
            LethalLevelLoaderNetworkManager.Instance.GetRandomExtendedDungeonFlowServerRpc();
        }

        internal static void PrepareDungeon()
        {
            DungeonGenerator dungeonGenerator = RoundManager.Instance.dungeonGenerator.Generator;
            ExtendedLevel currentExtendedLevel = LevelManager.CurrentExtendedLevel;
            ExtendedDungeonFlow currentExtendedDungeonFlow = DungeonManager.CurrentExtendedDungeonFlow;

            PatchDungeonSize(dungeonGenerator, currentExtendedLevel, currentExtendedDungeonFlow);
            PatchFireEscapes(dungeonGenerator, currentExtendedLevel, SceneManager.GetSceneByName(currentExtendedLevel.selectableLevel.sceneName));
            PatchDynamicGlobalProps(dungeonGenerator, currentExtendedDungeonFlow);
        }

        internal static void PatchDungeonSize(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (extendedDungeonFlow.enableDynamicDungeonSizeRestriction == true)
            {
                if (extendedLevel.selectableLevel.factorySizeMultiplier > extendedDungeonFlow.dungeonSizeMax)
                    dungeonGenerator.LengthMultiplier = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMax, extendedDungeonFlow.dungeonSizeLerpPercentage) * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                else if (extendedLevel.selectableLevel.factorySizeMultiplier < extendedDungeonFlow.dungeonSizeMin)
                    dungeonGenerator.LengthMultiplier = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMin, extendedDungeonFlow.dungeonSizeLerpPercentage) * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                DebugHelper.Log("Setting DungeonSize To: " + extendedLevel.selectableLevel.factorySizeMultiplier / RoundManager.Instance.mapSizeMultiplier);
            }
        }

        internal static List<EntranceTeleport> GetEntranceTeleports(Scene scene)
        {
            List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
            foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (EntranceTeleport entranceTeleport in rootObject.GetComponentsInChildren<EntranceTeleport>())
                    entranceTeleports.Add(entranceTeleport);
            return (entranceTeleports);
        }

        internal static void PatchFireEscapes(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel, Scene scene)
        {
            string debugString = "Fire Exit Patch Report, Details Below;" + "\n" + "\n";

            if (DungeonManager.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
            {
                List<EntranceTeleport> entranceTeleports = GetEntranceTeleports(scene).OrderBy(o => o.entranceId).ToList();

                foreach (EntranceTeleport entranceTeleport in entranceTeleports)
                {
                    entranceTeleport.entranceId = entranceTeleports.IndexOf(entranceTeleport);
                    entranceTeleport.dungeonFlowId = extendedDungeonFlow.dungeonID; //I'm pretty sure this is fine but this would be something to check if stuff goes weird.
                }

                debugString += "EntranceTeleport's Found, " + extendedLevel.NumberlessPlanetName + " Contains " + (entranceTeleports.Count) + " Entrances! ( " + (entranceTeleports.Count - 1) + " Fire Escapes) " + "\n";
                debugString += "Main Entrance: " + entranceTeleports[0].gameObject.name + " (Entrance ID: " + entranceTeleports[0].entranceId + ")" + " (Dungeon ID: " + entranceTeleports[0].dungeonFlowId + ")" + "\n";
                foreach (EntranceTeleport entranceTeleport in entranceTeleports)
                    debugString += "Alternate Entrance: " + entranceTeleport.gameObject.name + " (Entrance ID: " + entranceTeleport.entranceId + ")" + " (Dungeon ID: " + entranceTeleport.dungeonFlowId + ")" + "\n";

                foreach (GlobalPropSettings globalPropSettings in dungeonGenerator.DungeonFlow.GlobalProps)
                    if (globalPropSettings.ID == 1231)
                    {
                        debugString += "Found Fire Escape GlobalProp: (ID: 1231), Modifying Spawnrate Count From (" + globalPropSettings.Count.Min + "," + globalPropSettings.Count.Max + ") To (" + (entranceTeleports.Count - 1) + "," + (entranceTeleports.Count - 1) + ")" + "\n";
                        globalPropSettings.Count = new IntRange(entranceTeleports.Count - 1, entranceTeleports.Count - 1); //-1 Because .Count includes the Main Entrance.
                        break;
                    }

                DebugHelper.Log(debugString + "\n");
            }
        }

        public static void PatchDynamicGlobalProps(DungeonGenerator dungeonGenerator, ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (GlobalPropCountOverride globalPropOverride in extendedDungeonFlow.globalPropCountOverridesList)
                foreach (GlobalPropSettings globalProp in dungeonGenerator.DungeonFlow.GlobalProps)
                    if (globalPropOverride.globalPropID == globalProp.ID)
                    {
                        globalProp.Count.Min = globalProp.Count.Min * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / RoundManager.Instance.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                        globalProp.Count.Max = globalProp.Count.Max * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / RoundManager.Instance.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                    }
        }
    }
}
