using DunGen.Graph;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LethalLevelLoader.DungeonEvents;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedDungeonFlow")]
    public class ExtendedDungeonFlow : ExtendedContent
    {
        [Header("Extended DungeonFlow Settings")]
        /*Obsolete*/ public string contentSourceName = string.Empty;
        [field: SerializeField] public string DungeonName { get; internal set; } = string.Empty;
        [Space(5)] public DungeonFlow dungeonFlow;
        [Space(5)] public AudioClip dungeonFirstTimeAudio;

        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        public LevelMatchingProperties levelMatchingProperties;
        /*Obsolete*/ public List<StringWithRarity> dynamicLevelTagsList = new List<StringWithRarity>();
        /*Obsolete*/ public List<Vector2WithRarity> dynamicRoutePricesList = new List<Vector2WithRarity>();
        /*Obsolete*/ public List<StringWithRarity> dynamicCurrentWeatherList = new List<StringWithRarity>();
        /*Obsolete*/ public List<StringWithRarity> manualPlanetNameReferenceList = new List<StringWithRarity>();
        /*Obsolete*/ public List<StringWithRarity> manualContentSourceNameReferenceList = new List<StringWithRarity>();

        [Space(10)] [Header("Dynamic Dungeon Size Multiplier Lerp Settings")]
        public bool enableDynamicDungeonSizeRestriction = false;
        public float dungeonSizeMin = 1;
        public float dungeonSizeMax = 1;
        [Range(0, 1)] public float dungeonSizeLerpPercentage = 1;


        [Space(10)] [Header("Dynamic DungeonFlow Modification Settings")]
        public List<SpawnableMapObject> spawnableMapObjects = new List<SpawnableMapObject>();
        public List<GlobalPropCountOverride> globalPropCountOverridesList = new List<GlobalPropCountOverride>();

        [Space(10)] [Header("Misc. Settings")]
        [SerializeField] internal bool generateAutomaticConfigurationOptions = true;

        // HideInInspector
        [HideInInspector] public int DungeonID { get; internal set; }

        [HideInInspector] public bool IsCurrentDungeon => (DungeonManager.CurrentExtendedDungeonFlow == this);

        [HideInInspector] public DungeonEvents DungeonEvents { get; internal set; } = new DungeonEvents();

        internal static ExtendedDungeonFlow Create(DungeonFlow newDungeonFlow, AudioClip newFirstTimeDungeonAudio)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            newExtendedDungeonFlow.dungeonFlow = newDungeonFlow;
            newExtendedDungeonFlow.dungeonFirstTimeAudio = newFirstTimeDungeonAudio;

            if (newExtendedDungeonFlow.levelMatchingProperties == null)
                newExtendedDungeonFlow.TryCreateMatchingProperties();
            return (newExtendedDungeonFlow);
        }

        internal void Initialize()
        {
            GetDungeonFlowID();

            if (DungeonName == null || DungeonName == string.Empty)
                DungeonName = dungeonFlow.name;

            name = dungeonFlow.name.Replace("Flow", "") + "ExtendedDungeonFlow";
            if (levelMatchingProperties == null)
                TryCreateMatchingProperties();

            if (dungeonFirstTimeAudio == null)
            {
                DebugHelper.LogWarning("Custom Dungeon: " + DungeonName + " Is Missing A DungeonFirstTimeAudio Reference! Assigning Facility Audio To Prevent Errors.");
                dungeonFirstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
            }
        }

        private void GetDungeonFlowID()
        {
            if (ContentType == ContentType.Custom)
                DungeonID = PatchedContent.ExtendedDungeonFlows.Count;
            if (ContentType == ContentType.Vanilla)
                DungeonID = RoundManager.Instance.dungeonFlowTypes.ToList().IndexOf(dungeonFlow);
        }

        internal override void TryCreateMatchingProperties()
        {
            levelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            levelMatchingProperties.name = name + "MatchingProperties";
            levelMatchingProperties.levelTags = new List<StringWithRarity>(dynamicLevelTagsList);
            levelMatchingProperties.modNames = new List<StringWithRarity>(manualContentSourceNameReferenceList);
            levelMatchingProperties.planetNames = new List<StringWithRarity>(manualPlanetNameReferenceList);
            levelMatchingProperties.currentRoutePrice = new List<Vector2WithRarity>(dynamicRoutePricesList);
            levelMatchingProperties.currentWeather = new List<StringWithRarity>(dynamicCurrentWeatherList);
        }
    }

    [Serializable]
    public class GlobalPropCountOverride
    {
        public int globalPropID;
        [Range(0,1)] public float globalPropCountScaleRate = 0;
    }

    [System.Serializable]
    public class DungeonEvents
    {
        public ExtendedEvent<RoundManager> onBeforeDungeonGenerate = new ExtendedEvent<RoundManager>();
        public ExtendedEvent<List<GameObject>> onSpawnedSyncedObjects = new ExtendedEvent<List<GameObject>>();
        public ExtendedEvent<List<GameObject>> onSpawnedMapObjects = new ExtendedEvent<List<GameObject>>();
        public ExtendedEvent<List<GrabbableObject>> onSpawnedScrapObjects = new ExtendedEvent<List<GrabbableObject>>();
        public ExtendedEvent<(EnemyVent, EnemyAI)> onEnemySpawnedFromVent = new ExtendedEvent<(EnemyVent, EnemyAI)>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerEnterDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<(EntranceTeleport, PlayerControllerB)> onPlayerExitDungeon = new ExtendedEvent<(EntranceTeleport, PlayerControllerB)>();
        public ExtendedEvent<bool> onPowerSwitchToggle = new ExtendedEvent<bool>();
        public ExtendedEvent<LungProp> onApparatusTaken = new ExtendedEvent<LungProp>();
    }
}