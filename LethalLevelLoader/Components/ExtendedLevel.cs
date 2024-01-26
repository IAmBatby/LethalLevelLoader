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
        [Space(5)]
        [SerializeField] private int routePrice = 0;

        public int RoutePrice
        {
            get
            {
                if (routeNode != null)
                {
                    routePrice = routeNode.itemCost;
                    return (routeNode.itemCost);
                }
                else
                {
                    DebugHelper.Log("routeNode Is Missing! Using internal value!");
                    return (routePrice);
                }
            }
            set
            {
                if (routeNode != null)
                    routeNode.itemCost = value;
                routePrice = value;
            }
        }

        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        [Space(5)] public ContentType allowedDungeonContentTypes = ContentType.Any;
        [Space(5)] public List<string> levelTags = new List<string>();

        [HideInInspector] public ContentType levelType;
        [HideInInspector] public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);

        [HideInInspector] internal TerminalNode routeNode;

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
                selectableLevel.levelID = PatchedContent.ExtendedLevels.Count;
            }

            if (generateTerminalAssets == true) //Needs to be after levelID setting above.
            {
                Terminal_Patch.CreateLevelTerminalData(this, routePrice, out TerminalNode newRouteNode);
                routeNode = newRouteNode;
            }

            if (routeNode != null)
                routePrice = routeNode.itemCost;
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