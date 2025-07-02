using System.Collections.Generic;
using System.Linq;

namespace LethalLevelLoader
{
    public class MoonsCataloguePage
    {
        public List<ExtendedLevelGroup> ExtendedLevelGroups { get; private set; } = new List<ExtendedLevelGroup>();

        public List<ExtendedLevel> ExtendedLevels => ExtendedLevelGroups.SelectMany(g => g.Levels).ToList();

        public MoonsCataloguePage(List<ExtendedLevelGroup> newExtendedLevelGroupList) => RefreshLevelGroups(newExtendedLevelGroupList);

        public void RebuildLevelGroups(List<ExtendedLevelGroup> newExtendedLevelGroups, int splitCount) => RebuildLevelGroups(ExtendedLevelGroups.SelectMany(g => g.Levels).ToList(), splitCount);
        public void RebuildLevelGroups(List<ExtendedLevel> newExtendedLevels, int splitCount) => RebuildLevelGroups(newExtendedLevels.ToArray(), splitCount);
        public void RebuildLevelGroups(IOrderedEnumerable<ExtendedLevel> orderedExtendedLevels, int splitCount) => RebuildLevelGroups(orderedExtendedLevels.ToArray(), splitCount);
        public void RebuildLevelGroups(ExtendedLevel[] newExtendedLevels, int splitCount) => ExtendedLevelGroups = TerminalManager.GetExtendedLevelGroups(newExtendedLevels, splitCount);

        public void RefreshLevelGroups(List<ExtendedLevelGroup> newLevelGroups) => ExtendedLevelGroups = new List<ExtendedLevelGroup>(newLevelGroups.Select(group => new ExtendedLevelGroup(group.Levels)));
    }

    [System.Serializable]
    public class ExtendedLevelGroup
    {
        public List<ExtendedLevel> Levels { get; private set; }

        public int AverageCalculatedDifficulty => ((int)Levels.Select(l => l.CalculatedDifficultyRating).Average());

        public ExtendedLevelGroup(List<ExtendedLevel> newExtendedLevelsList) => Levels = new List<ExtendedLevel>(newExtendedLevelsList);
        public ExtendedLevelGroup(List<SelectableLevel> newSelectableLevelsList) => Levels = new List<ExtendedLevel>(newSelectableLevelsList.Select(s => s.AsExtended()));
    }

}
