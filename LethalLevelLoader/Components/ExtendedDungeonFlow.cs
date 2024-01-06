using DunGen.Graph;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLib/ExtendedDungeonFlow")]
    public class ExtendedDungeonFlow : ScriptableObject
    {
        public DungeonFlow dungeonFlow;
        public int dungeonID;
        public int dungeonRarity;

        public ContentType dungeonType;
        public string sourceName = "Lethal Company";
        public AudioClip dungeonFirstTimeAudio;

        public ExtendedDungeonPreferences extendedDungeonPreferences;



        public void Initialize(DungeonFlow newDungeonFlow, AudioClip newFirstTimeAudio, ContentType newDungeonType, string newSourceName, int newDungeonRarity = 0, ExtendedDungeonPreferences newPreferences = null)
        {
            dungeonFlow = newDungeonFlow;
            dungeonFirstTimeAudio = newFirstTimeAudio;
            dungeonType = newDungeonType;
            dungeonRarity = newDungeonRarity;

            if (name == string.Empty)
                name = dungeonFlow.name;

            //dungeonID = RoundManager.Instance.dungeonFlowTypes.Length + DungeonFlow_Patch.allExtendedDungeonsList.Count;

            if (extendedDungeonPreferences == null)
                extendedDungeonPreferences = ScriptableObject.CreateInstance<ExtendedDungeonPreferences>();
        }
    }
}
