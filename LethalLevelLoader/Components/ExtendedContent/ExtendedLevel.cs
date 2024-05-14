using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScriptableObject = UnityEngine.ScriptableObject;

public enum ContentType { Vanilla, Custom, Any } //Any & All included for built in checks.

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedLevel", menuName = "Lethal Level Loader/Extended Content/ExtendedLevel", order = 20)]
    public class ExtendedLevel : ExtendedContent
    {
        [field: Header("General Settings")]
        [field: SerializeField] public SelectableLevel SelectableLevel { get; set; }
        [Space(5)] [SerializeField] private int routePrice = 0;

        [field: Header("Extended Feature Settings")]
        [field: SerializeField] public bool OverrideDynamicRiskLevelAssignment { get; set; } = false;

        [field: Space(5)]

        [field: SerializeField] public GameObject OverrideQuicksandPrefab { get; set; }

        [field: Space(5)]

        [field: SerializeField] public bool IsRouteHidden { get; set; } = false;
        [field: SerializeField] public bool IsRouteLocked { get; set; } = false;
        [field: SerializeField] public string LockedRouteNodeText { get; set; } = string.Empty;

        [field: Space(5)]

        [field: SerializeField] public AnimationClip ShipFlyToMoonClip { get; set; }
        [field: SerializeField] public AnimationClip ShipFlyFromMoonClip { get; set; }

        [field: Space(5)]

        [field: SerializeField] public List<StringWithRarity> SceneSelections { get; set; } = new List<StringWithRarity>();

        [field: Space(5)]
        [field: Header("Terminal Route Override Settings")]

        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideInfoNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideRouteNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideRouteConfirmNodeDescription { get; set; } = string.Empty;

        [field: Space(10)]
        [field: Header("Misc. Settings")]
        [field: Space(5)]
        [field: SerializeField] public bool GenerateAutomaticConfigurationOptions { get; set; } = true;

        [Space(25)]
        [Header("Obsolete (Legacy Fields, Will Be Removed In The Future)")]
        [Obsolete] public SelectableLevel selectableLevel;
        [Obsolete][Space(5)] public string contentSourceName = string.Empty; //Levels from AssetBundles will have this as their Assembly Name.
        [Obsolete][Space(5)] public List<string> levelTags = new List<string>();

        //Runtime Stuff
        public int RoutePrice
        {
            get
            {
                if (RouteNode != null)
                {
                    routePrice = RouteNode.itemCost;
                    RouteConfirmNode.itemCost = routePrice;
                    return (RouteNode.itemCost);
                }
                else
                {
                    DebugHelper.LogWarning("routeNode Is Missing! Using internal value!", DebugType.Developer);
                    return (routePrice);
                }
            }
            set
            {
                if (RouteNode != null && RouteConfirmNode != null)
                {
                    RouteNode.itemCost = value;
                    RouteConfirmNode.itemCost = value;
                }
                else
                    DebugHelper.LogWarning("routeNode Is Missing! Only setting internal value!", DebugType.Developer);
                routePrice = value;
            }
        }

        public string NumberlessPlanetName => GetNumberlessPlanetName(SelectableLevel);
        public int CalculatedDifficultyRating => LevelManager.CalculateExtendedLevelDifficultyRating(this);
        public bool IsCurrentLevel => LevelManager.CurrentExtendedLevel == this;
        public bool IsLevelLoaded => SceneManager.GetSceneByName(SelectableLevel.sceneName).isLoaded;

        [HideInInspector] public LevelEvents LevelEvents { get; internal set; } = new LevelEvents();

        public TerminalNode RouteNode { get; internal set; }
        public TerminalNode RouteConfirmNode { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }

        //Dunno about these yet
        public List<ExtendedWeatherEffect> EnabledExtendedWeatherEffects { get; set; } = new List<ExtendedWeatherEffect>();
        public ExtendedWeatherEffect CurrentExtendedWeatherEffect { get; set; }

        internal static ExtendedLevel Create(SelectableLevel newSelectableLevel)
        {
            ExtendedLevel newExtendedLevel = ScriptableObject.CreateInstance<ExtendedLevel>();
            newExtendedLevel.SelectableLevel = newSelectableLevel;

            return (newExtendedLevel);
        }
        internal void Initialize(string newContentSourceName, bool generateTerminalAssets)
        {
            bool mainSceneRegistered = false;

            foreach (StringWithRarity sceneSelection in SceneSelections)
                if (sceneSelection.Name == SelectableLevel.sceneName)
                    mainSceneRegistered = true;

            if (mainSceneRegistered == false)
            {
                StringWithRarity newSceneSelection = new StringWithRarity(SelectableLevel.sceneName, 300);
                SceneSelections.Add(newSceneSelection);
            }

            foreach (StringWithRarity sceneSelection in new List<StringWithRarity>(SceneSelections))
                if (!PatchedContent.AllLevelSceneNames.Contains(sceneSelection.Name))
                {
                    DebugHelper.LogWarning("Removing SceneSelection From: " + SelectableLevel.PlanetName + " As SceneName: " + sceneSelection.Name + " Is Not Loaded!", DebugType.Developer);
                    SceneSelections.Remove(sceneSelection);
                }

            if (ShipFlyToMoonClip == null)
                ShipFlyToMoonClip = LevelLoader.defaultShipFlyToMoonClip;
            if (ShipFlyFromMoonClip == null)
                ShipFlyFromMoonClip = LevelLoader.defaultShipFlyFromMoonClip;

            if (OverrideQuicksandPrefab == null)
                OverrideQuicksandPrefab = LevelLoader.defaultQuicksandPrefab;

            if (ContentType == ContentType.Custom)
            {
                name = NumberlessPlanetName.StripSpecialCharacters() + "ExtendedLevel";
                SelectableLevel.name = NumberlessPlanetName.StripSpecialCharacters() + "Level";
                if (generateTerminalAssets == true) //Needs to be after levelID setting above.
                {
                    //DebugHelper.Log("Generating Terminal Assets For: " + NumberlessPlanetName);
                    TerminalManager.CreateLevelTerminalData(this, routePrice);
                }
            }

            if (ContentType == ContentType.Vanilla)
                GetVanillaInfoNode();
            SetExtendedDungeonFlowMatches();

            //Obsolete
        }

        internal void ConvertObsoleteValues()
        {
            if (levelTags.Count > 0 && ContentTags.Count == 0)
            {
                DebugHelper.LogWarning("ExtendedLevel.levelTags Is Obsolete and will be removed in following releases, Please use .ContentTags instead.", DebugType.Developer);
                foreach (ContentTag convertedContentTag in ContentTagManager.CreateNewContentTags(levelTags))
                    ContentTags.Add(convertedContentTag);
            }
            levelTags.Clear();

            if (SelectableLevel == null && selectableLevel != null)
            {
                DebugHelper.LogWarning("ExtendedLevel.selectableLevel Is Obsolete and will be removed in following releases, Please use .SelectableLevel instead.", DebugType.Developer);
                SelectableLevel = selectableLevel;
            }

            if (!string.IsNullOrEmpty(contentSourceName))
                DebugHelper.LogWarning("ExtendedLevel.contentSourceName is Obsolete and will be removed in following releases, Please use ExtendedMod.AuthorName instead.", DebugType.Developer);
        }

        internal static string GetNumberlessPlanetName(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null)
                return new string(selectableLevel.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
            else
                return string.Empty;
        }

        internal void SetLevelID()
        {
            if (ContentType == ContentType.Custom)
            {
                SelectableLevel.levelID = PatchedContent.ExtendedLevels.IndexOf(this);
                if (RouteNode != null)
                    RouteNode.displayPlanetInfo = SelectableLevel.levelID;
                if (RouteConfirmNode != null)
                    RouteConfirmNode.buyRerouteToMoon = SelectableLevel.levelID;
            }
        }

        internal void SetExtendedDungeonFlowMatches()
        {
            foreach (IntWithRarity intWithRarity in SelectableLevel.dungeonFlowTypes)
                if (DungeonManager.TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonFlowTypes[intWithRarity.id].dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(NumberlessPlanetName, intWithRarity.rarity));


            if (SelectableLevel.sceneName == "Level4March")
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    if (indoorMapType.dungeonFlow.name == "Level1Flow3Exits")
                        if (DungeonManager.TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out ExtendedDungeonFlow marchDungeonFlow))
                            marchDungeonFlow.LevelMatchingProperties.planetNames.Add(new StringWithRarity(NumberlessPlanetName, 300));
        }

        internal void GetVanillaInfoNode()
        {
            foreach (CompatibleNoun infoNoun in TerminalManager.routeInfoKeyword.compatibleNouns)
                if (infoNoun.noun.word == NumberlessPlanetName.ToLower())
                {
                    InfoNode = infoNoun.result;
                    break;
                }
        }

        public void ForceSetRoutePrice(int newValue)
        {
            if (Plugin.Instance != null)
                Debug.LogWarning("ForceSetRoutePrice Should Only Be Used In Editor! Consider Using RoutePrice Property To Sync TerminalNode's With New Value.");
            routePrice = newValue;
        }
    }
        

    [System.Serializable]
    public class LevelEvents
    {
        public ExtendedEvent onLevelLoaded = new ExtendedEvent();
        public ExtendedEvent onShipLand = new ExtendedEvent();
        public ExtendedEvent onShipLeave = new ExtendedEvent();
        public ExtendedEvent<EnemyAI> onDaytimeEnemySpawn = new ExtendedEvent<EnemyAI>();
        public ExtendedEvent<EnemyAI> onNighttimeEnemySpawn = new ExtendedEvent<EnemyAI>();
        public ExtendedEvent<StoryLog> onStoryLogCollected = new ExtendedEvent<StoryLog>();
        public ExtendedEvent<LungProp> onApparatusTaken = new ExtendedEvent<LungProp>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerEnterDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerExitDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<bool> onPowerSwitchToggle = new ExtendedEvent<bool>();
        public ExtendedEvent<DayMode> onDayModeToggle = new ExtendedEvent<DayMode>();
    }
}
