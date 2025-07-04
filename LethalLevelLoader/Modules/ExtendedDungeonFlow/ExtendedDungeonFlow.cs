﻿using DunGen.Graph;
using GameNetcodeStuff;
using LethalFoundation;
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
    public class ExtendedDungeonFlow : ExtendedContent<ExtendedDungeonFlow, DungeonFlow, DungeonManager>
    {
        public override DungeonFlow Content { get => DungeonFlow; protected set => DungeonFlow = value; }
        [field: Header("General Settings")]
        [field: SerializeField] public DungeonFlow DungeonFlow { get; set; }
        [field: SerializeField] public string DungeonName { get; set; } = string.Empty;
        [field: SerializeField] public float MapTileSize { get; set; } = 1f;
        [field: SerializeField] public AudioClip FirstTimeDungeonAudio { get; set; }

        [field: Space(5)]
        [field: Header("Dynamic Injection Matching Settings")]
        [field: SerializeField] public LevelMatchingProperties LevelMatchingProperties { get; set; }

        [field: Space(5)]
        [field: Header("Extended Feature Settings")]

        [field: Tooltip("When above Vector3.zero this value will restrict the max bounds of the interior.")]
        [field: SerializeField] public Vector3 OverrideRestrictedTilePlacementBounds { get; set; } = Vector3.zero;
        public bool OverrideTilePlacementBounds => (OverrideRestrictedTilePlacementBounds.sqrMagnitude > 1f);

        [field: SerializeField] public GameObject OverrideKeyPrefab { get; set; }
        [field: SerializeField] public List<SpawnableMapObject> SpawnableMapObjects { get; set; } = new List<SpawnableMapObject>();
        [field: SerializeField] public List<GlobalPropCountOverride> GlobalPropCountOverridesList { get; set; } = new List<GlobalPropCountOverride>();

        [field: Space(5)]

        [field: SerializeField] public bool IsDynamicDungeonSizeRestrictionEnabled { get; set; }
        [field: SerializeField] public Vector2 DynamicDungeonSizeMinMax { get; set; } = new Vector2(1, 1);
        [field: SerializeField][field: Range(0, 1)] public float DynamicDungeonSizeLerpRate { get; set; } = 1f;

        [field: Space(5)]
        [field: Tooltip("Overrides vanilla camera Far Plane Clip Distance, The highest value between current Level and Interior will be used.")]
        [field: Range(0f, 10000f)]
        [field: SerializeField] public float OverrideCameraMaxDistance = 400;


        [field: Space(10)][field: Header("Misc. Settings")]
        [field: SerializeField] public bool GenerateAutomaticConfigurationOptions { get; set; } = true;

        [Space(25)]
        [Header("Obsolete (Legacy Fields, Will Be Removed In The Future)")]
        //public bool IsDynamicDungeonSizeRestrictionEnabled = false;
        [Obsolete] public bool generateAutomaticConfigurationOptions = true;
        [Obsolete] public bool enableDynamicDungeonSizeRestriction = false;
        [Obsolete] public float dungeonSizeMin = 1;
        [Obsolete] public float dungeonSizeMax = 1;
        [Obsolete][Range(0, 1)] public float dungeonSizeLerpPercentage = 1;
        [Obsolete] public AudioClip dungeonFirstTimeAudio;
        [Obsolete] public DungeonFlow dungeonFlow;
        [Obsolete] public string dungeonDisplayName = string.Empty;
        [Obsolete] public string contentSourceName = string.Empty;
        [Obsolete] public List<StringWithRarity> dynamicLevelTagsList = new List<StringWithRarity>();
        [Obsolete] public List<Vector2WithRarity> dynamicRoutePricesList = new List<Vector2WithRarity>();
        [Obsolete] public List<StringWithRarity> dynamicCurrentWeatherList = new List<StringWithRarity>();
        [Obsolete] public List<StringWithRarity> manualPlanetNameReferenceList = new List<StringWithRarity>();
        [Obsolete] public List<StringWithRarity> manualContentSourceNameReferenceList = new List<StringWithRarity>();
        [Obsolete][HideInInspector] public int dungeonDefaultRarity;

        // HideInInspector
        public int DungeonID => GameID;
        public bool IsCurrentDungeon => (DungeonManager.CurrentExtendedDungeonFlow == this);
        [HideInInspector] public DungeonEvents DungeonEvents { get; internal set; } = new DungeonEvents();

        internal static ExtendedDungeonFlow Create(DungeonFlow newDungeonFlow, AudioClip newFirstTimeDungeonAudio)
        {
            ExtendedDungeonFlow newExtendedDungeonFlow = Create<ExtendedDungeonFlow,DungeonFlow,DungeonManager>(newDungeonFlow.name, newDungeonFlow);
            newExtendedDungeonFlow.FirstTimeDungeonAudio = newFirstTimeDungeonAudio;
            newExtendedDungeonFlow.LevelMatchingProperties = LevelMatchingProperties.Create(newExtendedDungeonFlow);
            return (newExtendedDungeonFlow);
        }

        internal override void Initialize()
        {
            LevelMatchingProperties = MatchingProperties.TryCreate(LevelMatchingProperties, this);

            DungeonName = string.IsNullOrEmpty(DungeonName) ? DungeonFlow.name : DungeonName;
            name = DungeonFlow.name.Replace("Flow", "") + "ExtendedDungeonFlow";

            if (FirstTimeDungeonAudio == null)
            {
                DebugHelper.LogWarning("Custom Dungeon: " + DungeonName + " Is Missing A DungeonFirstTimeAudio Reference! Assigning Facility Audio To Prevent Errors.", DebugType.User);
                FirstTimeDungeonAudio = Refs.FirstTimeDungeonAudios[0];
            }
        }

        internal override void OnBeforeRegistration()
        {
            if (DungeonFlow == null && dungeonFlow != null)
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.dungeonFlow is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.DungeonFlow instead.", DebugType.Developer);
                DungeonFlow = dungeonFlow;
                dungeonFlow = null;
            }
            if (string.IsNullOrEmpty(DungeonName) && !string.IsNullOrEmpty(dungeonDisplayName))
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.dungeonDisplayName is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.DungeonName instead.", DebugType.Developer);
                DungeonName = dungeonDisplayName;
                dungeonDisplayName = string.Empty;
            }
            if (FirstTimeDungeonAudio == null &&  dungeonFirstTimeAudio != null)
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.dungeonFirstTimeAudio is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.FirstTimeDungeonAudio instead.", DebugType.Developer);
                FirstTimeDungeonAudio = dungeonFirstTimeAudio;
                dungeonFirstTimeAudio = null;
            }
            if (dungeonSizeLerpPercentage != 1f)
                DebugHelper.LogWarning("ExtendedDungeonFlow.dungeonSizeLerpPercentage is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.DynamicDungeonSizeLerpRate instead.", DebugType.Developer);
            if (dungeonSizeMax != 1 || dungeonSizeMin != 1)
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.dungeonSizeMin and ExtendedDungeonFlow.dungeonSizeMax are Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.DynamicSungeonSizeMinMax instead.", DebugType.Developer);
                DynamicDungeonSizeMinMax = new Vector2(dungeonSizeMin, dungeonSizeMax);
            }
            if (!string.IsNullOrEmpty(contentSourceName))
                DebugHelper.LogWarning("ExtendedDungeonFlow.contentSourceName is Obsolete and will be removed in following releases, Please use ExtendedMod.AuthorName instead.", DebugType.Developer);
            if (LevelMatchingProperties == null && (dynamicLevelTagsList.Count > 0 || dynamicRoutePricesList.Count > 0 || dynamicCurrentWeatherList.Count > 0 || manualContentSourceNameReferenceList.Count > 0 || manualContentSourceNameReferenceList.Count > 0))
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow dynamic and manual match reference lists are Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.LevelMatchingProperties instead.", DebugType.Developer);
                LevelMatchingProperties = MatchingProperties.TryCreate(LevelMatchingProperties, this);
                LevelMatchingProperties.ApplyValues(newAuthorNames: manualContentSourceNameReferenceList, newPlanetNames: manualPlanetNameReferenceList, newLevelTags: dynamicLevelTagsList, newRoutePrices: dynamicRoutePricesList, newCurrentWeathers: dynamicCurrentWeatherList);
            }
            if (enableDynamicDungeonSizeRestriction != false || (IsDynamicDungeonSizeRestrictionEnabled != enableDynamicDungeonSizeRestriction))
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.enableDynamicDungeonSizeRestriction Is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.IsDynamicDungeonRestrictionEnabled instead.", DebugType.Developer);
                IsDynamicDungeonSizeRestrictionEnabled = enableDynamicDungeonSizeRestriction;
            }
            if (generateAutomaticConfigurationOptions == false || (GenerateAutomaticConfigurationOptions != generateAutomaticConfigurationOptions))
            {
                DebugHelper.LogWarning("ExtendedDungeonFlow.generateAutomaticConfigurationOptions Is Obsolete and will be removed in following releases, Please use ExtendedDungeonFlow.GenerateAutomaticConfigurationOptions instead.", DebugType.Developer);
                GenerateAutomaticConfigurationOptions = generateAutomaticConfigurationOptions;
            }
        }

        internal override List<GameObject> GetNetworkPrefabsForRegistration() => NoNetworkPrefabs;
        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration()
        {
            List<PrefabReference> returnList = new List<PrefabReference>();
            returnList.AddRange(DungeonFlow.GetSpawnSyncedObjects().Select(s => new SpawnSyncedObjectReference(s)));
            returnList.AddRange(SpawnableMapObjects.Select(s => new SpawnableMapObjectReference(s)));
            return (returnList);
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
        public ExtendedEvent onShipLand = new ExtendedEvent();
        public ExtendedEvent onShipLeave = new ExtendedEvent();
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