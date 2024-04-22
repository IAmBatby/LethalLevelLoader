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

                return (returnList);
            }
        }

        internal static ExtendedMod Create(string modName)
        {
            ExtendedMod newExtendedMod = ScriptableObject.CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = modName;
            newExtendedMod.name = modName.Sanitized() + "Mod";
            DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName, DebugType.Developer);
            return (newExtendedMod);
        }

        public static ExtendedMod Create(string modName, string authorName)
        {
            ExtendedMod newExtendedMod = ScriptableObject.CreateInstance<ExtendedMod>();
            newExtendedMod.ModName = modName;
            newExtendedMod.name = modName.SkipToLetters().RemoveWhitespace() + "Mod";
            newExtendedMod.AuthorName = authorName;
            if (Plugin.Instance != null)
                DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName + " by " + authorName, DebugType.Developer);
            return (newExtendedMod);
        }

        public static ExtendedMod Create(string modName, string authorName, ExtendedContent[] extendedContents)
        {
            ExtendedMod newExtendedMod = ScriptableObject.CreateInstance<ExtendedMod>();
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
                    if (newExtendedContent is ExtendedLevel extendedLevel)
                        RegisterExtendedContent(extendedLevel);
                    else if (newExtendedContent is ExtendedDungeonFlow extendedDungeonFlow)
                        RegisterExtendedContent(extendedDungeonFlow);
                    else if (newExtendedContent is ExtendedItem extendedItem)
                        RegisterExtendedContent(extendedItem);
                    else if (newExtendedContent is ExtendedEnemyType extendedEnemyType)
                        RegisterExtendedContent(extendedEnemyType);
                    else if (newExtendedContent is ExtendedWeatherEffect extendedWeatherEffect)
                        RegisterExtendedContent(extendedWeatherEffect);
                    else if (newExtendedContent is ExtendedFootstepSurface extendedFootstepSurface)
                        RegisterExtendedContent(extendedFootstepSurface);
                    else if (newExtendedContent is ExtendedStoryLog extendedStoryLog)
                        RegisterExtendedContent(extendedStoryLog);
                    else
                        throw new ArgumentException(nameof(newExtendedContent), newExtendedContent.name + " (" + newExtendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Unimplemented Registration Check!");
                }
                else
                    throw new ArgumentException(nameof(newExtendedContent), newExtendedContent.name + " (" + newExtendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Already Being Registered To This Mod!");
            }
            else
                throw new ArgumentNullException(nameof(newExtendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check!");
        }

        internal void RegisterExtendedContent(ExtendedLevel extendedLevel)
        {
            extendedLevel.ConvertObsoleteValues();
            TryThrowInvalidContentException(extendedLevel, Validators.ValidateExtendedContent(extendedLevel));

            ExtendedLevels.Add(extendedLevel);
            extendedLevel.ContentTags.Add(ContentTag.Create("Custom"));
            extendedLevel.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedDungeonFlow extendedDungeonFlow)
        {
            extendedDungeonFlow.ConvertObsoleteValues();
            TryThrowInvalidContentException(extendedDungeonFlow, Validators.ValidateExtendedContent(extendedDungeonFlow));

            ExtendedDungeonFlows.Add(extendedDungeonFlow);
            extendedDungeonFlow.ContentTags.Add(ContentTag.Create("Custom"));
            extendedDungeonFlow.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedItem extendedItem)
        {
            TryThrowInvalidContentException(extendedItem, Validators.ValidateExtendedContent(extendedItem));

            ExtendedItems.Add(extendedItem);
            extendedItem.ContentTags.Add(ContentTag.Create("Custom"));
            extendedItem.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedEnemyType extendedEnemyType)
        {
            TryThrowInvalidContentException(extendedEnemyType, Validators.ValidateExtendedContent(extendedEnemyType));

            ExtendedEnemyTypes.Add(extendedEnemyType);
            extendedEnemyType.ContentTags.Add(ContentTag.Create("Custom"));
            extendedEnemyType.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedWeatherEffect extendedWeatherEffect)
        {
            TryThrowInvalidContentException(extendedWeatherEffect, Validators.ValidateExtendedContent(extendedWeatherEffect));

            ExtendedWeatherEffects.Add(extendedWeatherEffect);
            extendedWeatherEffect.ContentTags.Add(ContentTag.Create("Custom"));
            extendedWeatherEffect.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedFootstepSurface extendedFootstepSurface)
        {
            TryThrowInvalidContentException(extendedFootstepSurface, Validators.ValidateExtendedContent(extendedFootstepSurface));

            ExtendedFootstepSurfaces.Add(extendedFootstepSurface);
            extendedFootstepSurface.ContentTags.Add(ContentTag.Create("Custom"));
            extendedFootstepSurface.ExtendedMod = this;
        }

        internal void RegisterExtendedContent(ExtendedStoryLog extendedStoryLog)
        {
            TryThrowInvalidContentException(extendedStoryLog, Validators.ValidateExtendedContent(extendedStoryLog));

            ExtendedStoryLogs.Add(extendedStoryLog);
            extendedStoryLog.ContentTags.Add(ContentTag.Create("Custom"));
            extendedStoryLog.ExtendedMod = this;
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
            if (currentExtendedContent is ExtendedLevel extendedLevel)
                ExtendedLevels.Remove(extendedLevel);
            else if (currentExtendedContent is ExtendedDungeonFlow extendedDungeonFlow)
                ExtendedDungeonFlows.Remove(extendedDungeonFlow);
            else if (currentExtendedContent is ExtendedItem extendedItem)
                ExtendedItems.Remove(extendedItem);

            currentExtendedContent.ExtendedMod = null;
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
        }

        internal void SortRegisteredContent()
        {
            //ExtendedLevels.Sort((s1, s2) => s1.name.CompareTo(s2.name)); 
            ExtendedDungeonFlows.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedItems.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedEnemyTypes.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedWeatherEffects.Sort((s1, s2) => s1.name.CompareTo(s2.name));
            ExtendedFootstepSurfaces.Sort((s1, s2) => s1.name.CompareTo(s2.name));
        }

        internal void Example()
        {
            AssetBundle assetBundle = null;

            ExtendedDungeonFlow myMainExtendedDungeonFlow = assetBundle.LoadAsset<ExtendedDungeonFlow>("Assets/CoolDungeonFlow");

            ExtendedEnemyType mySpookyEnemyType = assetBundle.LoadAsset<ExtendedEnemyType>("Assets/Ghost");

            ExtendedMod extendedMod = ExtendedMod.Create("BatbysMod", "IAmBatby", new ExtendedContent[2] { myMainExtendedDungeonFlow, mySpookyEnemyType });

            LethalLevelLoader.PatchedContent.RegisterExtendedMod(extendedMod);
        }
    }
}
