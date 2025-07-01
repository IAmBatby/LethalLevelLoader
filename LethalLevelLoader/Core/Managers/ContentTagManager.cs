﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public static class ContentTagManager
    {
        internal static Dictionary<string, List<ContentTag>> globalContentTagDictionary = new Dictionary<string, List<ContentTag>>();
        internal static Dictionary<string, List<ExtendedContent>> globalcontentTagExtendedContentDictionary = new Dictionary<string, List<ExtendedContent>>();

        internal static void PopulateContentTagData()
        {
            List<string> allContentTagStringsList = new List<string>();
            Dictionary<string, List<ContentTag>> contentTagDictionary = new Dictionary<string, List<ContentTag>>();
            List<ContentTag> allContentTagsList = new List<ContentTag>();

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods.Concat(new List<ExtendedMod>(){PatchedContent.VanillaMod}))
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                    foreach (ContentTag contentTag in extendedContent.ContentTags)
                        if (!allContentTagsList.Contains(contentTag))
                            allContentTagsList.Add(contentTag);

            foreach (ContentTag contentTag in allContentTagsList)
            {
                if (contentTagDictionary.TryGetValue(contentTag.contentTagName, out List<ContentTag> contentTagsList))
                    contentTagsList.Add(contentTag);
                else
                    contentTagDictionary.Add(contentTag.contentTagName, new List<ContentTag>{contentTag});
            }

            globalContentTagDictionary = new Dictionary<string, List<ContentTag>>(contentTagDictionary);

            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods.Concat(new List<ExtendedMod>() { PatchedContent.VanillaMod }))
                foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                    foreach (ContentTag contentTag in extendedContent.ContentTags)
                    {
                        if (globalcontentTagExtendedContentDictionary.TryGetValue(contentTag.contentTagName, out List<ExtendedContent> extendedContentList))
                            extendedContentList.Add(extendedContent);
                        else
                            globalcontentTagExtendedContentDictionary.Add(contentTag.contentTagName, new List<ExtendedContent>{extendedContent});
                    }    
                    string debugString = "Global Tag Dictionary Report" + "\n\n";

            foreach (KeyValuePair<string, List<ContentTag>> globalContentTagPair in contentTagDictionary)
                debugString += "\nTag: " + globalContentTagPair.Key + ", Found Matching ContentTags: " + globalContentTagPair.Value.Count;

            DebugHelper.Log(debugString, DebugType.Developer);
        }

        internal static List<ContentTag> CreateNewContentTags(List<string> tags)
        {
            List<ContentTag> returnList = new List<ContentTag>();

            foreach (string tag in tags)
                if (!string.IsNullOrEmpty(tag))
                    returnList.Add(ContentTag.Create(tag));

            return (returnList);
        }

        public static List<ExtendedContent> GetAllExtendedContentsByTag(string tag)
        {
            if (globalcontentTagExtendedContentDictionary.TryGetValue(tag, out List<ExtendedContent> extendedContents))
                return (extendedContents);
            else
                return (new List<ExtendedContent>());
        }

        public static bool TryGetContentTagColour(ExtendedContent extendedContent, string tag, out Color color)
        {
            color = Color.white;
            foreach (ContentTag contentTag in extendedContent.ContentTags)
                if (contentTag.contentTagName == tag)
                {
                    color = contentTag.contentTagColor;
                    return (true);
                }
            return (false);
        }

        internal static void MergeAllExtendedModTags()
        {
            foreach (ExtendedMod extendedMod in PatchedContent.ExtendedMods.Concat(new List<ExtendedMod>() { PatchedContent.VanillaMod }))
                MergeExtendedModTags(extendedMod);
        }

        internal static void MergeExtendedModTags(ExtendedMod extendedMod)
        {
            Dictionary<ContentTag, List<ExtendedContent>> foundContentTagsDict = new Dictionary<ContentTag, List<ExtendedContent>>();
            Dictionary<ContentTag, ContentTag> replaceContentTagDict = new Dictionary<ContentTag, ContentTag>();
            foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
                foreach (ContentTag contentTag in extendedContent.ContentTags)
                {
                    if (foundContentTagsDict.TryGetValue(contentTag, out List<ExtendedContent> foundContentTagsList))
                        foundContentTagsList.Add(extendedContent);
                    else
                        foundContentTagsDict.Add(contentTag, new List<ExtendedContent>() { extendedContent });
                }

            foreach (ContentTag validContentTag in foundContentTagsDict.Keys)
                foreach (ContentTag invalidContentTag in foundContentTagsDict.Keys)
                    if (validContentTag.contentTagName.ToLower() == invalidContentTag.contentTagName.ToLower())
                    {
                        if (!replaceContentTagDict.ContainsKey(invalidContentTag))
                            replaceContentTagDict.Add(invalidContentTag, validContentTag);
                    }

            foreach (ExtendedContent extendedContent in extendedMod.ExtendedContents)
            {
                foreach (KeyValuePair<ContentTag, ContentTag> contentTagPair in replaceContentTagDict)
                    if (extendedContent.ContentTags.Contains(contentTagPair.Key))
                        extendedContent.ContentTags[extendedContent.ContentTags.IndexOf(contentTagPair.Key)] = contentTagPair.Value;
            }
        }
    }
}
