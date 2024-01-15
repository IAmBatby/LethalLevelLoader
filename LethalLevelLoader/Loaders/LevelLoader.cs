using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;

namespace LethalLevelLoader
{
    public class LevelLoader
    {
        internal static List<MeshCollider> customLevelMeshCollidersList = new List<MeshCollider>();

        internal static bool serverHasPatched;
        internal static bool levelHasPatched;

        internal static int clientsPatchedCount = 0;
        internal static List<ulong> clientIDs = new List<ulong>();

        [HarmonyPatch(typeof(StartOfRound), "SceneManager_OnLoadComplete1")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static void OnLoadComplete1_Prefix(StartOfRound __instance, ulong clientId, string sceneName)
        {
            if (SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName) != null)
                if (SelectableLevel_Patch.TryGetExtendedLevel(StartOfRound.Instance.currentLevel, out ExtendedLevel extendedLevel, ContentType.Custom))
                {
                    InitializeCustomLevel(SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName), extendedLevel);
                    ClientFinishedLoading(SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName), clientId);

                    SpawnNetworkObjects(SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName), clientId);
                }
        }

        internal static void ClientFinishedLoading(Scene scene, ulong clientID)
        {
            StartOfRound.Instance.ClientPlayerList.TryGetValue(clientID, out int currentClientID);

            if (!clientIDs.Contains(clientID))
                clientIDs.Add(clientID);

            string hostStatus = string.Empty;
            if (RoundManager.Instance.IsServer)
                hostStatus = "Host";
            else
                hostStatus = "Client";

            DebugHelper.Log("OnLoadComplete Callback! ClientID Was: " + clientID + ", Instance Is " + hostStatus + "! Client Callback Progress: (" + clientIDs.Count + " / " + StartOfRound.Instance.ClientPlayerList.Keys.Count + ")");
        }

        [HarmonyPatch(typeof(RoundManager), "Update")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static void Update_Prefix(RoundManager __instance)
        {
            if (__instance.timeScript == null) //I don't know why but RoundManager loses it's TimeOfDay reference.
                __instance.timeScript = TimeOfDay.Instance;
        }

        internal static void InitializeCustomLevel(Scene scene, ExtendedLevel extendedLevel, bool disableTerrainOnFirstFrame = false)
        {
            if (levelHasPatched == false)
            {
                levelHasPatched = true;

                foreach (GameObject obj in scene.GetRootGameObjects()) //Disable everything in the Scene were injecting into
                {
                    obj.SetActive(false);
                    GameObject.DestroyImmediate(obj);
                    //TODO HIGH - DESTROY THESE.
                }

                if (extendedLevel.levelPrefab != null)
                {
                    GameObject preModifiedLevel = extendedLevel.levelPrefab;
                    NetworkManager networkManager = UnityEngine.Object.FindObjectOfType<NetworkManager>();
                    List<GameObject> registeredPrefabs = new List<GameObject>();

                    foreach (NetworkPrefab networkPrefab in networkManager.NetworkConfig.Prefabs.m_Prefabs)
                        registeredPrefabs.Add(networkPrefab.Prefab);

                    if (RoundManager.Instance.IsServer == false)
                    {
                        DebugHelper.Log("Found " + preModifiedLevel.GetComponentsInChildren<NetworkObject>().Length + " NetworkObjects In Non-Host Client. Destroying!");
                        ToggleNetworkObjects(new List<NetworkObject>(preModifiedLevel.GetComponentsInChildren<NetworkObject>()), false);
                    }

                    customLevelMeshCollidersList = new List<MeshCollider>(preModifiedLevel.GetComponentsInChildren<MeshCollider>());

                    foreach (MeshCollider meshCollider in new List<MeshCollider>(customLevelMeshCollidersList))
                    {
                        if (meshCollider.enabled == true)
                        {
                            meshCollider.gameObject.name += " (LLL Tracked)";
                            meshCollider.enabled = false;
                        }
                        else
                            customLevelMeshCollidersList.Remove(meshCollider);
                    }

                    DebugHelper.Log(extendedLevel.NumberlessPlanetName + " Has " + customLevelMeshCollidersList.Count + " Enabled MeshCollider's, Temporarily Disabling While Level Loads To Prevent Freezing");

                    preModifiedLevel.SetActive(false);

                    GameObject instantiatedLevel = GameObject.Instantiate(preModifiedLevel);

                    if (instantiatedLevel != null)
                    {             
                        SceneManager.MoveGameObjectToScene(instantiatedLevel, scene); //We move and detatch to replicate vanilla moon scene hierarchy.
                        if (RoundManager.Instance.IsServer == false)
                            DestroyClientNetworkObjects(scene);
                    }

                    instantiatedLevel.SetActive(true);
                    preModifiedLevel.SetActive(true);

                    if (RoundManager.Instance.IsServer == false)
                    {
                        //ToggleNetworkObjects(new List<NetworkObject>(preModifiedLevel.GetComponentsInChildren<NetworkObject>()), true);
                    }
                }
            }
            //DebugHelper.DebugSelectableLevelReferences(extendedLevel);
        }

        internal static void ToggleNetworkObjects(List<NetworkObject> networkObjects, bool enabled)
        {
            foreach (NetworkObject networkObject in networkObjects)
            {
                networkObject.enabled = enabled;
                networkObject.gameObject.SetActive(enabled);
                networkObject.gameObject.name += " (" + enabled.ToString() + ")";
                //GameObject.DestroyImmediate(networkObject.gameObject);
            }
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
        [HarmonyPrefix]
        [HarmonyPriority(350)]
        internal static void FinishGeneratingNewLevelClientRpc()
        {
            EnableMeshColliders();
        }

        internal static async void EnableMeshColliders()
        {
            List<MeshCollider> instansiatedCustomLevelMeshColliders = new List<MeshCollider>();

            int counter = 0;
            foreach (MeshCollider meshCollider in UnityEngine.Object.FindObjectsOfType<MeshCollider>())
                if (meshCollider.gameObject.name.Contains(" (LLL Tracked)"))
                    instansiatedCustomLevelMeshColliders.Add(meshCollider);

            Task[] meshColliderEnableTasks = new Task[instansiatedCustomLevelMeshColliders.Count];

            foreach (MeshCollider meshCollider in instansiatedCustomLevelMeshColliders)
            {
                meshColliderEnableTasks[counter] = EnableMeshCollider(meshCollider);
                counter++;
            }

            await Task.WhenAll(meshColliderEnableTasks);

            //customLevelMeshCollidersList.Clear();
        }

        internal static async Task EnableMeshCollider(MeshCollider meshCollider)
        {
            meshCollider.enabled = true;
            meshCollider.gameObject.name.Replace(" (LLL Tracked)", "");
            await Task.Yield();
        }

        internal static void SpawnNetworkObjects(Scene scene, ulong clientID)
        {
            if (clientIDs.Count == StartOfRound.Instance.ClientPlayerList.Keys.Count && StartOfRound.Instance.IsServer && serverHasPatched == false)
            {
                DebugHelper.Log("Client IDs Count Is: " + clientIDs.Count + ". Instance IsServer?: " + StartOfRound.Instance.IsServer.ToString() + ". ServerHasPatched: " + serverHasPatched);

                GameObject[] rootObjects = scene.GetRootGameObjects();
                serverHasPatched = true;

                int debugCounter = 0;
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                    foreach (NetworkObject networkObject in rootObject.GetComponentsInChildren<NetworkObject>())
                        if (networkObject.IsSpawned == false)
                        {
                            networkObject.SceneMigrationSynchronization = true;
                            networkObject.Spawn();
                            //networkObject.TrySetParent(parent: rootObjects[0].transform);
                            //networkObject.TryRemoveParent();
                            debugCounter++;
                        }

                DebugHelper.Log("Successfully Spawned " + debugCounter + " Registered NetworkObjects Found In The Custom Level");
            }
        }

        internal static List<string> spawnedNetworkObjectNames = new List<string>();

        internal static void DestroyClientNetworkObjects(Scene scene)
        {
            if (StartOfRound.Instance.IsServer == false)
            {
                DebugHelper.Log("Attempting To Destroy All NetworkPrefabs On Client!");

                int debugCounter = 0;
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                    foreach (NetworkObject networkObject in rootObject.GetComponentsInChildren<NetworkObject>(includeInactive: true))
                        if (networkObject.IsSpawned == false)
                        {
                            spawnedNetworkObjectNames.Add(networkObject.gameObject.name);

                            GameObject.DestroyImmediate(networkObject.gameObject);
                            debugCounter++;
                        }

                DebugHelper.Log("Successfully Destroyed " + debugCounter + " Registered NetworkObjects Found In The Custom Level");
            }
        }

        [HarmonyPatch(typeof(NetworkSpawnManager), "CreateLocalNetworkObject")]
        [HarmonyPostfix]
        [HarmonyPriority(350)]
        internal static void CreateLocalNetworkObject(ref NetworkObject __result)
        {
            Scene parentScene = SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName);
            GameObject[] rootObjects = null;

            if (SceneManager.GetSceneByName(SelectableLevel_Patch.injectionSceneName) != null)
                if (SelectableLevel_Patch.TryGetExtendedLevel(StartOfRound.Instance.currentLevel, out ExtendedLevel extendedLevel, ContentType.Custom))
                {
                    if (__result != null)
                    {
                        if (__result.gameObject.name.Contains("(Clone)"))
                            __result.gameObject.name.Replace("(Clone)", "(Spawned NetworkObject)");
                        else
                            __result.gameObject.name += "(Spawned NetworkObject)";

                        rootObjects = parentScene.GetRootGameObjects();
                        DebugHelper.Log(__result.gameObject.name);
                        if (RoundManager.Instance.IsServer == false)
                            foreach (string name in new List<string>(spawnedNetworkObjectNames))
                                if (__result.gameObject.name.Contains(name))
                                {
                                    DebugHelper.Log("Trying To Move: " + __result.gameObject.name + " To " + parentScene.name);
                                    SceneManager.MoveGameObjectToScene(__result.gameObject, parentScene);
                                    __result.gameObject.SetActive(true);
                                    __result.enabled = true;
                                    //__result.TrySetParent(rootObjects[0]);
                                    spawnedNetworkObjectNames.Remove(name);
                                }
                    }
                }


        }
    }
}