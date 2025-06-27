using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedEnemyType", menuName = "Lethal Level Loader/Extended Content/ExtendedEnemyType", order = 24)]
    public class ExtendedEnemyType : ExtendedContent<ExtendedEnemyType, EnemyType, EnemyManager>
    {
        public override RestorationPeriod RestorationPeriod => RestorationPeriod.MainMenu;
        public override EnemyType Content => EnemyType;
        [field: Header("General Settings")]

        [field: SerializeField] public EnemyType EnemyType { get; set; }
        [field: SerializeField] public string EnemyDisplayName { get; set; }

        [field: Space(5)]
        [field: Header("Dynamic Injection Matching Settings")]

        [field: SerializeField] public LevelMatchingProperties OutsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public LevelMatchingProperties DaytimeLevelMatchingProperties { get; set; }

        [field: SerializeField] public LevelMatchingProperties InsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public DungeonMatchingProperties InsideDungeonMatchingProperties { get; set; }

        [field: Space(5)]
        [field: Header("Terminal Bestiary Override Settings")]

        [field: SerializeField] [field: TextArea(2,20)] public string InfoNodeDescription { get; set; } = string.Empty;
        [field: SerializeField] public VideoClip InfoNodeVideoClip { get; set; }

        public EnemyAI Prefab { get; private set; }
        public ScanNodeProperties ScanNodeProperties { get; internal set; }
        public int EnemyID => GameID;
        public TerminalKeyword EnemyInfoKeyword { get; internal set; }
        public TerminalNode EnemyInfoNode { get; internal set; }

        //Might be obsolete
        public static ExtendedEnemyType Create(EnemyType enemyType, ExtendedMod extendedMod, ContentType contentType) => Create(enemyType);
        public static ExtendedEnemyType Create(EnemyType enemyType)
        {
            ExtendedEnemyType extendedEnemyType = ScriptableObject.CreateInstance<ExtendedEnemyType>();
            extendedEnemyType.EnemyType = enemyType;
            extendedEnemyType.name = enemyType.enemyName.SkipToLetters().RemoveWhitespace() + "ExtendedEnemyType";
            extendedEnemyType.TryCreateMatchingProperties();
            return (extendedEnemyType);
        }

        internal override void Initialize()
        {
            DebugHelper.Log("Initializing Custom Enemy: " + EnemyType.enemyName, DebugType.Developer);

            Prefab = EnemyType.enemyPrefab.GetComponent<EnemyAI>();
            ScanNodeProperties = Prefab.GetComponentInChildren<ScanNodeProperties>();

            TryCreateMatchingProperties();
        }

        protected override void OnGameIDChanged()
        {
            if (ScanNodeProperties != null) ScanNodeProperties.creatureScanID = GameID;
            if (EnemyInfoNode != null) EnemyInfoNode.creatureFileID = GameID;
        }

        internal override void TryCreateMatchingProperties()
        {
            if (InsideLevelMatchingProperties == null)
                InsideLevelMatchingProperties = LevelMatchingProperties.Create(this);
            if (InsideDungeonMatchingProperties == null)
                InsideDungeonMatchingProperties = DungeonMatchingProperties.Create(this);
            if (OutsideLevelMatchingProperties == null)
                OutsideLevelMatchingProperties = LevelMatchingProperties.Create(this);
            if (DaytimeLevelMatchingProperties == null)
                DaytimeLevelMatchingProperties = LevelMatchingProperties.Create(this);
        }

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
        internal override List<GameObject> GetNetworkPrefabsForRegistration()
        {
            return (EnemyType.enemyPrefab.GetComponentsInChildren<NetworkObject>().Select(n => n.gameObject).ToList());
        }
    }
}
