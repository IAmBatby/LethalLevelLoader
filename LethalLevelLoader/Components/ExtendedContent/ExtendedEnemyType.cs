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
        public EnemyType EnemyType;
        [SerializeField] internal string enemyDisplayName;

        [Space(10)]
        [Header("Dynamic Enemy Injections Settings")]
        [SerializeField] internal LevelMatchingProperties insideLevelMatchingProperties;
        [SerializeField] internal DungeonMatchingProperties insideDungeonMatchingProperties;
        [Space(5)]
        [SerializeField] internal LevelMatchingProperties outsideLevelMatchingProperties;
        [SerializeField] internal LevelMatchingProperties daytimeLevelMatchingProperties;

        [SerializeField][TextArea(2, 20)] internal string infoNodeDescription = string.Empty;
        [SerializeField] internal VideoClip infoNodeVideoClip;

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
            Debug.Log("Initializing Custom Enemy: " + EnemyType.enemyName);

            TryCreateMatchingProperties();
        }

        internal override void TryCreateMatchingProperties()
        {
            if (insideLevelMatchingProperties == null)
                insideLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            if (insideDungeonMatchingProperties == null)
                insideDungeonMatchingProperties = ScriptableObject.CreateInstance<DungeonMatchingProperties>();
            if (outsideLevelMatchingProperties == null)
                outsideLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
            if (daytimeLevelMatchingProperties == null)
                daytimeLevelMatchingProperties = ScriptableObject.CreateInstance<LevelMatchingProperties>();
        }
    }
}
