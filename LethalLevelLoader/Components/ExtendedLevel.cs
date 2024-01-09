using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ContentType { Vanilla, Custom, Any } //Any & All included for built in checks.

namespace LethalLevelLoader
{
    [CreateAssetMenu(menuName = "LethalLevelLoader/ExtendedLevel")]
    public class ExtendedLevel : ScriptableObject
    {
        [Header("Extended Level Settings")]
        [Space(5)]
        public string contentSourceName = "Lethal Company"; //Levels from AssetBundles will have this as their Assembly Name.
        [Space(5)]
        public SelectableLevel selectableLevel;
        public GameObject levelPrefab;
        [Space(5)]
        public int routePrice = 0;

        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        [Space(5)] public ContentType allowedDungeonContentTypes = ContentType.Any;
        [Space(5)] public List<string> levelTags = new List<string>();

        [HideInInspector] public ContentType levelType;
        [HideInInspector] public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);

        internal void Initialize(ContentType newLevelType, SelectableLevel newSelectableLevel = null, int newRoutePrice = 0, bool generateTerminalAssets = false, GameObject newLevelPrefab = null, string newSourceName = "Lethal Company")
        {
            DebugHelper.Log("Creating New Extended Level For Moon: " + ExtendedLevel.GetNumberlessPlanetName(newSelectableLevel));

            if (selectableLevel == null)
                selectableLevel = newSelectableLevel;

            if (contentSourceName != newSourceName)
                contentSourceName = newSourceName;


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

        internal static string GetNumberlessPlanetName(SelectableLevel selectableLevel)
        {
            if (selectableLevel != null)
                return new string(selectableLevel.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
            else
                return string.Empty;
        }
    }
}