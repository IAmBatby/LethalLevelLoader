using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedStoryLog")]
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
    }
}
