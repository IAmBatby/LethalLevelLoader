using UnityEngine;
using UnityEngine.Video;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedEnemyType", menuName = "Lethal Level Loader/Extended Content/ExtendedEnemyType", order = 24)]
    public class ExtendedEnemyType : ExtendedContent
    {
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

        public ScanNodeProperties ScanNodeProperties { get; internal set; }
        public int EnemyID { get; internal set; }
        public TerminalNode EnemyInfoNode { get; internal set; }

        public static ExtendedEnemyType Create(EnemyType enemyType, ExtendedMod extendedMod, ContentType contentType)
        {
            ExtendedEnemyType extendedEnemyType = ScriptableObject.CreateInstance<ExtendedEnemyType>();
            extendedEnemyType.EnemyType = enemyType;
            extendedEnemyType.name = enemyType.enemyName.SkipToLetters().RemoveWhitespace() + "ExtendedEnemyType";
            extendedEnemyType.ContentType = contentType;
            extendedMod.RegisterExtendedContent(extendedEnemyType);

            extendedEnemyType.TryCreateMatchingProperties();

            return (extendedEnemyType);
        }

        public void Initalize()
        {
            DebugHelper.Log("Initializing Custom Enemy: " + EnemyType.enemyName, DebugType.Developer);

            TryCreateMatchingProperties();
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
    }
}
