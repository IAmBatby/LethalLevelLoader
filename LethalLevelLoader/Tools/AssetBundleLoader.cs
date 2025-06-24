using DunGen;
using DunGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public class AssetBundleLoader : MonoBehaviour
    {
        public static AssetBundleLoader Instance;

        internal Plugin pluginInstace;

        public const string specifiedFileExtension = "*.lethalbundle";

        internal static DirectoryInfo lethalLibFile = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        internal static DirectoryInfo lethalLibFolder;
        internal static DirectoryInfo pluginsFolder;

        internal static List<AssetBundleInfo> AssetBundleInfos { get; private set; } = new List<AssetBundleInfo>(); 

        internal static Dictionary<string, ExtendedMod> obtainedExtendedModsDictionary = new Dictionary<string, ExtendedMod>();

        public enum LoadingStatus { Inactive, Loading, Complete };
        public static LoadingStatus CurrentLoadingStatus { get; internal set; } = LoadingStatus.Inactive;

        internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>(); 
        internal static Dictionary<string, string> assetBundleLoadTimes = new Dictionary<string, string>();

        internal static bool HaveBundlesFinishedLoading
        {
            get
            {
                bool bundlesFinishedLoading = true;
                foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                    if (assetBundle.Value == null)
                        bundlesFinishedLoading = false;
                return (bundlesFinishedLoading);
            }
        }

        internal static int BundlesFinishedLoadingCount
        {
            get
            {
                int bundlesFinishedLoading = 0;
                foreach (KeyValuePair<string, AssetBundle> assetBundle in assetBundles)
                    if (assetBundle.Value != null)
                        bundlesFinishedLoading++;
                return (bundlesFinishedLoading);
            }
        }

        public delegate void BundlesFinishedLoading();
        public static event BundlesFinishedLoading onBundlesFinishedLoading;

        public delegate void BundleFinishedLoading(AssetBundle assetBundle);
        public static event BundleFinishedLoading onBundleFinishedLoading;

        internal static void InvokeBundlesFinishedLoading() => onBundlesFinishedLoading?.Invoke();

        public static bool TryGetAssetBundleInfo(string scenePath, out AssetBundleInfo info)
        {
            info = new AssetBundleInfo();
            foreach (AssetBundleInfo bundleInfo in AssetBundleInfos)
                if (bundleInfo.IsSceneBundle && bundleInfo.ContainsScene(scenePath))
                {
                    info = bundleInfo;
                    return (true);
                }

            return (false);
        }

        public static void AddOnLethalBundleLoadedListener(Action<AssetBundle> invokedFunction, string lethalBundleFileName)
        {
            AssetBundles.AssetBundleLoader.onLethalBundleLoadedRequestDict.AddOrAddAdd(lethalBundleFileName, invokedFunction);
        }

        public static void AddOnExtendedModLoadedListener(Action<ExtendedMod> invokedFunction, string extendedModAuthorName = null, string extendedModModName = null)
        {
            LethalBundleManager.onExtendedModLoadedRequestDict.AddOrAddAdd(extendedModAuthorName, invokedFunction);
            LethalBundleManager.onExtendedModLoadedRequestDict.AddOrAddAdd(extendedModModName, invokedFunction);
        }

        internal static void RegisterNewExtendedContent(ExtendedContent extendedContent, string fallbackName)
        {
            LethalBundleManager.RegisterNewExtendedContent(extendedContent, null);
        }

        internal static void InitializeBundles()
        {
            foreach (ExtendedContent content in PatchedContent.ExtendedMods.SelectMany(m => m.ExtendedContents))
            {
                content.ContentType = ContentType.Custom;
                content.Initialize();
            }
        }

        internal static void CreateVanillaExtendedLevels(StartOfRound startOfRound)
        {
            DebugHelper.Log("Creating ExtendedLevels For Vanilla SelectableLevels", DebugType.Developer);

            foreach (SelectableLevel selectableLevel in startOfRound.levels)
            {
                ExtendedLevel extendedLevel = ExtendedLevel.Create(selectableLevel);

                foreach (CompatibleNoun compatibleRouteNoun in TerminalManager.routeKeyword.compatibleNouns)
                    if (compatibleRouteNoun.noun.name.Contains(ExtendedLevel.GetNumberlessPlanetName(selectableLevel)))
                    {
                        extendedLevel.RouteNode = compatibleRouteNoun.result;
                        extendedLevel.RouteConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                        extendedLevel.RoutePrice = compatibleRouteNoun.result.itemCost;
                        break;
                    }
                PatchedContent.AllLevelSceneNames.Add(extendedLevel.SelectableLevel.sceneName);

                extendedLevel.Initialize();
                extendedLevel.name = extendedLevel.NumberlessPlanetName + "ExtendedLevel";

                LevelManager.TryRegisterContent(PatchedContent.VanillaMod, extendedLevel);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlows()
        {
            foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                CreateVanillaExtendedDungeonFlow(indoorMapType.dungeonFlow);
        }

        internal static void CreateVanillaExtendedItems()
        {
            foreach (Item scrapItem in OriginalContent.Items)
            {
                ExtendedItem extendedVanillaItem = ExtendedItem.Create(scrapItem, PatchedContent.VanillaMod, ContentType.Vanilla);
                extendedVanillaItem.IsBuyableItem = false;
                ItemManager.TryRegisterContent(PatchedContent.VanillaMod, extendedVanillaItem);
            }

            int counter = 0;
            foreach (Item item in TerminalManager.Terminal.buyableItemsList)
            {
                ExtendedItem extendedVanillaItem = ExtendedItem.Create(item, PatchedContent.VanillaMod, ContentType.Vanilla);
                extendedVanillaItem.IsBuyableItem = true;

                foreach (CompatibleNoun compatibleNoun in TerminalManager.buyKeyword.compatibleNouns)
                    if (compatibleNoun.result.buyItemIndex == counter)
                    {
                        extendedVanillaItem.BuyNode = compatibleNoun.result;
                        extendedVanillaItem.BuyConfirmNode = compatibleNoun.result.terminalOptions[0].result;
                        foreach (CompatibleNoun infoCompatibleNoun in TerminalManager.routeInfoKeyword.compatibleNouns)
                            if (infoCompatibleNoun.noun.word == compatibleNoun.noun.word)
                                extendedVanillaItem.BuyInfoNode = infoCompatibleNoun.result;
                    }
                ItemManager.TryRegisterContent(PatchedContent.VanillaMod, extendedVanillaItem);
                counter++;
            }
        }

        internal static void CreateVanillaExtendedEnemyTypes()
        {
            foreach (EnemyType enemyType in OriginalContent.Enemies)
            {
                ExtendedEnemyType newExtendedEnemyType = ExtendedEnemyType.Create(enemyType, PatchedContent.VanillaMod, ContentType.Vanilla);
                ScanNodeProperties enemyScanNode = newExtendedEnemyType.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
                newExtendedEnemyType.EnemyDisplayName = enemyType.enemyName;
                if (enemyScanNode == null) continue;
                newExtendedEnemyType.ScanNodeProperties = enemyScanNode;
                newExtendedEnemyType.EnemyID = enemyScanNode.creatureScanID;
                newExtendedEnemyType.EnemyInfoNode = Patches.Terminal.enemyFiles[newExtendedEnemyType.EnemyID];
                newExtendedEnemyType.InfoNodeVideoClip = newExtendedEnemyType.EnemyInfoNode ? newExtendedEnemyType.EnemyInfoNode.displayVideo : null;
                newExtendedEnemyType.EnemyDisplayName = enemyScanNode.headerText;
                EnemyManager.TryRegisterContent(PatchedContent.VanillaMod, newExtendedEnemyType);
            }
        }

        internal static void CreateVanillaExtendedWeatherEffects(StartOfRound startOfRound, TimeOfDay timeOfDay)
        {
            foreach (LevelWeatherType levelWeatherType in Enum.GetValues(typeof(LevelWeatherType)))
            {
                ExtendedWeatherEffect newExtendedWeatherEffect;
                if (levelWeatherType != LevelWeatherType.None)
                    newExtendedWeatherEffect = ExtendedWeatherEffect.Create(levelWeatherType, timeOfDay.effects[(int)levelWeatherType], levelWeatherType.ToString(), ContentType.Vanilla);
                else
                    newExtendedWeatherEffect = ExtendedWeatherEffect.Create(levelWeatherType, null, null, levelWeatherType.ToString(), ContentType.Vanilla);
                
                WeatherManager.TryRegisterContent(PatchedContent.VanillaMod, newExtendedWeatherEffect);
            }
        }

        internal static void CreateVanillaExtendedDungeonFlow(DungeonFlow dungeonFlow)
        {
            AudioClip firstTimeDungeonAudio = null;
            string dungeonDisplayName = string.Empty;

            if (dungeonFlow.name.Contains("Level1"))
            {
                dungeonDisplayName = "Facility";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[0];
            }
            else if (dungeonFlow.name.Contains("Level2"))
            {
                dungeonDisplayName = "Haunted Mansion";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[1];
            }
            else if (dungeonFlow.name.Contains("Level3"))
            {
                dungeonDisplayName = "Mineshaft";
            }

            ExtendedDungeonFlow extendedDungeonFlow = ExtendedDungeonFlow.Create(dungeonFlow, firstTimeDungeonAudio);
            extendedDungeonFlow.DungeonName = dungeonDisplayName;

            extendedDungeonFlow.Initialize();
            DungeonManager.TryRegisterContent(PatchedContent.VanillaMod, extendedDungeonFlow);

            if (extendedDungeonFlow.DungeonID == -1)
                DungeonManager.RefreshDungeonFlowIDs();
            //Gotta assign the right audio later.
        }

        internal static void CreateVanillaExtendedBuyableVehicles()
        {
            foreach (BuyableVehicle vanillaBuyableVehicle in Patches.Terminal.buyableVehicles)
                CreateVanillaExtendedBuyableVehicle(vanillaBuyableVehicle);
        }

        internal static void CreateVanillaExtendedBuyableVehicle(BuyableVehicle buyableVehicle)
        {
            ExtendedBuyableVehicle newExtendedVanillaBuyableVehicle = ExtendedBuyableVehicle.Create(buyableVehicle);
            VehiclesManager.TryRegisterContent(PatchedContent.VanillaMod, newExtendedVanillaBuyableVehicle);   
        }

        internal static void CreateVanillaExtendedUnlockableItems(StartOfRound startOfRound)
        {
            foreach (UnlockableItem vanillaUnlockableItem in OriginalContent.UnlockableItems)
                CreateVanillaExtendedUnlockableItem(vanillaUnlockableItem);
        }

        internal static void CreateVanillaExtendedUnlockableItem(UnlockableItem unlockableItem)
        {
            ExtendedUnlockableItem newExtendedVanillaUnlockableItem = ExtendedUnlockableItem.Create(unlockableItem, PatchedContent.VanillaMod, ContentType.Vanilla);
            UnlockableItemManager.TryRegisterContent(PatchedContent.VanillaMod, newExtendedVanillaUnlockableItem);
        }

        internal static void SetVanillaLevelTags(ExtendedLevel vanillaLevel)
        {
            foreach (IntWithRarity intWithRarity in vanillaLevel.SelectableLevel.dungeonFlowTypes)
                if (DungeonManager.TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonFlowTypes[intWithRarity.id].dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, intWithRarity.rarity));

            if (vanillaLevel.SelectableLevel.sceneName == "Level4March")
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    if (indoorMapType.dungeonFlow.name == "Level1Flow3Exits")
                        if (DungeonManager.TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out ExtendedDungeonFlow marchDungeonFlow))
                            marchDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(vanillaLevel.NumberlessPlanetName, 300));

            CompatibleNoun infoNoun = TerminalManager.routeInfoKeyword.compatibleNouns.Where(n => n.noun.word == vanillaLevel.NumberlessPlanetName.ToLower()).FirstOrDefault();
            vanillaLevel.InfoNode = infoNoun != null ? infoNoun.result : null;
        }

        internal static string GetSceneName(string scenePath)
        {
            return (scenePath.Substring(scenePath.LastIndexOf('/') + 1).Replace(".unity", ""));
        }

        public static void RegisterExtendedDungeonFlow(ExtendedDungeonFlow extendedDungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedDungeonFlow() is deprecated. Please move to PatchedContent.RegisterExtendedDungeonFlow() to prevent issues in following updates.", DebugType.Developer);
            PatchedContent.RegisterExtendedDungeonFlow(extendedDungeonFlow);
        }

        public static void RegisterExtendedLevel(ExtendedLevel extendedLevel)
        {
            DebugHelper.LogWarning("AssetBundleLoader.RegisterExtendedLevel() is deprecated. Please move to PatchedContent.RegisterExtendedLevel() to prevent issues in following updates.", DebugType.Developer);
            PatchedContent.RegisterExtendedLevel(extendedLevel);
        }

        public static Tile[] GetAllTilesInDungeonFlow(DungeonFlow dungeonFlow)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllTilesInDungeonFlow() is deprecated. Please move to dungeonFlow.GetTiles() to prevent issues in following updates.", DebugType.Developer);
            return (dungeonFlow.GetTiles().ToArray());
        }

        public static RandomMapObject[] GetAllMapObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllMapObjectsInTiles() is deprecated. Please move to dungeonFlow.GetRandomMapObjects() to prevent issues in following updates.", DebugType.Developer);
            return (new List<RandomMapObject>().ToArray());
        }

        public static SpawnSyncedObject[] GetAllSpawnSyncedObjectsInTiles(Tile[] tiles)
        {
            DebugHelper.LogWarning("AssetBundleLoader.GetAllSpawnSyncedObjectsInTiles() is deprecated. Please move to dungeonFlow.GetSpawnSyncedObjects() to prevent issues in following updates.", DebugType.Developer);
            return (new List<SpawnSyncedObject>().ToArray());
        }
    }
}
