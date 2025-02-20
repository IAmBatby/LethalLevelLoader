using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedStoryLog", menuName = "Lethal Level Loader/Extended Content/ExtendedStoryLog", order = 26)]
    public class ExtendedStoryLog : ExtendedContent
    {
        public string sceneName = string.Empty;
        public int storyLogID;
        [Space(5)]
        //public string terminalKeywordVerb = string.Empty;
        public string terminalKeywordNoun = string.Empty;
        [Space(5)]
        public string storyLogTitle = string.Empty;
        [TextArea] public string storyLogDescription = string.Empty;

        [HideInInspector] internal int newStoryLogID;

        internal override void Register(ExtendedMod extendedMod)
        {
            base.Register(extendedMod);

            extendedMod.ExtendedStoryLogs.Add(this);
        }
    }
}
