using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace LethalLevelLoader
{
    public enum IntergrationStatus { Unprocessed, Invalidated, Validated, Registered, Initalized }
    public abstract class ExtendedContentManager : NetworkBehaviour
    {
        private static HashSet<ExtendedContent> ValidatedExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> InvalidatedExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> RegisteredExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> InitializedExtendedContents = new HashSet<ExtendedContent>();

        //This is pretty cursed but basicially I need to setup a static prefab for every ExtendedContentManager dynamically
        //and this is the best way to do so with no hardcoding and potential support for non LLL mods to implement content types.
        //We spawn a temp instance to utilise inheritence so we can make the real prefab in the context of each classes generic definition.
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            DebugHelper.Log("Test Init ExtendedContentManager", DebugType.User);
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
                if (type.IsAbstract == false && type.IsSubclassOf(typeof(ExtendedContentManager)))
                    if (Plugin.SetupObject.AddComponent(type) is ExtendedContentManager manager)
                        manager.CreateNetworkPrefab();
        }

        protected abstract void CreateNetworkPrefab();

        protected static void CatalogInvalidatedContent(ExtendedContent content) => TryAdd(InvalidatedExtendedContents, content);
        protected static void CatalogValidatedContent(ExtendedContent content) => TryAdd(ValidatedExtendedContents, content);
        protected static void CatalogRegisteredContent(ExtendedContent content) => TryAdd(RegisteredExtendedContents, content);
        protected static void CatalogInitializedContent(ExtendedContent content) => TryAdd(InitializedExtendedContents, content);

        protected static void TryAdd<T>(HashSet<T> set, T item)
        {
            if (!set.Contains(item))
                set.Add(item);
        }

        public static IntergrationStatus GetContentStatus(ExtendedContent content)
        {
            if (content == null) return (IntergrationStatus.Unprocessed);
            if (InitializedExtendedContents.Contains(content)) return (IntergrationStatus.Initalized);
            if (RegisteredExtendedContents.Contains(content)) return (IntergrationStatus.Registered);
            if (ValidatedExtendedContents.Contains(content)) return (IntergrationStatus.Validated);
            if (InvalidatedExtendedContents.Contains(content)) return (IntergrationStatus.Invalidated);
            return (IntergrationStatus.Unprocessed);
        }

        //private static

    }

    public abstract class ExtendedContentManager<E,C,M> : ExtendedContentManager, IExtendedManager<E,C,M> where E : ExtendedContent<E,C,M>, IExtendedContent<E,C,M> where M : ExtendedContentManager, IExtendedManager<E,C,M>
    {
        private static ExtendedContentManager<E, C, M> Prefab;
        public static M Instance => PatchedContent.GetExtendedManager<E, C, M>();

        public static List<E> ExtendedContents { get; private set; } = new List<E>();
        public static Dictionary<C, E> ExtensionDictionary { get; private set; } = new Dictionary<C, E>();

        public List<E> GetExtendedContents() => ExtendedContents;
        public Dictionary<C,E> GetExtensionDictionary() => ExtensionDictionary;

        private static void RegisterContent(ExtendedMod mod, E e)
        {
            e.OnBeforeRegistration();
            ExtendedContents.Add(e);
            ExtensionDictionary.Add(e.Content, e);
            CatalogValidatedContent(e);
            CatalogRegisteredContent(e);
            mod.RegisterExtendedContent(e);
        }

        public static bool TryRegisterContent(ExtendedMod mod, E content)
        {
            string errorText = string.Empty;
            if (content == null)
                errorText = "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + mod.ModName + "!";
            else if (content.Content == null)
                errorText = "ExtendedContent Could Not Be Registered To ExtendedMod: " + mod.ModName + " Due To Null Base Content!";
            else if (ExtendedContents.Contains(content) || ExtensionDictionary.ContainsKey(content.Content))
                errorText = "ExtendedContent Could Not Be Registered To ExtendedMod: " + mod.ModName + " Due To Already Being Registered To This Content Manager!";
            else
            {
                (bool, string) result = Prefab.ValidateExtendedContent(content);
                if (result.Item1 == false)
                    errorText = "Null ExtendedContent Could Not Be Registered To ExtendedMod: " + mod.ModName + " Due To Failed Validation Check! " + result.Item2;
            }
            if (!string.IsNullOrEmpty(errorText))
            {
                DebugHelper.LogError(errorText, DebugType.User);
                CatalogInvalidatedContent(content);
                return (false);
            }
            DebugHelper.Log("Registering: " + content, DebugType.User);
            RegisterContent(mod, content);
            return (true);
        }

        protected override sealed void CreateNetworkPrefab()
        {
            DebugHelper.Log("Initializing: " + this.GetType(), DebugType.User);
            Prefab = Utilities.CreateNetworkPrefab<ExtendedContentManager<E, C, M>>(GetType(), typeof(M).Name + " (NetworkPrefab)");
            GameObject.Destroy(this);
        }

        protected abstract (bool result, string log) ValidateExtendedContent(E content);
    }
}
