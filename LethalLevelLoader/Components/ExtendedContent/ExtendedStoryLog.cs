using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(fileName = "ExtendedStoryLog", menuName = "Lethal Level Loader/Extended Content/ExtendedStoryLog", order = 26)]
    public class ExtendedStoryLog : ExtendedContent<ExtendedStoryLog,StoryLogInfo,StoryLogManager>
    {
        public override RestorationPeriod RestorationPeriod => RestorationPeriod.Lobby;
        public override StoryLogInfo Content
        {
            get
            {
                if (Info == null)
                    Info = new StoryLogInfo(sceneName, storyLogID);
                return (Info);
            }
        }

        public StoryLogInfo Info { get; private set; }
        public string sceneName = string.Empty;
        public int storyLogID;
        [Space(5)]
        //public string terminalKeywordVerb = string.Empty;
        public string terminalKeywordNoun = string.Empty;
        [Space(5)]
        public string storyLogTitle = string.Empty;
        [TextArea] public string storyLogDescription = string.Empty;

        internal int StoryLogID => GameID;

        public TerminalKeyword StoryLogKeyword { get; internal set; }
        public TerminalNode StoryLogNode { get; internal set; }

        protected override void OnGameIDChanged()
        {
            if (StoryLogNode != null) StoryLogNode.storyLogFileID = GameID;
        }

        internal override List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration() => NoPrefabReferences;
        internal override List<GameObject> GetNetworkPrefabsForRegistration() => NoNetworkPrefabs;
    }

    public class StoryLogInfo
    {
        public string SceneName { get; private set; } = string.Empty;
        public int ID { get; private set; } = -1;

        public StoryLogInfo(string sceneName, int id)
        {
            SceneName = sceneName;
            ID = id;
        }
    }
}
