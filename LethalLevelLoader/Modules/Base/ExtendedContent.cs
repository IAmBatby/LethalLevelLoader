﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine;

namespace LethalLevelLoader
{
    public abstract class ExtendedContent : ScriptableObject
    {
        public ExtendedMod ExtendedMod { get; internal set; }
        public int GameID { get; private set; }

        public ContentType ContentType { get; internal set; } = ContentType.Vanilla;
        [field: SerializeField] public List<ContentTag> ContentTags { get; internal set; } = new List<ContentTag>();
        public string ModName => ExtendedMod.ModName;
        public string AuthorName => ExtendedMod.AuthorName;
        public string UniqueIdentificationName => AuthorName.ToLowerInvariant() + "." + ModName.ToLowerInvariant() + "." + name.ToLowerInvariant();
        public IntergrationStatus CurrentStatus => ExtendedContentManager.GetContentStatus(this);

        internal abstract void Register(ExtendedMod mod);
        protected List<PrefabReference> NoPrefabReferences { get; private set; } = new List<PrefabReference>();
        protected List<GameObject> NoNetworkPrefabs { get; private set; } = new List<GameObject>();
        internal abstract List<PrefabReference> GetPrefabReferencesForRestorationOrRegistration();
        internal abstract List<GameObject> GetNetworkPrefabsForRegistration();

        internal virtual void TryCreateMatchingProperties() { }
        internal virtual void Initialize() { }
        internal virtual void OnBeforeRegistration() { }
        protected virtual void OnGameIDChanged() { }

        internal static E Create<E,C,M>(string newName, C newContent) where E : ExtendedContent<C,M> where M : UnityEngine.Object, IContentManager
        {
            E instance = ScriptableObject.CreateInstance<E>();
            instance.SetContent(newContent);
            instance.name = newName;
            return (instance);
        }

        internal void SetGameID(int newID)
        {
            GameID = newID;
            OnGameIDChanged();
        }

        public bool TryGetTag(string tag) => ContentTags.Select(c => c.contentTagName).Contains(tag);
        public bool TryGetTag(string tag, out ContentTag returnTag)
        {
            returnTag = ContentTags.Where(c => c.contentTagName == tag).FirstOrDefault();
            return (false);
        }

        public bool TryAddTag(string tag)
        {
            if (TryGetTag(tag)) return (false);
            ContentTags.Add(ContentTag.Create(tag));
            return (true);
        }

        /*Obsolete*/ public List<string> ContentTagStrings { get; internal set; } = new List<string>();
    }

    public abstract class ExtendedContent<C, M> : ExtendedContent, IManagedContent<M>, IExtendedContent<C> where M : UnityEngine.Object, IContentManager
    {
        public virtual C Content { get; protected set; }
        internal void SetContent(C newContent) => Content = newContent;
    }

    public abstract class ExtendedContent<E,C,M> : ExtendedContent<C,M>, IManagedContent<M>, IExtendedContent<C> where M : UnityEngine.Object, IContentManager where E : ExtendedContent, IExtendedContent<C>, IManagedContent<M>
    {
        internal override void Register(ExtendedMod mod) => ExtendedContentManager<E, C>.TryRegisterContent(mod, this as E);
    }


    [Serializable]
    public class StringWithRarity
    {
        [SerializeField] private string _name;
        [SerializeField, Range(0,300)] private int _rarity;

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

        public Vector2WithRarity(Vector2 vector2, int newRarity) {_minMax.x = vector2.x; _minMax.y = vector2.y; _rarity = newRarity; }
        public Vector2WithRarity(float newMin, float newMax, int newRarity) { _minMax.x = newMin; _minMax.y = newMax; _rarity = newRarity; }
    }
}
