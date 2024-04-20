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
    [CreateAssetMenu(fileName = "ExtendedDungeonFlow", menuName = "Lethal Level Loader/Extended Content/ExtendedDungeonFlow", order = 21)]
    public class ExtendedDungeonFlow : ExtendedContent
    {
        [field: Header("General Settings")]
        [field: SerializeField] public string DungeonName{ get { return dungeonDisplayName; } set { dungeonDisplayName = value; } }
        [field: SerializeField] public float MapTileSize = 1f;

        [field: Space(5)]
        [field: Header("Dynamic Injection Matching Settings")]
        [field: SerializeField] public LevelMatchingProperties LevelMatchingProperties { get; set; }

        [field: Space(5)]
        [field: Header("Extended Feature Settings")]

        [field: SerializeField] public GameObject OverrideKeyPrefab { get; set; }
        [field: SerializeField] public List<SpawnableMapObject> SpawnableMapObjects { get; set; } = new List<SpawnableMapObject>();
        [field: SerializeField] public List<GlobalPropCountOverride> GlobalPropCountOverridesList { get; set; } = new List<GlobalPropCountOverride>();

        [field: Space(5)]

        public bool enableDynamicDungeonSizeRestriction = false;
        public float dungeonSizeMin = 1;
        public float dungeonSizeMax = 1;
        [Range(0, 1)] public float dungeonSizeLerpPercentage = 1;

        [Space(10)] [Header("Misc. Settings")]
        [SerializeField] internal bool generateAutomaticConfigurationOptions = true;

        [Space(25)]
        [Header("Obsolete (Legacy Fields, Will Be Removed In The Future)")]
        public AudioClip dungeonFirstTimeAudio;
        public DungeonFlow dungeonFlow;
        public string dungeonDisplayName = string.Empty;
        public string contentSourceName = string.Empty;
        public List<StringWithRarity> dynamicLevelTagsList = new List<StringWithRarity>();
        public List<Vector2WithRarity> dynamicRoutePricesList = new List<Vector2WithRarity>();
        public List<StringWithRarity> dynamicCurrentWeatherList = new List<StringWithRarity>();
        public List<StringWithRarity> manualPlanetNameReferenceList = new List<StringWithRarity>();
        public List<StringWithRarity> manualContentSourceNameReferenceList = new List<StringWithRarity>();
        [HideInInspector] public int dungeonDefaultRarity;

        // HideInInspector
        public int DungeonID { get; internal set; }
        public bool IsCurrentDungeon => (DungeonManager.CurrentExtendedDungeonFlow == this);
        [HideInInspector] public DungeonEvents dungeonEvents = new DungeonEvents();

        internal static ExtendedDungeonFlow Create(DungeonFlow newDungeonFlow, AudioClip newFirstTimeDungeonAudio)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            newExtendedDungeonFlow.dungeonFlow = newDungeonFlow;
            newExtendedDungeonFlow.dungeonFirstTimeAudio = newFirstTimeDungeonAudio;

            if (newExtendedDungeonFlow.LevelMatchingProperties == null)
                newExtendedDungeonFlow.TryCreateMatchingProperties();
            return (newExtendedDungeonFlow);
        }

        internal void Initialize()
        {
            GetDungeonFlowID();

            if (DungeonName == null || DungeonName == string.Empty)
                DungeonName = dungeonFlow.name;

            name = dungeonFlow.name.Replace("Flow", "") + "ExtendedDungeonFlow";
            if (LevelMatchingProperties == null)
                TryCreateMatchingProperties();

            if (dungeonFirstTimeAudio == null)
            {
                DebugHelper.LogWarning("Custom Dungeon: " + DungeonName + " Is Missing A DungeonFirstTimeAudio Reference! Assigning Facility Audio To Prevent Errors.");
                dungeonFirstTimeAudio = Patches.RoundManager.firstTimeDungeonAudios[0];
            }

            if (OverrideKeyPrefab == null)
                OverrideKeyPrefab = DungeonLoader.defaultKeyPrefab;
        }

        private void GetDungeonFlowID()
        {
            if (ContentType == ContentType.Custom)
                DungeonID = PatchedContent.ExtendedDungeonFlows.Count;
            if (ContentType == ContentType.Vanilla)
                foreach (IndoorMapType indoorMapType in Patches.RoundManager.dungeonFlowTypes)
                    if (indoorMapType.dungeonFlow == dungeonFlow)
                        DungeonID = Patches.RoundManager.dungeonFlowTypes.ToList().IndexOf(indoorMapType);
        }

        internal override void TryCreateMatchingProperties()
        {
            LevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            LevelMatchingProperties.name = name + "MatchingProperties";
            LevelMatchingProperties.levelTags = new List<StringWithRarity>(dynamicLevelTagsList);
            LevelMatchingProperties.modNames = new List<StringWithRarity>(manualContentSourceNameReferenceList);
            LevelMatchingProperties.planetNames = new List<StringWithRarity>(manualPlanetNameReferenceList);
            LevelMatchingProperties.currentRoutePrice = new List<Vector2WithRarity>(dynamicRoutePricesList);
            LevelMatchingProperties.currentWeather = new List<StringWithRarity>(dynamicCurrentWeatherList);
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