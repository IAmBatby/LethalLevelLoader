using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class StoryLogManager : ExtendedContentManager<ExtendedStoryLog, StoryLogInfo, StoryLogManager>
    {
        protected override (bool result, string log) ValidateExtendedContent(ExtendedStoryLog extendedStoryLog)
        {
            if (string.IsNullOrEmpty(extendedStoryLog.sceneName))
                return (false, "StoryLog SceneName Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.terminalKeywordNoun))
                return (false, "StoryLog TerminalKeywordNoun Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.storyLogTitle))
                return (false, "StoryLog Title Was Null Or Empty");
            if (string.IsNullOrEmpty(extendedStoryLog.storyLogDescription))
                return (false, "StoryLog Description Was Null Or Empty");

            return (true, string.Empty);
        }
    }
}
