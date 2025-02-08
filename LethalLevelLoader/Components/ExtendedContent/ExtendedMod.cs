using System;
using System.Collections.Generic;
using UnityEngine;

namespace LethalLevelLoader
{
    public enum ModMergeSetting { MatchingAuthorName, MatchingModName, Disabled }
    [CreateAssetMenu(fileName = "ExtendedMod", menuName = "Lethal Level Loader/ExtendedMod", order = 30)]
    public class ExtendedMod : ScriptableObject
    {
        [field: SerializeField] public string ModName { get; internal set; } = "Unspecified";
        [field: SerializeField] public string AuthorName { get; internal set; } = "Unknown";
        public List<string> ModNameAliases { get; internal set; } = new List<string>();
        [field: SerializeField] public ModMergeSetting ModMergeSetting { get; internal set; } = ModMergeSetting.MatchingAuthorName;

        [field: SerializeField]
        public List<ExtendedLevel> ExtendedLevels { get; private set; } = new List<ExtendedLevel>();

        [field: SerializeField]
        public List<ExtendedDungeonFlow> ExtendedDungeonFlows { get; private set; } = new List<ExtendedDungeonFlow>();

        [field: SerializeField]
        public List<ExtendedItem> ExtendedItems { get; private set; } = new List<ExtendedItem>();

        [field: SerializeField]
        public List<ExtendedEnemyType> ExtendedEnemyTypes { get; private set; } = new List<ExtendedEnemyType>();

        [field: SerializeField]
        public List<ExtendedWeatherEffect> ExtendedWeatherEffects { get; private set; } = new List<ExtendedWeatherEffect>();

        [field: SerializeField]
        public List<ExtendedFootstepSurface> ExtendedFootstepSurfaces { get; private set; } = new List<ExtendedFootstepSurface>();

        [field: SerializeField]
        public List<ExtendedStoryLog> ExtendedStoryLogs { get; private set; } = new List<ExtendedStoryLog>();

        [field: SerializeField]
        public List<ExtendedBuyableVehicle> ExtendedBuyableVehicles { get; private set; } = new List<ExtendedBuyableVehicle>();

        [field: SerializeField]
        public List<string> StreamingLethalBundleNames { get; private set; } = new List<string>();

        public List<ExtendedContent> ExtendedContents
        {
            get
            {
                List<ExtendedContent> returnList = new List<ExtendedContent>();
                foreach (ExtendedLevel level in ExtendedLevels)
                    returnList.Add(level);
                foreach (ExtendedDungeonFlow flow in ExtendedDungeonFlows)
                    returnList.Add(flow);
                foreach (ExtendedItem item in ExtendedItems)
                    returnList.Add(item);
                foreach (ExtendedEnemyType type in ExtendedEnemyTypes)
                    returnList.Add(type);
                foreach (ExtendedWeatherEffect weatherEffect in ExtendedWeatherEffects)
                    returnList.Add(weatherEffect);
                foreach (ExtendedFootstepSurface surface in ExtendedFootstepSurfaces)
                    returnList.Add(surface);
                foreach (ExtendedStoryLog storyLog in ExtendedStoryLogs)
                    returnList.Add(storyLog);
                foreach (ExtendedBuyableVehicle vehicle in ExtendedBuyableVehicles)
                    returnList.Add(vehicle);

                return (returnList);
            }
        }

        internal static ExtendedMod Create(string modName)
        {
            ExtendedMod newExtendedMod = CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = modName;
            newExtendedMod.name = modName.Sanitized() + "Mod";
            DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName, DebugType.Developer);
            return (newExtendedMod);
        }

        public static ExtendedMod Create(string modName, string authorName)
        {
            ExtendedMod newExtendedMod = CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = modName;
            newExtendedMod.name = modName.SkipToLetters().RemoveWhitespace() + "Mod";
            newExtendedMod.AuthorName = authorName;
            if (Plugin.Instance != null)
                DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName + " by " + authorName, DebugType.Developer);
            return (newExtendedMod);
        }

        public static ExtendedMod Create(string modName, string authorName, ExtendedContent[] extendedContents)
        {
            ExtendedMod newExtendedMod = CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = modName;
            newExtendedMod.name = modName.SkipToLetters().RemoveWhitespace() + "Mod";
            newExtendedMod.AuthorName = authorName;

            foreach (ExtendedContent extendedContent in extendedContents)
                newExtendedMod.RegisterExtendedContent(extendedContent);

            if (Plugin.Instance != null)
                DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName + " by " + authorName, DebugType.Developer);

            return (newExtendedMod);
        }

        internal void RegisterExtendedContent(ExtendedContent newExtendedContent)
        {
            if (newExtendedContent != null)
            {
                if (!ExtendedContents.Contains(newExtendedContent))
                {
                    newExtendedContent.Register(this);
                }
                else
                    throw new ArgumentException(nameof(newExtendedContent), newExtendedContent.name + " (" + newExtendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Already Being Registered To This Mod!");
            }
            else
                throw new ArgumentNullException(nameof(newExtendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check!");
        }
        internal void TryThrowInvalidContentException(ExtendedContent extendedContent, (bool,string) result)
        {
            if (result.Item1 == false)
            {
                if (extendedContent == null)
                    throw new ArgumentNullException(nameof(extendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check! " + result.Item2);

                throw new ArgumentException(nameof(extendedContent), extendedContent.name + " (" + extendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check! " + result.Item2);
            }
        }

        internal void UnregisterExtendedContent(ExtendedContent currentExtendedContent)
        {
            currentExtendedContent.Unregister(this);
            DebugHelper.LogWarning("Unregistered ExtendedContent: " + currentExtendedContent.name + " In ExtendedMod: " + ModName, DebugType.Developer);
        }

        internal void UnregisterAllExtendedContent()
        {
            ExtendedLevels.Clear();
            ExtendedDungeonFlows.Clear();
            ExtendedItems.Clear();
            ExtendedEnemyTypes.Clear();
            ExtendedWeatherEffects.Clear();
            ExtendedFootstepSurfaces.Clear();
            ExtendedStoryLogs.Clear();
            ExtendedBuyableVehicles.Clear();
        }

        internal void SortRegisteredContent()
        {
            ExtendedLevels.Sort((s1, s2) => s1.name.CompareTo(s2.name)); 
            ExtendedDungeonFlows.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedItems.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedEnemyTypes.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedWeatherEffects.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedFootstepSurfaces.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedStoryLogs.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedBuyableVehicles.Sort((s1, s2) => s1.name.CompareTo(s2.name));
        }
    }
}
