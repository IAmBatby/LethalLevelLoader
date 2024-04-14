using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ScriptableObject = UnityEngine.ScriptableObject;

public enum ContentType { Vanilla, Custom, Any } //Any & All included for built in checks.

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedLevel")]
    public class ExtendedLevel : ExtendedContent
    {
        [Header("General Settings")]
        /*Obsolete*/ [Space(5)] public string contentSourceName = string.Empty; //Levels from AssetBundles will have this as their Assembly Name.
        [Space(5)] public SelectableLevel selectableLevel;
        [Space(5)] [SerializeField] private int routePrice = 0;

        [Header("Extended Features Settings")]
        [Space(5)] public bool isHidden = false;
        [Space(5)] public bool isLocked = false;
        [Space(5)] public string lockedNodeText = string.Empty;
        [Space(5)] public bool overrideDynamicRiskLevelAssignment = false;
        [Space(5)] public GameObject overrideQuicksandPrefab;
        [Space(5)] public AnimationClip ShipFlyToMoonClip;
        [Space(5)] public AnimationClip ShipFlyFromMoonClip;
        [field: SerializeField] public List<StringWithRarity> SceneSelections { get; internal set; } = new List<StringWithRarity>();

        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        /*Obsolete*/ [Space(5)] public List<string> levelTags = new List<string>();

        [Space(10)]
        [Header("Terminal Override Settings")]
        [SerializeField][TextArea(2, 20)] internal string overrideInfoNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] internal string overrideRouteNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] internal string overrideRouteConfirmNodeDescription = string.Empty;

        [Space(10)]
        [Header("Misc. Settings")]
        [Space(5)] public bool generateAutomaticConfigurationOptions = true;

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
                    DebugHelper.LogWarning("routeNode Is Missing! Using internal value!");
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
                    DebugHelper.LogWarning("routeNode Is Missing! Only setting internal value!");
                routePrice = value;
            }
        }

        public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);
        public int CalculatedDifficultyRating => LevelManager.CalculateExtendedLevelDifficultyRating(this);
        public bool IsCurrentLevel => LevelManager.CurrentExtendedLevel == this;
        public bool IsLevelLoaded => SceneManager.GetSceneByName(selectableLevel.sceneName).isLoaded;

        [HideInInspector] public LevelEvents LevelEvents { get; internal set; } = new LevelEvents();

        public TerminalNode RouteNode { get; internal set; }
        public TerminalNode RouteConfirmNode { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }

        //Dunno about these yet
        public List<ExtendedWeatherEffect> enabledExtendedWeatherEffects = new List<ExtendedWeatherEffect>();
        public ExtendedWeatherEffect currentExtendedWeatherEffect;

        internal static ExtendedLevel Create(SelectableLevel newSelectableLevel)
        {
            ExtendedLevel newExtendedLevel = ScriptableObject.CreateInstance<ExtendedLevel>();
            newExtendedLevel.selectableLevel = newSelectableLevel;

            return (newExtendedLevel);
        }
        internal void Initialize(string newContentSourceName, bool generateTerminalAssets)
        {
            bool mainSceneRegistered = false;

            foreach (StringWithRarity sceneSelection in SceneSelections)
                if (sceneSelection.Name == selectableLevel.sceneName)
                    mainSceneRegistered = true;

            if (mainSceneRegistered == false)
            {
                StringWithRarity newSceneSelection = new StringWithRarity(selectableLevel.sceneName, 300);
                SceneSelections.Add(newSceneSelection);
            }

            foreach (StringWithRarity sceneSelection in new List<StringWithRarity>(SceneSelections))
                if (!PatchedContent.AllLevelSceneNames.Contains(sceneSelection.Name))
                {
                    DebugHelper.LogWarning("Removing SceneSelection From: " + selectableLevel.PlanetName + " As SceneName: " + sceneSelection.Name + " Is Not Loaded!");
                    SceneSelections.Remove(sceneSelection);
                }

            if (ShipFlyToMoonClip == null)
                ShipFlyToMoonClip = LevelLoader.defaultShipFlyToMoonClip;
            if (ShipFlyFromMoonClip == null)
                ShipFlyFromMoonClip = LevelLoader.defaultShipFlyFromMoonClip;

            if (overrideQuicksandPrefab == null)
                overrideQuicksandPrefab = LevelLoader.defaultQuicksandPrefab;

            if (ContentType == ContentType.Custom)
            {
                name = NumberlessPlanetName.StripSpecialCharacters() + "ExtendedLevel";
                selectableLevel.name = NumberlessPlanetName.StripSpecialCharacters() + "Level";
                if (generateTerminalAssets == true) //Needs to be after levelID setting above.
                {
                    //DebugHelper.Log("Generating Terminal Assets For: " + NumberlessPlanetName);
                    TerminalManager.CreateLevelTerminalData(this, routePrice);
                }
            }

            SetExtendedDungeonFlowMatches();
            //if (ContentType == ContentType.Vanilla)
                //AssetBundleLoader.SetVanillaLevelTags(this);

            //Obsolete
            if (levelTags.Count > 0 && ContentTags.Count == 0)
                foreach (ContentTag convertedContentTag in ContentTagManager.CreateNewContentTags(levelTags))
                    ContentTags.Add(convertedContentTag);
            levelTags.Clear();
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
                selectableLevel.levelID = PatchedContent.ExtendedLevels.IndexOf(this);
                if (RouteNode != null)
                    RouteNode.displayPlanetInfo = selectableLevel.levelID;
                if (RouteConfirmNode != null)
                    RouteConfirmNode.buyRerouteToMoon = selectableLevel.levelID;
            }
        }

        internal void SetExtendedDungeonFlowMatches()
        {
            foreach (IntWithRarity intWithRarity in selectableLevel.dungeonFlowTypes)
                if (DungeonManager.TryGetExtendedDungeonFlow(Patches.RoundManager.dungeonFlowTypes[intWithRarity.id].dungeonFlow, out ExtendedDungeonFlow extendedDungeonFlow))
                    extendedDungeonFlow.levelMatchingProperties.planetNames.Add(new StringWithRarity(NumberlessPlanetName, intWithRarity.rarity));


            if (selectableLevel.sceneName == "Level4March")
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    if (indoorMapType.dungeonFlow.name == "Level1Flow3Exits")
                        if (DungeonManager.TryGetExtendedDungeonFlow(indoorMapType.dungeonFlow, out ExtendedDungeonFlow marchDungeonFlow))
                            marchDungeonFlow.levelMatchingProperties.planetNames.Add(new StringWithRarity(NumberlessPlanetName, 300));
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
