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
    public enum RestorationPeriod { MainMenu, Lobby }
    public abstract class ExtendedContentManager : NetworkBehaviour
    {
        private static List<ExtendedContentManager> allContentManagerPrefabs = new List<ExtendedContentManager>();
        private static HashSet<ExtendedContent> ValidatedExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> InvalidatedExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> RegisteredExtendedContents = new HashSet<ExtendedContent>();
        private static HashSet<ExtendedContent> InitializedExtendedContents = new HashSet<ExtendedContent>();

        //Just quality of life.
        protected static RoundManager RoundManager => Patches.RoundManager;
        protected static StartOfRound StartOfRound => Patches.StartOfRound;
        protected static Terminal Terminal => Patches.Terminal;
        protected static TerminalManager.KeywordReferences Keywords => TerminalManager.Keywords;
        protected static TerminalManager.NodeReferences Nodes => TerminalManager.Nodes;

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

        internal static void ProcessContentNetworking()
        {
            foreach (ExtendedContent content in RegisteredExtendedContents)
                ExtendedNetworkManager.RegisterNetworkContent(content);
        }

        protected void AddPrefab(ExtendedContentManager contentManager) => allContentManagerPrefabs.Add(contentManager);
    }

    public abstract class ExtendedContentManager<E> : ExtendedContentManager, IContentManager<E> where E : UnityEngine.Object, IManagedContent, IExtendedContent;

    public abstract class ExtendedContentManager<E, C> : ExtendedContentManager<E> where E : ExtendedContent, IManagedContent, IExtendedContent<C>
    {
        private static ExtendedContentManager<E, C> Prefab;

        internal static List<E> ExtendedContents { get; private set; } = new List<E>();
        internal static Dictionary<C, E> ExtensionDictionary { get; private set; } = new Dictionary<C, E>();

        internal static List<E> ActiveContents { get; private set; } = new List<E>();

        public List<E> GetExtendedContents() => new List<E>(ExtendedContents);
        public Dictionary<C,E> GetExtensionDictionary() => new Dictionary<C, E>(ExtensionDictionary);

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
            Prefab = Utilities.CreateNetworkPrefab<ExtendedContentManager<E,C>>(GetType(), GetType().Name + " (NetworkPrefab)");
            AddPrefab(Prefab);

            Events.OnCurrentStateChanged.AddListener(OnGameStateChanged);
            Events.OnInInitalizedLobbyStateChanged.AddListener(OnLobbyStateChanged);

            Events.OnInitializeContent.AddListener(CreateExtendedVanillaContent, StepType.Before);
            Events.OnInitializeContent.AddListener(InitializeContent, StepType.On);
            Events.OnInitializeContent.AddListener(PopulateTerminalData, StepType.After);

            Events.OnPatchGame.AddListener(Prefab.PatchGame, StepType.On);

            GameObject.Destroy(this);
        }
        private static void OnGameStateChanged(GameStates state)
        {
            //Maybe network stuff here

            if (state == GameStates.MainMenu && Events.FurthestState == GameStates.Lobby || Events.FurthestState == GameStates.Moon)
                Prefab.OnLobbyUnloaded();
        }

        private static void OnLobbyStateChanged(bool state)
        {
            if (!state) return;
            if (Events.FurthestState == Events.CurrentState)
                Prefab.OnInitialLobbyLoaded();
            Prefab.OnLobbyLoaded();
        }

        protected virtual void OnInitialLobbyLoaded()
        {
            DebugHelper.Log(GetType() + ": OnInitialLobbyLoaded!", DebugType.User);
        }
        protected virtual void OnLobbyLoaded()
        {
            DebugHelper.Log(GetType() + ": OnLobbyLoaded!", DebugType.User);
        }
        protected virtual void OnLobbyUnloaded()
        {
            DebugHelper.Log(GetType() + ": OnLobbyUnloaded!", DebugType.User);
        }

        //This runs the first time a lobby is loaded in the current game session in order for content to initalize and setup itself which may require references that only exist when in the lobby.
        private static void InitializeContent()
        {
            foreach (E content in ExtendedContents)
                content.Initialize();
        }

        internal static bool ActivateContent(E content)
        {
            if (ActiveContents.Contains(content)) return (false);

            content.SetGameID(ActiveContents.Count);
            ActiveContents.Add(content);
            return (true);
        }

        //Vanilla stuff needs to be manually inserted to the front of our lists because we must make it after all custom stuff is loaded
        //But it must be first in order for ID's to be correct
        private static void CreateExtendedVanillaContent()
        {
            List<E> validContents = new List<E>();
            foreach (C content in Prefab.GetVanillaContent())
            {
                E extended = Prefab.ExtendVanillaContent(content);
                if (TryRegisterContent(PatchedContent.VanillaMod, extended) == false) continue;
                validContents.Add(extended);
                ExtendedContents.Remove(extended);
            }
            ExtendedContents.InsertRange(0,validContents);
        }

        private static void PopulateTerminalData()
        {
            foreach (E content in ExtendedContents)
                Prefab.PopulateContentTerminalData(content);
        }

        public static bool TryGetExtendedContent(C content, out E extendedContent)
        {
            extendedContent = null;
            if (content != null && ExtensionDictionary.TryGetValue(content, out E returnC))
                extendedContent = returnC;
            return (extendedContent != null);
        }

        //We'll see lol
        //protected virtual void OnModEnabled() { }
        //protected virtual void OnModDisabled() { }

        protected abstract (bool result, string log) ValidateExtendedContent(E content);

        //Seperating these two because it removes the foreach loop in every implementation which i think looks cleaner without
        protected abstract List<C> GetVanillaContent();
        protected abstract E ExtendVanillaContent(C content);

        //This runs on every lobby load and is where custom content is shoved into base game lists, functions etc.
        //This runs after InitializeContent if it's the initial lobby load.
        protected abstract void PatchGame();

        //This runs on every lobby unload and is where custom content is pulled out of the base game lists, functions etc.
        //The idea here is that the game should be in a state where the content was never patched in in the first place.
        protected abstract void UnpatchGame();

        protected abstract void PopulateContentTerminalData(E content);

    }
}
