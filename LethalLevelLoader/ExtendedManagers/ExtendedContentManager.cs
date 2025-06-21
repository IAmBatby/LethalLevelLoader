using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader
{
    public abstract class ExtendedContentManager : NetworkBehaviour
    {

    }

    public abstract class ExtendedContentManager<E,C,M> : ExtendedContentManager, IExtendedManager<E,C,M> where E : ExtendedContent<E,C,M>, IExtendedContent<E,C,M> where M : ExtendedContentManager, IExtendedManager<E,C,M>
    {
        public static List<E> ExtendedContents { get; private set; } = new List<E>();
        public static Dictionary<C, E> ExtensionDictionary { get; private set; } = new Dictionary<C, E>();

        public List<E> GetExtendedContents() => ExtendedContents;
        public Dictionary<C,E> GetExtensionDictionary() => ExtensionDictionary;

        public static void RegisterContent(E e)
        {
            if (e == null || e.Content == null)
                DebugHelper.LogError("Null " + typeof(E) + "!", DebugType.User);
            else if (e.Content == null)
                DebugHelper.LogError("Null " + typeof(E) + " " + e.name + " Content!", DebugType.User);
            else if (!ExtendedContents.Contains(e) && !ExtensionDictionary.ContainsKey(e.Content))
            {
                ExtendedContents.Add(e);
                ExtensionDictionary.Add(e.Content, e);
            }
            else
                DebugHelper.LogWarning("Already Registered " + typeof(E) + " " + e.name + " Content!", DebugType.User);
        }

        public static M Instance => PatchedContent.GetExtendedManager<E,C,M>();
    }
}
