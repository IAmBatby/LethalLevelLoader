using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLevelLoader
{
    [System.Serializable]
    public class ExtendedLevelGroup
    {
        public List<ExtendedLevel> extendedLevelsList;

        public ExtendedLevelGroup(List<ExtendedLevel> newExtendedLevelsList)
        {
            if (newExtendedLevelsList != null)
                extendedLevelsList = new List<ExtendedLevel>(newExtendedLevelsList);
            else
                newExtendedLevelsList = new List<ExtendedLevel>();
        }
    }
}
