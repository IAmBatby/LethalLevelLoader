using DunGen;
using HarmonyLib;
using System.Collections;
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
        [HarmonyPriority(350)]
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
            {
                if (dungeonGenerator.LengthMultiplier > extendedDungeonFlow.dungeonSizeMax)
                {
                    float newDungeonSize = Mathf.Lerp(extendedDungeonFlow.dungeonSizeMax, extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeLerpPercentage);
                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Requested A Dungeon Size Of " + extendedLevel.selectableLevel.factorySizeMultiplier + ". This Value Exceeds The Dungeon's Supplied Maximum Size, Scaling It Down To " + newDungeonSize);
                    dungeonGenerator.LengthMultiplier = newDungeonSize * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                }
                else if (dungeonGenerator.LengthMultiplier < extendedDungeonFlow.dungeonSizeMin)
                {
                    float newDungeonSize = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMin, extendedDungeonFlow.dungeonSizeLerpPercentage);
                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Requested A Dungeon Size Of " + extendedLevel.selectableLevel.factorySizeMultiplier + ". This Value Exceeds The Dungeon's Supplied Minimum Size, Scaling It Down To " + newDungeonSize);
                    dungeonGenerator.LengthMultiplier = newDungeonSize * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                }
            }
        }

        public static void PatchFireEscapes(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel, Scene scene)
        {
            string debugString = "Fire Exit Patch Report, Details Below;" + "\n" + "\n";

            List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
            EntranceTeleport lowestIDEntranceTeleport = null;

            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
            {
                int fireEscapesAmount = 0;

                foreach (GameObject rootObject in scene.GetRootGameObjects())
                    foreach (EntranceTeleport entranceTeleport in rootObject.GetComponentsInChildren<EntranceTeleport>())
                    {
                        entranceTeleport.dungeonFlowId = extendedDungeonFlow.dungeonID;

                        if (extendedDungeonFlow.dungeonType == ContentType.Vanilla)
                            entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[entranceTeleport.dungeonFlowId];

                        if (extendedDungeonFlow.dungeonType == ContentType.Custom)
                        {
                            if (extendedDungeonFlow.dungeonFirstTimeAudio != null)
                                entranceTeleport.firstTimeAudio = extendedDungeonFlow.dungeonFirstTimeAudio;
                            else
                                entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
                        }

                        fireEscapesAmount++;
                        if (lowestIDEntranceTeleport == null)
                            lowestIDEntranceTeleport = entranceTeleport;
                        if (lowestIDEntranceTeleport != null && entranceTeleport.entranceId < lowestIDEntranceTeleport.entranceId)
                            lowestIDEntranceTeleport = entranceTeleport;

                        entranceTeleports.Add(entranceTeleport);
                    }

                //To reduce the strict id requirements on entrance teleports
                lowestIDEntranceTeleport.entranceId = 0;
                int dungeonIDCounter = 1;
                DebugHelper.Log("Main Entrance: " + lowestIDEntranceTeleport.gameObject.name + " (Entrance ID: " + lowestIDEntranceTeleport.entranceId + ")" + " (Dungeon ID: " + lowestIDEntranceTeleport.dungeonFlowId + ")");
                foreach (EntranceTeleport entranceTeleport in entranceTeleports)
                    if (entranceTeleport != lowestIDEntranceTeleport)
                    {
                        entranceTeleport.entranceId = dungeonIDCounter;
                        DebugHelper.Log("Alternate Entrance: " + entranceTeleport.gameObject.name + " (Entrance ID: " + entranceTeleport.entranceId + ")" + " (Dungeon ID: " + entranceTeleport.dungeonFlowId + ")");
                        dungeonIDCounter++;
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
}
