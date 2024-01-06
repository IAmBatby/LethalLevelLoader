using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;


namespace LethalLevelLoader
{
    public class LevelLoader
    {
        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPostfix]
        public static void OnLoadComplete1_Postfix()
        {
            if (SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName) != null)
                if (SelectableLevel_Patch.TryGetExtendedLevel(StartOfRound.Instance.currentLevel, out ExtendedLevel extendedLevel, (ContentType)ContentType.Custom))
                    InitializeCustomLevel(SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName), extendedLevel, false);
        }


        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPrefix]
        public static void Update_Prefix(RoundManager __instance)
        {
            if (__instance.timeScript == null) //I don't know why but RoundManager loses it's TimeOfDay reference.
                __instance.timeScript = TimeOfDay.Instance;
        }

        public static void InitializeCustomLevel(Scene scene, ExtendedLevel extendedLevel, bool disableTerrainOnFirstFrame = false)
        {
            foreach (GameObject obj in scene.GetRootGameObjects()) //Disable everything in the Scene were injecting into
            {
                obj.SetActive(false);
                //TODO HIGH - DESTROY THESE.
            }

            if (extendedLevel.levelPrefab != null)
            {
                GameObject instantiatedLevel = GameObject.Instantiate(extendedLevel.levelPrefab);

                if (instantiatedLevel != null)
                {
                    SceneManager.MoveGameObjectToScene(instantiatedLevel, scene); //We move and detatch to replicate vanilla moon scene hierarchy.

                    if (RoundManager.Instance.IsServer)
                        SpawnNetworkObjects(instantiatedLevel.scene);
                }
            }
            DebugHelper.DebugSelectableLevelReferences(extendedLevel);
        }

        public static void SpawnNetworkObjects(Scene scene)
        {
            int debugCounter = 0;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
                foreach (NetworkObject networkObject in rootObject.GetComponentsInChildren<NetworkObject>())
                    if (networkObject.IsSpawned == false)
                    {
                        networkObject.Spawn();
                        debugCounter++;
                    }

            DebugHelper.Log("Successfully Spawned " + debugCounter + "NetworkObjects Found In The Custom Level");
        }
    }
}