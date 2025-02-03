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
        public static string debugLevelsString = string.Empty;
        public static string debugDungeonsString = string.Empty;
        public static ConfigFile configFile;

        internal static void BindConfigs()
        {
            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.VanillaExtendedDungeonFlows)
            {
                ExtendedDungeonConfig newConfig = new ExtendedDungeonConfig(configFile, "Vanilla Dungeon:  " + extendedDungeonFlow.DungeonName.StripSpecialCharacters() + " (" + extendedDungeonFlow.DungeonFlow.name + ")", 7);
                newConfig.BindConfigs(extendedDungeonFlow);
            }

            foreach (ExtendedDungeonFlow extendedDungeonFlow in PatchedContent.CustomExtendedDungeonFlows)
            {
                ExtendedDungeonConfig newConfig = new ExtendedDungeonConfig(configFile, "Custom Dungeon:  " + extendedDungeonFlow.DungeonName.StripSpecialCharacters(), 9);
                newConfig.BindConfigs(extendedDungeonFlow);
                if (extendedDungeonFlow.dynamicLevelTagsList.Count > 0 || extendedDungeonFlow.dynamicRoutePricesList.Count > 0 || extendedDungeonFlow.dynamicCurrentWeatherList.Count > 0 || extendedDungeonFlow.manualPlanetNameReferenceList.Count > 0 || extendedDungeonFlow.manualContentSourceNameReferenceList.Count > 0)
                {
                    DebugHelper.LogWarning("ExtendedDungeonFlow: " + extendedDungeonFlow.name + ": ExtendedDungeonFlow dynamic and manual match reference lists are Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.LevelMatchingProperties instead.", DebugType.Developer);
                    extendedDungeonFlow.LevelMatchingProperties.ApplyValues(newAuthorNames: extendedDungeonFlow.manualContentSourceNameReferenceList, newPlanetNames: extendedDungeonFlow.manualPlanetNameReferenceList, newLevelTags: extendedDungeonFlow.dynamicLevelTagsList, newRoutePrices: extendedDungeonFlow.dynamicRoutePricesList, newCurrentWeathers: extendedDungeonFlow.dynamicCurrentWeatherList);
                }
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.VanillaExtendedLevels)
            {
                ExtendedLevelConfig newConfig = new ExtendedLevelConfig(configFile, "Vanilla Level:  " + extendedLevel.SelectableLevel.PlanetName.StripSpecialCharacters(), 6);
                newConfig.BindConfigs(extendedLevel);
            }

            foreach (ExtendedLevel extendedLevel in PatchedContent.CustomExtendedLevels)
            {
                ExtendedLevelConfig newConfig = new ExtendedLevelConfig(configFile, "Custom Level:  " + extendedLevel.SelectableLevel.PlanetName.StripSpecialCharacters(), 8);
                newConfig.BindConfigs(extendedLevel);
            }

            if (debugLevelsString.Contains(", ") && debugLevelsString.LastIndexOf(", ") == (debugLevelsString.Length - 2))
                debugLevelsString = debugLevelsString.Remove(debugLevelsString.LastIndexOf(", "), 2);

            if (debugDungeonsString.Contains(", ") && debugDungeonsString.LastIndexOf(", ") == (debugDungeonsString.Length - 2))
                debugDungeonsString = debugDungeonsString.Remove(debugDungeonsString.LastIndexOf(", "), 2);

            debugLevelsString = string.Empty;
            debugDungeonsString = string.Empty;
        }

        internal static void BindGeneralConfigs()
        {
            configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "LethalLevelLoader.cfg"), false);

            GeneralSettingsConfig newGeneralSettingsConfig = new GeneralSettingsConfig(configFile, " - LethalLevelLoader Settings -", 5);
            newGeneralSettingsConfig.BindConfigs();
            DebugHelper.Log("Config Level Set As: " + Settings.debugType.ToString(), DebugType.User);
        }

        internal static string GetConfigCategory(string categoryName, string contentName)
        {
            return (categoryName + contentName);
        }
    }

    public class GeneralSettingsConfig : ConfigTemplate
    {
        private ConfigEntry<PreviewInfoType> previewInfoTypeToggle;
        private ConfigEntry<SortInfoType> sortInfoTypeToggle;
        private ConfigEntry<FilterInfoType> filterInfoTypeToggle;
        private ConfigEntry<SimulateInfoType> simulateInfoTypeToggle;
        private ConfigEntry<DebugType> debugTypeToggle;

        private ConfigEntry<int> moonsCatalogueSplitCount;
        private ConfigEntry<bool> requireMatchesOnAllDungeonFlows;

        public GeneralSettingsConfig(ConfigFile newConfigFile, string newCategory, int newSortingPriority) : base(newConfigFile, newCategory, newSortingPriority) { }

        public void BindConfigs()
        {

            debugTypeToggle = BindValue("LethalLevelLoader Debugging Mode", "Controls what type of debug logs you recieve, If you use mods, Keep this set to User, If you create content with LethalLeveLoader, set this to Developer", DebugType.User);
            previewInfoTypeToggle = BindValue("Terminal >Moons PreviewInfo Default", "What LethalLevelLoader displays next to each moon in the >moons Terminal listing.", PreviewInfoType.Weather);
            sortInfoTypeToggle = BindValue("Terminal >Moons SortInfo Default", "How LethalLevelLoader sorts each moon in the >moons Terminal listing.", SortInfoType.None);
            filterInfoTypeToggle = BindValue("Terminal >Moons FilterInfo Default", "How LethalLevelLoader filters each moon in the >moons Terminal listing.", FilterInfoType.None);
            simulateInfoTypeToggle = BindValue("Terminal >Simulate Results Type Default", "The format used to display odds using the >simulate Terminal keyword.", SimulateInfoType.Percentage);

            moonsCatalogueSplitCount = BindValue("Moons Catalogue Group Split Count", "The amount of moons that will be in each automatically generated group.", 3);

            requireMatchesOnAllDungeonFlows = BindValue("Require Matches On All Possible DungeonFlows", "By default any Dungeons requested by the loading level will skip the matching process and be in the possible selection pool, Set this to false to disable this feature", true);

            Settings.debugType = debugTypeToggle.Value;
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

        public ExtendedDungeonConfig(ConfigFile newConfigFile, string newCategory, int sortingPriority) : base(newConfigFile, newCategory, sortingPriority) { }

        public void BindConfigs(ExtendedDungeonFlow extendedDungeonFlow)
        {
            if (extendedDungeonFlow.GenerateAutomaticConfigurationOptions == true)
            {
                enableContentConfiguration = BindValue("Enable Content Configuration", "Enable This To Utilise Any Of The Configuration Options Below.", false);

                subCategory = "General Settings - ";
                enableDynamicDungeonSizeRestriction = BindValue("Enable Dynamic Dungeon Size Restriction", "Enable this to allow the following three settings to function.", extendedDungeonFlow.IsDynamicDungeonSizeRestrictionEnabled);
                minimumDungeonSizeMultiplier = BindValue("Minimum Dungeon Size Multiplier", "If The Level's Dungeon Size Multiplier Is Below This Value, The Size Multiplier Will Be Restricted Based On The RestrictDungeonSizeScaler Setting", extendedDungeonFlow.DynamicDungeonSizeMinMax.x);
                maximumDungeonSizeMultiplier = BindValue("Maximum Dungeon Size Multiplier", "If The Level's Dungeon Size Multiplier Is Above This Value, The Size Multiplier Will Be Restricted Based On The RestrictDungeonSizeScaler Setting", extendedDungeonFlow.DynamicDungeonSizeMinMax.y);

                string description = "If The Level's Dungeon Size Multiplier Is Above Or Below The Previous Two Settings, The Dungeon Size Multiplier Will Be Set To The Value Between The Level's Dungeon Size Multiplier And This Value." + "\n";
                description += "Example #1: If Set To 0, The Dungeon Size Will Not Be Higher Than Maximum Dungeon Size Multiplier." + "\n";
                description += "Example #2: If Set To 0.5, The Dungeon Size Will Be Between The Maxiumum Dungeon Size Multiplier And The Level's Dungeon Size Multiplier." + "\n";
                description += "Example #3: If Set To 1, The Dungeon Size Will Be The Level's Dungeon Size Multiplier With No Changes Applied." + "\n";
                description += "(Minimum, 0, Maximum: 1)";
                restrictDungeonSizeScaler = BindValue("Restrict Dungeon Size Scaler", description, extendedDungeonFlow.DynamicDungeonSizeLerpRate);

                // ----- Getting -----
                subCategory = "Dungeon Injection Settings - ";
                manualModNames = BindValue("Manual Mod Names List", "Add this Dungeon to any Level's randomisaton pool in a specific mod based on matching Mod Names. (Minimum: 0, Maximum: 9999)", ConfigHelper.StringWithRaritiesToString(extendedDungeonFlow.LevelMatchingProperties.modNames));
                manualLevelNames = BindValue("Manual Level Names List", "Add this Dungeon to a Level's randomisaton pool based on matching Level Names. (Minimum: 0, Maximum: 9999)", ConfigHelper.StringWithRaritiesToString(extendedDungeonFlow.LevelMatchingProperties.planetNames));

                dynamicLevelTags = BindValue("Dynamic Level Tags List", "Add this Dungeon to a Level's randomisaton pool based on matching Level Tags. (Minimum: 0, Maximum: 9999)", ConfigHelper.StringWithRaritiesToString(extendedDungeonFlow.LevelMatchingProperties.levelTags));
                dynamicRoutePrices = BindValue("Dynamic Route Price List", "Add this Dungeon to a Level's randomisaton pool based on matching Route Prices. (Minimum: 0, Maximum: 9999)", ConfigHelper.Vector2WithRaritiesToString(extendedDungeonFlow.LevelMatchingProperties.currentRoutePrice));

                if (enableContentConfiguration.Value == true)
                {
                    // ----- Setting -----

                    DebugHelper.Log(extendedDungeonFlow.DungeonName + " enabled content configeration", DebugType.Developer);

                    extendedDungeonFlow.IsDynamicDungeonSizeRestrictionEnabled = enableContentConfiguration.Value;

                    extendedDungeonFlow.DynamicDungeonSizeMinMax = new Vector2(minimumDungeonSizeMultiplier.Value, maximumDungeonSizeMultiplier.Value);
                    extendedDungeonFlow.DynamicDungeonSizeLerpRate = restrictDungeonSizeScaler.Value;

                    extendedDungeonFlow.LevelMatchingProperties.modNames = ConfigHelper.ConvertToStringWithRarityList(manualModNames.Value, new Vector2(0, 9999));
                    extendedDungeonFlow.LevelMatchingProperties.planetNames = ConfigHelper.ConvertToStringWithRarityList(manualLevelNames.Value, new Vector2(0, 9999));

                    extendedDungeonFlow.LevelMatchingProperties.currentRoutePrice = ConfigHelper.ConvertToVector2WithRarityList(dynamicRoutePrices.Value, new Vector2(0, 9999));
                    extendedDungeonFlow.LevelMatchingProperties.levelTags = ConfigHelper.ConvertToStringWithRarityList(dynamicLevelTags.Value, new Vector2(0, 9999));

                    foreach (StringWithRarity stringWithRarity in ConfigHelper.ConvertToStringWithRarityList(dynamicLevelTags.Value, new Vector2(0, 9999)))
                        DebugHelper.Log(stringWithRarity.Name + " | " + stringWithRarity.Rarity, DebugType.Developer);

                    if (extendedDungeonFlow.ContentType == ContentType.Vanilla)
                        ConfigLoader.debugDungeonsString += extendedDungeonFlow.DungeonName +  "(" + extendedDungeonFlow.DungeonFlow.name + ")" + ", ";
                    else if (extendedDungeonFlow.ContentType == ContentType.Custom)
                        ConfigLoader.debugDungeonsString += extendedDungeonFlow.DungeonName + ", ";
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

        public ExtendedLevelConfig(ConfigFile newConfigFile, string newCategory, int sortingPriority) : base(newConfigFile, newCategory, sortingPriority) { }

        public void BindConfigs(ExtendedLevel extendedLevel)
        {
            SelectableLevel selectableLevel = extendedLevel.SelectableLevel;
            if (extendedLevel.GenerateAutomaticConfigurationOptions == true)
            {
                // ----- Getting ----- //

                enableContentConfiguration = BindValue("Enable Content Configuration", "Enable This To Utilise Any Of The Configuration Options Below.", false);

                subCategory = "General Settings - ";

                routePrice = BindValue("Planet Route Price", "Override The Route Price For This Level.", extendedLevel.RoutePrice);
                daySpeedMultiplier = BindValue("Day Speed Multiplier", "Override The Day Speed Multiplier For This Level.", selectableLevel.DaySpeedMultiplier);
                doesPlanetHaveTime = BindValue("Does Planet Have Time", "Override If Time Passes In This Level.", selectableLevel.planetHasTime);

                isLevelHidden = BindValue("Is Level Hidden In Terminal", "Override If The Level Is Listed In The Moons Catalogue", extendedLevel.IsRouteHidden);
                isLevelRegistered = BindValue("Is Level Registered In Terminal", "Override If The Level Is Registered In The Terminal. Use This To Disable Specific Levels (Only Works For Custom Levels)", true);

                subCategory = "Scrap Settings - ";

                minScrapItemSpawns = BindValue("Minimum Scrap Item Spawns", "Override How Many Item's Will Spawn In This Level.", selectableLevel.minScrap);
                maxScrapItemSpawns = BindValue("Maximum Scrap Item Spawns", "Override How Many Item's Can Spawn In This Level.", selectableLevel.maxScrap);
                minTotalScrapValue = BindValue("Minimum Total Scrap Value", "Override How Much Total Value The Spawned Scrap Will Amount To In This Level.", selectableLevel.minTotalScrapValue);
                maxTotalScrapValue = BindValue("Maximum Total Scrap Value", "Override How Much Total Value The Spawned Scrap Could Amount To In This Level.", selectableLevel.maxTotalScrapValue);
                scrapOverrides = BindValue("Scrap Spawning List", "Add To Or Override The Spawnable Scrap Pool. (Minimum: 0, Maximum: 100)", ConfigHelper.SpawnableItemsWithRaritiesToString(selectableLevel.spawnableScrap));

                subCategory = "Enemy Settings - ";

                maxInsideEnemyPowerCount = BindValue("Maximum Inside Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Inside The Dungeon.", selectableLevel.maxEnemyPowerCount);
                maxOutsideDaytimeEnemyPowerCount = BindValue("Maximum Outside, Daytime Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Outside During The Day.", selectableLevel.maxDaytimeEnemyPowerCount);
                maxOutsideNighttimeEnemyPowerCount = BindValue("Maximum Outside, Nighttime Enemy Power Count", "Override The Maximum Power Used To Spawn Enemies Outside During The Night.", selectableLevel.maxOutsideEnemyPowerCount);

                insideEnemiesOverrides = BindValue("Inside Enemies Spawning List", "Add To Or Override The Inside Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", ConfigHelper.SpawnableEnemiesWithRaritiesToString(selectableLevel.Enemies));
                outsideDaytimeEnemiesOverrides = BindValue("Outside Daytime Enemies Spawning List", "Add To Or Override The Outside, Daytime Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", ConfigHelper.SpawnableEnemiesWithRaritiesToString(selectableLevel.DaytimeEnemies));
                outsideNighttimeEnemiesOverrides = BindValue("Outside Nighttime Enemies Spawning List", "Add To Or Override The Outside, Nighttime Enemy Spawn Pool. (Minimum: 0, Maximum: 100)", ConfigHelper.SpawnableEnemiesWithRaritiesToString(selectableLevel.OutsideEnemies));

                if (enableContentConfiguration.Value == true)
                {
                    // ----- Setting ----- //

                    //General
                    extendedLevel.RoutePrice = routePrice.Value;
                    selectableLevel.DaySpeedMultiplier = daySpeedMultiplier.Value;
                    selectableLevel.planetHasTime = doesPlanetHaveTime.Value;
                    extendedLevel.IsRouteHidden = isLevelHidden.Value;
                    if (isLevelRegistered.Value == false)
                        foreach (CompatibleNoun compatibleNoun in new List<CompatibleNoun>(TerminalManager.routeKeyword.compatibleNouns))
                            if (compatibleNoun.result == extendedLevel.RouteNode)
                            {
                                List<CompatibleNoun> modifiedNounsList = new List<CompatibleNoun>(TerminalManager.routeKeyword.compatibleNouns);
                                modifiedNounsList.Remove(compatibleNoun);
                                TerminalManager.routeKeyword.compatibleNouns = modifiedNounsList.ToArray();
                                extendedLevel.IsRouteRemoved = true;
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
    }

    public class ConfigTemplate
    {
        public ConfigFile configFile;
        public string subCategory = string.Empty;
        public int sortingPriority = 0;

        private string _category = string.Empty;
        public string Category
        {
            get { return (GetSortingSpaces() + _category); }
            set { _category = value; }
        }

        public ConfigTemplate(ConfigFile newConfigFile, string newCategory, int newSortingPriority)
        {
            configFile = newConfigFile;
            Category = newCategory;
            sortingPriority = newSortingPriority;
        }

        public ConfigEntry<T> BindValue<T>(string configTitle, string configDescription, T genericValue)
        {
            return (configFile.Bind(Category, subCategory + configTitle, genericValue, configDescription));
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