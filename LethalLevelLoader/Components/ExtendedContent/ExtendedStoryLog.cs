using System;
using System.Collections.Generic;
using System.Text;
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

        internal override (bool result, string log) Validate()
        {
            if (string.IsNullOrEmpty(sceneName))
                return (false, "StoryLog SceneName Was Null Or Empty");
            if (string.IsNullOrEmpty(terminalKeywordNoun))
                return (false, "StoryLog TerminalKeywordNoun Was Null Or Empty");
            if (string.IsNullOrEmpty(storyLogTitle))
                return (false, "StoryLog Title Was Null Or Empty");
            if (string.IsNullOrEmpty(storyLogDescription))
                return (false, "StoryLog Description Was Null Or Empty");

            return (true, string.Empty);
        }
    }
}