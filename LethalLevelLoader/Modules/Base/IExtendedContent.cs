using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace LethalLevelLoader
{
    public interface IExtendedContent;
    public interface IExtendedContent<C> : IExtendedContent
    {
        public C Content { get; }
    }

    public interface IContentManager;
    public interface IContentManager<E> : IContentManager where E : UnityEngine.Object, IManagedContent;

    public interface IManagedContent;
    public interface IManagedContent<M> : IManagedContent where M : UnityEngine.Object, IContentManager;
}
