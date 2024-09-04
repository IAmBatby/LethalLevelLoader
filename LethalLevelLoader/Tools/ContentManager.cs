using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine;

namespace LethalLevelLoader
{
    public static class ContentManager
    {
        public static bool HasInitialized { get; private set; }
        internal static void InitializeMods()
        {
            PatchedContent.VanillaMod = CreateVanillaExtendedMod();
            InitializeMod(PatchedContent.VanillaMod, ContentType.Vanilla);

            foreach (ExtendedMod customMod in PatchedContent.ExtendedMods)
                InitializeMod(customMod, ContentType.Custom);

            HasInitialized = true;
        }

        internal static void InitializeMod(ExtendedMod extendedMod, ContentType contentType)
        {
            DebugHelper.Log("Initializing Mod: " + extendedMod.ModName, DebugType.User);

            foreach (ExtendedContent registeredContent in extendedMod.ExtendedContents)
                InitializeExtendedContent(registeredContent, contentType);
        }

        internal static void RegisterExtendedMod(ExtendedMod extendedMod)
        {
            if (HasInitialized == true)
            {
                DebugHelper.LogError("Could Not Register ExtendedMod: " + extendedMod.name + " As It Has Missed Initialization Pass!", DebugType.User);
                return;
            }

            DebugHelper.Log("Found ExtendedMod: " + extendedMod.name, DebugType.User);
            extendedMod.ModNameAliases.Add(extendedMod.ModName);
            ExtendedMod activeExtendedMod = extendedMod;

            if (extendedMod.ModMergeSetting != ModMergeSetting.Disabled)
                foreach (ExtendedMod registeredExtendedMod in AssetBundleLoader.obtainedExtendedModsDictionary.Values)
                    if (registeredExtendedMod.ModMergeSetting != ModMergeSetting.Disabled)
                        if (extendedMod.ModMergeSetting == registeredExtendedMod.ModMergeSetting && (extendedMod.ModName == registeredExtendedMod.ModName || extendedMod.AuthorName == registeredExtendedMod.AuthorName))
                            activeExtendedMod = registeredExtendedMod;

            if (activeExtendedMod != extendedMod)
            {
                if (!activeExtendedMod.ModName.Contains(activeExtendedMod.AuthorName))
                {
                    DebugHelper.Log("Renaming ExtendedMod: " + activeExtendedMod.ModName + " To: " + activeExtendedMod.AuthorName + "sMod" + " Due To Upcoming ExtendedMod Merge!", DebugType.Developer);
                    activeExtendedMod.ModNameAliases.Add(extendedMod.ModName);
                    activeExtendedMod.ModName = activeExtendedMod.AuthorName + "sMod";
                }
                DebugHelper.Log("Merging ExtendedMod: " + extendedMod.ModName + " (" + extendedMod.AuthorName + ")" + " With Already Obtained ExtendedMod: " + activeExtendedMod.ModName + " (" + activeExtendedMod.AuthorName + ")", DebugType.Developer);
            }
            else
            {
                AssetBundleLoader.obtainedExtendedModsDictionary.Add(extendedMod.AuthorName, extendedMod);
            }

            List<ExtendedContent> serializedExtendedContents = new List<ExtendedContent>(activeExtendedMod.ExtendedContents);
            activeExtendedMod.ClearAllExtendedContent();
            foreach (ExtendedContent extendedContent in serializedExtendedContents)
            {
                try
                {
                    activeExtendedMod.RegisterExtendedContent(extendedContent);
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError(ex, DebugType.User);
                }
            }
        }

        internal static void InitializeExtendedContent(ExtendedContent extendedContent, ContentType contentType)
        {
            extendedContent.ContentType = contentType;
            extendedContent.Initialize();

            PatchedContent.ExtendedContentsLists[extendedContent.GetType()].Add(extendedContent);
            PatchedContent.ExtendedContents.Add(extendedContent);

            Debug.Log("Initialized: " + extendedContent.UniqueIdentificationName);
        }

        internal static ExtendedMod CreateVanillaExtendedMod()
        {
            ExtendedMod vanillaMod = ExtendedMod.Create("LethalCompany", "Zeekerss");
            List<ExtendedContent> allVanillaExtendedContents = new List<ExtendedContent>();

            foreach (SelectableLevel selectableLevel in OriginalContent.SelectableLevels)
                allVanillaExtendedContents.Add(CreateVanillaExtendedLevel(selectableLevel));

            foreach (DungeonFlow dungeonFlow in OriginalContent.DungeonFlows)
                allVanillaExtendedContents.Add(CreateVanillaExtendedDungeonFlow(dungeonFlow));

            foreach (Item item in OriginalContent.Items)
            {
                if (Patches.Terminal.buyableItemsList.Contains(item))
                    allVanillaExtendedContents.Add(CreateVanillaExtendedItem(item, isBuyable: true));
                else
                    allVanillaExtendedContents.Add(CreateVanillaExtendedItem(item, isBuyable: false));
            }

            foreach (EnemyType enemy in OriginalContent.Enemies)
                allVanillaExtendedContents.Add(CreateVanillaExtendedEnemyType(enemy));

            foreach (LevelWeatherType levelWeatherType in Enum.GetValues(typeof(LevelWeatherType)))
                allVanillaExtendedContents.Add(CreateVanillaExtendedWeatherEffect(levelWeatherType));

            foreach (BuyableVehicle buyableVehicle in OriginalContent.BuyableVehicles)
                allVanillaExtendedContents.Add(CreateVanillaExtendedBuyableVehicle(buyableVehicle));

            DebugHelper.Log("Created: " + allVanillaExtendedContents.Count + " Vanilla ExtendedContents! Processing!", DebugType.User);

            foreach (ExtendedContent extendedContent in allVanillaExtendedContents)
                vanillaMod.RegisterExtendedContent(extendedContent);

            return (vanillaMod);
        }

        internal static ExtendedLevel CreateVanillaExtendedLevel(SelectableLevel vanillaLevel)
        {
            ExtendedLevel extendedLevel = ExtendedLevel.Create(vanillaLevel);
            foreach (CompatibleNoun compatibleRouteNoun in TerminalManager.routeKeyword.compatibleNouns)
                if (compatibleRouteNoun.noun.name.Contains(ExtendedLevel.GetNumberlessPlanetName(vanillaLevel)))
                {
                    extendedLevel.RouteNode = compatibleRouteNoun.result;
                    extendedLevel.RouteConfirmNode = compatibleRouteNoun.result.terminalOptions[1].result;
                    extendedLevel.RoutePrice = compatibleRouteNoun.result.itemCost;
                    extendedLevel.OverrideCameraMaxDistance = 0f;
                    break;
                }
            return (extendedLevel);
        }

        internal static ExtendedDungeonFlow CreateVanillaExtendedDungeonFlow(DungeonFlow vanillaFlow)
        {
            string DungeonName = vanillaFlow.name;
            AudioClip firstTimeDungeonAudio = null;
            if (vanillaFlow.name.Contains("Level1"))
            {
                DungeonName = "Facility";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[0];
            }
            else if (vanillaFlow.name.Contains("Level2"))
            {
                DungeonName = "Haunted Mansion";
                firstTimeDungeonAudio = Patches.RoundManager.firstTimeDungeonAudios[1];
            }
            else if (vanillaFlow.name.Contains("Level3"))
            {
                DungeonName = "Mineshaft";
            }
            ExtendedDungeonFlow extendedDungeonFlow = ExtendedDungeonFlow.Create(vanillaFlow, firstTimeDungeonAudio);
            extendedDungeonFlow.DungeonName = DungeonName;

            if (extendedDungeonFlow.DungeonID == -1)
                DungeonManager.RefreshDungeonFlowIDs();

            return (extendedDungeonFlow);
        }

        internal static ExtendedItem CreateVanillaExtendedItem(Item item, bool isBuyable)
        {
            ExtendedItem extendedItem = ExtendedItem.Create(item);
            extendedItem.IsBuyableItem = isBuyable;

            int counter = 0;
            if (isBuyable == true)
                foreach (CompatibleNoun compatibleNoun in TerminalManager.buyKeyword.compatibleNouns)
                    if (compatibleNoun.result.buyItemIndex == counter)
                    {
                        extendedItem.BuyNode = compatibleNoun.result;
                        extendedItem.BuyConfirmNode = compatibleNoun.result.terminalOptions[0].result;
                        foreach (CompatibleNoun infoCompatibleNoun in TerminalManager.routeInfoKeyword.compatibleNouns)
                            if (infoCompatibleNoun.noun.word == compatibleNoun.noun.word)
                                extendedItem.BuyInfoNode = infoCompatibleNoun.result;
                    }

            return (extendedItem);
        }

        internal static ExtendedEnemyType CreateVanillaExtendedEnemyType(EnemyType enemyType)
        {
            ExtendedEnemyType extendedEnemy = ExtendedEnemyType.Create(enemyType);
            ScanNodeProperties enemyScanNode = extendedEnemy.EnemyType.enemyPrefab.GetComponentInChildren<ScanNodeProperties>();
            if (enemyScanNode != null)
            {
                extendedEnemy.ScanNodeProperties = enemyScanNode;
                extendedEnemy.EnemyID = enemyScanNode.creatureScanID;
                extendedEnemy.EnemyInfoNode = Patches.Terminal.enemyFiles[extendedEnemy.EnemyID];
                if (extendedEnemy.EnemyInfoNode != null)
                    extendedEnemy.InfoNodeVideoClip = extendedEnemy.EnemyInfoNode.displayVideo;
                extendedEnemy.EnemyDisplayName = enemyScanNode.headerText;
            }
            else
                extendedEnemy.EnemyDisplayName = enemyType.enemyName;

            return (extendedEnemy);
        }

        internal static ExtendedWeatherEffect CreateVanillaExtendedWeatherEffect(LevelWeatherType levelWeatherType)
        {
            if (levelWeatherType != LevelWeatherType.None)
                return (ExtendedWeatherEffect.Create(levelWeatherType, Patches.TimeOfDay.effects[(int)levelWeatherType], levelWeatherType.ToString()));
            else
                return (ExtendedWeatherEffect.Create(levelWeatherType, null, null, levelWeatherType.ToString()));

        }

        internal static ExtendedBuyableVehicle CreateVanillaExtendedBuyableVehicle(BuyableVehicle vanillaVehicle)
        {
            return (ExtendedBuyableVehicle.Create(vanillaVehicle));
        }

    }
}