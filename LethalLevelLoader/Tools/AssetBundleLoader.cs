using DunGen;
using DunGen.Graph;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace LethalLevelLoader
{
    public static class AssetBundleLoader
    {
        public const string specifiedFileExtension = "*.lethalbundle";

        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        internal static List<ExtendedLevel> obtainedExtendedLevelsList = new List<ExtendedLevel>();
        internal static List<ExtendedDungeonFlow> obtainedExtendedDungeonFlowsList = new List<ExtendedDungeonFlow>();

        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        internal static void PreInitSceneScriptAwake()
        {
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
                FindBundles();
        }

        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(RoundManager), "Awake")]
        [HarmonyPostfix]
        internal static void RoundManagerAwake_Postfix()
        {
            
            if (LethalLevelLoaderPlugin.hasVanillaBeenPatched == false)
            {
                CreateVanillaExtendedDungeonFlows();
                CreateVanillaExtendedLevels(StartOfRound.Instance);
                InitializeBundles();
            }
        }

        [HarmonyPriority(350)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        [HarmonyPrefix]
        internal static void GameNetworkManagerStart_Prefix()
        {
            DebugHelper.Log("Registering Bundle Content!");

            foreach (ExtendedDungeonFlow extendedDungeonFlow in obtainedExtendedDungeonFlowsList)
                RegisterDungeonContent(extendedDungeonFlow.dungeonFlow);

            foreach (ExtendedLevel extendedLevel in obtainedExtendedLevelsList)
                RegisterCustomLevelNetworkObjects(extendedLevel);
        }

        internal static void FindBundles()
        {
            DebugHelper.Log("Finding LethalBundles!");
            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;

            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
                LoadBundle(file);
        }

        internal static void LoadBundle(string bundleFile)
        {
            AssetBundle newBundle = AssetBundle.LoadFromFile(bundleFile);

            if (newBundle != null)
            {
                DebugHelper.Log("Loading Custom Content From Bundle: " + newBundle.name);

                foreach (ExtendedLevel extendedLevel in newBundle.LoadAllAssets<ExtendedLevel>())
                    obtainedExtendedLevelsList.Add(extendedLevel);
            }
        }

        internal static void InitializeBundles()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in obtainedExtendedDungeonFlowsList)
            {
                extendedDungeonFlow.Initialize(ContentType.Custom);
                extendedDungeonFlow.manualPlanetNameReferenceList.Add(new StringWithRarity("Tenebrous", 300));
                DungeonFlow_Patch.AddExtendedDungeonFlow(extendedDungeonFlow);
            }
            foreach (ExtendedLevel extendedLevel in obtainedExtendedLevelsList)
            {
                extendedLevel.Initialize(ContentType.Custom, generateTerminalAssets: true);
                SelectableLevel_Patch.AddSelectableLevel(extendedLevel);
                WarmUpBundleShaders(extendedLevel);
            }
            //DebugHelper.DebugAllLevels();
        }

        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            obtainedExtendedDungeonFlowsList.Add(extendedDungeonFlow);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            obtainedExtendedLevelsList.Add(extendedLevel);
        }

        internal static void CreateVanillaExtendedLevels(StartOfRound startOfRound)
        {
            DebugHelper.Log("Creating ExtendedLevels For Vanilla SelectableLevels");
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();

            foreach (SelectableLevel selectableLevel in startOfRound.levels)
            {
                //DebugHelper.Log("Moons SelectableLevel Is: " + (selectableLevel != null));
                ExtendedLevel extendedLevel = ScriptableObject.CreateInstance<ExtendedLevel>();

                int vanillaRoutePrice = 0;
                foreach (CompatibleNoun compatibleRouteNoun in Terminal_Patch.RouteKeyword.compatibleNouns)
                    if (compatibleRouteNoun.noun.name.Contains(selectableLevel.PlanetName))
                        vanillaRoutePrice = compatibleRouteNoun.result.itemCost;
                extendedLevel.Initialize(ContentType.Vanilla, newSelectableLevel: selectableLevel, newRoutePrice: vanillaRoutePrice, generateTerminalAssets: false);

                SetVanillaLevelTags(extendedLevel);
                SelectableLevel_Patch.AddSelectableLevel(extendedLevel);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlows()
        {
            DebugHelper.Log("Creating ExtendedDungeonFlows For Vanilla DungeonFlows");

            if (RoundManager.Instance.dungeonFlowTypes != null)
                foreach (DungeonFlow dungeonFlow in RoundManager.Instance.dungeonFlowTypes)
                    CreateVanillaExtendedDungeonFlow(dungeonFlow);
            else
                DebugHelper.Log("Error! RoundManager dungeonFlowTypes Array Was Null!");
        }

        internal static void CreateVanillaExtendedDungeonFlow(DungeonFlow dungeonFlow)
        {
            ExtendedDungeonFlow extendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            extendedDungeonFlow.dungeonFlow = dungeonFlow;
            extendedDungeonFlow.contentSourceName = "Lethal Company";
            extendedDungeonFlow.dungeonFirstTimeAudio = null;
            extendedDungeonFlow.Initialize(ContentType.Vanilla);
            DungeonFlow_Patch.AddExtendedDungeonFlow(extendedDungeonFlow);
            //Gotta assign the right audio later.
        }

        internal static void RestoreVanillaDungeonAssetReferences(ExtendedDungeonFlow extendedDungeonFlow)
        {
            Tile[] allTiles = GetAllTilesInDungeonFlow(extendedDungeonFlow.dungeonFlow);

            foreach (RandomMapObject randomMapObject in GetAllMapObjectsInTiles(allTiles))
            {
                foreach (GameObject spawnablePrefab in new List<GameObject>(randomMapObject.spawnablePrefabs))
                    foreach (GameObject vanillaPrefab in ContentExtractor.vanillaSpawnableInsideMapObjectsList)
                        if (spawnablePrefab.name == vanillaPrefab.name)
                        {
                            int index = randomMapObject.spawnablePrefabs.IndexOf(spawnablePrefab);
                            randomMapObject.spawnablePrefabs[index] = vanillaPrefab;
                        }
            }
        }

        internal static void RestoreVanillaLevelAssetReferences(ExtendedLevel extendedLevel)
        {

            AudioSource[] moonAudioSources = extendedLevel.levelPrefab.GetComponentsInChildren<AudioSource>();

            //DebugHelper.Log("Found " + moonAudioSources.Length + " AudioSources In Custom Moon: " + extendedLevel.NumberlessPlanetName);

            foreach (AudioSource audioSource in moonAudioSources)
            {
                if (audioSource.outputAudioMixerGroup == null)
                {
                    audioSource.outputAudioMixerGroup = ContentExtractor.vanillaAudioMixerGroupsList[0];
                    DebugHelper.Log("AudioGroupMixer Reference Inside " + audioSource.name + " Was Null, Assigning Master SFX Mixer For Safety!");
                }
            }

            foreach (SpawnableItemWithRarity spawnableItem in extendedLevel.selectableLevel.spawnableScrap)
                foreach (Item vanillaItem in ContentExtractor.vanillaItemsList)
                    if (spawnableItem.spawnableItem.itemName == vanillaItem.itemName)
                        spawnableItem.spawnableItem = vanillaItem;

            foreach (SpawnableEnemyWithRarity spawnableEnemy in extendedLevel.selectableLevel.Enemies)
                foreach (EnemyType vanillaEnemy in ContentExtractor.vanillaEnemiesList)
                    if (spawnableEnemy.enemyType != null && spawnableEnemy.enemyType.enemyName == vanillaEnemy.enemyName)
                        spawnableEnemy.enemyType = vanillaEnemy;

            foreach (SpawnableEnemyWithRarity enemyType in extendedLevel.selectableLevel.OutsideEnemies)
                foreach (EnemyType vanillaEnemyType in ContentExtractor.vanillaEnemiesList)
                    if (enemyType.enemyType != null && enemyType.enemyType.enemyName == vanillaEnemyType.enemyName)
                        enemyType.enemyType = vanillaEnemyType;

            foreach (SpawnableEnemyWithRarity enemyType in extendedLevel.selectableLevel.DaytimeEnemies)
                foreach (EnemyType vanillaEnemyType in ContentExtractor.vanillaEnemiesList)
                    if (enemyType.enemyType != null && enemyType.enemyType.enemyName == vanillaEnemyType.enemyName)
                        enemyType.enemyType = vanillaEnemyType;

            foreach (SpawnableMapObject spawnableMapObject in extendedLevel.selectableLevel.spawnableMapObjects)
                foreach (GameObject vanillaSpawnableMapObject in ContentExtractor.vanillaSpawnableInsideMapObjectsList)
                    if (spawnableMapObject.prefabToSpawn != null && spawnableMapObject.prefabToSpawn.name == vanillaSpawnableMapObject.name)
                        spawnableMapObject.prefabToSpawn = vanillaSpawnableMapObject;

            foreach (SpawnableOutsideObjectWithRarity spawnableOutsideObject in extendedLevel.selectableLevel.spawnableOutsideObjects)
                foreach (SpawnableOutsideObject vanillaSpawnableOutsideObject in ContentExtractor.vanillaSpawnableOutsideMapObjectsList)
                    if (spawnableOutsideObject.spawnableObject != null && spawnableOutsideObject.spawnableObject.name == vanillaSpawnableOutsideObject.name)
                        spawnableOutsideObject.spawnableObject = vanillaSpawnableOutsideObject;

            foreach (LevelAmbienceLibrary vanillaAmbienceLibrary in ContentExtractor.vanillaAmbienceLibrariesList)
                if (extendedLevel.selectableLevel.levelAmbienceClips != null && extendedLevel.selectableLevel.levelAmbienceClips.name == vanillaAmbienceLibrary.name)
                    extendedLevel.selectableLevel.levelAmbienceClips = vanillaAmbienceLibrary;

        }

        internal static void RegisterCustomLevelNetworkObjects(ExtendedLevel extendedLevel)
        {
            int debugCounter = 0;
            foreach (NetworkObject networkObject in extendedLevel.levelPrefab.GetComponentsInChildren<NetworkObject>())
            {
                NetworkManager_Patch.RegisterNetworkPrefab(networkObject.gameObject); 
                debugCounter++;
            }

            DebugHelper.Log("Registered " + debugCounter + " NetworkObject's Found In Custom Level: " + extendedLevel.NumberlessPlanetName);
        }

        internal static void RegisterDungeonContent(DungeonFlow dungeonFlow)
        {
            Tile[] allTiles = GetAllTilesInDungeonFlow(dungeonFlow);
            int debugCounter = 0;
            foreach (SpawnSyncedObject spawnSyncedObject in GetAllSpawnSyncedObjectsInTiles(allTiles))
            {
                NetworkManager_Patch.TryRestoreVanillaSpawnSyncPrefab(spawnSyncedObject);
                if (spawnSyncedObject.spawnPrefab.GetComponent<NetworkObject>() == null)
                    spawnSyncedObject.spawnPrefab.AddComponent<NetworkObject>();
                NetworkManager_Patch.RegisterNetworkPrefab(spawnSyncedObject.spawnPrefab);
                debugCounter++;
            }

            DebugHelper.Log("Registered " + debugCounter + " NetworkObject's Found In Custom DungeonFlow: " + dungeonFlow.name);
        }

        internal static void WarmUpBundleShaders(ExtendedLevel extendedLevel)
        {
            List<(Shader, ShaderWarmupSetup)> shaderWithWarmupSetupList = new List<(Shader, ShaderWarmupSetup)>();

            ShaderWarmupSetup warmupSetup;
            foreach (MeshRenderer meshRenderer in extendedLevel.levelPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    warmupSetup = new ShaderWarmupSetup();
                    warmupSetup.vdecl = meshFilter.mesh.GetVertexAttributes();

                    foreach (Material material in meshRenderer.materials)
                        shaderWithWarmupSetupList.Add((material.shader, warmupSetup));
                }
            }
            DebugHelper.Log("Warming Up " + shaderWithWarmupSetupList.Count + " Shaders Found In: " + extendedLevel.NumberlessPlanetName);
            foreach ((Shader, ShaderWarmupSetup) shaderWithWarmupSetup in shaderWithWarmupSetupList)
            {
                ShaderWarmup.WarmupShader(shaderWithWarmupSetup.Item1, shaderWithWarmupSetup.Item2);
            }
        }

        internal static void SetVanillaLevelTags(ExtendedLevel vanillaLevel)
        {
            vanillaLevel.levelTags.Add("Vanilla");

            foreach (IntWithRarity intWithRarity in vanillaLevel.selectableLevel.dungeonFlowTypes)
                if (DungeonFlow_Patch.TryGetExtendedDungeonFlow(RoundManager.Instance.dungeonFlowTypes[intWithRarity.id], out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.manualPlanetNameReferenceList.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, intWithRarity.rarity));

            if (vanillaLevel.NumberlessPlanetName == "Experimentation")
                vanillaLevel.levelTags.Add("Wasteland");
            else if (vanillaLevel.NumberlessPlanetName == "Assurance")
            {
                vanillaLevel.levelTags.Add("Desert");
                vanillaLevel.levelTags.Add("Canyon");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Vow")
            {
                vanillaLevel.levelTags.Add("Forest");
                vanillaLevel.levelTags.Add("Valley");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Gordion")
            {
                vanillaLevel.levelTags.Add("Company");
                vanillaLevel.levelTags.Add("Quota");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Offense")
            {
                vanillaLevel.levelTags.Add("Desert");
                vanillaLevel.levelTags.Add("Canyon");
            }
            else if (vanillaLevel.NumberlessPlanetName == "March")
            {
                vanillaLevel.levelTags.Add("Forest");
                vanillaLevel.levelTags.Add("Valley");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Rend")
            {
                vanillaLevel.levelTags.Add("Snow");
                vanillaLevel.levelTags.Add("Ice");
                vanillaLevel.levelTags.Add("Tundra");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Dine")
            {
                vanillaLevel.levelTags.Add("Snow");
                vanillaLevel.levelTags.Add("Ice");
                vanillaLevel.levelTags.Add("Tundra");
            }
            else if (vanillaLevel.NumberlessPlanetName == "Titan")
            {
                vanillaLevel.levelTags.Add("Snow");
                vanillaLevel.levelTags.Add("Ice");
                vanillaLevel.levelTags.Add("Tundra");
            }
        }

        public static Tile[] GetAllTilesInDungeonFlow(DungeonFlow dungeonFlow)
        {
            List<Tile> tilesList = new List<Tile>();

            foreach (GraphNode dungeonNode in dungeonFlow.Nodes)
                foreach (TileSet dungeonTileSet in dungeonNode.TileSets)
                    foreach (GameObjectChance dungeonTileWeight in dungeonTileSet.TileWeights.Weights)
                        foreach (Tile dungeonTile in dungeonTileWeight.Value.GetComponentsInChildren<Tile>())
                            tilesList.Add(dungeonTile);

            foreach (GraphLine dungeonLine in dungeonFlow.Lines)
                foreach (DungeonArchetype dungeonArchetype in dungeonLine.DungeonArchetypes)
                {
                    foreach (TileSet dungeonTileSet in dungeonArchetype.BranchCapTileSets)
                        foreach (GameObjectChance dungeonTileWeight in dungeonTileSet.TileWeights.Weights)
                            foreach (Tile dungeonTile in dungeonTileWeight.Value.GetComponentsInChildren<Tile>())
                                tilesList.Add(dungeonTile);

                    foreach (TileSet dungeonTileSet in dungeonArchetype.TileSets)
                        foreach (GameObjectChance dungeonTileWeight in dungeonTileSet.TileWeights.Weights)
                            foreach (Tile dungeonTile in dungeonTileWeight.Value.GetComponentsInChildren<Tile>())
                                tilesList.Add(dungeonTile);
                }

            return (tilesList.ToArray());
        }

        public static RandomMapObject[] GetAllMapObjectsInTiles(Tile[] tiles)
        {
            List<RandomMapObject> returnList = new List<RandomMapObject>();

            foreach (Tile dungeonTile in tiles)
                foreach (RandomMapObject randomMapObject in dungeonTile.gameObject.GetComponentsInChildren<RandomMapObject>())
                {
                    returnList.Add(randomMapObject);
                }

            return (returnList.ToArray());
        }

        public static SpawnSyncedObject[] GetAllSpawnSyncedObjectsInTiles(Tile[] tiles)
        {
            List<SpawnSyncedObject> returnList = new List<SpawnSyncedObject>();

            foreach (Tile dungeonTile in tiles)
            {
                foreach (Doorway dungeonDoorway in dungeonTile.gameObject.GetComponentsInChildren<Doorway>())
                {
                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.ConnectorPrefabWeights)
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                            returnList.Add(spawnSyncedObject);

                    foreach (GameObjectWeight doorwayTileWeight in dungeonDoorway.BlockerPrefabWeights)
                        foreach (SpawnSyncedObject spawnSyncedObject in doorwayTileWeight.GameObject.GetComponentsInChildren<SpawnSyncedObject>())
                            returnList.Add(spawnSyncedObject);
                }

                foreach (SpawnSyncedObject spawnSyncedObject in dungeonTile.gameObject.GetComponentsInChildren<SpawnSyncedObject>())
                    returnList.Add(spawnSyncedObject);
            }
            return (returnList.ToArray());
        }
    }
}
