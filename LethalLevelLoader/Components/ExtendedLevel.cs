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
    public class ExtendedLevel : ScriptableObject
    {
        [Header("General Settings")]
        [Space(5)] public string contentSourceName = string.Empty; //Levels from AssetBundles will have this as their Assembly Name.
        [Space(5)] public SelectableLevel selectableLevel;
        [Space(5)] [SerializeField] private int routePrice = 0;

        [Header("Extended Features Settings")]
        [Space(5)] public bool isHidden = false;
        [Space(5)] public bool isLocked = false;
        [Space(5)] public string lockedNodeText = string.Empty;

        [Space(10)] public List<StoryLogData> storyLogs = new List<StoryLogData>();

        [Space(10)] public List<ExtendedFootstepSurface> extendedFootstepSurfaces = new List<ExtendedFootstepSurface>();


        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        [Space(5)] public ContentType allowedDungeonContentTypes = ContentType.Any;
        [Space(5)] public List<string> levelTags = new List<string>();

        [Space(10)]
        [Header("Terminal Override Settings")]
        [SerializeField][TextArea(2, 20)] public string overrideInfoNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] public string overrideRouteNodeDescription = string.Empty;
        [SerializeField][TextArea(2, 20)] public string overrideRouteConfirmNodeDescription = string.Empty;

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

        [HideInInspector] public ContentType levelType;
        public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);
        public int CalculatedDifficultyRating => LevelManager.CalculateExtendedLevelDifficultyRating(this);
        public bool IsCurrentLevel => LevelManager.CurrentExtendedLevel == this;
        public bool IsLoadedLevel => SceneManager.GetSceneByName(selectableLevel.sceneName).isLoaded;

        [HideInInspector] public LevelEvents levelEvents = new LevelEvents();

        public TerminalNode RouteNode { get; internal set; }
        public TerminalNode RouteConfirmNode { get; set; }
        public TerminalNode InfoNode { get; set; }

        public List<ExtendedWeatherEffect> enabledExtendedWeatherEffects = new List<ExtendedWeatherEffect>();
        public ExtendedWeatherEffect currentExtendedWeatherEffect;

        internal bool isLethalExpansion = false;

        internal static ExtendedLevel Create(SelectableLevel newSelectableLevel, ContentType newContentType)
        {
            ExtendedLevel newExtendedLevel = ScriptableObject.CreateInstance<ExtendedLevel>();
            

            newExtendedLevel.levelType = newContentType;
            newExtendedLevel.selectableLevel = newSelectableLevel;

            return (newExtendedLevel);
        }
        internal void Initialize(string newContentSourceName, bool generateTerminalAssets)
        {
            if (levelType == ContentType.Vanilla && selectableLevel.levelID > 8)
            {
                DebugHelper.LogWarning("LethalExpansion SelectableLevel " + NumberlessPlanetName + " Found, Setting To LevelType: Custom.");
                levelType = ContentType.Custom;
                //generateTerminalAssets = true;
                contentSourceName = "Lethal Expansion";
                levelTags.Clear();
                isLethalExpansion = true;
            }

            if (contentSourceName == string.Empty)
                contentSourceName = newContentSourceName;

            if (levelType == ContentType.Custom)
                levelTags.Add("Custom");

            if (isLethalExpansion == false)
                SetLevelID();

            if (generateTerminalAssets == true) //Needs to be after levelID setting above.
            {
                //DebugHelper.Log("Generating Terminal Assets For: " + NumberlessPlanetName);
                TerminalManager.CreateLevelTerminalData(this, routePrice);
            }

            if (levelType == ContentType.Custom)
            {
                name = NumberlessPlanetName.StripSpecialCharacters() + "ExtendedLevel";
                selectableLevel.name = NumberlessPlanetName.StripSpecialCharacters() + "Level";
                LevelManager.RegisterExtendedFootstepSurfaces(this);
            }

            levelEvents.onDayModeToggle.AddListener(DebugDaymodeToggle);
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
            if (levelType == ContentType.Custom)
            {
                selectableLevel.levelID = PatchedContent.ExtendedLevels.IndexOf(this);
                if (RouteNode != null)
                    RouteNode.displayPlanetInfo = selectableLevel.levelID;
                if (RouteConfirmNode != null)
                    RouteConfirmNode.buyRerouteToMoon = selectableLevel.levelID;
            }
        }

        internal void DebugDaymodeToggle(DayMode dayMode)
        {
            DebugHelper.Log("DayMode Toggle Event: " + dayMode.ToString());
        }

        public void ForceSetRoutePrice(int newValue)
        {
            Debug.LogWarning("ForceSetRoutePrice Should Only Be Used In Editor! Consider Using RoutePrice Property To Sync TerminalNode's With New Value.");
            routePrice = newValue;
        }
    }
        

    [System.Serializable]
    public class LevelEvents
    {
        public ExtendedEvent onLevelLoaded = new ExtendedEvent();
        public ExtendedEvent onNighttime = new ExtendedEvent();
        public ExtendedEvent<EnemyAI> onDaytimeEnemySpawn = new ExtendedEvent<EnemyAI>();
        public ExtendedEvent<EnemyAI> onNighttimeEnemySpawn = new ExtendedEvent<EnemyAI>();
        public ExtendedEvent<StoryLog> onStoryLogCollected = new ExtendedEvent<StoryLog>();
        public ExtendedEvent<LungProp> onApparatusTaken = new ExtendedEvent<LungProp>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerEnterDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerExitDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<bool> onPowerSwitchToggle = new ExtendedEvent<bool>();
        public ExtendedEvent<DayMode> onDayModeToggle = new ExtendedEvent<DayMode>();

    }

    [System.Serializable]
    public class StoryLogData
    {
        public int storyLogID;
        public string terminalWord = string.Empty;
        public string storyLogTitle = string.Empty;
        [TextArea] public string storyLogDescription = string.Empty;

        [HideInInspector] internal int newStoryLogID;
    }
}
