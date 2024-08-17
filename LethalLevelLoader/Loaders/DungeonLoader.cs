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

        public bool UpdateRarity(int newRarity) { if (newRarity > rarity) { rarity = newRarity; return (true); } return (false); }
    }

    public static class DungeonLoader
    {
        internal static GameObject defaultKeyPrefab;
        
        internal static void SelectDungeon()
        {
            Patches.RoundManager.dungeonGenerator.Generator.DungeonFlow = null;
            if (LethalLevelLoaderNetworkManager.Instance.IsServer)
                LethalLevelLoaderNetworkManager.Instance.GetRandomExtendedDungeonFlowServerRpc();
        }

        internal static void PrepareDungeon()
        {
            DungeonGenerator dungeonGenerator = Patches.RoundManager.dungeonGenerator.Generator;
            ExtendedLevel currentExtendedLevel = LevelManager.CurrentExtendedLevel;
            ExtendedDungeonFlow currentExtendedDungeonFlow = DungeonManager.CurrentExtendedDungeonFlow;

            //PatchDungeonSize(dungeonGenerator, currentExtendedLevel, currentExtendedDungeonFlow);
            PatchFireEscapes(dungeonGenerator, currentExtendedLevel, SceneManager.GetSceneByName(currentExtendedLevel.SelectableLevel.sceneName));
            PatchDynamicGlobalProps(dungeonGenerator, currentExtendedDungeonFlow);
        }

        public static float GetClampedDungeonSize()
        {
            ExtendedDungeonFlow extendedDungeonFlow = DungeonManager.CurrentExtendedDungeonFlow;
            ExtendedLevel extendedLevel = LevelManager.CurrentExtendedLevel;
            float calculatedMultiplier = CalculateDungeonMultiplier(LevelManager.CurrentExtendedLevel, DungeonManager.CurrentExtendedDungeonFlow);
            if (DungeonManager.CurrentExtendedDungeonFlow != null && DungeonManager.CurrentExtendedDungeonFlow.IsDynamicDungeonSizeRestrictionEnabled == true)
            {
                if (calculatedMultiplier > extendedDungeonFlow.DynamicDungeonSizeMinMax.y)
                    calculatedMultiplier = Mathf.Lerp(calculatedMultiplier, extendedDungeonFlow.DynamicDungeonSizeMinMax.y, extendedDungeonFlow.DynamicDungeonSizeLerpRate); //This is how vanilla does it.
                else if (calculatedMultiplier < extendedDungeonFlow.DynamicDungeonSizeMinMax.x)
                    calculatedMultiplier = Mathf.Lerp(calculatedMultiplier, extendedDungeonFlow.DynamicDungeonSizeMinMax.x, extendedDungeonFlow.DynamicDungeonSizeLerpRate);//This is how vanilla does it.
                DebugHelper.Log("Current ExtendedLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " ExtendedLevel DungeonSize Is: " + LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier + " | Overriding DungeonSize To: " + calculatedMultiplier, DebugType.User);
            }
            else
                DebugHelper.Log("CurrentLevel: " + LevelManager.CurrentExtendedLevel.NumberlessPlanetName + " DungeonSize Is: " + LevelManager.CurrentExtendedLevel.SelectableLevel.factorySizeMultiplier + " | Leaving DungeonSize As: " + calculatedMultiplier, DebugType.User);
            return (calculatedMultiplier);
        }

        public static float CalculateDungeonMultiplier(ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (IndoorMapType indoorMapType in RoundManager.Instance.dungeonFlowTypes)
                if (indoorMapType.dungeonFlow == extendedDungeonFlow.DungeonFlow)
                    return (extendedLevel.SelectableLevel.factorySizeMultiplier / indoorMapType.MapTileSize * RoundManager.Instance.mapSizeMultiplier);

            return 1f;
        }

        internal static void PatchDungeonSize(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel, ExtendedDungeonFlow extendedDungeonFlow)
        {
            /*if (extendedDungeonFlow.enableDynamicDungeonSizeRestriction == true)
            {
                if (extendedLevel.selectableLevel.factorySizeMultiplier > extendedDungeonFlow.dungeonSizeMax)
                    dungeonGenerator.LengthMultiplier = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMax, extendedDungeonFlow.dungeonSizeLerpPercentage) * Patches.RoundManager.mapSizeMultiplier; //This is how vanilla does it.
                else if (extendedLevel.selectableLevel.factorySizeMultiplier < extendedDungeonFlow.dungeonSizeMin)
                    dungeonGenerator.LengthMultiplier = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMin, extendedDungeonFlow.dungeonSizeLerpPercentage) * Patches.RoundManager.mapSizeMultiplier; //This is how vanilla does it.
                DebugHelper.Log("Setting DungeonSize To: " + extendedLevel.selectableLevel.factorySizeMultiplier / Patches.RoundManager.mapSizeMultiplier);
            }*/
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
                    //entranceTeleport.dungeonFlowId = extendedDungeonFlow.DungeonID; //I'm pretty sure this is fine but this would be something to check if stuff goes weird.
                }

                debugString += "EntranceTeleport's Found, " + extendedLevel.NumberlessPlanetName + " Contains " + (entranceTeleports.Count) + " Entrances! ( " + (entranceTeleports.Count - 1) + " Fire Escapes) " + "\n";
                debugString += "Main Entrance: " + entranceTeleports[0].gameObject.name + " (Entrance ID: " + entranceTeleports[0].entranceId + ")" + "\n";
                foreach (EntranceTeleport entranceTeleport in entranceTeleports)
                    if (entranceTeleport.entranceId != 0)
                        debugString += "Alternate Entrance: " + entranceTeleport.gameObject.name + " (Entrance ID: " + entranceTeleport.entranceId + ")" + "\n";

                foreach (GlobalPropSettings globalPropSettings in dungeonGenerator.DungeonFlow.GlobalProps)
                    if (globalPropSettings.ID == 1231)
                    {
                        debugString += "Found Fire Escape GlobalProp: (ID: 1231), Modifying Spawn rate Count From (" + globalPropSettings.Count.Min + "," + globalPropSettings.Count.Max + ") To (" + (entranceTeleports.Count - 1) + "," + (entranceTeleports.Count - 1) + ")" + "\n";
                        globalPropSettings.Count = new IntRange(entranceTeleports.Count - 1, entranceTeleports.Count - 1); //-1 Because .Count includes the Main Entrance.
                        break;
                    }

                DebugHelper.Log(debugString + "\n", DebugType.User);
            }
        }

        public static void PatchDynamicGlobalProps(DungeonGenerator dungeonGenerator, ExtendedDungeonFlow extendedDungeonFlow)
        {
            foreach (GlobalPropCountOverride globalPropOverride in extendedDungeonFlow.GlobalPropCountOverridesList)
                foreach (GlobalPropSettings globalProp in dungeonGenerator.DungeonFlow.GlobalProps)
                    if (globalPropOverride.globalPropID == globalProp.ID)
                    {
                        globalProp.Count.Min = globalProp.Count.Min * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / Patches.RoundManager.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                        globalProp.Count.Max = globalProp.Count.Max * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / Patches.RoundManager.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                    }
        }
    }
}
