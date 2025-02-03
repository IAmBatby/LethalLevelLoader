using LethalLevelLoader.AssetBundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalLevelLoader
{
    public static class LethalBundleManager
    {
        public enum ModProcessingStatus { Inactive, Loading, Complete };
        public static ModProcessingStatus CurrentStatus { get; internal set; } = ModProcessingStatus.Inactive;

        private static List<AssetBundleGroup> assetBundleGroups = new List<AssetBundleGroup>();

        private static Dictionary<AssetBundleGroup, List<ExtendedMod>> obtainedExtendedModsDict = new Dictionary<AssetBundleGroup, List<ExtendedMod>>();
        private static List<ExtendedMod> obtainedExtendedModsList = new List<ExtendedMod>();

        public static ExtendedEvent OnFinishedProcessing { get; private set; } = new ExtendedEvent();

        public static bool HasFinalisedFoundContent { get; internal set; }

        //Semi legacy
        internal static Dictionary<string, List<Action<ExtendedMod>>> onExtendedModLoadedRequestDict = new Dictionary<string, List<Action<ExtendedMod>>>();

        internal static void Start()
        {
            DebugHelper.Log("LethalBundleManger: Starting!", DebugType.User);

            PatchedContent.VanillaMod = ExtendedMod.Create("LethalCompany", "Zeekerss");

            TryLoadLethalBundles();
        }

        private static bool TryLoadLethalBundles()
        {
            DebugHelper.Log("LethalBundleManger: Now Loading Bundles!", DebugType.User);

            if (AssetBundles.AssetBundleLoader.LoadAllBundlesRequest(specifiedFileExtension: ".lethalbundle", onProcessedCallback: OnAssetBundleGroupCreated))
            {
                CurrentStatus = ModProcessingStatus.Loading;
                AssetBundles.AssetBundleLoader.OnBundlesFinishedProcessing.AddListener(OnAssetBundleLoadRequestFinished);
                return (true);
            }
            else
                return (false);

        }

        private static void OnAssetBundleGroupCreated(AssetBundleGroup newGroup)
        {
            DebugHelper.Log("LethalBundleManger Recieved Group: " + newGroup.GroupName, DebugType.User);

            FindContentInAssetBundleGroup(newGroup);
        }

        private static void OnAssetBundleLoadRequestFinished()
        {
            DebugHelper.Log("LethalBundleManger Finished Requested Load", DebugType.User);
            AssetBundles.AssetBundleLoader.OnBundlesFinishedProcessing.RemoveListener(OnAssetBundleLoadRequestFinished);

            FinialiseFoundContent();
        }

        private static void FindContentInAssetBundleGroup(AssetBundleGroup group)
        {
            List<ExtendedMod> extendedMods = group.LoadAllAssets<ExtendedMod>();
            if (extendedMods.Count > 0)
            {
                foreach (ExtendedMod mod in extendedMods)
                    RegisterExtendedMod(mod, group);
            }
            else
            {
                foreach (ExtendedContent extendedContent in group.LoadAllAssets<ExtendedContent>())
                    RegisterNewExtendedContent(extendedContent, group);
            }
        }

        //All sorts of fucked
        internal static void RegisterExtendedMod(ExtendedMod extendedMod, AssetBundleGroup source)
        {
            DebugHelper.Log("Found ExtendedMod: " + extendedMod.name, DebugType.User);
            extendedMod.ModNameAliases.Add(extendedMod.ModName);
            ExtendedMod matchingExtendedMod = null;
            foreach (ExtendedMod registeredExtendedMod in obtainedExtendedModsList)
            {
                if (extendedMod.ModMergeSetting == ModMergeSetting.MatchingModName && registeredExtendedMod.ModMergeSetting == ModMergeSetting.MatchingModName)
                {
                    if (registeredExtendedMod.ModName == extendedMod.ModName)
                        matchingExtendedMod = registeredExtendedMod;
                }
                else if (extendedMod.ModMergeSetting == ModMergeSetting.MatchingAuthorName && registeredExtendedMod.ModMergeSetting == ModMergeSetting.MatchingAuthorName)
                {
                    if (registeredExtendedMod.AuthorName == extendedMod.AuthorName)
                        matchingExtendedMod = registeredExtendedMod;
                }
            }

            if (matchingExtendedMod != null)
            {
                if (!matchingExtendedMod.ModName.Contains(matchingExtendedMod.AuthorName))
                {
                    DebugHelper.Log("Renaming ExtendedMod: " + matchingExtendedMod.ModName + " To: " + matchingExtendedMod.AuthorName + "sMod" + " Due To Upcoming ExtendedMod Merge!", DebugType.Developer);
                    matchingExtendedMod.ModNameAliases.Add(extendedMod.ModName);
                    matchingExtendedMod.ModName = matchingExtendedMod.AuthorName + "sMod";
                    //matchingExtendedMod.name = matchingExtendedMod.ModName;
                }
                DebugHelper.Log("Merging ExtendedMod: " + extendedMod.ModName + " (" + extendedMod.AuthorName + ")" + " With Already Obtained ExtendedMod: " + matchingExtendedMod.ModName + " (" + matchingExtendedMod.AuthorName + ")", DebugType.Developer);
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                {
                    try
                    {
                        matchingExtendedMod.RegisterExtendedContent(extendedContent);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogError(ex, DebugType.User);
                    }
                }
            }
            else
            {
                obtainedExtendedModsList.Add(extendedMod);
                if (source != null)
                {
                    if (obtainedExtendedModsDict.TryGetValue(source, out List<ExtendedMod> extendedModList))
                    {
                        if (!extendedModList.Contains(extendedMod))
                            extendedModList.Add(extendedMod);
                    }
                    else
                        obtainedExtendedModsDict.Add(source, new List<ExtendedMod> { extendedMod });
                }


                List<ExtendedContent> serializedExtendedContents = new List<ExtendedContent>(extendedMod.ExtendedContents);
                extendedMod.UnregisterAllExtendedContent();
                foreach (ExtendedContent extendedContent in serializedExtendedContents)
                {
                    try
                    {
                        extendedMod.RegisterExtendedContent(extendedContent);
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogError(ex, DebugType.User);
                    }
                }
            }
        }

        internal static void RegisterNewExtendedContent(ExtendedContent extendedContent, AssetBundleGroup source)
        {
            if (extendedContent == null)
            {
                DebugHelper.LogError("Failed to register new ExtendedContent as it was null!", DebugType.User);
                return;
            }

            string fallbackName = source == null ? extendedContent.name : source.GroupName;


            ExtendedMod extendedMod = null;
            if (extendedContent is ExtendedLevel extendedLevel)
            {
                if (string.IsNullOrEmpty(extendedLevel.contentSourceName))
                    extendedLevel.contentSourceName = fallbackName;
                extendedMod = GetOrCreateExtendedMod(source, extendedLevel.contentSourceName);
            }
            else if (extendedContent is ExtendedDungeonFlow extendedDungeonFlow)
            {
                if (string.IsNullOrEmpty(extendedDungeonFlow.contentSourceName))
                    extendedDungeonFlow.contentSourceName = fallbackName;
                extendedMod = GetOrCreateExtendedMod(source, extendedDungeonFlow.contentSourceName);
            }
            else if (extendedContent is ExtendedItem extendedItem)
            {
                extendedMod = GetOrCreateExtendedMod(source, extendedItem.Item.itemName.RemoveWhitespace());
            }
            else if (extendedContent is ExtendedEnemyType extendedEnemyType)
            {
                extendedMod = GetOrCreateExtendedMod(source, extendedEnemyType.EnemyType.enemyName.RemoveWhitespace());
            }
            else if (extendedContent is ExtendedWeatherEffect extendedWeatherEffect)
            {
                //if (extendedWeatherEffect.contentSourceName == string.Empty)
                //extendedWeatherEffect.contentSourceName = fallbackName;
                //extendedMod = GetOrCreateExtendedMod(extendedWeatherEffect.contentSourceName);
            }
            else if (extendedContent is ExtendedBuyableVehicle extendedBuyableVehicle)
            {
                extendedMod = GetOrCreateExtendedMod(source, extendedBuyableVehicle.name);
            }

            if (extendedMod != null)
            {
                try
                {
                    extendedMod.RegisterExtendedContent(extendedContent);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError(ex, DebugType.User);
                }
            }
        }

        internal static ExtendedMod GetOrCreateExtendedMod(AssetBundleGroup source, string contentSourceName)
        {
            if (source == null)
            {
                DebugHelper.Log("Creating New ExtendedMod: " + contentSourceName, DebugType.Developer);
                ExtendedMod newExtendedMod = ExtendedMod.Create(contentSourceName);
                obtainedExtendedModsList.Add(newExtendedMod);
                return (newExtendedMod);
            }

            if (obtainedExtendedModsDict.TryGetValue(source, out List<ExtendedMod> extendedModList))
                return (extendedModList.First());
            else
            {
                DebugHelper.Log("Creating New ExtendedMod: " + contentSourceName, DebugType.Developer);
                ExtendedMod newExtendedMod = ExtendedMod.Create(contentSourceName);
                obtainedExtendedModsList.Add(newExtendedMod);
                if (obtainedExtendedModsDict.TryGetValue(source, out List<ExtendedMod> foundExtendedModList))
                {
                    if (!foundExtendedModList.Contains(newExtendedMod))
                        foundExtendedModList.Add(newExtendedMod);
                }
                else
                    obtainedExtendedModsDict.Add(source, new List<ExtendedMod> { newExtendedMod });
                return (newExtendedMod);
            }
        }

        internal static void FinialiseFoundContent()
        {
            foreach (ExtendedMod obtainedExtendedMod in obtainedExtendedModsList)
            {
                PatchedContent.ExtendedMods.Add(obtainedExtendedMod);
                DebugHelper.DebugExtendedMod(obtainedExtendedMod);
            }

            PatchedContent.ExtendedMods = new List<ExtendedMod>(PatchedContent.ExtendedMods.OrderBy(o => o.ModName).ToList());

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                extendedMod.SortRegisteredContent();

            //TODO: Mod requested onLethalBundleLoaded callbacks

            //TODO: Mod requested onExtendedModLoaded callbacks

            foreach (KeyValuePair<string, List<Action<ExtendedMod>>> extendedModRequest in onExtendedModLoadedRequestDict)
                foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods)
                    if (extendedMod.ModNameAliases.Contains(extendedModRequest.Key) || extendedMod.AuthorName == extendedModRequest.Key)
                        foreach (Action<ExtendedMod> extendedModEvent in extendedModRequest.Value)
                            extendedModEvent.Invoke(extendedMod); 


            NetworkRegisterCustomScenes();

            AssetBundles.AssetBundleLoader.ClearCache();

            DebugHelper.Log("Custom Content Processed. Unlocking Main Menu.", DebugType.User);

            HasFinalisedFoundContent = true;

            CurrentStatus = ModProcessingStatus.Complete;

            OnFinishedProcessing.Invoke();
        }

        internal static void NetworkRegisterCustomScenes()
        {
            //TODO For starters this is gonna be pretty unsafe
            List<string> vanillaSceneNames = new List<string>();
            List<string> customSceneNames = new List<string>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                vanillaSceneNames.Add(AssetBundleUtilities.GetSceneName(SceneUtility.GetScenePathByBuildIndex(i)));

            foreach (AssetBundleGroup assetBundleGroup in AssetBundles.AssetBundleLoader.Instance.AssetBundleGroups)
                foreach (AssetBundles.AssetBundleInfo info in assetBundleGroup.GetAssetBundleInfos())
                    foreach (string customSceneName in info.GetSceneNames())
                        if (!customSceneNames.Contains(customSceneName))
                            customSceneNames.Add(customSceneName);

            foreach (string customSceneName in customSceneNames)
                if (!vanillaSceneNames.Contains(customSceneName))
                {
                    NetworkScenePatcher.AddScenePath(customSceneName);
                    if (!PatchedContent.AllLevelSceneNames.Contains(customSceneName))
                        PatchedContent.AllLevelSceneNames.Add(customSceneName);
                }

            foreach (string loadedSceneName in PatchedContent.AllLevelSceneNames)
                DebugHelper.Log("Loaded SceneName: " + loadedSceneName, DebugType.Developer);
        }
    }
}
