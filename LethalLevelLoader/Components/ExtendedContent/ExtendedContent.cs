using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LethalLevelLoader
{
    public class ExtendedContent : ScriptableObject
    {
        public ExtendedMod ExtendedMod { get; internal set; }
        public ContentType ContentType { get; internal set; } = ContentType.Vanilla;
        /*Obsolete*/ public List<string> ContentTagStrings { get; internal set; } = new List<string>();
        [field: SerializeField] public List<ContentTag> ContentTags { get; internal set; } = new List<ContentTag>();
        //public List<string> ContentTagsAsStrings => ContentTags.Select(t => t.contentTagName).ToList();

        public string ModName => ExtendedMod.ModName;
        public string AuthorName => ExtendedMod.AuthorName;

        internal virtual void TryCreateMatchingProperties()
        {

        }

        public bool TryGetTag(string tag)
        {
            foreach (ContentTag contentTag in ContentTags)
                if (contentTag.contentTagName == tag)
                    return (true);
            return (false);
        }

        public bool TryGetTag(string tag, out ContentTag returnTag)
        {
            returnTag = null;
            foreach (ContentTag contentTag in ContentTags)
                if (contentTag.contentTagName == tag)
                {
                    returnTag = contentTag;
                    return (true);
                }
            return (false);
        }

        public bool TryAddTag(string tag)
        {
            if (TryGetTag(tag) == false)
            {
                ContentTags.Add(ContentTag.Create(tag));
                return (true);
            }
            return (false);
        }
    }

    [Serializable]
    public class StringWithRarity
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        [Range(0, 300)]
        private int _rarity;

        [HideInInspector] public string Name { get { return (_name); } set { _name = value; } }
        [HideInInspector] public int Rarity { get { return (_rarity); } set { _rarity = value; } }
        [HideInInspector] public StringWithRarity(string newName, int newRarity) { _name = newName; _rarity = newRarity; }
    }

    [Serializable]
    public class Vector2WithRarity
    {
        [SerializeField] private Vector2 _minMax;
        [SerializeField] private int _rarity;

        [HideInInspector] public float Min { get { return (_minMax.x); } set { _minMax.x = value; } }
        [HideInInspector] public float Max { get { return (_minMax.y); } set { _minMax.y = value; } }
        [HideInInspector] public int Rarity { get { return (_rarity); } set { _rarity = value; } }

        public Vector2WithRarity(Vector2 vector2, int newRarity)
        {
            _minMax.x = vector2.x;
            _minMax.y = vector2.y;
            _rarity = newRarity;
        }

        public Vector2WithRarity(float newMin, float newMax, int newRarity)
        {
            _minMax.x = newMin;
            _minMax.y = newMax;
            _rarity = newRarity;
        }
    }
}
