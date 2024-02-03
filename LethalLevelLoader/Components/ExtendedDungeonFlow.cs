using DunGen.Graph;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static LethalLevelLoader.DungeonEvents;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedDungeonFlow")]
    public class ExtendedDungeonFlow : ScriptableObject
    {
        [Header("Extended DungeonFlow Settings")]
        public string contentSourceName = string.Empty;
        [Space(5)] public string dungeonDisplayName = string.Empty;
        [Space(5)] public DungeonFlow dungeonFlow;
        [Space(5)] public AudioClip dungeonFirstTimeAudio;

        [Space(10)] [Header("Dynamic DungeonFlow Injections Settings")]
        public List<StringWithRarity> dynamicLevelTagsList = new List<StringWithRarity>();
        [Space(5)] public List<Vector2WithRarity> dynamicRoutePricesList = new List<Vector2WithRarity>();
        [Space(5)] public List<StringWithRarity> dynamicCurrentWeatherList = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> manualPlanetNameReferenceList = new List<StringWithRarity>();
        [Space(5)] public List<StringWithRarity> manualContentSourceNameReferenceList = new List<StringWithRarity>();

        [Space(10)] [Header("Dynamic Dungeon Size Multiplier Lerp Settings")]
        public bool enableDynamicDungeonSizeRestriction = false;
        public float dungeonSizeMin = 1;
        public float dungeonSizeMax = 1;
        [Range(0, 1)] public float dungeonSizeLerpPercentage = 1;


        [Space(10)] [Header("Dynamic DungeonFlow Modification Settings")]
        public List<GlobalPropCountOverride> globalPropCountOverridesList = new List<GlobalPropCountOverride>();

        [Space(10)] [Header("Misc. Settings")]
        public bool generateAutomaticConfigurationOptions = true;

        [Space(10)] [Header("Experimental Settings (Currently Unused As Of LethalLevelLoader 1.1.0")]
        public GameObject mainEntrancePropPrefab;
        [Space(5)] public GameObject fireExitPropPrefab;
        [Space(5)] public Animator mainEntrancePropAnimator;
        [Space(5)] public Animator fireExitPropAnimator;

        // HideInInspector
        [HideInInspector] public ContentType dungeonType;
        [HideInInspector] public int dungeonID;
        [HideInInspector] public int dungeonDefaultRarity; //To Be Deprecated

        [HideInInspector] public DungeonEvents dungeonEvents = new DungeonEvents();

        internal static ExtendedDungeonFlow Create(DungeonFlow newDungeonFlow, AudioClip newFirstTimeDungeonAudio, string contentSourceName)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = ScriptableObject.CreateInstance<ExtendedDungeonFlow>();
            newExtendedDungeonFlow.dungeonFlow = newDungeonFlow;
            newExtendedDungeonFlow.dungeonFirstTimeAudio = newFirstTimeDungeonAudio;
            newExtendedDungeonFlow.contentSourceName = contentSourceName;
            return (newExtendedDungeonFlow);
        }

        internal void Initialize(ContentType newDungeonType)
        {
            dungeonType = newDungeonType;

            if (dungeonType == ContentType.Custom)
                dungeonID = PatchedContent.ExtendedDungeonFlows.Count;
            if (dungeonType == ContentType.Vanilla)
                dungeonID = RoundManager.Instance.dungeonFlowTypes.ToList().IndexOf(dungeonFlow);

            if (dungeonDisplayName == null || dungeonDisplayName == string.Empty)
                dungeonDisplayName = dungeonFlow.name;

            name = dungeonFlow.name.Replace("Flow", "") + "ExtendedDungeonFlow";

            if (dungeonFirstTimeAudio == null)
            {
                DebugHelper.Log("Warning, Custom Dungeon: " + dungeonDisplayName + " Is Missing A DungeonFirstTimeAudio Reference! Assigning Facility Audio To Prevent Errors.");
                dungeonFirstTimeAudio = RoundManager.Instance.firstTimeDungeonAudios[0];
            }

            dungeonEvents.onBeforeDungeonGenerate.AddListener(OnBeforeDungeonGenerate);
            dungeonEvents.onSpawnedScrapObjects.AddListener(OnSpawnScrapObjects);
            dungeonEvents.onSpawnedSyncedObjects.AddListener(OnSpawnSyncedObjects);
            dungeonEvents.onEnemySpawnedFromVent.AddListener(OnEnemySpawnedFromVent);
            dungeonEvents.onSpawnedMapObjects.AddListener(OnSpawnMapObjects);
            dungeonEvents.onPlayerEnterDungeon.AddListener(OnPlayerEnterDungeon);
            dungeonEvents.onPlayerExitDungeon.AddListener(OnPlayerExitDungeon);
        }

        private void OnBeforeDungeonGenerate(RoundManager roundManager)
        {
            DebugHelper.Log(dungeonDisplayName + " recieved event invoke !");
        }

        private void OnSpawnScrapObjects(List<GrabbableObject> scraps)
        {
            DebugHelper.Log(dungeonDisplayName + " recieved spawn scrap event invoke !");
            foreach (GrabbableObject gbb in scraps)
                DebugHelper.Log(gbb.itemProperties.itemName);
        }


        private void OnSpawnSyncedObjects(List<GameObject> scraps)
        {
            DebugHelper.Log(dungeonDisplayName + " recieved spawn synced prop event invoke !");
            foreach (GameObject gbb in scraps)
                DebugHelper.Log(gbb.name);
        }

        private void OnEnemySpawnedFromVent((EnemyVent, EnemyAI) enemyPair)
        {
            DebugHelper.Log(dungeonDisplayName + " recieved enemy vent spawn " + enemyPair.Item2.enemyType.enemyName);
        }

        private void OnSpawnMapObjects(List<GameObject> scraps)
        {
            DebugHelper.Log(dungeonDisplayName + "spawn random objects");
            foreach (GameObject gbb in scraps)
                DebugHelper.Log(gbb.name);
        }

        private void OnPlayerEnterDungeon((EntranceTeleport, PlayerControllerB) playerTeleporterPair)
        {
            DebugHelper.Log(dungeonDisplayName + playerTeleporterPair.Item2.playerUsername + " Entered Dungeon via Entrance: " + playerTeleporterPair.Item1.gameObject.name);
        }

        private void OnPlayerExitDungeon((EntranceTeleport, PlayerControllerB) playerTeleporterPair)
        {
            DebugHelper.Log(dungeonDisplayName + playerTeleporterPair.Item2.playerUsername + " Exited Dungeon via Entrance: " + playerTeleporterPair.Item1.gameObject.name);
        }
    }

    [Serializable]
    public class StringWithRarity
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        [Range(0, 300)]
        private int _rarity;

        [HideInInspector] public string Name { get { return (_name); } set { _name = value; } }
        [HideInInspector] public int Rarity { get { return (_rarity); } set { _rarity = value; } }
        [HideInInspector] public StringWithRarity(string newName, int newRarity) { _name = newName; _rarity = newRarity; }
    }

    [Serializable]
    public class Vector2WithRarity
    {
        [SerializeField] private Vector2 _minMax;
        [SerializeField] private int _rarity;

        [HideInInspector] public float Min { get { return (_minMax.x); } set { _minMax.x = value; } }
        [HideInInspector] public float Max { get { return (_minMax.y); } set { _minMax.y = value; } }
        [HideInInspector] public int Rarity { get { return (_rarity); } set { _rarity = value; } }

        public Vector2WithRarity(Vector2 vector2, int newRarity)
        {
            _minMax.x = vector2.x;
            _minMax.y = vector2.y;
            _rarity = newRarity;
        }

        public Vector2WithRarity(float newMin, float newMax, int newRarity)
        {
            _minMax.x = newMin;
            _minMax.y = newMax;
            _rarity = newRarity;
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