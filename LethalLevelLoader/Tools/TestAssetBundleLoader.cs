using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using static LethalLevelLoader.AssetBundleLoader;

namespace LethalLevelLoader
{
    public class TestAssetBundleLoader : MonoBehaviour
    {
        public static TestAssetBundleLoader Instance;
        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        List<string> nonSceneBundleStrings = new List<string>();

        List<ExtendedMod> deeplicatedMods = new List<ExtendedMod>();
        List<ExtendedContent> deeplicatedContent = new List<ExtendedContent>();

        private int startedCounter;
        private int finishedCounter;

        Dictionary<string,string> sceneNameBundleNameDict = new Dictionary<string,string>();

        internal void LoadAllBundles()
        {
            lethalLibFolder = lethalLibFile.Parent;
            pluginsFolder = lethalLibFile.Parent.Parent;
            Instance = this;

            startedCounter = 0;
            finishedCounter = 0;
            foreach (string file in Directory.GetFiles(pluginsFolder.FullName, specifiedFileExtension, SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!fileInfo.Name.Contains("scene"))
                {
                    startedCounter++;
                    LethalBundleInfo newBundleInfo = new LethalBundleInfo(fileInfo.Name);
                    assetBundles.Add(newBundleInfo);
                    UpdateLoadingBundlesHeaderText(null);
                    StartCoroutine(LoadBundle(file, newBundleInfo));
                }
                else
                    DebugHelper.LogWarning("Skipping " + file, DebugType.User);

            }

            if (startedCounter == 0)
            {
                DebugHelper.Log("No Bundles Found!", DebugType.User);
                noBundlesFound = true;
                OnBundlesFinishedLoading();
                CurrentLoadingStatus = LoadingStatus.Complete;
            }
        }

        IEnumerator LoadBundle(string bundleFile,  LethalBundleInfo bundleInfo)
        {
            string bundleString = Path.Combine(Application.streamingAssetsPath, bundleFile);
            AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromFileAsync(bundleString);
            yield return newBundleRequest;

            AssetBundle newBundle = newBundleRequest.assetBundle;
            string debugString = "Loading Test Bundle: " + newBundle.name + "\n";
            finishedCounter++;
            if (newBundle.isStreamedSceneAssetBundle == false)
            {
                nonSceneBundleStrings.Add(bundleString);
                ExtendedMod[] extendedMods = newBundle.LoadAllAssets<ExtendedMod>();
                ExtendedContent[] extendedContents = newBundle.LoadAllAssets<ExtendedContent>();
                List<ExtendedContent> processedList = new List<ExtendedContent>();
                List<ExtendedContent> orphanedContent = new List<ExtendedContent>();
                foreach (ExtendedMod extendedMod in extendedMods)
                {
                    foreach (ExtendedContent content in extendedMod.ExtendedContents)
                        processedList.Add(content);
                    DeeplicateAndRegisterMod(extendedMod);
                }
                foreach (ExtendedContent content in extendedContents)
                    if (!processedList.Contains(content))
                        if (DeeplicateExtendedContent(content, out ExtendedContent copiedContent))
                            orphanedContent.Add(copiedContent);
                if (orphanedContent.Count > 0)
                    PatchedContent.RegisterExtendedMod(ExtendedMod.Create(bundleFile + extendedContents.Length, bundleFile + extendedContents.Length, orphanedContent.ToArray()));
                debugString += "Found #" + extendedMods.Length + " ExtendedMods & #" + extendedContents.Length + " ExtendedContents" + "\n";
            }
            else
            {
                string[] scenePaths = newBundle.GetAllScenePaths();
                foreach (string sceneName in newBundle.GetAllScenePaths())
                    if (!sceneNameBundleNameDict.ContainsKey(GetSceneName(sceneName)))
                        sceneNameBundleNameDict.Add(GetSceneName(sceneName), bundleString);
                debugString += "Found #" + scenePaths.Length + " ScenePaths" + "\n";
            }

            DebugHelper.Log(debugString, DebugType.User);
            //newBundle.Unload(true);
            //Resources.UnloadUnusedAssets();
            //Caching.ClearCache();
            //Caching.CleanCache();

            if (startedCounter == finishedCounter)
                OnBundlesFinishedLoading();
        }

        internal bool TryLoadSceneBundle(string sceneName)
        {
            if (sceneNameBundleNameDict.TryGetValue(sceneName, out string sceneBundleName))
            {
                StartCoroutine(LoadSceneBundle(sceneBundleName));
                return (true);
            }
            return (false);
        }

        IEnumerator LoadSceneBundle(string sceneBundleName)
        {
            AssetBundleCreateRequest newBundleRequest = AssetBundle.LoadFromFileAsync(sceneBundleName);
            yield return newBundleRequest;

            AssetBundle newBundle = newBundleRequest.assetBundle;

            DebugHelper.Log("Hotloaded Scene Bundle: " + newBundle.name, DebugType.User);
        }

        internal void DeeplicateAndRegisterMod(ExtendedMod bundleMod)
        {
            List<ExtendedContent> extendedContents = new List<ExtendedContent>(bundleMod.ExtendedContents);
            List<ExtendedContent> deeplicatedContents = new List<ExtendedContent>();
            ExtendedMod deeplicatedmod = DeeplicateExtendedMod(bundleMod);
            foreach (ExtendedContent extendedContent in extendedContents)
                if (DeeplicateExtendedContent(extendedContent, out ExtendedContent deeplicatedContent))
                    deeplicatedContents.Add(deeplicatedContent);

            ExtendedMod orphanMod = ExtendedMod.Create(deeplicatedmod.ModName, deeplicatedmod.AuthorName, deeplicatedContents.ToArray());
            orphanMod.ModMergeSetting = ModMergeSetting.Disabled;
            PatchedContent.RegisterExtendedMod(ExtendedMod.Create(deeplicatedmod.ModName, deeplicatedmod.AuthorName, deeplicatedContents.ToArray()));
        }

        internal ExtendedMod DeeplicateExtendedMod(ExtendedMod bundleMod)
        {
            ExtendedMod copyMod = ScriptableObject.Instantiate(bundleMod);

            List<ExtendedContent> extendedContents = new List<ExtendedContent>(copyMod.ExtendedContents);
            List<ExtendedContent> copiedContent = new List<ExtendedContent>();
            copyMod.ClearAllExtendedContent();

            //foreach (ExtendedContent content in extendedContents)
                //copiedContent.Add(DeeplicateExtendedContent(content));

            return (copyMod);
        }

        internal bool DeeplicateExtendedContent(ExtendedContent content, out ExtendedContent copyContent)
        {
            copyContent = ScriptableObject.Instantiate(content);
            copyContent.ContentTags.Clear();
            copyContent.DataTags.Clear();
            copyContent.ExtendedMod = null;
            if (copyContent is ExtendedDungeonFlow dungeon)
                copyContent = null;
            else if (copyContent is ExtendedItem item)
            {
                item.LevelMatchingProperties = TryDuplicate(item.LevelMatchingProperties);
                item.DungeonMatchingProperties = TryDuplicate(item.DungeonMatchingProperties);
                if (item.Item != null)
                    item.Item = DeeplicateItem(item.Item);
                else
                    item.Item = DeeplicateItem(ScriptableObject.CreateInstance<Item>());
            }
            else if (copyContent is ExtendedEnemyType enemyType)
            {
                enemyType.DaytimeLevelMatchingProperties = TryDuplicate(enemyType.DaytimeLevelMatchingProperties);
                enemyType.OutsideLevelMatchingProperties = TryDuplicate(enemyType.OutsideLevelMatchingProperties);
                enemyType.InsideDungeonMatchingProperties = TryDuplicate(enemyType.InsideDungeonMatchingProperties);
                enemyType.InsideLevelMatchingProperties = TryDuplicate(enemyType.InsideLevelMatchingProperties);
                if (enemyType.EnemyType != null)
                    enemyType.EnemyType = DeeplicateEnemyType(enemyType.EnemyType);
                else
                    enemyType.EnemyType = DeeplicateEnemyType(ScriptableObject.CreateInstance<EnemyType>());
            }
            else if (copyContent is ExtendedLevel level)
            {
                if (level.SelectableLevel != null)
                {
                    level.SelectableLevel = DeeplicateSelectableLevel(level.SelectableLevel);
                    level.selectableLevel = null;
                }
                else if (level.selectableLevel != null)
                {
                    level.SelectableLevel = DeeplicateSelectableLevel(level.selectableLevel);
                    level.selectableLevel = null;
                }
            }

            return (copyContent != null);
        }

        internal T TryDuplicate<T>(T matchingProperties) where T : MatchingProperties
        {
            if (matchingProperties != null)
                return (ScriptableObject.Instantiate(matchingProperties) as T);
            else
                return (null);
        }

        internal Item DeeplicateItem(Item item)
        {
            Item copyItem = ScriptableObject.Instantiate<Item>(item);
            copyItem.spawnPrefab = PrefabHelper.CreatePrefab("ItemPrefab");
            copyItem.spawnPrefab.AddComponent<PhysicsProp>();
            copyItem.spawnPrefab.GetComponent<PhysicsProp>().itemProperties = copyItem;

            copyItem.itemIcon = null;
            copyItem.grabSFX = null;
            copyItem.dropSFX = null;
            copyItem.pocketSFX = null;
            copyItem.throwSFX = null;

            copyItem.meshVariants.Clear();
            copyItem.materialVariants.Clear();


            return (copyItem);
        }

        internal EnemyType DeeplicateEnemyType(EnemyType enemyType)
        {
            EnemyType copyEnemy = ScriptableObject.Instantiate(enemyType);
            copyEnemy.enemyPrefab = PrefabHelper.CreatePrefab("EnemyPrefab");
            copyEnemy.enemyPrefab.AddComponent<TestEnemy>();
            copyEnemy.enemyPrefab.GetComponent<TestEnemy>().enemyType = copyEnemy;

            copyEnemy.overrideVentSFX = null;
            copyEnemy.nestSpawnPrefab = null;
            copyEnemy.hitBodySFX = null;
            copyEnemy.hitEnemyVoiceSFX = null;
            copyEnemy.deathSFX = null;
            copyEnemy.stunSFX = null;
            copyEnemy.miscAnimations.Clear();
            copyEnemy.audioClips.Clear();
            return (copyEnemy);
        }

        internal SelectableLevel DeeplicateSelectableLevel(SelectableLevel level)
        {
            SelectableLevel copyLevel = ScriptableObject.Instantiate(level);
            copyLevel.planetPrefab = PrefabHelper.CreatePrefab("PlanetPrefab");
            copyLevel.planetPrefab.AddComponent<Animator>();
            copyLevel.planetPrefab.GetComponent<Animator>().runtimeAnimatorController = new RuntimeAnimatorController();
            copyLevel.videoReel = null;

            copyLevel.spawnableMapObjects = new List<SpawnableMapObject>().ToArray();
            copyLevel.spawnableOutsideObjects = new List<SpawnableOutsideObjectWithRarity>().ToArray();
            copyLevel.spawnableScrap.Clear();
            copyLevel.Enemies.Clear();
            copyLevel.DaytimeEnemies.Clear();
            copyLevel.OutsideEnemies.Clear();
            copyLevel.levelAmbienceClips = null;

            string sceneName = copyLevel.sceneName;
            NetworkScenePatcher.AddScenePath(sceneName);
            if (!PatchedContent.AllLevelSceneNames.Contains(sceneName))
                PatchedContent.AllLevelSceneNames.Add(sceneName);

            return (copyLevel);
        }
    }
}
