using DunGen;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public ExtendedDungeonFlowWithRarity(ExtendedDungeonFlow newExtendedDungeonFlow, int newRarity)
        { extendedDungeonFlow = newExtendedDungeonFlow; rarity = newRarity; }
    }
    
    public class DungeonLoader
    {
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        public static void Generate_Prefix(DungeonGenerator __instance)
        {
            DebugHelper.Log("Started To Prefix Patch DungeonGenerator Generate!");
            Scene scene = RoundManager.Instance.dungeonGenerator.gameObject.scene;

            if (SelectableLevel_Patch.TryGetExtendedLevel(RoundManager.Instance.currentLevel, out ExtendedLevel extendedLevel))
            {
                SetDungeonFlow(__instance, extendedLevel);
                PatchDungeonSize(__instance, extendedLevel);
                PatchFireEscapes(__instance, extendedLevel, scene);
            }
        }

        public static void SetDungeonFlow(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel)
        {
            DebugHelper.Log("Setting DungeonFlow!");
            RoundManager roundManager = RoundManager.Instance;

            Random levelRandom = RoundManager.Instance.LevelRandom;

            int randomisedDungeonIndex = -1;

            List<int> randomWeightsList = new List<int>();
            string debugString = "Current Level + (" + extendedLevel.NumberlessPlanetName + ") Weights List: " + "\n" + "\n";

            List<ExtendedDungeonFlowWithRarity> availableExtendedFlowsList = DungeonFlow_Patch.GetValidExtendedDungeonFlows(extendedLevel).ToList();

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
                randomWeightsList.Add(extendedDungeon.rarity);

            randomisedDungeonIndex = roundManager.GetRandomWeightedIndex(randomWeightsList.ToArray(), levelRandom);

            foreach (ExtendedDungeonFlowWithRarity extendedDungeon in availableExtendedFlowsList)
            {
                debugString += extendedDungeon.extendedDungeonFlow.dungeonFlow.name + " | " + extendedDungeon.rarity;
                if (extendedDungeon.extendedDungeonFlow == availableExtendedFlowsList[randomisedDungeonIndex].extendedDungeonFlow)
                    debugString += " - Selected DungeonFlow" + "\n";
                else
                    debugString += "\n";
            }

            DebugHelper.Log(debugString + "\n");

            dungeonGenerator.DungeonFlow = availableExtendedFlowsList[randomisedDungeonIndex].extendedDungeonFlow.dungeonFlow;
        }

        public static void PatchDungeonSize(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel)
        {
            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                if (dungeonGenerator.LengthMultiplier > extendedDungeonFlow.extendedDungeonPreferences.sizeMultiplierMax)
                {
                    float newDungeonSize = Mathf.Lerp(extendedDungeonFlow.extendedDungeonPreferences.sizeMultiplierMax, extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.extendedDungeonPreferences.sizeMultiplierClampPercentage);
                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Requested A Dungeon Size Of " + extendedLevel.selectableLevel.factorySizeMultiplier + ". This Value Exceeds The Dungeon's Supplied Maximum Size, Scaling It Down To " + newDungeonSize);
                    dungeonGenerator.LengthMultiplier = newDungeonSize * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                }
        }

        public static void PatchFireEscapes(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel, Scene scene)
        {
            string debugString = "Fire Exit Patch Report, Details Below;" + "\n" + "\n";

            List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
            int fireEscapesAmount = 0;
            bool mainEntranceFound = false;

            foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (EntranceTeleport entranceTeleport in rootObject.GetComponentsInChildren<EntranceTeleport>())
                {
                    fireEscapesAmount++;
                    entranceTeleport.dungeonFlowId = -1;
                    entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
                    DebugHelper.Log("EntranceTeleporter Is: " + (entranceTeleport.IsSpawned == true));
                    if (entranceTeleport.entranceId == 0)
                        DebugHelper.Log("Main Entrance Found!");
                    else
                        entranceTeleport.entranceId = fireEscapesAmount;
                }

            fireEscapesAmount -= 1; //To Remove Main Entrance From The Count.

            if (fireEscapesAmount != 0)
                debugString += "EntranceTeleport's Found, " + extendedLevel.NumberlessPlanetName + " Contains " + (fireEscapesAmount + 1) + " Entrances! ( " + fireEscapesAmount + " Fire Escapes) " + "\n";

            Vector2 oldCount = Vector2.zero;
            foreach (GlobalPropSettings globalPropSettings in dungeonGenerator.DungeonFlow.GlobalProps)
            {
                if (globalPropSettings.ID == 1231)
                {
                    globalPropSettings.Count = new IntRange(fireEscapesAmount, fireEscapesAmount);
                    oldCount = new Vector2(globalPropSettings.Count.Min, globalPropSettings.Count.Max);
                }
            }
            if (oldCount != Vector2.zero)
                debugString += "Found Fire Escape GlobalProp: (ID: 1231), Modifying Spawnrate Count From (" + oldCount.x + "," + oldCount.y + ") To (" + fireEscapesAmount + "," + fireEscapesAmount + ")" + "\n";
            else
                debugString += "Fire Escape GlobalProp Could Not Be Found! Fire Escapes Will Not Be Patched!" + "\n";


            DebugHelper.Log(debugString + "\n");
        }
    }
}
