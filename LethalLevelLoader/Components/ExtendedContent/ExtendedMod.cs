using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<ExtendedUnlockableItem> ExtendedUnlockableItems { get; private set; } = new List<ExtendedUnlockableItem>();

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
                foreach (ExtendedUnlockableItem unlockableItem in ExtendedUnlockableItems)
                    returnList.Add(unlockableItem);

                return (returnList);
            }
        }

        public static ExtendedMod Create(string modName) => CreateNewMod(modName);  //Obsolete
        public static ExtendedMod Create(string modName, string authorName) => CreateNewMod(modName, authorName); //Obsolete
        public static ExtendedMod Create(string modName, string authorName, ExtendedContent[] extendedContents) => CreateNewMod(modName, authorName, extendedContents); //Obsolete

        public static ExtendedMod CreateNewMod(string modName = null, string authorName = null, params ExtendedContent[] contents)
        {
            ExtendedMod newExtendedMod = CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = string.IsNullOrEmpty(modName) ? newExtendedMod.ModName : modName;
            newExtendedMod.AuthorName = string.IsNullOrEmpty(authorName) ? newExtendedMod.AuthorName : authorName;
            newExtendedMod.name = modName.SkipToLetters().RemoveWhitespace() + "Mod";
            newExtendedMod.TryRegisterExtendedContents(contents);
            DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName + " by " + authorName, DebugType.Developer);
            return (newExtendedMod);
        }

        public void TryRegisterExtendedContents(params ExtendedContent[] extendedContents)
        {
            for (int i = 0; i < extendedContents.Length; i++)
                TryRegisterExtendedContent(extendedContents[i]);
        }

        public void TryRegisterExtendedContent(ExtendedContent newExtendedContent)
        {
            try
            {
                if (newExtendedContent == null)
                    throw new ArgumentNullException(nameof(newExtendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check!");
                else if (ExtendedContents.Contains(newExtendedContent))
                    throw new ArgumentException(nameof(newExtendedContent), newExtendedContent.name + " (" + newExtendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Already Being Registered To This Mod!");

                newExtendedContent.Register(this);
            }
            catch (Exception ex)
            {
                DebugHelper.LogError(ex, DebugType.User);
            }
        }

        //Remove soon hopefully, less bad than it was
        internal void RegisterExtendedContent(ExtendedContent extendedContent)
        {
            //if (extendedContent.CurrentStatus) //Do this later
            extendedContent.ExtendedMod = this;
            if (extendedContent.ContentType == ContentType.Custom)
                extendedContent.ContentTags.Add(ContentTag.Create("Custom"));

            if (extendedContent is ExtendedLevel extendedLevel)
                ExtendedLevels.Add(extendedLevel);
            else if (extendedContent is ExtendedDungeonFlow extendedDungeonFlow)
                ExtendedDungeonFlows.Add(extendedDungeonFlow);
            else if (extendedContent is ExtendedItem extendedItem)
                ExtendedItems.Add(extendedItem);
            else if (extendedContent is ExtendedEnemyType extendedEnemyType)
                ExtendedEnemyTypes.Add(extendedEnemyType);
            else if (extendedContent is ExtendedWeatherEffect extendedWeatherEffect)
                ExtendedWeatherEffects.Add(extendedWeatherEffect);
            else if (extendedContent is ExtendedStoryLog extendedStoryLog)
                ExtendedStoryLogs.Add(extendedStoryLog);
            else if (extendedContent is ExtendedFootstepSurface extendedFootstepSurface)
                ExtendedFootstepSurfaces.Add(extendedFootstepSurface);
            else if (extendedContent is ExtendedBuyableVehicle extendedBuyableVehicle)
                ExtendedBuyableVehicles.Add(extendedBuyableVehicle);
            else if (extendedContent is ExtendedUnlockableItem extendedUnlockableItem)
                ExtendedUnlockableItems.Add(extendedUnlockableItem);
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
            ExtendedUnlockableItems.Clear();
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
            ExtendedUnlockableItems.Sort((s1, s2) => s1.name.CompareTo(s2.name));
        }
    }
}
