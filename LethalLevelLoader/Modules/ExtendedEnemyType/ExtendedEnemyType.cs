using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedEnemyType", menuName = "Lethal Level Loader/Extended Content/ExtendedEnemyType", order = 24)]
    public class ExtendedEnemyType : ExtendedContent<ExtendedEnemyType, EnemyType, EnemyManager>, ITerminalInfoEntry
    {
        public override EnemyType Content { get => EnemyType; protected set => EnemyType = value; }

        [field: Header("General Settings")]
        [field: SerializeField] public EnemyType EnemyType { get; set; }
        [field: SerializeField] public string EnemyDisplayName { get; set; }

        [field: Space(5), Header("Dynamic Injection Matching Settings")]
        [field: SerializeField] public LevelMatchingProperties OutsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public LevelMatchingProperties DaytimeLevelMatchingProperties { get; set; }
        [field: SerializeField] public LevelMatchingProperties InsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public DungeonMatchingProperties InsideDungeonMatchingProperties { get; set; }

        [field: Space(5), Header("Terminal Bestiary Override Settings")]
        [field: SerializeField] [field: TextArea(2,20)] public string InfoNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] public VideoClip InfoNodeVideoClip { get; set; }

        public EnemyAI Prefab { get; private set; }
        public ScanNodeProperties ScanNodeProperties { get; internal set; }
        public int EnemyID => GameID;

        public TerminalKeyword NounKeyword { get; internal set; }
        public TerminalNode InfoNode { get; internal set; }
        TerminalKeyword ITerminalInfoEntry.RegistryKeyword => TerminalManager.Keywords.Info;
        public List<CompatibleNoun> GetRegistrations() => new() {(this as ITerminalInfoEntry).GetPair()};

        //Might be obsolete
        public static ExtendedEnemyType Create(EnemyType enemyType, ExtendedMod extendedMod, ContentType contentType) => Create(enemyType);
        public static ExtendedEnemyType Create(EnemyType enemyType)
        {
            ExtendedEnemyType extendedEnemyType = Create<ExtendedEnemyType, EnemyType, EnemyManager>(enemyType.enemyName.SkipToLetters().RemoveWhitespace() + "ExtendedEnemyType", enemyType);
            extendedEnemyType.TryCreateMatchingProperties();
            return (extendedEnemyType);
        }

        internal override void Initialize()
        {
            DebugHelper.Log("Initializing Enemy: " + EnemyType.enemyName, DebugType.Developer);
            Prefab = EnemyType.enemyPrefab.GetComponent<EnemyAI>();
            ScanNodeProperties = Prefab.GetComponentInChildren<ScanNodeProperties>();
            TryCreateMatchingProperties();
        }

        protected override void OnGameIDChanged()
        {
            if (ScanNodeProperties != null) ScanNodeProperties.creatureScanID = GameID;
            if (InfoNode != null) InfoNode.creatureFileID = GameID;
        }

        internal override void TryCreateMatchingProperties()
        {
            InsideLevelMatchingProperties = MatchingProperties.TryCreate(InsideLevelMatchingProperties, this);
            OutsideLevelMatchingProperties = MatchingProperties.TryCreate(OutsideLevelMatchingProperties, this);
            InsideDungeonMatchingProperties = MatchingProperties.TryCreate(InsideDungeonMatchingProperties, this);
            DaytimeLevelMatchingProperties = MatchingProperties.TryCreate(DaytimeLevelMatchingProperties, this);
        }

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
        internal override List<GameObject> GetNetworkPrefabsForRegistration() => EnemyType.enemyPrefab.GetComponentsInChildren<NetworkObject>().Select(n => n.gameObject).ToList();
    }
}
