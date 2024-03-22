using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LethalLevelLoader.Tools
{
    internal static class ConfigLoader
    {
        public const string spacer = "-------------------------";

        public static string debugLevelsString = string.Empty;
        public static string debugDungeonsString = string.Empty;

        internal static void BindConfigs()
        {
            ConfigFile newConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "LethalLevelLoader.cfg"), false);

            GeneralSettingsConfig newGeneralSettingsConfig = new GeneralSettingsConfig(newConfigFile, "LethalLevelLoader Settings", 5);
            newGeneralSettingsConfig.BindConfigs();

            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
            {
                ExtendedDungeonConfig newConfig = new ExtendedDungeonConfig(newConfigFile, "Vanilla Dungeon: " + (PatchedContent.VanillaExtendedDungeonFlows.IndexOf(extendedDungeonFlow) + 1).ToString() + ". - " + extendedDungeonFlow.dungeonDisplayName.StripSpecialCharacters() + " (" + extendedDungeonFlow.dungeonFlow.name + ")", 7);
                newConfig.BindConfigs(extendedDungeonFlow);
            }

            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                ExtendedDungeonConfig newConfig = new ExtendedDungeonConfig(newConfigFile, "Custom Dungeon: " + (PatchedContent.CustomExtendedDungeonFlows.IndexOf(extendedDungeonFlow) + 1).ToString() + ". - " + extendedDungeonFlow.dungeonDisplayName.StripSpecialCharacters(), 9);
                newConfig.BindConfigs(extendedDungeonFlow);
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaExtendedLevels)
            {
                ExtendedLevelConfig newConfig = new ExtendedLevelConfig(newConfigFile, "Vanilla Level: " + (PatchedContent.VanillaExtendedLevels.IndexOf(extendedLevel) + 1).ToString() + ". - " + extendedLevel.selectableLevel.PlanetName.StripSpecialCharacters(), 6);
                newConfig.BindConfigs(extendedLevel);
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.CustomExtendedLevels)
            {
                ExtendedLevelConfig newConfig = new ExtendedLevelConfig(newConfigFile, "Custom Level: " + (PatchedContent.CustomExtendedLevels.IndexOf(extendedLevel) + 1).ToString() + ". - " + extendedLevel.selectableLevel.PlanetName.StripSpecialCharacters(), 8);
                newConfig.BindConfigs(extendedLevel);
            }

            if (debugLevelsString.Contains(", ") && debugLevelsString.LastIndexOf(", ") == (debugLevelsString.Length - 2))
                debugLevelsString = debugLevelsString.Remove(debugLevelsString.LastIndexOf(", "), 2);

            if (debugDungeonsString.Contains(", ") && debugDungeonsString.LastIndexOf(", ") == (debugDungeonsString.Length - 2))
                debugDungeonsString = debugDungeonsString.Remove(debugDungeonsString.LastIndexOf(", "), 2);

            debugLevelsString = string.Empty;
            debugDungeonsString = string.Empty;
        }

        internal static string GetConfigCatagory(string catagoryName, string contentName)
        {
            return (spacer + " " + catagoryName + contentName + " " + spacer);
        }
    }

    public class GeneralSettingsConfig : ConfigTemplate
    {
        private ConfigEntry<PreviewInfoType> previewInfoTypeToggle;
        private ConfigEntry<SortInfoType> sortInfoTypeToggle;
        private ConfigEntry<FilterInfoType> filterInfoTypeToggle;
        private ConfigEntry<SimulateInfoType> simulateInfoTypeToggle;

        private ConfigEntry<int> moonsCatalogueSplitCount;
        private ConfigEntry<bool> requireMatchesOnAllDungeonFlows;

        public GeneralSettingsConfig(ConfigFile newConfigFile, string newCatagory, int newSortingPriority) : base(newConfigFile, newCatagory, newSortingPriority) { }

        public void BindConfigs()
        {
            previewInfoTypeToggle = BindValue("Terminal >Moons PreviewInfo Default", "What LethalLevelLoader displays next to each moon in the >moons Terminal listing.", PreviewInfoType.Weather);
            sortInfoTypeToggle = BindValue("Terminal >Moons SortInfo Default", "How LethalLevelLoader sorts each moon in the >moons Terminal listing.", SortInfoType.None);
            filterInfoTypeToggle = BindValue("Terminal >Moons FilterInfo Default", "How LethalLevelLoader filters each moon in the >moons Terminal listing.", FilterInfoType.None);
            simulateInfoTypeToggle = BindValue("Terminal >Simulate Results Type Default", "The format used to display odds using the >simulate Terminal keyword.", SimulateInfoType.Percentage);

            moonsCatalogueSplitCount = BindValue("Moons Catalogue Group Split Count", "The amount of moons that will be in each automatically generated group.", 3);

            requireMatchesOnAllDungeonFlows = BindValue("Require Matches On All Possible DungeonFlows", "By default any Dungeons requested by the loading level will skip the matching process and be in the possible selection pool, Set this to false to disable this feature", true);

            Settings.levelPreviewInfoType = previewInfoTypeToggle.Value;
            Settings.levelPreviewSortType = sortInfoTypeToggle.Value;
            Settings.levelPreviewFilterType = filterInfoTypeToggle.Value;
            Settings.levelSimulateInfoType = simulateInfoTypeToggle.Value;

            if (moonsCatalogueSplitCount.Value > 0)
                Settings.moonsCatalogueSplitCount = moonsCatalogueSplitCount.Value;

            Settings.allDungeonFlowsRequireMatching = requireMatchesOnAllDungeonFlows.Value;
        }
    }

    public class ExtendedDungeonConfig : ConfigTemplate
    {
        public ConfigEntry<bool> enableContentConfiguration;

        public ConfigEntry<string> manualLevelNames;
        public ConfigEntry<string> manualModNames;

        public ConfigEntry<bool> enableDynamicDungeonSizeRestriction;
        public ConfigEntry<float> minimumDungeonSizeMultiplier;
        public ConfigEntry<float> maximumDungeonSizeMultiplier;
        public ConfigEntry<float> restrictDungeonSizeScaler;

        public ConfigEntry<string> dynamicLevelTags;
        public ConfigEntry<string> dynamicRoutePrices;

        public ConfigEntry<bool> disabledWarning;

        public ExtendedDungeonConfig(ConfigFile newConfigFile, string newCatagory, int sortingPriority) : base(newConfigFile, newCatagory, sortingPriority) { }

        public void BindConfigs(ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (extendedDungeonFlow.generateAutomaticConfigurationOptions == true)
            {
                enableContentConfiguration = BindValue("Enable Content Configuration", "Enable This To Utilise Any Of The Configuration Options Below.", false);

                subCatagory = "General Settings - ";
                enableDynamicDungeonSizeRestriction = BindValue("Enable Dynamic Dungeon Size Restriction", "Enable this to allow the following three settings to function.", extendedDungeonFlow.enableDynamicDungeonSizeRestriction);
                minimumDungeonSizeMultiplier = BindValue("Minimum Dungeon Size Multiplier", "If The Level's Dungeon Size Multiplier Is Below This Value, The Size Multiplier Will Be Restricted Based On The RestrictDungeonSizeScaler Setting", extendedDungeonFlow.dungeonSizeMin);
                maximumDungeonSizeMultiplier = BindValue("Maximum Dungeon Size Multiplier", "If The Level's Dungeon Size Multiplier Is Above This Value, The Size Multiplier Will Be Restricted Based On The RestrictDungeonSizeScaler Setting", extendedDungeonFlow.dungeonSizeMax);

                string description = "If The Level's Dungeon Size Multiplier Is Above Or Below The Previous Two Settings, The Dungeon Size Multiplier Will Be Set To The Value Between The Level's Dungeon Size Multiplier And This Value." + "\n";
                description += "Example #1: If Set To 0, The Dungeon Size Will Not Be Higher Than Maximum Dungeon Size Multiplier." + "\n";
                description += "Example #2: If Set To 0.5, The Dungeon Size Will Be Between The Maxiumum Dungeon Size Multiplier And The Level's Dungeon Size Multiplier." + "\n";
                description += "Example #3: If Set To 1, The Dungeon Size Will Be The Level's Dungeon Size Multiplier With No Changes Applied." + "\n";
                description += "(Minimum, 0, Maximum: 1)";
                restrictDungeonSizeScaler = BindValue("Restrict Dungeon Size Scaler", description, extendedDungeonFlow.dungeonSizeLerpPercentage);

                // ----- Getting -----
                subCatagory = "Dungeon Injection Settings - ";
                manualModNames = BindValue("Manual Mod Names List", "Add this Dungeon to any Level's randomisaton pool in a specific mod based on matching Mod Names. (Minimum: 0, Maximum: 9999)", "Lethal Company:300, MoreMoonsMod: 100");
                manualLevelNames = BindValue("Manual Level Names List", "Add this Dungeon to a Level's randomisaton pool based on matching Level Names. (Minimum: 0, Maximum: 9999)", "Titan:300,vowLevel:40,220. Assurance: 41, Egypt");

                dynamicLevelTags = BindValue("Dynamic Level Tags List", "Add this Dungeon to a Level's randomisaton pool based on matching Level Tags. (Minimum: 0, Maximum: 9999)", "Wasteland:200");
                dynamicRoutePrices = BindValue("Dynamic Route Price List", "Add this Dungeon to a Level's randomisaton pool based on matching Route Prices. (Minimum: 0, Maximum: 9999)", "500-800:30, 20-20:800, 100-100,4");

                if (enableContentConfiguration.Value == true)
                {
                    // ----- Setting -----

                    DebugHelper.Log(extendedDungeonFlow.dungeonDisplayName + " enabled content configeration");

                    extendedDungeonFlow.enableDynamicDungeonSizeRestriction = enableContentConfiguration.Value;

                    extendedDungeonFlow.dungeonSizeMin = minimumDungeonSizeMultiplier.Value;
                    extendedDungeonFlow.dungeonSizeMax = maximumDungeonSizeMultiplier.Value;
                    extendedDungeonFlow.dungeonSizeLerpPercentage = restrictDungeonSizeScaler.Value;

                    extendedDungeonFlow.manualContentSourceNameReferenceList = ConfigHelper.ConvertToStringWithRarityList(manualModNames.Value, new Vector2(0, 9999));
                    extendedDungeonFlow.manualPlanetNameReferenceList = ConfigHelper.ConvertToStringWithRarityList(manualLevelNames.Value, new Vector2(0, 9999));

                    extendedDungeonFlow.dynamicRoutePricesList = ConfigHelper.ConvertToVector2WithRarityList(dynamicRoutePrices.Value, new Vector2(0, 9999));
                    extendedDungeonFlow.dynamicLevelTagsList = ConfigHelper.ConvertToStringWithRarityList(dynamicLevelTags.Value, new Vector2(0, 9999));

                    foreach (StringWithRarity stringWithRarity in ConfigHelper.ConvertToStringWithRarityList(dynamicLevelTags.Value, new Vector2(0, 9999)))
                        DebugHelper.Log(stringWithRarity.Name + " | " + stringWithRarity.Rarity);

                    if (extendedDungeonFlow.dungeonType == ContentType.Vanilla)
                        ConfigLoader.debugDungeonsString += extendedDungeonFlow.dungeonDisplayName +  "(" + extendedDungeonFlow.dungeonFlow.name + ")" + ", ";
                    else if (extendedDungeonFlow.dungeonType == ContentType.Custom)
                        ConfigLoader.debugDungeonsString += extendedDungeonFlow.dungeonDisplayName + ", ";
                }
            }
            else
            {
                string description = "The author of this content has chosen not to allow for LethalLevelLoader to generate a custom configuration template for them." + "\n";
                description += "This is likely due to said content author providing alternative configuration options in their own Config.";
                enableContentConfiguration = BindValue("Content Author Disabled Automatic Configuration File Warning", description, false);
            }
        }
    }

    public class ExtendedLevelConfig : ConfigTemplate
    {
        //General
        public ConfigEntry<bool> enableContentConfiguration;

        public ConfigEntry<int> routePrice;
        public ConfigEntry<float> daySpeedMultiplier;
        public ConfigEntry<bool> doesPlanetHaveTime;
        public ConfigEntry<bool> isLevelHidden;
        public ConfigEntry<bool> isLevelRegistered;

        //Scrap
        public ConfigEntry<int> minScrapItemSpawns;
        public ConfigEntry<int> maxScrapItemSpawns;

        public ConfigEntry<int> minTotalScrapValue;
        public ConfigEntry<int> maxTotalScrapValue;

        public ConfigEntry<string> scrapOverrides;

        //Enemies
        public ConfigEntry<int> maxInsideEnemyPowerCount;
        public ConfigEntry<int> maxOutsideDaytimeEnemyPowerCount;
        public ConfigEntry<int> maxOutsideNighttimeEnemyPowerCount;

        public ConfigEntry<string> insideEnemiesOverrides;
        public ConfigEntry<string> outsideDaytimeEnemiesOverrides;
        public ConfigEntry<string> outsideNighttimeEnemiesOverrides;

        public ConfigEntry<bool> disabledWarning;

        public ExtendedLevelConfig(ConfigFile newConfigFile, string newCatagory, int sortingPriority) : base(newConfigFile, newCatagory, sortingPriority) { }

        public void BindConfigs(ExtendedLevel extendedLevel)
        {
            SelectableLevel selectableLevel = extendedLevel.selectableLevel;
            if (extendedLevel.generateAutomaticConfigurationOptions == true)
            {
                // ----- Getting ----- //

                enableContentConfiguration = BindValue("Enable Content Configuration", "Enable This To Utilise Any Of The Configuration Options Below.", false);

                subCatagory = "General Settings - ";

                routePrice = BindValue("Planet Route Price", "Override The Route Price For This Level.", extendedLevel.RoutePrice);
                daySpeedMultiplier = BindValue("Day Speed Multiplier", "Override The Day Speed Multiplier For This Level.", selectableLevel.DaySpeedMultiplier);
                doesPlanetHaveTime = BindValue("Does Planet Have Time", "Override If Time Passes In This Level.", selectableLevel.planetHasTime);

                isLevelHidden = BindValue("Is Level Hidden In Terminal", "Override If The Level Is Listed In The Moons Catalogue", extendedLevel.isHidden);
                isLevelRegistered = BindValue("Is Level Registered In Terminal", "Override If The Level Is Registered In The Terminal. Use This To Disable Specific Levels (Only Works For Custom Levels)", true);

                subCatagory = "Scrap Settings - ";

                minScrapItemSpawns = BindValue("Minimum Scrap Item Spawns", "Override How Many Item's Will Spawn In This Level.", selectableLevel.minScrap);
                maxScrapItemSpawns = BindValue("Maximum Scrap Item Spawns", "Override How Many Item's Can Spawn In This Level.", selectableLevel.maxScrap);
                minTotalScrapValue = BindValue("Minimum Total Scrap Value", "Override How Much Total Value The Spawned Scrap Will Amount To In This Level.", selectableLevel.minTotalScrapValue);
                maxTotalScrapValue = BindValue("Maximum Total Scrap Value", "Override How Much Total Value The Spawned Scrap Could Amount To In This Level.", selectableLevel.maxTotalScrapValue);
                scrapOverrides = BindValue("Scrap Spawning List", "Add To Or Override The Spawnable Scrap Pool. (Minimum: 0, Maximum: 100)", SpawnableItemsWithRaritiesToString(selectableLevel.spawnableScrap));

                subCatagory = "Enemy Settings - ";

                maxInsideEnemyPowerCount = BindValue("Maximum Inside Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Inside The Dungeon.", selectableLevel.maxEnemyPowerCount);
                maxOutsideDaytimeEnemyPowerCount = BindValue("Maximum Outside, Daytime Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Outside During The Day.", selectableLevel.maxDaytimeEnemyPowerCount);
                maxOutsideNighttimeEnemyPowerCount = BindValue("Maximum Outside, Nighttime Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Outside During The Night.", selectableLevel.maxOutsideEnemyPowerCount);

                insideEnemiesOverrides = BindValue("Inside Enemies Spawning List", "Add To Or Override The Inside Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", SpawnableEnemiesWithRaritiesToString(selectableLevel.Enemies));
                outsideDaytimeEnemiesOverrides = BindValue("Outside Daytime Enemies Spawning List", "Add To Or Override The Outside, Daytime Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", SpawnableEnemiesWithRaritiesToString(selectableLevel.DaytimeEnemies));
                outsideNighttimeEnemiesOverrides = BindValue("Outside Nighttime Enemies Spawning List", "Add To Or Override The Outside, Nighttime Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", SpawnableEnemiesWithRaritiesToString(selectableLevel.OutsideEnemies));

                if (enableContentConfiguration.Value == true)
                {
                    // ----- Setting ----- //

                    //General
                    extendedLevel.RoutePrice = routePrice.Value;
                    selectableLevel.DaySpeedMultiplier = daySpeedMultiplier.Value;
                    selectableLevel.planetHasTime = doesPlanetHaveTime.Value;
                    extendedLevel.isHidden = isLevelHidden.Value;
                    if (isLevelRegistered.Value == false)
                        foreach (CompatibleNoun compatibleNoun in new List<CompatibleNoun>(TerminalManager.routeKeyword.compatibleNouns))
                            if (compatibleNoun.result == extendedLevel.RouteNode)
                            {
                                List<CompatibleNoun> modifiedNounsList = new List<CompatibleNoun>(TerminalManager.routeKeyword.compatibleNouns);
                                modifiedNounsList.Remove(compatibleNoun);
                                TerminalManager.routeKeyword.compatibleNouns = modifiedNounsList.ToArray();
                            }

                    //Scrap
                    selectableLevel.minScrap = minScrapItemSpawns.Value;
                    selectableLevel.maxScrap = maxScrapItemSpawns.Value;

                    selectableLevel.minTotalScrapValue = minTotalScrapValue.Value;
                    selectableLevel.maxTotalScrapValue = maxTotalScrapValue.Value;

                    selectableLevel.spawnableScrap = ConfigHelper.ConvertToSpawnableItemWithRarityList(scrapOverrides.Value, new Vector2(0, 100));

                    //Enemies
                    selectableLevel.maxEnemyPowerCount = maxInsideEnemyPowerCount.Value;
                    selectableLevel.maxDaytimeEnemyPowerCount = maxOutsideDaytimeEnemyPowerCount.Value;
                    selectableLevel.maxOutsideEnemyPowerCount = maxOutsideNighttimeEnemyPowerCount.Value;

                    selectableLevel.Enemies = ConfigHelper.ConvertToSpawnableEnemyWithRarityList(insideEnemiesOverrides.Value, new Vector2(0, 100));
                    selectableLevel.DaytimeEnemies = ConfigHelper.ConvertToSpawnableEnemyWithRarityList(outsideDaytimeEnemiesOverrides.Value, new Vector2(0, 100));
                    selectableLevel.OutsideEnemies = ConfigHelper.ConvertToSpawnableEnemyWithRarityList(outsideNighttimeEnemiesOverrides.Value, new Vector2(0, 100));

                    ConfigLoader.debugLevelsString += selectableLevel.PlanetName + ", ";
                }
            }
            else
            {
                string description = "The author of this content has chosen not to allow for LethalLevelLoader to generate a custom configuration template for them." + "\n";
                description += "This is likely due to said content author providing alternative configuration options in their own Config.";
                enableContentConfiguration = BindValue("Content Author Disabled Automatic Configuration File Warning", description, false);
            }
        }

        public string SpawnableEnemiesWithRaritiesToString(List<SpawnableEnemyWithRarity> spawnableEnemiesList)
        {
            string returnString = string.Empty;

            foreach (SpawnableEnemyWithRarity spawnableEnemyWithRarity in spawnableEnemiesList)
                returnString += spawnableEnemyWithRarity.enemyType.enemyName + ConfigHelper.keyPairSeperator + spawnableEnemyWithRarity.rarity.ToString() + ConfigHelper.indexSeperator;
            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";
            return (returnString);
        }

        public string SpawnableItemsWithRaritiesToString(List<SpawnableItemWithRarity> spawnableItemsList)
        {
            string returnString = string.Empty;

            foreach (SpawnableItemWithRarity spawnableItemWithRarity in spawnableItemsList)
                returnString += spawnableItemWithRarity.spawnableItem.itemName + ConfigHelper.keyPairSeperator + spawnableItemWithRarity.rarity.ToString() + ConfigHelper.indexSeperator;
            if (returnString.Contains(",") && returnString.LastIndexOf(",") == (returnString.Length - 1))
                returnString = returnString.Remove(returnString.LastIndexOf(","), 1);

            if (returnString == string.Empty)
                returnString = "Default Values Were Empty";
            return (returnString);
        }
    }

    public class ConfigTemplate
    {
        public ConfigFile configFile;
        public string subCatagory = string.Empty;
        public int sortingPriority = 0;

        private string _catagory = string.Empty;
        public string Catagory
        {
            get { return (GetSortingSpaces() + ConfigLoader.spacer + " " + _catagory + " " + ConfigLoader.spacer); }
            set { _catagory = value; }
        }

        public ConfigTemplate(ConfigFile newConfigFile, string newCatagory, int newSortingPriority)
        {
            configFile = newConfigFile;
            Catagory = newCatagory;
            sortingPriority = newSortingPriority;
        }

        public ConfigEntry<T> BindValue<T>(string configTitle, string configDescription, T genericValue)
        {
            return (configFile.Bind(Catagory, subCatagory + configTitle, genericValue, configDescription));
        }

        public string GetSortingSpaces()
        {
            string returnString = string.Empty;
            for (int i = 0; i < sortingPriority; i++)
                returnString += "​"; //Zero Width Space In Here, Do Not Let It Escape!
            return returnString;
        }
    }
}
