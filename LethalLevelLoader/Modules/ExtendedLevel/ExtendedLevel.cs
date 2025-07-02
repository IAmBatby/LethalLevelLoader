using DunGen.Graph;
using GameNetcodeStuff;
using LethalFoundation;
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
    public class ExtendedLevel : ExtendedContent<ExtendedLevel, SelectableLevel, LevelManager>, ITerminalInfoEntry, ITerminalPurchasableEntry
    {
        public override SelectableLevel Content { get => SelectableLevel; protected set => SelectableLevel = value; }

        [field: Header("General Settings")]
        [field: SerializeField] public SelectableLevel SelectableLevel { get; set; }
        [Space(5)] [SerializeField] private int routePrice = 0;
        public int PurchasePrice { get => routePrice; private set => routePrice = value; }

        [field: Header("Extended Feature Settings")]
        [field: SerializeField] public bool OverrideDynamicRiskLevelAssignment { get; set; } = false;

        [field: Space(5)]
        [field: SerializeField] public GameObject OverrideQuicksandPrefab { get; set; }

        [field: Space(5)]
        [field: SerializeField] public bool IsRouteHidden { get; set; } = false;
        [field: SerializeField] public bool IsRouteLocked { get; set; } = false;
        public bool IsRouteRemoved { get; set; } = false;
        [field: SerializeField] public string LockedRouteNodeText { get; set; } = string.Empty;

        [field: Space(5)]
        [field: SerializeField] public AnimationClip ShipFlyToMoonClip { get; set; }
        [field: SerializeField] public AnimationClip ShipFlyFromMoonClip { get; set; }

        [field: Space(5), SerializeField] public List<StringWithRarity> SceneSelections { get; set; } = new List<StringWithRarity>();
        public List<string> SceneSelectionNames => SceneSelections.Select(s => s.Name).ToList();
        [field: Space(5), Tooltip("Overrides vanilla camera Far Plane Clip Distance, The highest value between current Level and Interior will be used.")]
        [field: Range(0f, 10000f)]
        [field: SerializeField] public float OverrideCameraMaxDistance = 400;

        [field: Space(5), Header("Weather Fog Distance Override Settings")]
        [field: SerializeField] public Vector3 OverrideDustStormVolumeSize { get; set; } = Vector3.zero;
        [field: SerializeField] public Vector3 OverrideFoggyVolumeSize { get; set; } = Vector3.zero;

        [field: Space(5), Header("Terminal Route Override Settings")]
        [field: SerializeField] public string OverrideRouteNoun { get; set; } = string.Empty;   
        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideInfoNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideRouteNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] [field: TextArea(2, 20)] public string OverrideRouteConfirmNodeDescription { get; set; } = string.Empty;

        [field: Space(10), Header("Misc. Settings"), Space(5)]
        [field: SerializeField] public bool GenerateAutomaticConfigurationOptions { get; set; } = true;

        [Space(25), Header("Obsolete (Legacy Fields, Will Be Removed In The Future)")]
        [Obsolete] public SelectableLevel selectableLevel;
        [Obsolete][Space(5)] public string contentSourceName = string.Empty; //Levels from AssetBundles will have this as their Assembly Name.
        [Obsolete][Space(5)] public List<string> levelTags = new List<string>();

        //Runtime Stuff

        public string TerminalNoun => string.IsNullOrEmpty(OverrideRouteNoun) ? NumberlessPlanetName.StripSpecialCharacters().Sanitized() : OverrideRouteNoun.StripSpecialCharacters().Sanitized();
        public string NumberlessPlanetName => new string(SelectableLevel.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
        public int CalculatedDifficultyRating => LevelManager.CalculateExtendedLevelDifficultyRating(this);
        public bool IsCurrentLevel => LevelManager.CurrentExtendedLevel == this;
        public bool IsLevelLoaded => Refs.IsCurrentLevelLoaded && SceneSelectionNames.Contains(Refs.LoadedLevelScene.name);

        [HideInInspector] public LevelEvents LevelEvents { get; internal set; } = new LevelEvents();

        public TerminalKeyword NounKeyword { get; internal set; }
        public TerminalNode PurchasePromptNode { get; internal set; }
        public TerminalNode PurchaseConfirmNode { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }
        public TerminalNode SimulateNode { get; internal set; }

        TerminalKeyword ITerminalPurchasableEntry.RegistryKeyword => TerminalManager.Keywords.Route;
        TerminalKeyword ITerminalInfoEntry.RegistryKeyword => TerminalManager.Keywords.Info;
        public List<CompatibleNoun> GetRegistrations() => new() { (this as ITerminalInfoEntry).GetPair(), (this as ITerminalPurchasableEntry).GetPair() };

        internal static ExtendedLevel Create(SelectableLevel newSelectableLevel) => Create<ExtendedLevel, SelectableLevel, LevelManager>(newSelectableLevel.name, newSelectableLevel);

        internal override void Initialize()
        {
            if (!SceneSelections.Select(s => s.Name).Contains(SelectableLevel.sceneName))
                SceneSelections.Add(new StringWithRarity(SelectableLevel.sceneName, 300));

            foreach (StringWithRarity sceneSelection in new List<StringWithRarity>(SceneSelections))
                if (!PatchedContent.AllLevelSceneNames.Contains(sceneSelection.Name))
                {
                    DebugHelper.LogWarning("Removing SceneSelection From: " + SelectableLevel.PlanetName + " As SceneName: " + sceneSelection.Name + " Is Not Loaded!", DebugType.Developer);
                    SceneSelections.Remove(sceneSelection);
                }

            ShipFlyToMoonClip = ShipFlyToMoonClip != null ? ShipFlyToMoonClip : LevelLoader.defaultShipFlyToMoonClip;
            ShipFlyFromMoonClip = ShipFlyFromMoonClip != null ? ShipFlyFromMoonClip : LevelLoader.defaultShipFlyFromMoonClip;
            OverrideQuicksandPrefab = OverrideQuicksandPrefab != null ? OverrideQuicksandPrefab : LevelLoader.defaultQuicksandPrefab;

            if (ContentType == ContentType.Custom)
            {
                name = NumberlessPlanetName.StripSpecialCharacters() + "ExtendedLevel";
                SelectableLevel.name = NumberlessPlanetName.StripSpecialCharacters() + "Level";
            }

            SetExtendedDungeonFlowMatches();
        }

        internal override void OnBeforeRegistration()
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

        protected override void OnGameIDChanged()
        {
            SelectableLevel.levelID = GameID;
            if (PurchasePromptNode != null) PurchasePromptNode.displayPlanetInfo = GameID;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.buyRerouteToMoon = GameID;
        }

        public void SetPurchasePrice(int newPrice)
        {
            PurchasePrice = newPrice;
            if (PurchasePromptNode != null) PurchasePromptNode.itemCost = newPrice;
            if (PurchaseConfirmNode != null) PurchaseConfirmNode.itemCost = newPrice;
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

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration()
        {
            List<PrefabReference> returnList = new List<PrefabReference>();
            returnList.AddRange(SelectableLevel.spawnableOutsideObjects.Select(s => new SpawnableOutsideObjectReference(s.spawnableObject)));
            returnList.AddRange(SelectableLevel.spawnableMapObjects.Select(s => new SpawnableMapObjectReference(s)));
            return (returnList);
        }
        internal override List<GameObject> GetNetworkPrefabsForRegistration() => NoNetworkPrefabs;
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
