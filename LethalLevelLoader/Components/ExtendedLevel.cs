using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ContentType { Vanilla, Custom, Any } //Any & All included for built in checks.

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLib/ExtendedLevel")]
    public class ExtendedLevel : ScriptableObject
    {
        public SelectableLevel selectableLevel;
        public GameObject levelPrefab;

        public ContentType levelType;
        public string sourceName = "Lethal Company"; //Levels from AssetBundles will have this as their Assembly Name.
        public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);
        public int routePrice = 0;
        public ContentType allowedDungeonTypes = ContentType.Any;
        public List<string> levelTags = new List<string>();

        public void Initialize(ContentType newLevelType, SelectableLevel newSelectableLevel = null, int newRoutePrice = 0, bool generateTerminalAssets = false, GameObject newLevelPrefab = null, string newSourceName = "Lethal Company")
        {
            DebugHelper.Log("Creating New Extended Level For Moon: " + ExtendedLevel.GetNumberlessPlanetName(newSelectableLevel));

            if (selectableLevel == null)
                selectableLevel = newSelectableLevel;

            if (sourceName != newSourceName)
                sourceName = newSourceName;


            levelType = newLevelType;

            if (routePrice == 0)
                routePrice = newRoutePrice;

            if (levelType == ContentType.Custom)
            {
                levelTags.Add("Custom");
                selectableLevel.levelID = SelectableLevel_Patch.allLevelsList.Count;
                selectableLevel.sceneName = SelectableLevel_Patch.injectionSceneName;
            }

            if (generateTerminalAssets == true) //Needs to be after levelID setting above.
                Terminal_Patch.CreateLevelTerminalData(this);
        }

        public static string GetNumberlessPlanetName(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null)
                return new string(selectableLevel.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
            else
                return string.Empty;
        }
    }
}