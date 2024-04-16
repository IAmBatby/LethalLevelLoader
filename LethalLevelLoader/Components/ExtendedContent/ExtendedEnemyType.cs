using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedEnemyType")]
    public class ExtendedEnemyType : ExtendedContent
    {
        [field: SerializeField] public EnemyType EnemyType { get; set; }
        [field: SerializeField] public string EnemyDisplayName { get; set; }

        [field: SerializeField] public LevelMatchingProperties InsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public DungeonMatchingProperties InsideDungeonMatchingProperties { get; set; }
        [field: SerializeField] public LevelMatchingProperties OutsideLevelMatchingProperties { get; set; }
        [field: SerializeField] public LevelMatchingProperties DaytimeLevelMatchingProperties { get; set; }

        [field: SerializeField] public string InfoNodeDescription { get; set; } = string.Empty;
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
            DebugHelper.Log("Initializing Custom Enemy: " + EnemyType.enemyName);

            TryCreateMatchingProperties();
        }

        internal override void TryCreateMatchingProperties()
        {
            if (InsideLevelMatchingProperties == null)
                InsideLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            if (InsideDungeonMatchingProperties == null)
                InsideDungeonMatchingProperties = ScriptableObject.CreateInstance<DungeonMatchingProperties>();
            if (OutsideLevelMatchingProperties == null)
                OutsideLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            if (DaytimeLevelMatchingProperties == null)
                DaytimeLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
        }
    }
}
