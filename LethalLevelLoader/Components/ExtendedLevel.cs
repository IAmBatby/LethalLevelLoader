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
        [Space(5)] public string contentSourceName = string.Empty; //Levels from AssetBundles will have this as their Assembly Name.
        [Space(5)] public SelectableLevel selectableLevel;
        [Space(5)] [SerializeField] private int routePrice = 0;
        [Space(5)] public bool isHidden = false;
        [Space(5)] public bool isLocked = false;
        [Space(5)] public string lockedNodeText = string.Empty;

        [Space(10)] public List<StoryLogData> storyLogs = new List<StoryLogData>();

        public int RoutePrice
        {
            get
            {
                if (routeNode != null)
                {
                    routePrice = routeNode.itemCost;
                    routeConfirmNode.itemCost = routePrice;
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
                routeNode.itemCost = value;
                routeConfirmNode.itemCost = value;
                routePrice = value;
            }
        }

        [Space(10)]
        [Header("Dynamic DungeonFlow Injections Settings")]
        [Space(5)] public ContentType allowedDungeonContentTypes = ContentType.Any;
        [Space(5)] public List<string> levelTags = new List<string>();

        [HideInInspector] public ContentType levelType;
        [HideInInspector] public string NumberlessPlanetName => GetNumberlessPlanetName(selectableLevel);

        [SerializeField][TextArea] internal string infoNodeDescripton = string.Empty;
        [HideInInspector] internal TerminalNode routeNode;
        [HideInInspector] internal TerminalNode routeConfirmNode;

        [Space(10)]
        [Header("Misc. Settings")]
        [Space(5)] public bool generateAutomaticConfigurationOptions = true;

        internal static ExtendedLevel Create(SelectableLevel newSelectableLevel, ContentType newContentType)
        {
            ExtendedLevel newExtendedLevel = ScriptableObject.CreateInstance<ExtendedLevel>();

            newExtendedLevel.levelType = newContentType;
            newExtendedLevel.selectableLevel = newSelectableLevel;

            return (newExtendedLevel);
        }
        internal void Initialize(string newContentSourceName, bool generateTerminalAssets)
        {
            DebugHelper.Log("Initializing Extended Level For Moon: " + GetNumberlessPlanetName(selectableLevel));
            DebugHelper.extendedLevelLogReports.Add(this, new ExtendedLevelLogReport(this));

            if (contentSourceName == string.Empty)
                contentSourceName = newContentSourceName;

            if (levelType == ContentType.Custom)
            {
                levelTags.Add("Custom");
                selectableLevel.levelID = PatchedContent.ExtendedLevels.Count;
            }

            if (generateTerminalAssets == true) //Needs to be after levelID setting above.
                Terminal_Patch.CreateLevelTerminalData(this, routePrice);
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

[System.Serializable]
public class StoryLogData
{
    public int storyLogID;
    public string terminalWord = string.Empty;
    public string storyLogTitle = string.Empty;
    [TextArea] public string storyLogDescription = string.Empty;

    [HideInInspector] internal int newStoryLogID;
}
