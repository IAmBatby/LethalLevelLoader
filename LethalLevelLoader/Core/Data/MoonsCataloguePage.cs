using System.Collections.Generic;
using System.Linq;

namespace LethalLevelLoader
{
    public class MoonsCataloguePage
    {
        private List<ExtendedLevelGroup> extendedLevelGroups;
        public List<ExtendedLevelGroup> ExtendedLevelGroups => extendedLevelGroups;

        public List<ExtendedLevel> ExtendedLevels
        {
            get
            {
                List<ExtendedLevel> returnList = new List<ExtendedLevel>();
                foreach (ExtendedLevelGroup group in extendedLevelGroups)
                    foreach (ExtendedLevel level in group.extendedLevelsList)
                        returnList.Add(level);

                return (returnList);
            }
        }

        public MoonsCataloguePage(List<ExtendedLevelGroup> newExtendedLevelGroupList)
        {
            extendedLevelGroups = new List<ExtendedLevelGroup>();
            extendedLevelGroups.Clear();

            foreach (ExtendedLevelGroup newExtendedLevelGroup in newExtendedLevelGroupList)
                extendedLevelGroups.Add(new ExtendedLevelGroup(newExtendedLevelGroup.extendedLevelsList));
        }

        public void RebuildLevelGroups(List<ExtendedLevelGroup> newExtendedLevelGroups, int splitCount)
        {
            List<ExtendedLevel> converteredList = new List<ExtendedLevel>();
            foreach (ExtendedLevelGroup extendedLevelGroup in extendedLevelGroups)
                foreach (ExtendedLevel level in extendedLevelGroup.extendedLevelsList)
                    converteredList.Add(level);
            RebuildLevelGroups(converteredList.ToArray(), splitCount);
        }

        public void RebuildLevelGroups(List<ExtendedLevel> newExtendedLevels, int splitCount)
        {
            RebuildLevelGroups(newExtendedLevels.ToArray(), splitCount);
        }

        public void RebuildLevelGroups(IOrderedEnumerable<ExtendedLevel> orderedExtendedLevels, int splitCount)
        {
            RebuildLevelGroups(orderedExtendedLevels.ToArray(), splitCount);
        }

        public void RebuildLevelGroups(ExtendedLevel[] newExtendedLevels, int splitCount)
        {
            extendedLevelGroups = TerminalManager.GetExtendedLevelGroups(newExtendedLevels, splitCount);
        }

        public void RefreshLevelGroups(List<ExtendedLevelGroup> newLevelGroups)
        {
            extendedLevelGroups.Clear();
            foreach (ExtendedLevelGroup group in newLevelGroups)
                if (group.extendedLevelsList.Count != 0)
                    extendedLevelGroups.Add(new ExtendedLevelGroup(group.extendedLevelsList));
        }
    }

    [System.Serializable]
    public class ExtendedLevelGroup
    {
        public List<ExtendedLevel> extendedLevelsList;

        public int AverageCalculatedDifficulty => GetAverageCalculatedDifficulty();

        public ExtendedLevelGroup(List<ExtendedLevel> newExtendedLevelsList)
        {
            extendedLevelsList = new List<ExtendedLevel>(newExtendedLevelsList);
        }

        public ExtendedLevelGroup(List<SelectableLevel> newSelectableLevelsList)
        {
            extendedLevelsList = new List<ExtendedLevel>();
            foreach (SelectableLevel level in newSelectableLevelsList)
                extendedLevelsList.Add(LevelManager.GetExtendedLevel(level));
        }

        public int GetAverageCalculatedDifficulty()
        {
            List<int> calculatedDifficulties = new List<int>();
            foreach (ExtendedLevel level in extendedLevelsList)
                calculatedDifficulties.Add(level.CalculatedDifficultyRating);
            return ((int)calculatedDifficulties.Average());
        }
    }

}
