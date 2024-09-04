using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

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

        private Dictionary<Type, IList> _extendedContentListsDict;
        private Dictionary<Type, IList> ExtendedContentsLists
        {
            get
            {
                if (_extendedContentListsDict == null)
                    PopulateExtendedContentsListDict();
                return (_extendedContentListsDict);
            }
        }

        private ContentTag customTag;

        internal static ExtendedMod Create(string modName = null, string authorName = null, ExtendedContent[] extendedContents = null)
        {
            ExtendedMod newExtendedMod = CreateInstance<ExtendedMod>();

            if (!string.IsNullOrEmpty(modName))
                newExtendedMod.ModName = modName;
            if (!string.IsNullOrEmpty(authorName))
                newExtendedMod.AuthorName = authorName;
            if (extendedContents != null)
                foreach (ExtendedContent content in extendedContents)
                    newExtendedMod.ExtendedContentsLists[content.GetType()].Add(content);

            newExtendedMod.name = newExtendedMod.ModName.SkipToLetters().RemoveWhitespace() + "Mod";

            if (Plugin.Instance != null)
                DebugHelper.Log("Created New ExtendedMod: " + newExtendedMod.ModName + " by " + newExtendedMod.AuthorName, DebugType.Developer);

            return (newExtendedMod);
        }

        internal void RegisterExtendedContent(ExtendedContent newExtendedContent)
        {
            if (newExtendedContent == null)
                throw new ArgumentNullException(nameof(newExtendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check!");
            if (ExtendedContents.Contains(newExtendedContent))
                throw new ArgumentException(nameof(newExtendedContent), newExtendedContent.name + " (" + newExtendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Already Being Registered To This Mod!");

            ProcessExtendedContent(newExtendedContent);
        }

        private void ProcessExtendedContent(ExtendedContent newExtendedContent)
        {
            newExtendedContent.TryRecoverObsoleteValues();
            TryThrowInvalidContentException(newExtendedContent, newExtendedContent.Validate());
            newExtendedContent.ContentTags.Add(GetOrCreateCustomTag());
            AddExtendedContent(newExtendedContent);
            newExtendedContent.ExtendedMod = this;
            DebugHelper.Log("Successfully Registed ExtendedContent: " + newExtendedContent.UniqueIdentificationName, DebugType.User);
        }

        private void AddExtendedContent(ExtendedContent newExtendedContent)
        {
            ExtendedContentsLists[newExtendedContent.GetType()].Add(newExtendedContent); //This looks unsafe but I'm happy for this to hard crash because this needs to go right.
        }

        internal ContentTag GetOrCreateCustomTag()
        {
            if (customTag == null)
                customTag = ContentTag.Create<ContentTag>("Custom", Color.white);
            return (customTag);
        }

        internal void TryThrowInvalidContentException(ExtendedContent extendedContent, (bool, string) result)
        {
            if (result.Item1 == false && extendedContent == null)
                throw new ArgumentNullException(nameof(extendedContent), "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check! " + result.Item2);
            else if (result.Item1 == false)
                throw new ArgumentException(nameof(extendedContent), extendedContent.name + " (" + extendedContent.GetType().Name + ") " + " Could Not Be Registered To ExtendedMod: " + ModName + " Due To Failed Validation Check! " + result.Item2);
        }

        internal void UnregisterExtendedContent(ExtendedContent currentExtendedContent)
        {
            ExtendedContentsLists[currentExtendedContent.GetType()].Remove(currentExtendedContent);
            //ExtendedContents.Remove(currentExtendedContent);
            currentExtendedContent.ExtendedMod = null;
            DebugHelper.LogWarning("Unregistered ExtendedContent: " + currentExtendedContent.name + " In ExtendedMod: " + ModName, DebugType.Developer);
        }

        internal List<T> GetExtendedContentList<T>()
        {
            ExtendedContentsLists.TryGetValue(typeof(T), out IList value);
            return (value as List<T>);
        }

        internal void PopulateExtendedContentsListDict()
        {
            _extendedContentListsDict = new Dictionary<Type, IList>();
            AddExtendedContentsListToDict(ExtendedLevels);
            AddExtendedContentsListToDict(ExtendedDungeonFlows);
            AddExtendedContentsListToDict(ExtendedItems);
            AddExtendedContentsListToDict(ExtendedEnemyTypes);
            AddExtendedContentsListToDict(ExtendedWeatherEffects);
            AddExtendedContentsListToDict(ExtendedStoryLogs);
            AddExtendedContentsListToDict(ExtendedFootstepSurfaces);
            AddExtendedContentsListToDict(ExtendedBuyableVehicles);
        }

        internal void AddExtendedContentsListToDict<T>(List<T> extendedContentsList) where T : ExtendedContent
        {
            _extendedContentListsDict.Add(typeof(T), extendedContentsList);
        }


        internal void ClearAllExtendedContent()
        {
            foreach (IList extendedContentList in ExtendedContentsLists.Values)
                extendedContentList.Clear();
        }

        internal void SortRegisteredContent()
        {
            foreach (IList extendedContentList in ExtendedContentsLists.Values)
                if (extendedContentList is List<ExtendedContent> castedList)
                    castedList.Sort((s1, s2) => s1.name.CompareTo(s2.name));
        }
    }
}
