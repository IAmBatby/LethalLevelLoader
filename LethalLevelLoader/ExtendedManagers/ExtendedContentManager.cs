using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalLevelLoader.ExtendedManagers
{
    public abstract class ExtendedContentManager : NetworkBehaviour
    {

    }

    public abstract class ExtendedContentManager<E,C,M> : ExtendedContentManager, IExtendedManager<E,C,M> where E : ExtendedContent<E,C,M>, IExtendedContent<E,C,M> where M : ExtendedContentManager, IExtendedManager<E,C,M>
    {
        public static List<E> ExtendedContents { get; set; } = new List<E>();
        public static Dictionary<C, E> ExtensionDictionary { get; set; } = new Dictionary<C, E>();

        public List<E> GetExtendedContents() => ExtendedContents;
        public Dictionary<C,E> GetExtensionDictionary() => ExtensionDictionary;

        public static M Instance => PatchedContent.GetExtendedManager<E,C,M>();
    }
}
