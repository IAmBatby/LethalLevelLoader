using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static DunGen.Graph.DungeonFlow;
using Random = System.Random;

namespace LethalLevelLoader
{
    [System.Serializable]
    public class UnityEventDungeonGenerator : UnityEvent<DungeonGenerator> { }

    [System.Serializable]
    public class UnityEventSpawnMapHazards : UnityEvent<GameObject[]> { }

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
        public static UnityEventDungeonGenerator onBeforeDungeonGenerate;
        private static bool hasSetDungeonFlow;

        [HarmonyPatch(typeof(EntranceTeleport), "Awake")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static void EntranceTeleportAwake_Prefix(EntranceTeleport __instance)
        {
            DebugHelper.Log("EntranceTeleport Spawn!" + __instance.gameObject.name);
        }
        [HarmonyPatch(typeof(DungeonGenerator), "Generate")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static bool Generate_Prefix(DungeonGenerator __instance)
        {
            //DebugHelper.Log("Started To Prefix Patch DungeonGenerator Generate!");
            Scene scene;

            if (SelectableLevel_Patch.TryGetExtendedLevel(RoundManager.Instance.currentLevel, out ExtendedLevel extendedLevel))
            {
                scene = SceneManager.GetSceneByName(extendedLevel.selectableLevel.sceneName);
                DebugHelper.Log("DungeonGenerator Prefix");
                if (hasSetDungeonFlow == false)
                {
                    if (NetworkManager.Singleton.IsServer == true)
                    {
                        int extendedLevelID = SelectableLevel_Patch.allLevelsList.IndexOf(extendedLevel);
                        DebugHelper.Log("ExtendedLevel ID: " + extendedLevelID);
                        LethalLevelLoaderNetworkBehaviour.Instance.SetDungeonFlowServerRpc(extendedLevelID);
                        return (false);
                    }
                    else
                        return (false);
                }

                PatchDungeonSize(__instance, extendedLevel);
                PatchFireEscapes(__instance, extendedLevel, scene);
                PatchDynamicGlobalProps(__instance);

                DungeonFlow_Patch.TryGetExtendedDungeonFlow(__instance.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow);
                onBeforeDungeonGenerate?.Invoke(__instance);
                extendedDungeonFlow.onBeforeExtendedDungeonGenerate?.Invoke(__instance);
                hasSetDungeonFlow = false;
            }

            return (true);
        }

        internal static void HasSetDungeonFlow(bool hasSetDungeonFlowParam)
        {
            hasSetDungeonFlow = hasSetDungeonFlowParam;
        }

        internal static void PatchDungeonSize(DungeonGenerator dungeonGenerator, ExtendedLevel extendedLevel)
        {
            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
            {
                if ((int)extendedLevel.selectableLevel.factorySizeMultiplier == (int)extendedDungeonFlow.dungeonSizeMax)
                if (extendedLevel.selectableLevel.factorySizeMultiplier > extendedDungeonFlow.dungeonSizeMax && !Mathf.Approximately(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMax))
                {
                    float newDungeonSize = Mathf.Lerp(extendedDungeonFlow.dungeonSizeMax, extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeLerpPercentage);
                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Requested A Dungeon Size Of " + extendedLevel.selectableLevel.factorySizeMultiplier + ". This Value Exceeds The Dungeon's Supplied Maximum Size, Scaling It Down To " + newDungeonSize);
                    dungeonGenerator.LengthMultiplier = newDungeonSize * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                }
                else if (extendedLevel.selectableLevel.factorySizeMultiplier < extendedDungeonFlow.dungeonSizeMin && !Mathf.Approximately(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMin))
                {
                    float newDungeonSize = Mathf.Lerp(extendedLevel.selectableLevel.factorySizeMultiplier, extendedDungeonFlow.dungeonSizeMin, extendedDungeonFlow.dungeonSizeLerpPercentage);
                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Requested A Dungeon Size Of " + extendedLevel.selectableLevel.factorySizeMultiplier + ". This Value Exceeds The Dungeon's Supplied Minimum Size, Scaling It Down To " + newDungeonSize);
                    dungeonGenerator.LengthMultiplier = newDungeonSize * RoundManager.Instance.mapSizeMultiplier; //This is how vanilla does it.
                }
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

            DebugHelper.Log("DungeonGenerator Is: " + dungeonGenerator);
            DebugHelper.Log("ExtendedLevel Is: " + extendedLevel);
            DebugHelper.Log("Scene Is: " + scene);
            DebugHelper.Log("RoundManager Is: " + RoundManager.Instance);

            List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
            EntranceTeleport lowestIDEntranceTeleport = null;

            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
            {
                    foreach (EntranceTeleport entranceTeleport in GetEntranceTeleports(scene))
                    {
                        entranceTeleport.dungeonFlowId = extendedDungeonFlow.dungeonID;

                        if (extendedDungeonFlow.dungeonType == ContentType.Vanilla)
                        {
                            if (extendedDungeonFlow.dungeonID < RoundManager.Instance.firstTimeDungeonAudios.Length)
                                entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[entranceTeleport.dungeonFlowId];
                            else
                                entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
                        }

                        if (extendedDungeonFlow.dungeonType == ContentType.Custom)
                        {
                            if (extendedDungeonFlow.dungeonFirstTimeAudio != null)
                                entranceTeleport.firstTimeAudio = extendedDungeonFlow.dungeonFirstTimeAudio;
                            else
                                entranceTeleport.firstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
                        }
                        if (lowestIDEntranceTeleport == null)
                            lowestIDEntranceTeleport = entranceTeleport;
                        if (lowestIDEntranceTeleport != null && entranceTeleport.entranceId < lowestIDEntranceTeleport.entranceId)
                            lowestIDEntranceTeleport = entranceTeleport;

                        entranceTeleports.Add(entranceTeleport);
                    }

                if (entranceTeleports.Count != 0)
                    debugString += "EntranceTeleport's Found, " + extendedLevel.NumberlessPlanetName + " Contains " + (entranceTeleports.Count) + " Entrances! ( " + (entranceTeleports.Count - 1) + " Fire Escapes) " + "\n";

                //To reduce the strict id requirements on entrance teleports
                lowestIDEntranceTeleport.entranceId = 0;

                int dungeonIDCounter = 1;
                debugString += "Main Entrance: " + lowestIDEntranceTeleport.gameObject.name + " (Entrance ID: " + lowestIDEntranceTeleport.entranceId + ")" + " (Dungeon ID: " + lowestIDEntranceTeleport.dungeonFlowId + ")" + "\n";
                foreach (EntranceTeleport entranceTeleport in entranceTeleports)
                    if (entranceTeleport != lowestIDEntranceTeleport)
                    {
                        entranceTeleport.entranceId = dungeonIDCounter;
                        debugString += "Alternate Entrance: " + entranceTeleport.gameObject.name + " (Entrance ID: " + entranceTeleport.entranceId + ")" + " (Dungeon ID: " + entranceTeleport.dungeonFlowId + ")" + "\n";
                        dungeonIDCounter++;
                    }

                Vector2 oldCount = Vector2.zero;
                foreach (GlobalPropSettings globalPropSettings in dungeonGenerator.DungeonFlow.GlobalProps)
                {
                    if (globalPropSettings.ID == 1231)
                    {
                        globalPropSettings.Count = new IntRange(entranceTeleports.Count - 1, entranceTeleports.Count - 1);
                        oldCount = new Vector2(globalPropSettings.Count.Min, globalPropSettings.Count.Max);
                    }
                }
                if (oldCount != Vector2.zero)
                    debugString += "Found Fire Escape GlobalProp: (ID: 1231), Modifying Spawnrate Count From (" + oldCount.x + "," + oldCount.y + ") To (" + (entranceTeleports.Count - 1) + "," + (entranceTeleports.Count - 1) + ")" + "\n";
                else
                    debugString += "Fire Escape GlobalProp Could Not Be Found! Fire Escapes Will Not Be Patched!" + "\n";


                DebugHelper.Log(debugString + "\n");
            }
        }

        public static void PatchDynamicGlobalProps(DungeonGenerator dungeonGenerator)
        {
            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(dungeonGenerator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                foreach (GlobalPropCountOverride globalPropOverride in extendedDungeonFlow.globalPropCountOverridesList)
                    foreach (GlobalPropSettings globalProp in dungeonGenerator.DungeonFlow.GlobalProps)
                    {
                        if (globalPropOverride.globalPropID == globalProp.ID)
                        {
                            globalProp.Count.Min = globalProp.Count.Min * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / RoundManager.Instance.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                            globalProp.Count.Max = globalProp.Count.Max * Mathf.RoundToInt(Mathf.Lerp(1, (dungeonGenerator.LengthMultiplier / RoundManager.Instance.mapSizeMultiplier), globalPropOverride.globalPropCountScaleRate));
                        }
                    }
        }

        private static List<GameObject> spawnMapObjectsPrespawnList = new List<GameObject>();

        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        public static void SpawnMapObjects_Prefix(RoundManager __instance)
        {
            spawnMapObjectsPrespawnList.Clear();

            foreach (Transform child in __instance.mapPropsContainer.transform)
                spawnMapObjectsPrespawnList.Add(child.gameObject);
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        public static void SpawnMapObjects_Postfix(RoundManager __instance)
        {
            List<GameObject> spawnedMapObjects = new List<GameObject>();

            foreach (Transform spawnedMapObject in __instance.mapPropsContainer.transform)
                if (!spawnMapObjectsPrespawnList.Contains(spawnedMapObject.gameObject))
                    spawnedMapObjects.Add(spawnedMapObject.gameObject);

            if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(__instance.dungeonGenerator.Generator.DungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                extendedDungeonFlow.onSpawnMapHazardsSpawn?.Invoke(spawnedMapObjects.ToArray());
        }
    }
}
