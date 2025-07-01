using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    public class StoryLogManager : ExtendedContentManager<ExtendedStoryLog, StoryLogInfo>
    {
        protected override List<StoryLogInfo> GetVanillaContent() => new List<StoryLogInfo>();
        protected override ExtendedStoryLog ExtendVanillaContent(StoryLogInfo content) => null;

        protected override void PatchGame()
        {
            DebugHelper.Log(GetType().Name + " Patching Game!", DebugType.User);

            foreach (ExtendedStoryLog log in ExtendedContents)
            {
                if (log.StoryLogKeyword == null || log.StoryLogNode == null) continue;

                if (!Terminal.logEntryFiles.Contains(log.StoryLogNode))
                    Terminal.logEntryFiles.Add(log.StoryLogNode);

                log.SetGameID(Terminal.logEntryFiles.IndexOf(log.StoryLogNode));

                if (!TerminalManager.Keywords.View.Contains(log.StoryLogKeyword, log.StoryLogNode))
                    TerminalManager.Keywords.View.AddNoun(log.StoryLogKeyword,log.StoryLogNode);
            }

        }

        protected override void UnpatchGame()
        {
            DebugHelper.Log(GetType().Name + " Unpatching Game!", DebugType.User);
        }

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

        protected override void PopulateContentTerminalData(ExtendedStoryLog content)
        {
            TerminalKeyword keyword = null;
            TerminalNode node = null;
            if (Terminal.logEntryFiles.Count > content.GameID)
            {
                node = Terminal.logEntryFiles[content.GameID];
                foreach (CompatibleNoun noun in TerminalManager.Keywords.View.compatibleNouns)
                    if (noun.result == node)
                    {
                        keyword = noun.noun;
                        break;
                    }
            }
            else
            {
                keyword = TerminalManager.CreateNewTerminalKeyword(content.terminalKeywordNoun + "Keyword", content.terminalKeywordNoun, TerminalManager.Keywords.View);
                node = TerminalManager.CreateNewTerminalNode("LogFile" + (Terminal.logEntryFiles.Count + 1), content.storyLogDescription);
                node.clearPreviousText = true;
                node.creatureName = content.storyLogTitle;
            }

            content.StoryLogKeyword = keyword;
            content.StoryLogNode = node;
        }
    }
}
